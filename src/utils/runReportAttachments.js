// src/utils/runReportAttachments.js
// Shared by TestRunReportPage.vue and TestExecutionPage.vue.
// Loads defect attachments (screenshots / files) from execution_attachments and
// prepares them for the on-screen report and for the jsPDF report.

const API_ROOT = 'https://localhost:7049'
const API = `${API_ROOT}/api`
const MAX_IMG_PX = 1400 // downscale big screenshots so the PDF stays small

export const isImage = (name = '') => /\.(png|jpe?g|gif|webp|bmp)$/i.test(name)

// file_path may be a full URL or a path relative to the API's static files root
export const fileUrl = (a) => {
  const p = String(a.file_path || '').replace(/\\/g, '/')
  if (/^https?:/i.test(p)) return p
  return `${API_ROOT}/${p.replace(/^\/+/, '')}`
}

/**
 * Loads attachments for the given execution step ids.
 * Returns Map<String(executionStepId), [{ id, file_name, url, is_image }]>
 */
export const loadAttachmentMap = async (stepIds) => {
  const ids = new Set(stepIds.filter(Boolean).map(String))
  const byStep = new Map()
  if (!ids.size) return byStep

  let all = []
  try {
    const res = await fetch(`${API}/ExecutionAttachments`)
    if (res.ok) all = await res.json()
    else console.warn('ExecutionAttachments list failed:', res.status)
  } catch (err) {
    console.error('Could not load attachments:', err)
  }

  all
    .filter((a) => ids.has(String(a.execution_step_id)))
    .forEach((a) => {
      const k = String(a.execution_step_id)
      if (!byStep.has(k)) byStep.set(k, [])
      byStep.get(k).push({
        id: a.id,
        file_name: a.file_name,
        url: fileUrl(a),
        is_image: isImage(a.file_name),
      })
    })
  return byStep
}

/**
 * Adds `defect.attachments` to every step that has a defect.
 * Each step must carry `execution_step_id`.
 */
export const withAttachments = async (steps) => {
  const byStep = await loadAttachmentMap(
    steps.filter((s) => s.execution_step_id && s.defect).map((s) => s.execution_step_id)
  )
  if (!byStep.size) return steps
  return steps.map((s) => {
    const list = byStep.get(String(s.execution_step_id))
    if (!s.defect || !list) return s
    return { ...s, defect: { ...s.defect, attachments: list } }
  })
}

// ---- PDF helpers ----
const loadImage = (src) =>
  new Promise((resolve, reject) => {
    const im = new Image()
    im.onload = () => resolve(im)
    im.onerror = reject
    im.src = src
  })

const blobToDataUrl = (blob) =>
  new Promise((resolve, reject) => {
    const fr = new FileReader()
    fr.onload = () => resolve(fr.result)
    fr.onerror = reject
    fr.readAsDataURL(blob)
  })

// Fetch -> canvas -> PNG/JPEG data URL (jsPDF only understands PNG/JPEG reliably)
const toPdfImage = async (url) => {
  const res = await fetch(url)
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  const blob = await res.blob()
  const isJpeg = /jpe?g/i.test(blob.type)
  const im = await loadImage(await blobToDataUrl(blob))
  const scale = Math.min(1, MAX_IMG_PX / im.naturalWidth)
  const w = Math.round(im.naturalWidth * scale)
  const h = Math.round(im.naturalHeight * scale)
  const canvas = document.createElement('canvas')
  canvas.width = w
  canvas.height = h
  const ctx = canvas.getContext('2d')
  ctx.fillStyle = '#ffffff'
  ctx.fillRect(0, 0, w, h)
  ctx.drawImage(im, 0, 0, w, h)
  return {
    dataUrl: canvas.toDataURL(isJpeg ? 'image/jpeg' : 'image/png', 0.9),
    format: isJpeg ? 'JPEG' : 'PNG',
    w,
    h,
  }
}

/** Downloads every image attachment and adds dataUrl/format/w/h. Failures fall back to filename only. */
export const embedImages = async (steps) =>
  Promise.all(
    steps.map(async (s) => {
      const atts = s.defect?.attachments
      if (!atts?.length) return s
      const out = await Promise.all(
        atts.map(async (a) => {
          if (!a.is_image) return a
          try {
            return { ...a, ...(await toPdfImage(a.url)) }
          } catch (err) {
            console.warn('Could not embed image', a.file_name, err)
            return a
          }
        })
      )
      return { ...s, defect: { ...s.defect, attachments: out } }
    })
  )