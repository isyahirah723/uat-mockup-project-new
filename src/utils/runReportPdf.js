import { jsPDF } from 'jspdf'
import { kotraLogoBase64 } from '@/assets/kotraLogo'



const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')

// Duration between two timestamps as mm:ss (hours roll into minutes, e.g. 75:03)
export const formatDuration = (start, end) => {
  const ms = new Date(end) - new Date(start)
  if (!start || !end || Number.isNaN(ms) || ms < 0) return '-'
  const totalSec = Math.floor(ms / 1000)
  const mm = Math.floor(totalSec / 60)
  const ss = totalSec % 60
  return `${String(mm).padStart(2, '0')}:${String(ss).padStart(2, '0')}`
}

// Colour of a step's status dot / label
const statusRgb = (status) => {
  if (status === 'Passed') return [34, 197, 94]
  if (status === 'Failed') return [239, 68, 68]
  if (status === 'Blocked' || status === 'N/A') return [148, 163, 184]
  return [100, 116, 139] // Not executed
}

const statusLabel = (status) => {
  if (status === 'Passed') return 'Passed'
  if (status === 'Failed') return 'Failed'
  if (status === 'Blocked') return 'Blocked'
  if (status === 'N/A') return 'N/A'
  return 'Not Run'
}

// Donut chart as a PNG data URL for jsPDF.addImage
const buildResultsPieChart = (passed, failed, skipped) => {
  const size = 240
  const canvas = document.createElement('canvas')
  canvas.width = size
  canvas.height = size
  const ctx = canvas.getContext('2d')
  const cx = size / 2
  const cy = size / 2
  const radius = size / 2 - 8
  const total = passed + failed + skipped

  const slices = total === 0
    ? [{ value: 1, color: '#e2e8f0' }]
    : [
        { value: passed, color: '#22c55e' },
        { value: failed, color: '#ef4444' },
        { value: skipped, color: '#94a3b8' },
      ].filter((sl) => sl.value > 0)
  const sum = slices.reduce((a, sl) => a + sl.value, 0)

  let angle = -Math.PI / 2
  slices.forEach((sl) => {
    const sweep = (sl.value / sum) * Math.PI * 2
    ctx.beginPath()
    ctx.moveTo(cx, cy)
    ctx.arc(cx, cy, radius, angle, angle + sweep)
    ctx.closePath()
    ctx.fillStyle = sl.color
    ctx.fill()
    angle += sweep
  })

  ctx.beginPath()
  ctx.arc(cx, cy, radius * 0.55, 0, Math.PI * 2)
  ctx.fillStyle = '#ffffff'
  ctx.fill()

  return canvas.toDataURL('image/png')
}

// Scales tried in order (biggest first). First one that fits on 1 page wins.
const FIT_SCALES = [1, 0.94, 0.88, 0.82, 0.76, 0.7, 0.64]
// Used when even the smallest scale cannot fit 1 page: keep it readable and let it run onto page 2+
const FALLBACK_SCALE = 0.85

// Lays out the whole report at a given scale and returns the jsPDF doc (not saved yet)
const buildPdf = (s, data, pieDataUrl) => {
  const { testCase, run, runLabel, testerName, department, steps = [], feedbackComment = '' } = data

  const doc = new jsPDF()
  const pageWidth = doc.internal.pageSize.getWidth()
  const pageHeight = doc.internal.pageSize.getHeight()
  const margin = 12
  const top = 12
  const contentWidth = pageWidth - margin * 2
  const bottomLimit = pageHeight - 10

  const u = (n) => n * s // scaled length (mm)
  const fs = (n) => Math.max(5.5, n * s) // scaled font size (pt), never unreadably small
  let y = top

  // Start a new page when the next block would run past the bottom margin
  const ensureSpace = (h) => {
    if (y + h > bottomLimit) {
      doc.addPage()
      y = top
    }
  }

  // ---- Header (compact) ----
  doc.setFontSize(fs(15))
  doc.setFont(undefined, 'bold')
  doc.setTextColor(15, 23, 42)
  doc.text('Test Case Run Report', margin, y + u(5))

  const logoWidth = u(24)
  let logoBottom = y
  try {
    const logoProps = doc.getImageProperties(kotraLogoBase64)
    const logoHeight = (logoProps.height / logoProps.width) * logoWidth
    doc.addImage(kotraLogoBase64, 'PNG', pageWidth - margin - logoWidth, y, logoWidth, logoHeight)
    logoBottom = y + logoHeight
  } catch (e) {
    // logo is optional — the report still works without it
  }

  y += u(10)
  doc.setFontSize(fs(7.5))
  doc.setFont(undefined, 'normal')
  doc.setTextColor(100, 116, 139)
  const companyLines = doc.splitTextToSize(
    'Kotra Pharma (M) Sdn. Bhd.  |  Block B, Jalan TTC 31, Cheng Industrial Estate, 75250 Cheng, Melaka, Malaysia',
    contentWidth - logoWidth - 4
  )
  doc.text(companyLines, margin, y)
  y += companyLines.length * u(3.4)
  y = Math.max(y, logoBottom) + u(1.5)

  doc.setDrawColor(15, 118, 110)
  doc.setLineWidth(0.6)
  doc.line(margin, y, pageWidth - margin, y)
  y += u(3)

  const sectionHeader = (title) => {
    ensureSpace(u(20)) 
    y += u(2.5)
    doc.setFontSize(fs(10))
    doc.setFont(undefined, 'bold')
    doc.setTextColor(15, 23, 42)
    doc.text(title, margin, y)
    y += u(1.5)
    doc.setDrawColor(15, 118, 110)
    doc.setLineWidth(0.5)
    doc.line(margin, y, pageWidth - margin, y)
    y += u(4.5)
  }

  
  const startedAt = run?.started_at
  const endedAt = run?.completed_at || new Date().toISOString()
  const caseLabel = [testCase?.test_case_code, testCase?.title].filter(Boolean).join(' - ') || '-'

  const LH = u(4.3)
  const labelW = u(27)
  const cell = (label, value, x, colW) => {
    doc.setFontSize(fs(8))
    doc.setFont(undefined, 'bold')
    doc.setTextColor(100, 116, 139)
    doc.text(label, x, y)
    doc.setFont(undefined, 'normal')
    doc.setTextColor(30, 41, 59)
    const lines = doc.splitTextToSize(String(value ?? '-'), colW - labelW - u(2))
    doc.text(lines, x + labelW, y)
    return lines.length * LH
  }

  sectionHeader('Test Run Overview')

  
  ensureSpace(LH * 2)
  y += cell('Test Case', caseLabel, margin, contentWidth) + u(0.8)

  const cells = [
    ['Run', runLabel || (run?.id ? `#${run.id}` : '-')],
    ['Tester', testerName || '-'],
  ]
  if (department) cells.push(['Department', department])
  cells.push(
    ['Overall Result', run?.run_status || '-'],
    ['Started', formatDateTime(startedAt)],
    ['Ended', formatDateTime(endedAt)],
    ['Duration (mm:ss)', formatDuration(startedAt, endedAt)]
  )
  const colW = contentWidth / 2
  for (let i = 0; i < cells.length; i += 2) {
    ensureSpace(LH * 2)
    const h1 = cell(cells[i][0], cells[i][1], margin, colW)
    const h2 = cells[i + 1] ? cell(cells[i + 1][0], cells[i + 1][1], margin + colW, colW) : 0
    y += Math.max(h1, h2) + u(0.8)
  }

  
  sectionHeader('Test Results Summary')
  const passedCount = steps.filter((st) => st.execution_status === 'Passed').length
  const failedCount = steps.filter((st) => st.execution_status === 'Failed').length
  const skippedCount = steps.filter((st) => st.execution_status === 'Blocked' || st.execution_status === 'N/A').length
  const totalCount = passedCount + failedCount + skippedCount

  const pieSize = u(24)
  ensureSpace(pieSize + u(2))
  const summaryTop = y
  const legend = [
    ['Passed', passedCount, [34, 197, 94]],
    ['Failed', failedCount, [239, 68, 68]],
  ]
 
  if (skippedCount > 0) legend.push(['Skipped', skippedCount, [148, 163, 184]])

  doc.setFontSize(fs(8.5))
  legend.forEach(([label, count, rgb]) => {
    doc.setFillColor(...rgb)
    doc.rect(margin, y - u(2.8), u(3), u(3), 'F')
    doc.setFont(undefined, 'bold')
    doc.setTextColor(15, 23, 42)
    doc.text(String(label), margin + u(5), y)
    doc.setFont(undefined, 'normal')
    doc.text(`${count} (${totalCount ? Math.round((count / totalCount) * 100) : 0}%)`, margin + u(30), y)
    y += u(5.2)
  })

  doc.addImage(pieDataUrl, 'PNG', pageWidth - margin - pieSize, summaryTop - u(3), pieSize, pieSize)
  y = Math.max(y, summaryTop - u(3) + pieSize) + u(1)

  // ---- Test Case Summary (steps, with the defect's Actual Result under the step) ----
  sectionHeader('Test Case Summary')
  doc.setFontSize(fs(8.5))
  doc.setFont(undefined, 'bold')
  doc.setTextColor(15, 23, 42)
  const titleLines = doc.splitTextToSize(testCase?.title || '-', contentWidth)
  ensureSpace(titleLines.length * u(4.2) + u(2))
  doc.text(titleLines, margin, y)
  y += titleLines.length * u(4.2) + u(1.5)

  steps.forEach((st, idx) => {
    const status = st.execution_status || 'Not Executed'
    const [r, g, b] = statusRgb(status)

    doc.setFontSize(fs(8.5))
    doc.setFont(undefined, 'normal')
    const labelLines = doc.splitTextToSize(`${idx + 1}. ${st.step_name || '-'}`, contentWidth - 8 - 22)
    ensureSpace(labelLines.length * u(4.2) + u(3))

    doc.setFillColor(r, g, b)
    doc.circle(margin + 2.5, y - u(1.1), u(1.2), 'F')
    doc.setTextColor(30, 41, 59)
    doc.text(labelLines, margin + 7, y)
    doc.setFont(undefined, 'bold')
    doc.setTextColor(r, g, b)
    doc.text(statusLabel(status), pageWidth - margin, y, { align: 'right' })
    y += labelLines.length * u(4.2) + u(1)

    // ---- Defect logged on this step ----
    const d = st.defect
    const hasDefect = d && (d.actual_result || d.description || d.affected_impact || d.comments || d.severity || d.ticket_id || d.attachments?.length)
    if (hasDefect) {
      const x = margin + 8
      const w = pageWidth - margin - x - 2
      let barStart = y - u(2)

      const drawBar = () => {
        if (y - u(1.5) > barStart) {
          doc.setDrawColor(239, 68, 68)
          doc.setLineWidth(0.7)
          doc.line(x - 3, barStart, x - 3, y - u(1.5))
        }
      }
      const breakIfNeeded = (h) => {
        if (y + h > bottomLimit) {
          drawBar()
          doc.addPage()
          y = top
          barStart = y - 3
        }
      }
      // Full-width wrapped paragraph: long text only uses the lines it needs
      const para = (label, text, primary) => {
        breakIfNeeded(u(8))
        doc.setFontSize(fs(6.5))
        doc.setFont(undefined, 'bold')
        doc.setTextColor(185, 28, 28)
        doc.text(label.toUpperCase(), x, y)
        y += u(3.3)

        const lh = primary ? u(4) : u(3.7)
        doc.setFontSize(fs(primary ? 8.5 : 7.5))
        doc.setFont(undefined, 'normal')
        if (primary) doc.setTextColor(15, 23, 42)
        else doc.setTextColor(71, 85, 105)
        doc.splitTextToSize(String(text), w).forEach((ln) => {
          breakIfNeeded(lh)
          doc.text(ln, x, y)
          y += lh
        })
        y += u(1)
      }

      if (d.actual_result) para('Actual Result', d.actual_result, true)

      const meta = []
      if (d.severity) meta.push(`Severity: ${d.severity}`)
      if (d.ticket_id) meta.push(`Ticket: ${d.ticket_id}`)
      if (meta.length) {
        breakIfNeeded(u(5))
        doc.setFontSize(fs(7))
        doc.setFont(undefined, 'bold')
        doc.setTextColor(100, 116, 139)
        doc.text(meta.join('   |   '), x, y)
        y += u(4)
      }

      if (d.description) para('Defect Description', d.description, false)
      if (d.affected_impact) para('Affected / Impact', d.affected_impact, false)
      if (d.comments) para('Comment', d.comments, false)

      // Evidence: images are small thumbnails laid out side by side; other files are listed as links
      if (d.attachments?.length) {
        const imgs = d.attachments.filter((a) => a.dataUrl && a.w && a.h)
        const files = d.attachments.filter((a) => !(a.dataUrl && a.w && a.h))

        breakIfNeeded(u(8))
        doc.setFontSize(fs(6.5))
        doc.setFont(undefined, 'bold')
        doc.setTextColor(185, 28, 28)
        doc.text('ATTACHMENTS', x, y)
        y += u(3.2)

        // thumbnail size limits (mm, before scaling) — change these two to make images bigger/smaller
        const maxW = u(46)
        const maxH = u(24)
        const gap = u(3)
        const capH = u(3.4)

        // pack thumbnails into rows
        const rows = []
        let cur = { items: [], w: 0, h: 0 }
        imgs.forEach((a) => {
          let iw = Math.min(maxW, a.w * 0.2646)
          let ih = (a.h / a.w) * iw
          if (ih > maxH) {
            ih = maxH
            iw = (a.w / a.h) * ih
          }
          if (cur.items.length && cur.w + gap + iw > w) {
            rows.push(cur)
            cur = { items: [], w: 0, h: 0 }
          }
          cur.w += (cur.items.length ? gap : 0) + iw
          cur.h = Math.max(cur.h, ih)
          cur.items.push({ a, iw, ih })
        })
        if (cur.items.length) rows.push(cur)

        rows.forEach((row) => {
          breakIfNeeded(row.h + capH + u(1.5))
          let cx = x
          row.items.forEach(({ a, iw, ih }) => {
            doc.addImage(a.dataUrl, a.format, cx, y, iw, ih)
            doc.setDrawColor(226, 232, 240)
            doc.setLineWidth(0.2)
            doc.rect(cx, y, iw, ih)
            doc.setFontSize(fs(6))
            doc.setFont(undefined, 'normal')
            doc.setTextColor(100, 116, 139)
            doc.text(doc.splitTextToSize(String(a.file_name || 'image'), iw)[0], cx, y + ih + u(2.6))
            cx += iw + gap
          })
          y += row.h + capH + u(1)
        })

        files.forEach((a) => {
          breakIfNeeded(u(4.5))
          doc.setFontSize(fs(7.5))
          doc.setFont(undefined, 'normal')
          doc.setTextColor(185, 28, 28)
          const name = doc.splitTextToSize(String(a.file_name || 'file'), w)[0]
          if (a.url) doc.textWithLink(`Attachment: ${name}`, x, y, { url: a.url })
          else doc.text(`Attachment: ${name}`, x, y)
          y += u(4.2)
        })
        y += u(0.5)
      }

      drawBar()
    }

    y += u(1)
  })

  // ---- Overall Feedback ----
  if (feedbackComment && String(feedbackComment).trim()) {
    sectionHeader('Overall Feedback')
    doc.setFontSize(fs(8.5))
    doc.setFont(undefined, 'normal')
    doc.setTextColor(30, 41, 59)
    doc.splitTextToSize(String(feedbackComment), contentWidth).forEach((ln) => {
      ensureSpace(u(4.3))
      doc.text(ln, margin, y)
      y += u(4.3)
    })
  }

  return doc
}

/**
 * @param {object} p
 * @param {object} p.testCase   { test_case_code, title }
 * @param {object} p.run        { id, run_status, started_at, completed_at }
 * @param {string} [p.runLabel] e.g. "RUN-002" (falls back to #<run id>)
 * @param {string} p.testerName
 * @param {string} [p.department]
 * @param {Array}  p.steps      [{ step_name, execution_status, defect: { actual_result, description,
 *                                affected_impact, severity, ticket_id, comments,
 *                                attachments: [{ file_name, url, dataUrl?, format?, w?, h? }] } | null }]
 * @param {string} [p.feedbackComment]
 */
export const generateRunReportPdf = (p) => {
  const steps = p.steps || []
  const passed = steps.filter((s) => s.execution_status === 'Passed').length
  const failed = steps.filter((s) => s.execution_status === 'Failed').length
  const skipped = steps.filter((s) => s.execution_status === 'Blocked' || s.execution_status === 'N/A').length
  const pieDataUrl = buildResultsPieChart(passed, failed, skipped)

  // 1) try to fit everything on ONE page (largest scale that fits)
  let doc = null
  for (const scale of FIT_SCALES) {
    const candidate = buildPdf(scale, p, pieDataUrl)
    if (candidate.getNumberOfPages() === 1) {
      doc = candidate
      break
    }
  }
  // 2) too much content (e.g. very long Overall Feedback): readable size, flows to page 2+
  if (!doc) doc = buildPdf(FALLBACK_SCALE, p, pieDataUrl)

  const safeName = (p.testerName || 'tester').replace(/\s+/g, '')
  doc.save(`${p.testCase?.test_case_code || 'TestCase'}_${safeName}_Run${p.run?.id || ''}.pdf`)
}