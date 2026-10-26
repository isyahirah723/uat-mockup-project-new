<template>
  <div class="cycle-page">

    <!-- Header bar -->
    <div class="page-bar elevation-1">
      <div class="d-flex align-center" style="gap: 12px;">
        <v-btn icon variant="tonal" size="small" class="back-btn" title="Back to Test Cycles" @click="goBack">
          <v-icon size="20">mdi-arrow-left</v-icon>
        </v-btn>
        <div>
          <div class="page-title font-weight-bold">Test Cycle Details</div>
          <div v-if="cycle" class="page-crumb">
            <span class="crumb-code">{{ cycle.cycle_code || cycle.id }}</span>
            <span class="crumb-sep">/</span>
            <span>{{ cycle.name }}</span>
          </div>
        </div>
      </div>
      <v-chip v-if="cycle" :color="statusColor(cycle.status)" size="small" label class="font-weight-bold">
        {{ cycle.status || 'Active' }}
      </v-chip>
    </div>

    <div class="page-body">

      <div v-if="loading" class="text-center py-12">
        <v-progress-circular indeterminate color="primary" size="48" width="4" />
      </div>

      <v-alert v-else-if="error" type="error" variant="tonal" density="compact" class="rounded-lg">
        {{ error }}
      </v-alert>

      <template v-else-if="cycle">

        <!-- Overview -->
        <div class="card mb-5">
          <div class="card-title">Cycle Overview</div>
          <div class="info-grid">
            <div class="info-item">
              <div class="info-label">Cycle ID</div>
              <div class="info-value">{{ cycle.cycle_code || '-' }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Version Tag</div>
              <div class="info-value">{{ cycle.name || '-' }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Department</div>
              <div class="info-value">{{ cycle.department || '-' }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Project</div>
              <div class="info-value">{{ cycle.project || '-' }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Start Date</div>
              <div class="info-value">{{ formatDate(cycle.dt_start) }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">End Date</div>
              <div class="info-value">{{ formatDate(cycle.dt_end) }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Created Date</div>
              <div class="info-value">{{ formatDate(cycle.created_date) }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Created By</div>
              <div class="info-value">{{ createdByName }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Assigned To</div>
              <div class="info-value">{{ assignedToName }}</div>
            </div>
            <div class="info-item">
              <div class="info-label">Auto Rule</div>
              <div class="info-value">{{ autoRuleLabel(cycle.auto_assign_rule) }}</div>
            </div>
          </div>
        </div>

        <!-- Cycle result (decided by critical test cases, not just the percentage) -->
        <div class="card mb-5">
          <div class="card-title">Cycle Result</div>
          <div v-if="cases.length === 0" class="text-caption text-grey">No test cases in this cycle yet.</div>
          <div v-else>
            <div class="d-flex align-center flex-wrap mb-3" style="gap: 12px;">
              <v-chip :color="verdictColor(ev.verdict)" variant="flat" label class="font-weight-bold text-white px-4">
                <v-icon start size="18">{{ ev.verdict === 'Passed' ? 'mdi-check-circle-outline' : ev.verdict === 'Failed' ? 'mdi-close-circle-outline' : 'mdi-clock-outline' }}</v-icon>
                {{ ev.verdict }}
              </v-chip>
              <div class="verdict-note">{{ verdictNote }}</div>
            </div>

            <div class="metric-grid mb-4">
              <div class="metric">
                <div class="info-label">Critical test cases</div>
                <div class="metric-value">
                  <template v-if="ev.critical.total">{{ ev.critical.passed }} / {{ ev.critical.total }} passed</template>
                  <template v-else>None set</template>
                </div>
                <div v-if="ev.critical.failed" class="text-caption text-red-darken-1">{{ ev.critical.failed }} failed</div>
              </div>
              <div class="metric">
                <div class="info-label">All test cases</div>
                <div class="metric-value">{{ ev.passed }} / {{ ev.total }} passed</div>
                <div v-if="ev.failed" class="text-caption text-red-darken-1">{{ ev.failed }} failed</div>
              </div>
              <div class="metric">
                <div class="info-label">Pass rate (for reference)</div>
                <div class="metric-value">{{ ev.pct }}%</div>
              </div>
            </div>

            <v-progress-linear
              :model-value="ev.pct"
              :color="verdictColor(ev.verdict) === 'error' ? 'red-darken-1' : '#0f766e'"
              bg-color="grey-lighten-2"
              height="8"
              rounded
            />

            <v-alert v-if="ev.nonCriticalFailed > 0 && ev.critical.total" type="warning" variant="tonal" density="compact" class="mt-4 rounded-lg">
              {{ ev.nonCriticalFailed }} non-critical test case{{ ev.nonCriticalFailed === 1 ? '' : 's' }} failed. This does not stop the cycle from passing, but should be reviewed.
            </v-alert>
          </div>
        </div>

        <!-- Test cases in this cycle -->
        <div class="card">
          <div class="card-title">Test Cases ({{ cases.length }})</div>
          <div v-if="cases.length === 0" class="text-caption text-grey">No test cases in this cycle yet.</div>
          <v-table v-else density="compact" class="case-table">
            <thead>
              <tr>
                <th>ID</th>
                <th>TITLE</th>
                <th>PRIORITY</th>
                <th>RESULT</th>
                <th class="text-center" style="width: 90px;">RUNS</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="tc in cases" :key="tc.id">
                <td class="id-text font-weight-bold text-no-wrap">{{ tc.test_case_code || tc.id }}</td>
                <td>{{ tc.title }}</td>
                <td>
                  <v-chip v-if="isCritical(tc)" size="x-small" label variant="flat" color="red-darken-1" class="font-weight-bold text-white">Critical</v-chip>
                  <span v-else>{{ tc.priority || 'Medium' }}</span>
                </td>
                <td>
                  <v-chip size="x-small" label variant="tonal" :color="caseResultColor(ev.results[tc.id])" class="font-weight-bold">
                    {{ ev.results[tc.id] || 'Not Run' }}
                  </v-chip>
                </td>
                <td class="text-center">
                  <v-btn
                    icon
                    variant="tonal"
                    size="x-small"
                    color="teal-darken-2"
                    title="View runs"
                    style="width: 22px; height: 22px;"
                    @click="openRuns(tc)"
                  >
                    <v-icon size="14">mdi-play-circle-outline</v-icon>
                  </v-btn>
                </td>
              </tr>
            </tbody>
          </v-table>
        </div>

      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { evaluateCycle, isCritical, verdictColor, caseResultColor } from '@/utils/cycleResult'

const API = 'https://localhost:7049/api'
const route = useRoute()
const router = useRouter()
const cycleId = route.params.id

const loading = ref(true)
const error = ref('')
const cycle = ref(null)
const cases = ref([])
const users = ref([])
const runs = ref([])

const goBack = () => router.push({ path: '/test-cases', query: { tab: 'cycles' } })
const openRuns = (tc) => router.push({ name: 'TestCaseRuns', params: { id: tc.id } })

const formatDate = (s) => (s ? String(s).split('T')[0] : '-')
const userName = (id) => users.value.find((u) => u.id === id)?.full_name || null
const createdByName = computed(() => cycle.value?.created_by_name || userName(cycle.value?.created_by) || 'System')
const assignedToName = computed(() => cycle.value?.assigned_to_name || userName(cycle.value?.assigned_to) || 'Unassigned')

const autoRuleLabels = {
  default_latest: 'Latest Active',
  by_module: 'By Module',
  by_priority: 'By Priority',
  by_department: 'By Department',
  none: 'Manual',
}
const autoRuleLabel = (rule) => autoRuleLabels[rule] || rule || 'Manual'

const statusColor = (st) => {
  if (st === 'Active') return 'success'
  if (st === 'Completed') return 'info'
  if (st === 'Closed' || st === 'Archived') return 'grey'
  return 'primary'
}
const ev = computed(() => evaluateCycle(cases.value, runs.value))

const verdictNote = computed(() => {
  const e = ev.value
  if (!e.total) return ''
  if (e.basis === 'critical') {
    if (e.verdict === 'Failed') return `Failed because ${e.critical.failed} critical test case${e.critical.failed === 1 ? '' : 's'} failed, regardless of the ${e.pct}% overall pass rate.`
    if (e.verdict === 'Passed') return 'All critical test cases passed.'
    return `Waiting for ${e.critical.pending} critical test case${e.critical.pending === 1 ? '' : 's'} to be run or passed.`
  }
  if (e.verdict === 'Failed') return 'No test case is marked Critical, so any failed test case fails the cycle.'
  if (e.verdict === 'Passed') return 'No test case is marked Critical. All test cases passed.'
  return 'No test case is marked Critical. Waiting for the remaining test cases to be run.'
})

onMounted(async () => {
  try {
    const [cyclesRes, casesRes, usersRes, runsRes] = await Promise.all([
      fetch(`${API}/TestCycles`),
      fetch(`${API}/TestCases`),
      fetch(`${API}/Users`),
      fetch(`${API}/TestRuns`),
    ])
    if (!cyclesRes.ok) throw new Error('Failed to load test cycles')
    const allCycles = await cyclesRes.json()
    const found = allCycles.find((c) => String(c.id) === String(cycleId))
    if (!found) throw new Error('Test cycle not found')
    cycle.value = found

    users.value = usersRes.ok ? await usersRes.json() : []
    runs.value = runsRes.ok ? await runsRes.json() : []
    const allCases = casesRes.ok ? await casesRes.json() : []
    cases.value = allCases.filter((tc) => String(tc.cycle_id) === String(cycleId))
  } catch (e) {
    console.error(e)
    error.value = e.message || 'Failed to load test cycle'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.cycle-page {
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
.crumb-code { font-weight: 700; color: var(--acc-teal, #0f766e); }
.crumb-sep { color: #cbd5e1; }
.back-btn { border-radius: 8px; }
.page-body { max-width: 960px; margin: 0 auto; padding: 24px 28px 48px; }

.card {
  background: rgb(var(--v-theme-surface)); border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); border-radius: 12px; padding: 20px 24px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
}
.card-title {
  font-size: 0.95rem; font-weight: 700; color: rgb(var(--v-theme-on-surface)); margin-bottom: 14px;
  padding-bottom: 10px; border-bottom: 2px solid #0f766e;
}
.info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px 24px; }
.info-label { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.05em; text-transform: uppercase; color: rgba(var(--v-theme-on-surface), 0.5); margin-bottom: 3px; }
.info-value { font-size: 0.88rem; font-weight: 600; color: rgb(var(--v-theme-on-surface)); overflow-wrap: anywhere; }

.verdict-note { font-size: 0.85rem; color: rgba(var(--v-theme-on-surface), 0.75); }
.metric-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(190px, 1fr)); gap: 16px 24px; }
.metric-value { font-size: 1.05rem; font-weight: 700; color: rgb(var(--v-theme-on-surface)); }

.id-text { color: rgb(var(--v-theme-on-surface)); }
.case-table :deep(th) {
  font-size: 0.7rem; font-weight: 700; letter-spacing: 0.05em; color: rgba(var(--v-theme-on-surface), 0.7);
}
</style>