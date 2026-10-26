<template>
  <v-container class="pa-4 mx-auto report-page" style="max-width: 980px;">
    <!-- Toolbar (hidden when printing) -->
    <div class="d-flex align-center justify-space-between flex-wrap mb-4 no-print" style="gap: 8px;">
      <div class="d-flex align-center" style="gap: 12px;">
        <v-btn icon size="small" variant="tonal" color="#0f766e" title="Back to runs" @click="goBack">
          <v-icon>mdi-arrow-left</v-icon>
        </v-btn>
        <div class="text-h6 font-weight-bold">Test Run Report</div>
      </div>
      <v-btn
        color="#b91c1c"
        class="text-white text-capitalize font-weight-bold rounded-lg"
        prepend-icon="mdi-file-pdf-box"
        :disabled="!run || generating"
        :loading="generating"
        @click="generateReport"
      >Download PDF Report</v-btn>
    </div>

    <v-alert v-if="pdfError" type="error" variant="tonal" density="compact" closable class="mb-3 rounded-lg no-print" @click:close="pdfError = ''">
      {{ pdfError }}
    </v-alert>

    <div v-if="loading" class="text-center py-10">
      <v-progress-circular indeterminate color="#0f766e" />
    </div>

    <v-alert v-else-if="error" type="error" variant="tonal" class="rounded-lg">{{ error }}</v-alert>

    <div v-else-if="run" class="report-sheet">
      <!-- Title -->
      <div class="d-flex align-start justify-space-between flex-wrap mb-4" style="gap: 12px;">
        <div>
          <div class="text-caption text-grey font-weight-bold">{{ runCode }}</div>
          <div class="text-h5 font-weight-bold">{{ run.title || testCase?.title || 'Test run' }}</div>
          <div class="text-body-2 text-grey">{{ run.test_case_code || testCase?.test_case_code }}</div>
        </div>
        <v-chip label class="font-weight-bold text-white" size="large" :color="statusColor(run.run_status)">
          {{ run.run_status || 'Not Started' }}
        </v-chip>
      </div>

      <!-- Details -->
      <v-card variant="outlined" class="rounded-lg pa-4 mb-4">
        <v-row dense>
          <v-col cols="6" md="3"><div class="lbl">Tester</div><div class="val">{{ run.user_name || '-' }}</div></v-col>
          <v-col cols="6" md="3"><div class="lbl">Started</div><div class="val">{{ formatDateTime(run.started_at) }}</div></v-col>
          <v-col cols="6" md="3"><div class="lbl">Completed</div><div class="val">{{ formatDateTime(run.completed_at) }}</div></v-col>
          <v-col cols="6" md="3"><div class="lbl">Duration</div><div class="val">{{ duration }}</div></v-col>
          <v-col v-if="testCase?.module" cols="6" md="3"><div class="lbl">Module</div><div class="val">{{ testCase.module }}</div></v-col>
          <v-col v-if="testCase?.priority" cols="6" md="3"><div class="lbl">Priority</div><div class="val">{{ testCase.priority }}</div></v-col>
          <v-col v-if="testCase?.version_tag" cols="6" md="3"><div class="lbl">Version</div><div class="val">{{ testCase.version_tag }}</div></v-col>
        </v-row>
        <div v-if="testCase?.test_description" class="mt-3">
          <div class="lbl">Description</div>
          <div class="val" style="white-space: pre-wrap;">{{ testCase.test_description }}</div>
        </div>
      </v-card>

      <!-- Summary -->
      <v-row dense class="mb-4">
        <v-col v-for="s in summary" :key="s.label" cols="6" md="3">
          <v-card variant="outlined" class="rounded-lg pa-3 text-center">
            <div class="text-h5 font-weight-bold" :style="{ color: s.color }">{{ s.value }}</div>
            <div class="text-caption text-grey font-weight-bold">{{ s.label }}</div>
          </v-card>
        </v-col>
      </v-row>

      <!-- Steps -->
      <div class="text-subtitle-1 font-weight-bold mb-2">Step results</div>
      <div v-if="!steps.length" class="text-grey text-body-2 mb-4">No step results were recorded for this run.</div>

      <v-card v-for="(s, i) in steps" :key="s.id" variant="outlined" class="rounded-lg pa-3 mb-3 step-card">
        <div class="d-flex align-start justify-space-between" style="gap: 8px;">
          <div class="d-flex" style="gap: 10px;">
            <div class="step-no">{{ i + 1 }}</div>
            <div>
              <div class="font-weight-bold">{{ s.step_name || 'Step ' + (i + 1) }}</div>
              <div v-if="expectedFor(s)" class="text-caption text-grey mt-1">Expected: {{ expectedFor(s) }}</div>
            </div>
          </div>
          <v-chip size="small" label class="font-weight-bold text-white" :color="statusColor(s.execution_status)">
            {{ s.execution_status || 'Pending' }}
          </v-chip>
        </div>

        <div v-if="s.comments" class="mt-2 text-body-2">
          <span class="lbl">Remarks: </span><span style="white-space: pre-wrap;">{{ s.comments }}</span>
        </div>

        <div v-if="s.has_defect" class="defect-box mt-2 pa-2 rounded">
          <div class="d-flex align-center flex-wrap" style="gap: 8px;">
            <v-icon size="16" color="error">mdi-bug-outline</v-icon>
            <span class="font-weight-bold text-body-2">Defect {{ s.ticket_id || '' }}</span>
            <v-chip v-if="s.severity" size="x-small" label variant="tonal" color="error">{{ s.severity }}</v-chip>
          </div>
          <div v-if="s.actual_result" class="text-body-2 mt-1" style="white-space: pre-wrap;">
            <span class="lbl">Actual result: </span>{{ s.actual_result }}
          </div>
        </div>

        <div v-if="filesFor(s).length" class="mt-2">
          <div class="lbl mb-1">Attachments</div>
          <div class="d-flex flex-wrap" style="gap: 8px;">
            <template v-for="a in filesFor(s)" :key="a.id">
              <a v-if="isImage(a.file_name)" :href="fileUrl(a)" target="_blank" class="thumb">
                <img :src="fileUrl(a)" :alt="a.file_name" />
              </a>
              <v-chip v-else size="small" label variant="tonal" color="teal-darken-2" :href="fileUrl(a)" target="_blank">
                <v-icon start size="14">mdi-paperclip</v-icon>{{ a.file_name }}
              </v-chip>
            </template>
          </div>
        </div>
      </v-card>
    </div>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { generateRunReportPdf } from '@/utils/runReportPdf'
import { withAttachments, embedImages } from '@/utils/runReportAttachments'

const API = 'https://localhost:7049/api'
const FILE_ORIGIN = API.replace(/\/api$/, '')
const route = useRoute()
const router = useRouter()
const testCaseId = Number(route.params.id)
const runId = Number(route.params.runId)

const loading = ref(true)
const error = ref('')
const run = ref(null)
const testCase = ref(null)
const steps = ref([])
const attachments = ref([])
const cycle = ref(null)
const department = ref('')
const feedbackComment = ref('')
const generating = ref(false)
const pdfError = ref('')

const getJson = async (url) => {
  const res = await fetch(url).catch(() => null)
  if (!res) throw new Error('Cannot connect to the API.')
  if (!res.ok) return null
  return res.json()
}

onMounted(async () => {
  try {
    run.value = await getJson(`${API}/TestRuns/${runId}`)
    if (!run.value) {
      error.value = `Run ${runId} was not found.`
      return
    }
    const [tc, st, att] = await Promise.all([
      getJson(`${API}/TestCases/${run.value.test_case_id || testCaseId}`),
      getJson(`${API}/ExecutionSteps/by-run/${runId}`),
      getJson(`${API}/ExecutionAttachments`),
    ])
    testCase.value = tc

    // Extra data the PDF needs (same inputs the tester's own "Download PDF Report" uses)
    const [cy, us, fb] = await Promise.all([
      tc?.cycle_id ? getJson(`${API}/TestCycles/${tc.cycle_id}`).catch(() => null) : null,
      getJson(`${API}/Users`).catch(() => []),
      getJson(`${API}/TestFeedbacks/by-test-case/${run.value.test_case_id || testCaseId}`).catch(() => []),
    ])
    cycle.value = cy || null
    const tester = (Array.isArray(us) ? us : []).find((u) => u.id === run.value.executed_by)
    department.value = tester?.department || ''
    const mine = (Array.isArray(fb) ? fb : []).filter((f) => f.user_name && f.user_name === run.value.user_name && f.comment)
    feedbackComment.value = mine.length ? mine[0].comment : ''
    steps.value = Array.isArray(st) ? st : []
    const ids = new Set(steps.value.map((s) => s.id))
    attachments.value = (Array.isArray(att) ? att : []).filter((a) => ids.has(a.execution_step_id))
  } catch (e) {
    error.value = e.message || 'Could not load the report.'
  } finally {
    loading.value = false
  }
})

// ---------- helpers ----------
const runCode = computed(() => 'RUN-' + String(runId).padStart(3, '0'))
const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')
const statusColor = (s) => {
  switch (s) {
    case 'Passed': return 'success'
    case 'Failed': return 'error'
    case 'Blocked': return 'warning'
    case 'In Progress': return 'indigo-darken-2'
    default: return 'grey'
  }
}
const duration = computed(() => {
  if (!run.value?.started_at || !run.value?.completed_at) return '-'
  const ms = new Date(run.value.completed_at) - new Date(run.value.started_at)
  if (!(ms >= 0)) return '-'
  const s = Math.round(ms / 1000)
  const m = Math.floor(s / 60)
  return m ? `${m} min ${s % 60} s` : `${s} s`
})

const count = (st) => steps.value.filter((s) => s.execution_status === st).length
const summary = computed(() => [
  { label: 'Steps', value: steps.value.length, color: '#0f766e' },
  { label: 'Passed', value: count('Passed'), color: '#2e7d32' },
  { label: 'Failed', value: count('Failed'), color: '#c62828' },
  { label: 'Blocked', value: count('Blocked'), color: '#ef6c00' },
])

// expected result comes from the test case step with the same order number
const expectedFor = (s) => {
  const t = (testCase.value?.steps || []).find((x) => x.step_order === s.sequence_order)
  return t?.expected || ''
}

const filesFor = (s) => attachments.value.filter((a) => a.execution_step_id === s.id)
const fileUrl = (a) => (/^https?:/i.test(a.file_path) ? a.file_path : FILE_ORIGIN + a.file_path)
const isImage = (name = '') => /\.(png|jpe?g|gif|webp)$/i.test(name)


// Rebuild the steps in exactly the shape the execution page hands to the PDF generator
const reportSteps = computed(() => {
  const tmpl = testCase.value?.steps || []
  return steps.value.map((es) => {
    const t = tmpl.find((x) => x.step_order === es.sequence_order)
    return {
      test_step_id: t?.id ?? null,
      step_name: t?.description || es.step_name,
      sequence_order: es.sequence_order,
      expected: t?.expected || '',
      stage: 'done',
      execution_status: es.execution_status,
      execution_step_id: es.id,
      ticket_id: es.ticket_id || null,
      saving: false,
      has_defect: !!es.has_defect,
      defect: es.has_defect
        ? { actual_result: es.actual_result || '', severity: es.severity || 'Medium', ticket_id: es.ticket_id || '', comments: es.comments || '' }
        : null,
      remarks: es.comments || '',
      files: [],
      attachments: [],
    }
  })
})

const generateReport = async () => {
  if (!run.value) return
  generating.value = true
  pdfError.value = ''
  try {
    const stepsForPdf = await embedImages(await withAttachments(reportSteps.value))
    generateRunReportPdf({
      testCase: {
        ...(testCase.value || {}),
        id: run.value.test_case_id || testCaseId,
        test_case_code: run.value.test_case_code || testCase.value?.test_case_code,
        title: run.value.title || testCase.value?.title,
        run_as: run.value.executed_by,
        run_as_name: run.value.user_name,
        department: department.value,
      },
      cycle: cycle.value,
      run: run.value,
      testerName: run.value.user_name,
      department: department.value,
      steps: stepsForPdf,
      feedbackComment: feedbackComment.value,
    })
  } catch (e) {
    console.error('Failed to generate the PDF report:', e)
    pdfError.value = 'Could not generate the PDF report. ' + (e?.message || '')
  } finally {
    generating.value = false
  }
}
const goBack = () => router.push({ name: 'TestCaseRuns', params: { id: testCaseId } })
</script>

<style scoped>
.lbl {
  font-size: 0.7rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: rgba(var(--v-theme-on-surface), 0.6);
}
.val { font-size: 0.9rem; font-weight: 600; }
.step-no {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background: #0f766e;
  color: #fff;
  font-size: 0.75rem;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}
.defect-box {
  background: rgba(198, 40, 40, 0.08);
  border: 1px solid rgba(198, 40, 40, 0.3);
}
.thumb {
  display: block;
  width: 120px;
  height: 80px;
  border-radius: 6px;
  overflow: hidden;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}
.thumb img { width: 100%; height: 100%; object-fit: cover; }
.step-card { break-inside: avoid; }
</style>

<style>
@page { size: A4; margin: 12mm; }

@media print {
  .report-page .no-print,
  .no-print.d-flex,
  .v-navigation-drawer,
  .v-app-bar,
  header,
  nav { display: none !important; }
  body,
  .v-application,
  .v-main {
    background: #fff !important;
    color: #000 !important;
    padding: 0 !important;
  }
  .report-page { max-width: 100% !important; }
  .report-sheet .v-card { border-color: #bbb !important; background: #fff !important; color: #000 !important; }
  .report-sheet .text-grey { color: #555 !important; }
}
</style>