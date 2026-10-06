<template>
  <div class="report-page">

    <!-- Header bar -->
    <div class="page-bar elevation-1">
      <div class="d-flex align-center" style="gap: 12px;">
        <v-btn icon variant="tonal" size="small" class="back-btn" @click="goBack" title="Back to Runs">
          <v-icon size="20">mdi-arrow-left</v-icon>
        </v-btn>
        <div>
          <div class="page-title font-weight-bold">Test Case Run Report</div>
          <div v-if="testCase" class="page-crumb">
            <span class="crumb-code">{{ testCase.test_case_code || testCase.id }}</span>
            <span class="crumb-sep">/</span>
            <span>{{ testCase.title }}</span>
          </div>
        </div>
      </div>
      <v-btn
        color="#b91c1c"
        size="small"
        class="text-white text-capitalize font-weight-bold rounded-lg"
        elevation="0"
        prepend-icon="mdi-file-pdf-box"
        :disabled="!run || loading"
        @click="downloadPdf"
      >
        Download PDF Report
      </v-btn>
    </div>

    <div class="page-body">

      <div v-if="loading" class="text-center py-12">
        <v-progress-circular indeterminate color="primary" size="48" width="4" />
      </div>

      <v-alert v-else-if="error" type="error" variant="tonal" density="compact" class="rounded-lg">
        {{ error }}
      </v-alert>

      <template v-else-if="run">

        <!-- Overview -->
        <div class="card mb-5">
          <div class="card-title">Test Run Overview</div>
          <div class="info-grid">
            <div class="info-item">
              <div class="info-label">Run</div>
              <div class="info-value">{{ runLabel }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Tester</div>
              <div class="info-value">{{ testerName }}</div>
            </div>
            <div class="info-item" v-if="department">
              <div class="info-label">Department</div>
              <div class="info-value">{{ department }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Started</div>
              <div class="info-value">{{ formatDateTime(run.started_at) }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Ended</div>
              <div class="info-value">{{ formatDateTime(run.completed_at) }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Duration (mm:ss)</div>
              <div class="info-value">{{ duration }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Overall Result</div>
              <div class="info-value">
                <v-chip size="small" variant="flat" class="text-white font-weight-bold" :color="overallColor">{{ run.run_status }}</v-chip>
              </div>
            </div>
          </div>
        </div>

        <!-- Results summary -->
        <div class="card mb-5">
          <div class="card-title">Test Results Summary</div>
          <div class="summary-row">
            <div class="donut-wrap">
              <svg viewBox="0 0 100 100" width="120" height="120">
                <circle cx="50" cy="50" r="40" fill="none" stroke="#e2e8f0" stroke-width="14" />
                <circle
                  v-for="(sg, i) in donutSegments"
                  :key="i"
                  cx="50" cy="50" r="40" fill="none"
                  :stroke="sg.color" stroke-width="14"
                  :stroke-dasharray="sg.dash" :stroke-dashoffset="sg.offset"
                  transform="rotate(-90 50 50)"
                />
              </svg>
              <div class="donut-center">
                <div class="donut-num">{{ passRate }}%</div>
                <div class="donut-label">passed</div>
              </div>
            </div>
            <div class="legend">
              <div v-for="l in legend" :key="l.label" class="legend-item">
                <span class="legend-dot" :style="{ background: l.color }"></span>
                <span class="legend-label">{{ l.label }}</span>
                <span class="legend-count">{{ l.count }} ({{ pct(l.count) }}%)</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Steps -->
        <div class="card mb-5">
          <div class="card-title">Test Case Summary</div>
          <div class="case-title">{{ testCase?.title }}</div>

          <div v-if="!steps.length" class="text-caption text-grey">No steps recorded for this run.</div>

          <div v-for="(s, i) in steps" :key="i" class="step-row">
            <div class="step-head">
              <span class="step-dot" :style="{ background: statusHex(s.execution_status) }"></span>
              <span class="step-name">{{ i + 1 }}. {{ s.step_name }}</span>
              <span class="step-status" :style="{ color: statusHex(s.execution_status) }">{{ statusText(s.execution_status) }}</span>
            </div>

            <!-- Defect logged on this step: Actual Result shown right under the step -->
            <div v-if="s.defect" class="defect-box">
              <template v-if="s.defect.actual_result">
                <div class="defect-label">Actual Result</div>
                <div class="defect-text defect-main">{{ s.defect.actual_result }}</div>
              </template>

              <div v-if="s.defect.severity || s.defect.ticket_id" class="defect-meta">
                <span v-if="s.defect.severity" class="sev" :class="'sev-' + String(s.defect.severity).toLowerCase()">{{ s.defect.severity }}</span>
                <span v-if="s.defect.ticket_id">Ticket: {{ s.defect.ticket_id }}</span>
              </div>

              <!-- Evidence uploaded by the tester: screenshots as thumbnails, other files as links -->
              <div v-if="s.defect.attachments?.length" class="att-block">
                <div class="defect-label">Attachments</div>
                <div class="att-list">
                  <template v-for="a in s.defect.attachments" :key="a.id">
                    <a v-if="a.is_image" :href="a.url" target="_blank" rel="noopener" class="att-thumb" :title="a.file_name">
                      <img :src="a.url" :alt="a.file_name" />
                    </a>
                    <a v-else :href="a.url" target="_blank" rel="noopener" class="att-file">
                      <v-icon size="14">mdi-paperclip</v-icon> {{ a.file_name }}
                    </a>
                  </template>
                </div>
              </div>

              <template v-if="hasMoreDetails(s.defect)">
                <button type="button" class="more-btn" @click="s.open = !s.open">
                  <v-icon size="14">{{ s.open ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
                  {{ s.open ? 'Hide defect details' : 'Show defect details' }}
                </button>
                <div v-if="s.open" class="defect-more">
                  <div v-if="s.defect.description">
                    <div class="defect-label">Defect Description</div>
                    <div class="defect-text">{{ s.defect.description }}</div>
                  </div>
                  <div v-if="s.defect.affected_impact">
                    <div class="defect-label">Affected / Impact</div>
                    <div class="defect-text">{{ s.defect.affected_impact }}</div>
                  </div>
                  <div v-if="s.defect.comments">
                    <div class="defect-label">Comment</div>
                    <div class="defect-text">{{ s.defect.comments }}</div>
                  </div>
                </div>
              </template>
            </div>
          </div>
        </div>

        <!-- Feedback -->
        <div v-if="feedbackComment" class="card mb-5">
          <div class="card-title">Overall Feedback</div>
          <div class="defect-text">{{ feedbackComment }}</div>
        </div>

      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { generateRunReportPdf, formatDuration } from '@/utils/runReportPdf'
import { withAttachments, embedImages } from '@/utils/runReportAttachments'

const API = 'https://localhost:7049/api'
const route = useRoute()
const router = useRouter()
const testCaseId = route.params.id
const runId = route.params.runId

const loading = ref(true)
const error = ref('')
const testCase = ref(null)
const run = ref(null)
const runLabel = ref('-')
const testerName = ref('-')
const department = ref('')
const steps = ref([])
const feedbackComment = ref('')

const goBack = () => router.push({ name: 'TestCaseRuns', params: { id: testCaseId } })
const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')
const duration = computed(() => formatDuration(run.value?.started_at, run.value?.completed_at))

const statusHex = (st) => {
  if (st === 'Passed') return '#22c55e'
  if (st === 'Failed') return '#ef4444'
  if (st === 'Blocked' || st === 'N/A') return '#94a3b8'
  return '#64748b'
}
const statusText = (st) => (['Passed', 'Failed', 'Blocked', 'N/A'].includes(st) ? st : 'Not Run')

const overallColor = computed(() => {
  const st = run.value?.run_status
  if (st === 'Passed') return '#10b981'
  if (st === 'Failed' || st === 'Not Approved') return '#ef4444'
  return '#64748b'
})

// ---- summary ----
const passed = computed(() => steps.value.filter((s) => s.execution_status === 'Passed').length)
const failed = computed(() => steps.value.filter((s) => s.execution_status === 'Failed').length)
const skipped = computed(() => steps.value.filter((s) => s.execution_status === 'Blocked' || s.execution_status === 'N/A').length)
const total = computed(() => passed.value + failed.value + skipped.value)
const pct = (n) => (total.value ? Math.round((n / total.value) * 100) : 0)
const passRate = computed(() => pct(passed.value))

// "Skipped" is only listed when a step really was Blocked / N/A in this run
const legend = computed(() => {
  const list = [
    { label: 'Passed', count: passed.value, color: '#22c55e' },
    { label: 'Failed', count: failed.value, color: '#ef4444' },
  ]
  if (skipped.value > 0) list.push({ label: 'Skipped', count: skipped.value, color: '#94a3b8' })
  return list
})

const CIRC = 2 * Math.PI * 40
const donutSegments = computed(() => {
  if (!total.value) return []
  let acc = 0
  return legend.value
    .filter((l) => l.count > 0)
    .map((l) => {
      const len = (l.count / total.value) * CIRC
      const seg = { color: l.color, dash: `${len} ${CIRC - len}`, offset: -acc }
      acc += len
      return seg
    })
})

const hasMoreDetails = (d) => !!(d.description || d.affected_impact || d.comments)

// ---- data ----
const buildDefect = (e) => {
  if (!e) return null
  const failedLike = e.execution_status === 'Failed' || e.execution_status === 'Blocked'
  if (!failedLike && !e.has_defect) return null
  const d = {
    actual_result: e.actual_result || '',
    description: e.description || '',
    affected_impact: e.affected_impact || '',
    severity: e.severity || '',
    ticket_id: e.ticket_id || '',
    comments: e.comments || '',
  }
  return Object.values(d).some(Boolean) ? d : null
}

const load = async () => {
  loading.value = true
  error.value = ''
  try {
    const [tcRes, runsRes, usersRes, stepsRes, execRes, fbRes] = await Promise.all([
      fetch(`${API}/TestCases/${testCaseId}`),
      fetch(`${API}/TestRuns/by-testcase/${testCaseId}`),
      fetch(`${API}/Users`),
      fetch(`${API}/TestSteps?testCaseId=${testCaseId}`),
      fetch(`${API}/ExecutionSteps/by-run/${runId}`),
      fetch(`${API}/TestFeedbacks`),
    ])

    testCase.value = tcRes.ok ? await tcRes.json() : null

    const runs = runsRes.ok ? await runsRes.json() : []
    const found = runs.find((r) => String(r.id) === String(runId))
    if (!found) {
      error.value = 'Test run not found.'
      return
    }
    run.value = found

    // Same per-tester numbering as the Runs page (RUN-001, RUN-002 ...)
    const sameTester = runs.filter((r) => r.executed_by === found.executed_by).sort((a, b) => a.id - b.id)
    const num = sameTester.findIndex((r) => r.id === found.id) + 1
    runLabel.value = 'RUN-' + String(num || found.id).padStart(3, '0')

    const users = usersRes.ok ? await usersRes.json() : []
    const user = users.find((u) => u.id === found.executed_by)
    testerName.value = user?.full_name || `User #${found.executed_by}`
    department.value = user?.department || ''

    const fbAll = fbRes.ok ? await fbRes.json() : []
    const fb = fbAll.filter((f) => String(f.run_id) === String(runId)).pop()
    feedbackComment.value = fb?.comment || ''

    const template = (stepsRes.ok ? await stepsRes.json() : [])
      .slice()
      .sort((a, b) => (a.step_order || 0) - (b.step_order || 0))
    const execAll = execRes.ok ? await execRes.json() : []
    const latestByOrder = new Map()
    execAll.slice().sort((a, b) => (a.id || 0) - (b.id || 0)).forEach((e) => latestByOrder.set(String(e.sequence_order), e))

    const rows = template.length
      ? template.map((t) => ({ name: t.description, exec: latestByOrder.get(String(t.step_order)) }))
      : [...latestByOrder.values()]
          .sort((a, b) => (a.sequence_order || 0) - (b.sequence_order || 0))
          .map((e) => ({ name: e.step_name, exec: e }))

    steps.value = rows.map((r) => ({
      step_name: r.exec?.step_name || r.name || '-',
      execution_status: r.exec?.execution_status || null,
      execution_step_id: r.exec?.id || null,
      defect: buildDefect(r.exec),
      open: false,
    }))
    steps.value = await withAttachments(steps.value)
  } catch (err) {
    console.error('Failed to load run report:', err)
    error.value = 'Could not load the run report. Please try again.'
  } finally {
    loading.value = false
  }
}

const downloadPdf = async () => {
  // images must be downloaded and converted before jsPDF can embed them
  const stepsForPdf = await embedImages(steps.value)
  generateRunReportPdf({
    testCase: testCase.value,
    run: run.value,
    runLabel: runLabel.value,
    testerName: testerName.value,
    department: department.value,
    steps: stepsForPdf,
    feedbackComment: feedbackComment.value,
  })
}

onMounted(load)
</script>

<style scoped>
.report-page {
  background: #f8fafc;
  min-height: 100vh;
  color: #1e293b;
  font-family: 'Inter', 'Segoe UI', -apple-system, sans-serif;
}
.page-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 28px; background: #ffffff; border-bottom: 1px solid #e2e8f0;
}
.page-title { font-size: 1.15rem; color: #1e293b; }
.page-crumb { font-size: 0.82rem; color: #64748b; display: flex; gap: 6px; align-items: center; }
.crumb-code { font-weight: 700; color: #3b82f6; }
.crumb-sep { color: #cbd5e1; }
.back-btn { border-radius: 8px; }
.page-body { max-width: 960px; margin: 0 auto; padding: 24px 28px 48px; }

.card {
  background: #ffffff; border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px 24px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
}
.card-title {
  font-size: 0.95rem; font-weight: 700; color: #0f172a; margin-bottom: 14px;
  padding-bottom: 10px; border-bottom: 2px solid #0f766e;
}

.info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px 24px; }
.info-label { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.05em; text-transform: uppercase; color: #94a3b8; margin-bottom: 3px; }
.info-value { font-size: 0.88rem; font-weight: 600; color: #0f172a; overflow-wrap: anywhere; }

.summary-row { display: flex; align-items: center; gap: 32px; flex-wrap: wrap; }
.donut-wrap { position: relative; width: 120px; height: 120px; flex-shrink: 0; }
.donut-center { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; }
.donut-num { font-size: 1.4rem; font-weight: 700; color: #0f172a; line-height: 1.1; }
.donut-label { font-size: 0.65rem; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; }
.legend-item { display: flex; align-items: center; gap: 10px; font-size: 0.88rem; margin-bottom: 8px; }
.legend-dot { width: 10px; height: 10px; border-radius: 3px; flex-shrink: 0; }
.legend-label { font-weight: 700; min-width: 64px; }
.legend-count { color: #475569; }

.case-title { font-weight: 700; font-size: 0.95rem; margin-bottom: 12px; overflow-wrap: anywhere; }
.step-row { padding: 10px 0; border-bottom: 1px solid #f1f5f9; }
.step-row:last-child { border-bottom: none; }
.step-head { display: flex; align-items: flex-start; gap: 10px; }
.step-dot { width: 9px; height: 9px; border-radius: 50%; margin-top: 6px; flex-shrink: 0; }
.step-name { flex: 1; min-width: 0; font-size: 0.9rem; color: #1e293b; overflow-wrap: anywhere; }
.step-status { font-size: 0.82rem; font-weight: 700; white-space: nowrap; }

/* Defect block under the step. Long text wraps inside the box and only takes the height it needs. */
.defect-box {
  margin: 8px 0 2px 19px; padding: 10px 14px;
  background: #fef2f2; border-left: 3px solid #ef4444; border-radius: 0 8px 8px 0;
}
.defect-label { font-size: 0.65rem; font-weight: 700; letter-spacing: 0.06em; text-transform: uppercase; color: #b91c1c; margin-bottom: 2px; }
.defect-text {
  font-size: 0.82rem; line-height: 1.45; color: #475569;
  white-space: pre-wrap; overflow-wrap: anywhere; margin: 0 0 8px;
}
.defect-main { font-size: 0.88rem; color: #0f172a; }
.defect-meta { display: flex; align-items: center; gap: 12px; font-size: 0.75rem; font-weight: 600; color: #64748b; margin-bottom: 6px; }
.sev { color: #fff; padding: 1px 8px; border-radius: 6px; font-size: 0.7rem; font-weight: 700; background: #64748b; }
.sev-critical, .sev-high { background: #ef4444; }
.sev-medium { background: #f59e0b; }
.sev-low { background: #10b981; }
.more-btn {
  display: inline-flex; align-items: center; gap: 2px; background: none; border: none; padding: 0;
  font-size: 0.75rem; font-weight: 600; color: #b91c1c; cursor: pointer;
}
.more-btn:hover { text-decoration: underline; }
.att-block { margin: 4px 0 8px; }
.att-list { display: flex; flex-wrap: wrap; gap: 8px; }
.att-thumb img { height: 96px; max-width: 180px; object-fit: cover; border-radius: 6px; border: 1px solid #e2e8f0; background: #fff; display: block; }
.att-file { display: inline-flex; align-items: center; gap: 4px; font-size: 0.78rem; font-weight: 600; color: #b91c1c; background: #fff; border: 1px solid #fecaca; border-radius: 6px; padding: 4px 10px; text-decoration: none; }
.att-file:hover { background: #fef2f2; }
.defect-more { margin-top: 8px; }
.defect-more > div:last-child .defect-text { margin-bottom: 0; }
</style>