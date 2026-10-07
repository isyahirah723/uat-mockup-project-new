<template>
  <div class="runs-page">

    <!-- Header bar -->
    <div class="page-bar elevation-1">
      <div class="d-flex align-center" style="gap: 12px;">
        <v-btn icon variant="tonal" size="small" class="back-btn" @click="goBack" title="Back to Test Cases">
          <v-icon size="20">mdi-arrow-left</v-icon>
        </v-btn>
        <div>
          <div class="page-title font-weight-bold">Runs &amp; results</div>
          <div v-if="testCase" class="page-crumb">
            <span class="crumb-code">{{ testCase.test_case_code || testCase.id }}</span>
            <span class="crumb-sep">/</span>
            <span>{{ testCase.title }}</span>
          </div>
        </div>
      </div>
      <v-btn size="small" variant="flat" color="primary" class="text-capitalize refresh-btn font-weight-bold" prepend-icon="mdi-refresh" :loading="loading" @click="fetchRows()">
        Refresh
      </v-btn>
    </div>

    <div class="page-body">

      <!-- Summary tiles -->
      <div class="tiles">

        <!-- Active -->
        <div class="tile active-tile">
          <div class="tile-visual">
            <svg viewBox="0 0 140 140" width="140" height="140">
              <circle cx="70" cy="70" r="52" fill="none" stroke="#94a3b8" stroke-opacity="0.3" stroke-width="16" />
              <circle
                v-for="(s, i) in donutSegments"
                :key="'d' + i"
                cx="70" cy="70" r="52" fill="none"
                :stroke="s.color" stroke-width="16"
                :stroke-dasharray="s.dash" :stroke-dashoffset="s.offset"
                transform="rotate(-90 70 70)"
                stroke-linecap="round"
              />
            </svg>
            <div class="donut-center">
              <div class="donut-label">ACTIVE</div>
              <div class="donut-num">{{ activeRows.length }}</div>
              <div v-if="unstartedCount" class="donut-sub">{{ unstartedCount }} unstarted</div>
            </div>
          </div>
          <div class="tile-meta">
            <div class="meta-title">ACTIVE RUNS</div>
            <div class="meta-line"><v-icon size="16" color="#3b82f6">mdi-clock-outline</v-icon><span><b>{{ remainingSteps }}</b> step{{ remainingSteps === 1 ? '' : 's' }} remaining</span></div>
            <div class="meta-line"><v-icon size="16" color="#3b82f6">mdi-account-group-outline</v-icon><span><b>{{ rows.length }}</b> tester{{ rows.length === 1 ? '' : 's' }}</span></div>
          </div>
        </div>

        <!-- Latest results -->
        <div class="tile latest-tile">
          <div class="tile-visual gauge">
            <svg viewBox="0 0 160 90" width="160" height="90">
              <circle cx="80" cy="80" r="60" fill="none" stroke="#94a3b8" stroke-opacity="0.3" stroke-width="22"
                      :stroke-dasharray="`${gaugeHalf} ${gaugeFull}`" transform="rotate(180 80 80)" />
              <circle
                v-for="(s, i) in gaugeSegments"
                :key="'g' + i"
                cx="80" cy="80" r="60" fill="none"
                :stroke="s.color" stroke-width="22"
                :stroke-dasharray="s.dash" :stroke-dashoffset="s.offset"
                transform="rotate(180 80 80)"
              />
            </svg>
          </div>
          <div class="tile-meta">
            <div class="meta-title">LATEST RESULTS</div>
            <div class="meta-line"><v-icon size="16" color="#10b981">mdi-check-circle-outline</v-icon><span><b>{{ successPct }}%</b> successful</span></div>
            <div class="meta-line"><v-icon size="16" color="#64748b">mdi-calendar-outline</v-icon><span>{{ lastActivityText }}</span></div>
          </div>
        </div>

        <!-- Recently closed -->
        <div class="tile tile-closed">
          <div class="closed-top">
            <div class="meta-title">RECENTLY CLOSED</div>
            <div class="closed-num">{{ closedRunsAll.length }}</div>
            <div class="closed-sub">▲ {{ closedThisMonth }} this month</div>
          </div>
          <svg class="spark" viewBox="0 0 300 70" preserveAspectRatio="none">
            <path :d="sparkArea" fill="url(#sparkGradient)" />
            <defs>
              <linearGradient id="sparkGradient" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stop-color="#3b82f6" stop-opacity="0.3" />
                <stop offset="100%" stop-color="#3b82f6" stop-opacity="0.0" />
              </linearGradient>
            </defs>
            <path :d="sparkLine" fill="none" stroke="#3b82f6" stroke-width="2.5" stroke-linecap="round" />
          </svg>
        </div>
      </div>

      <!-- Tabs (Kemas kini reka bentuk pil & bahasa) -->
      <div class="tabs">
        <button class="tab" :class="{ on: tab === 'active' }" @click="tab = 'active'">ACTIVE <span class="tab-count">{{ activeRows.length }}</span></button>
        <button class="tab" :class="{ on: tab === 'closed' }" @click="tab = 'closed'">CLOSED <span class="tab-count">{{ closedRows.length }}</span></button>
      </div>

      <!-- Group + table -->
      <div v-if="loading && !rows.length" class="text-center py-12">
        <v-progress-circular indeterminate color="primary" size="48" width="4" />
      </div>

      <div v-else-if="rows.length === 0" class="empty">
        <v-icon size="56" color="#94a3b8">mdi-account-off-outline</v-icon>
        <div class="empty-title">No testers assigned</div>
        <div class="empty-sub">Assign testers to this test case to see runs here.</div>
      </div>

      <template v-else>
        <div class="group-head">
          <div class="group-flag"><v-icon size="16" color="#3b82f6">mdi-flag-outline</v-icon></div>
          <span class="group-name font-weight-bold">{{ testCase?.title || 'Test case' }}</span>
          <span v-if="testCase?.priority" class="badge" :class="'p-' + String(testCase.priority).toLowerCase()">{{ testCase.priority }}</span>
          <span v-if="testCase?.approval_status" class="badge" :class="testCase.approval_status === 'Approved' ? 'b-green' : 'b-orange'">{{ testCase.approval_status }}</span>
          <span class="group-right font-weight-medium">{{ totalSteps }} step{{ totalSteps === 1 ? '' : 's' }}</span>
        </div>

        <div class="table-container elevation-1">
          <table class="runs-table">
            <thead>
              <tr>
                <th style="width: 30%;">Run</th>
                <th style="width: 15%;">State</th>
                <th style="width: 15%;">Tags</th>
                <th style="width: 12%;">Contributors</th>
                <th style="width: 20%;">Status</th>
                <th style="width: 8%;"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="t in visibleRows" :key="t.run_id ? 'r' + t.run_id : 'u' + t.user_id" class="run-row" @click="rowClick(t)">
                <td>
                  <div class="d-flex align-center" style="gap: 12px;">
                    <div class="run-icon"><v-icon size="16" color="#3b82f6">mdi-chart-pie</v-icon></div>
                    <div>
                      <div class="run-name">{{ t.user_name }}</div>
                      <div class="run-sub">{{ t.run_id ? formatRunNumber(t.run_number) : 'No run yet' }}</div>
                    </div>
                  </div>
                </td>
                <td>
                  <span class="state" :style="{ color: stateOf(t).color }">
                    <v-icon size="16" :color="stateOf(t).color">{{ stateOf(t).icon }}</v-icon>
                    <span class="font-weight-medium">{{ stateOf(t).label }}</span>
                  </span>
                </td>
                <td>
                  <span v-if="t.department" class="tag"><v-icon size="12" class="mr-1">mdi-tag-outline</v-icon>{{ t.department }}</span>
                </td>
                <td>
                  <div class="avatar" :title="t.user_name">{{ initials(t.user_name) }}</div>
                </td>
                <td>
                  <div class="d-flex align-center" style="gap: 12px;">
                    <div class="bar">
                      <span v-for="(s, i) in barSegments(t)" :key="i" class="bar-seg" :style="{ width: s.w + '%', background: s.color }" />
                    </div>
                    <span class="pct font-weight-bold">{{ progressPct(t) }}%</span>
                  </div>
                </td>
                <td class="text-right" @click.stop>
                  <v-btn size="small" variant="flat" color="primary" class="text-white text-capitalize font-weight-bold run-btn px-4" @click="startExecutionFor(t)">
                    {{ !t.run_id ? 'Start' : isClosed(t) ? 'Run again' : 'Continue' }}
                  </v-btn>
                </td>
              </tr>
              <tr v-if="visibleRows.length === 0">
                <td colspan="6" class="text-center py-8 empty-sub">No runs in this view.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const API = 'https://localhost:7049/api'
const route = useRoute()
const router = useRouter()
const testCaseId = route.params.id

const testCase = ref(null)
const rows = ref([])
const allRuns = ref([])
const loading = ref(false)

const tab = ref('active')
const search = ref('')
const stateFilter = ref('All states')
const deptFilter = ref('All tags')

const goBack = () => router.back()
const initials = (n) => (n || '?').split(' ').map((p) => p[0]).slice(0, 2).join('').toUpperCase()
const formatRunNumber = (n) => 'RUN-' + String(n).padStart(3, '0')

const FAIL_SET = ['Failed', 'Fail', 'Not Approved']
const COLORS = { pass: '#10b981', fail: '#ef4444', blocked: '#f59e0b', na: '#94a3b8', progress: '#3b82f6', unstarted: '#93c5fd' }

const totalSteps = computed(() => testCase.value?.steps?.length || 0)

const isClosed = (t) => !!t.run_id && t.run_status !== 'In Progress'

const stateOf = (t) => {
  if (!t.run_id) return { label: 'New', icon: 'mdi-lightbulb-on-outline', color: '#f59e0b' }
  if (t.run_status === 'In Progress') return { label: 'In progress', icon: 'mdi-history', color: '#3b82f6' }
  if (FAIL_SET.includes(t.run_status)) return { label: 'Failed', icon: 'mdi-close-circle-outline', color: '#ef4444' }
  return { label: 'Completed', icon: 'mdi-check-circle-outline', color: '#10b981' }
}

const progressPct = (t) => {
  if (isClosed(t)) return 100
  if (!totalSteps.value) return 0
  return Math.min(100, Math.round((t.counts.total / totalSteps.value) * 100))
}

const barSegments = (t) => {
  if (isClosed(t) && t.counts.total === 0) {
    return [{ w: 100, color: FAIL_SET.includes(t.run_status) ? COLORS.fail : COLORS.pass }]
  }
  const denom = Math.max(totalSteps.value, t.counts.total, 1)
  return [
    { w: (t.counts.pass / denom) * 100, color: COLORS.pass },
    { w: (t.counts.blocked / denom) * 100, color: COLORS.blocked },
    { w: (t.counts.fail / denom) * 100, color: COLORS.fail },
    { w: (t.counts.na / denom) * 100, color: COLORS.na },
  ].filter((s) => s.w > 0)
}

const closedHistory = ref([])
const stepsCache = {}
const closedRows = computed(() => closedHistory.value)
const activeRows = computed(() => rows.value.filter((t) => !isClosed(t)))
const unstartedCount = computed(() => activeRows.value.filter((t) => !t.run_id).length)

const stateOptions = ['All states', 'New', 'In progress', 'Completed', 'Failed']
const deptOptions = computed(() => ['All tags', ...new Set(rows.value.map((t) => t.department).filter(Boolean))])

const visibleRows = computed(() => {
  const base = tab.value === 'active' ? activeRows.value : closedRows.value
  const q = search.value.trim().toLowerCase()
  return base.filter((t) => {
    if (q && !t.user_name.toLowerCase().includes(q) && !String(t.run_id || '').includes(q) && !String(t.run_number || '').includes(q)) return false
    if (stateFilter.value !== 'All states' && stateOf(t).label !== stateFilter.value) return false
    if (deptFilter.value !== 'All tags' && t.department !== deptFilter.value) return false
    return true
  })
})

const remainingSteps = computed(() =>
  activeRows.value.reduce((sum, t) => sum + Math.max(totalSteps.value - t.counts.total, 0), 0)
)

const buildSegments = (parts, r, fraction) => {
  const C = 2 * Math.PI * r
  const L = C * fraction
  const sum = parts.reduce((s, p) => s + p.value, 0)
  if (!sum) return []
  let acc = 0
  return parts.filter((p) => p.value > 0).map((p) => {
    const len = (p.value / sum) * L
    const seg = { color: p.color, dash: `${len} ${C - len}`, offset: -acc }
    acc += len
    return seg
  })
}

const donutSegments = computed(() =>
  buildSegments([
    { value: activeRows.value.filter((t) => t.run_id).length, color: COLORS.progress },
    { value: unstartedCount.value, color: COLORS.unstarted },
  ], 52, 1)
)

const gaugeFull = 2 * Math.PI * 60
const gaugeHalf = Math.PI * 60

const totals = computed(() =>
  [...closedHistory.value, ...activeRows.value].reduce(
    (a, t) => {
      if (t.counts.total === 0 && isClosed(t)) {
        if (FAIL_SET.includes(t.run_status)) a.fail += 1
        else a.pass += 1
        a.total += 1
        return a
      }
      a.pass += t.counts.pass; a.fail += t.counts.fail; a.blocked += t.counts.blocked; a.na += t.counts.na; a.total += t.counts.total
      return a
    },
    { pass: 0, fail: 0, blocked: 0, na: 0, total: 0 }
  )
)

const gaugeSegments = computed(() =>
  buildSegments([
    { value: totals.value.pass, color: COLORS.pass },
    { value: totals.value.blocked, color: COLORS.blocked },
    { value: totals.value.fail, color: COLORS.fail },
    { value: totals.value.na, color: COLORS.na },
  ], 60, 0.5)
)

const successPct = computed(() => (totals.value.total ? Math.round((totals.value.pass / totals.value.total) * 100) : 0))

const lastActivityText = computed(() => {
  const dates = allRuns.value.map((r) => r.completed_at || r.started_at).filter(Boolean).map((d) => new Date(d))
  if (!dates.length) return 'No runs yet'
  const latest = new Date(Math.max(...dates))
  return 'Last run ' + latest.toLocaleDateString('en-MY', { day: 'numeric', month: 'short', year: 'numeric' })
})

const closedRunsAll = computed(() => allRuns.value.filter((r) => r.run_status !== 'In Progress' && r.completed_at))

const closedThisMonth = computed(() => {
  const now = new Date()
  return closedRunsAll.value.filter((r) => {
    const d = new Date(r.completed_at)
    return d.getMonth() === now.getMonth() && d.getFullYear() === now.getFullYear()
  }).length
})

const sparkPoints = computed(() => {
  const days = 14
  const today = new Date(); today.setHours(23, 59, 59, 999)
  const vals = []
  for (let i = days - 1; i >= 0; i--) {
    const end = new Date(today); end.setDate(end.getDate() - i)
    vals.push(closedRunsAll.value.filter((r) => new Date(r.completed_at) <= end).length)
  }
  const max = Math.max(...vals), min = Math.min(...vals)
  return vals.map((v, i) => {
    const x = (i / (days - 1)) * 300
    const y = max === min ? 40 : 62 - ((v - min) / (max - min)) * 50
    return [x, y]
  })
})
const sparkLine = computed(() => 'M' + sparkPoints.value.map(([x, y]) => `${x.toFixed(1)},${y.toFixed(1)}`).join(' L'))
const sparkArea = computed(() => `${sparkLine.value} L300,70 L0,70 Z`)

const formatSteps = (steps) => {
  const c = { pass: 0, fail: 0, blocked: 0, na: 0, total: 0 }
  for (const s of steps) {
    const st = s.execution_status
    if (st === 'Passed') c.pass++
    else if (st === 'Failed') c.fail++
    else if (st === 'Blocked') c.blocked++
    else if (st === 'N/A') c.na++
    else continue
    c.total++
  }
  return c
}

const fetchTestCase = async () => {
  try {
    const res = await fetch(`${API}/TestCases/${testCaseId}`)
    if (res.ok) testCase.value = await res.json()
  } catch (err) {
    console.error('Failed to load test case:', err)
  }
}

const fetchRows = async (silent = false) => {
  if (!silent) loading.value = true
  try {
    const [assignRes, runsRes, usersRes, feedbacksRes] = await Promise.all([
      fetch(`${API}/TestAssignments`),
      fetch(`${API}/TestRuns/by-testcase/${testCaseId}`),
      fetch(`${API}/Users`),
      fetch(`${API}/TestFeedbacks`),
    ])

    const assignedAll = (assignRes.ok ? await assignRes.json() : []).filter((a) => String(a.test_case_id) === String(testCaseId))
    const seen = new Set()
    const assigned = assignedAll.filter((a) => (seen.has(a.user_id) ? false : seen.add(a.user_id)))

    const runs = runsRes.ok ? await runsRes.json() : []
    allRuns.value = runs
    const lastRunByUser = {}
    for (const r of runs) if (!(r.executed_by in lastRunByUser)) lastRunByUser[r.executed_by] = r

    const runsByTester = {}
    for (const r of runs) {
      if (!runsByTester[r.executed_by]) runsByTester[r.executed_by] = []
      runsByTester[r.executed_by].push(r)
    }
    const runNumberById = {}
    Object.values(runsByTester).forEach((list) => {
      list.slice().sort((a, b) => a.id - b.id).forEach((r, idx) => { runNumberById[r.id] = idx + 1 })
    })
    const runNumber = (id) => (id in runNumberById ? runNumberById[id] : id)

    const users = usersRes.ok ? await usersRes.json() : []
    const nameById = {}
    users.forEach((u) => { nameById[u.id] = u.full_name })

    const feedbacksAll = feedbacksRes.ok ? await feedbacksRes.json() : []
    const feedbackByRunId = {}
    for (const f of feedbacksAll) feedbackByRunId[String(f.run_id)] = f

    rows.value = await Promise.all(assigned.map(async (a) => {
      const run = lastRunByUser[a.user_id]
      let steps = []
      if (run) {
        const r = await fetch(`${API}/ExecutionSteps/by-run/${run.id}`)
        steps = r.ok ? await r.json() : []
      }
      const fb = run ? feedbackByRunId[String(run.id)] : null
      return {
        user_id: a.user_id,
        user_name: a.user_name || nameById[a.user_id] || `User #${a.user_id}`,
        department: a.department || users.find((u) => u.id === a.user_id)?.department || null,
        run_id: run ? run.id : null,
        run_number: run ? runNumber(run.id) : null,
        run_status: run ? run.run_status : 'Not Started',
        started_at: run ? run.started_at : null,
        counts: formatSteps(steps),
        catatan: fb?.comment || '',
      }
    }))

    const closed = runs.filter((r) => r.run_status !== 'In Progress' && r.completed_at)
    await Promise.all(closed.filter((r) => !(r.id in stepsCache)).map(async (r) => {
      const x = await fetch(`${API}/ExecutionSteps/by-run/${r.id}`)
      stepsCache[r.id] = x.ok ? await x.json() : []
    }))
    closedHistory.value = closed
      .slice()
      .sort((a, b) => new Date(b.completed_at) - new Date(a.completed_at))
      .map((r) => {
        const a = assigned.find((x) => x.user_id === r.executed_by)
        const fb = feedbackByRunId[String(r.id)]
        return {
          user_id: r.executed_by,
          user_name: a?.user_name || nameById[r.executed_by] || `User #${r.executed_by}`,
          department: a?.department || users.find((u) => u.id === r.executed_by)?.department || null,
          run_id: r.id,
          run_number: runNumber(r.id),
          run_status: r.run_status,
          started_at: r.started_at,
          counts: formatSteps(stepsCache[r.id]),
          steps: stepsCache[r.id] || [],
          catatan: fb?.comment || '',
        }
      })
  } catch (err) {
    console.error('Failed to load test runs:', err)
  } finally {
    loading.value = false
  }
}

// Closed run -> open its report page (with the PDF download) instead of a popup
const openReport = (t) => {
  router.push({
    name: 'TestRunReport',
    params: { id: testCase.value?.id ?? testCaseId, runId: t.run_id },
  })
}

const rowClick = (t) => {
  if (isClosed(t)) openReport(t)
  else startExecutionFor(t)
}

// Open the step-by-step execution as its own page
const startExecutionFor = (tester) => {
  router.push({
    name: 'TestExecution',
    params: { id: testCase.value?.id ?? testCaseId },
    query: {
      run_as: tester.user_id,
      run_as_name: tester.user_name,
      department: tester.department || '',
    },
  })
}

let timer = null
onMounted(async () => {
  await fetchTestCase()
  await fetchRows()
  timer = setInterval(() => fetchRows(true), 20000)
})
onUnmounted(() => clearInterval(timer))
</script>

<style scoped>
.runs-page {
  background: rgba(var(--v-theme-on-surface), 0.05);
  min-height: 100vh;
  color: rgb(var(--v-theme-on-surface));
  font-family: 'Inter', 'Segoe UI', -apple-system, sans-serif;
}
.page-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 28px; background: rgb(var(--v-theme-surface)); border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}
.page-title { font-size: 1.15rem; color: rgb(var(--v-theme-on-surface)); }
.page-crumb { font-size: 0.82rem; color: rgba(var(--v-theme-on-surface), 0.7); display: flex; gap: 6px; align-items: center; }
.crumb-code { font-weight: 700; color: #3b82f6; } /* Ditukar kepada warna biru */
.crumb-sep { color: #cbd5e1; }
.page-body { max-width: 1200px; margin: 0 auto; padding: 24px 28px 48px; }

/* tiles */
.tiles { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 24px; }
@media (max-width: 900px) { .tiles { grid-template-columns: 1fr; } }
.tile {
  background: rgb(var(--v-theme-surface)); border-radius: 12px; padding: 20px 22px; min-height: 178px;
  display: flex; align-items: center; gap: 20px; border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05), 0 2px 4px -2px rgba(0, 0, 0, 0.05);
  transition: transform 0.2s ease, box-shadow 0.2s ease;
}
.tile:hover {
  transform: translateY(-2px);
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.08), 0 4px 6px -4px rgba(0, 0, 0, 0.08);
}
.tile-visual { position: relative; width: 140px; height: 140px; flex-shrink: 0; }
.tile-visual.gauge { width: 160px; height: 90px; margin-top: 28px; align-self: flex-start; }
.donut-center { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; }
.donut-label { font-size: 0.7rem; color: rgba(var(--v-theme-on-surface), 0.7); font-weight: 600; letter-spacing: 0.05em; }
.donut-num { font-size: 2.1rem; font-weight: 700; line-height: 1.1; color: rgb(var(--v-theme-on-surface)); }
.donut-sub { font-size: 0.7rem; color: #10b981; font-weight: 600; margin-top: 2px; }
.tile-meta { min-width: 0; }
.meta-title { font-size: 0.7rem; font-weight: 700; letter-spacing: 0.06em; color: rgba(var(--v-theme-on-surface), 0.7); margin-bottom: 10px; }
.meta-line { display: flex; align-items: center; gap: 8px; font-size: 0.85rem; line-height: 1.4; margin-bottom: 8px; color: rgb(var(--v-theme-on-surface)); }
.meta-line b { font-weight: 700; color: rgb(var(--v-theme-on-surface)); }

.tile-closed { flex-direction: column; align-items: stretch; padding: 20px 0 0; gap: 0; overflow: hidden; }
.closed-top { text-align: center; padding: 0 20px; }
.closed-num { font-size: 2.1rem; font-weight: 700; line-height: 1.15; color: rgb(var(--v-theme-on-surface)); }
.closed-sub { font-size: 0.75rem; font-weight: 600; color: #10b981; margin-top: 2px; }
.spark { width: 100%; height: 70px; margin-top: auto; display: block; }

/* Tabs Moden (Gaya Pil / Pill Buttons) */
.tabs { 
  display: flex; 
  gap: 12px; 
  border-bottom: none; 
  margin-bottom: 20px; 
}

.tab {
  background: rgb(var(--v-theme-surface)); 
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); 
  padding: 8px 18px; 
  font-size: 0.8rem; 
  font-weight: 600; 
  letter-spacing: 0.04em;
  color: rgba(var(--v-theme-on-surface), 0.7); 
  cursor: pointer; 
  border-radius: 8px; 
  transition: all 0.2s ease;
  box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
}

.tab:hover {
  background: rgba(var(--v-theme-on-surface), 0.05);
  color: rgb(var(--v-theme-on-surface));
}

.tab.on { 
  background: rgba(59, 130, 246, 0.10); 
  color: #3b82f6; 
  border-color: rgba(59, 130, 246, 0.30); 
  box-shadow: 0 2px 4px rgba(59, 130, 246, 0.1);
}

.tab-count { 
  background: rgba(0, 0, 0, 0.06);
  padding: 2px 6px;
  border-radius: 6px;
  margin-left: 6px;
  font-size: 0.72rem;
  color: rgba(var(--v-theme-on-surface), 0.7); 
}

.tab.on .tab-count { 
  background: rgba(59, 130, 246, 0.18);
  color: #3b82f6; 
}

/* filters */
.filter-bar {
  display: flex; align-items: center; background: rgb(var(--v-theme-surface)); border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  border-radius: 10px; margin-bottom: 20px; height: 46px; overflow: hidden;
}
.filter-search { flex: 1; padding: 0 14px; }
.filter-select { max-width: 200px; padding: 0 14px; border-left: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.filter-bar :deep(.v-field__input) { font-size: 0.85rem; min-height: 44px; padding-top: 0; padding-bottom: 0; }
.filter-bar :deep(.v-field__prepend-inner) { padding-top: 0; align-items: center; }
.filter-bar :deep(.v-field__outline) { display: none; }

/* group + table */
.group-head {
  display: flex; align-items: center; gap: 12px; background: rgba(59, 130, 246, 0.10);
  padding: 12px 16px; border-radius: 10px 10px 0 0; border: 1px solid rgba(59, 130, 246, 0.30); border-bottom: none;
}
.group-flag { width: 28px; height: 26px; background: rgba(59, 130, 246, 0.18); border-radius: 6px; display: flex; align-items: center; justify-content: center; }
.group-name { color: var(--acc-blue-deep, #1e40af); font-size: 1rem; }
.group-right { margin-left: auto; font-size: 0.82rem; color: #3b82f6; }
.badge { font-size: 0.68rem; font-weight: 700; color: #fff; padding: 3px 8px; border-radius: 6px; }
.b-green { background: #10b981; }
.b-orange, .p-high, .p-critical { background: #f97316; }
.p-medium { background: #f59e0b; }
.p-low { background: #10b981; }

.table-container { background: rgb(var(--v-theme-surface)); border-radius: 0 0 10px 10px; border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); overflow: hidden; }
.runs-table { width: 100%; border-collapse: collapse; }
.runs-table thead th {
  text-align: left; font-size: 0.78rem; font-weight: 700; color: rgba(var(--v-theme-on-surface), 0.7);
  padding: 12px 16px; background: rgba(var(--v-theme-on-surface), 0.05); border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}
.run-row { cursor: pointer; transition: background 0.15s ease; }
.run-row:hover { background: rgba(var(--v-theme-on-surface), 0.05); }
.run-row td { padding: 14px 16px; border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); font-size: 0.85rem; vertical-align: middle; }
.run-icon { width: 32px; height: 32px; background: rgba(59, 130, 246, 0.10); border-radius: 8px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.run-name { font-weight: 600; color: rgb(var(--v-theme-on-surface)); }
.run-sub { font-size: 0.72rem; color: rgba(var(--v-theme-on-surface), 0.5); margin-top: 1px; }
.state { display: inline-flex; align-items: center; gap: 6px; }
.tag { display: inline-flex; align-items: center; background: rgba(var(--v-theme-on-surface), 0.05); color: rgba(var(--v-theme-on-surface), 0.7); font-weight: 500; font-size: 0.75rem; padding: 3px 10px; border-radius: 6px; border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.avatar {
  width: 30px; height: 30px; border-radius: 50%; background: linear-gradient(135deg, #3b82f6, #1d4ed8); color: #fff;
  font-size: 0.7rem; font-weight: 700; display: flex; align-items: center; justify-content: center; box-shadow: 0 2px 4px rgba(59, 130, 246, 0.3);
}
.bar { flex: 1; height: 10px; background: rgba(148, 163, 184, 0.3); border-radius: 5px; display: flex; overflow: hidden; }
.bar-seg { height: 100%; transition: width 0.3s; }
.pct { width: 42px; font-size: 0.82rem; color: rgb(var(--v-theme-on-surface)); text-align: right; }
.run-btn { letter-spacing: 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(59, 130, 246, 0.2); }
.back-btn { border-radius: 8px; }
.refresh-btn { border-radius: 8px; }

.empty { text-align: center; padding: 64px 0; background: rgb(var(--v-theme-surface)); border-radius: 12px; border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.empty-title { font-weight: 700; font-size: 1rem; margin-top: 10px; color: rgb(var(--v-theme-on-surface)); }
.empty-sub { color: rgba(var(--v-theme-on-surface), 0.7); font-size: 0.85rem; }
</style>