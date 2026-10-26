<template>
  <v-container class="pa-4 mx-auto" style="max-width: 1280px;">
    <!-- Header -->
    <v-row class="mb-3" align="center">
      <v-col cols="12" md="8">
        <div class="d-flex align-center" style="gap: 12px;">
          <v-btn icon size="small" variant="tonal" color="#0f766e" title="Back to Test Cases" @click="goBack">
            <v-icon>mdi-arrow-left</v-icon>
          </v-btn>
          <div>
            <div class="text-h5 page-heading" style="font-size: 1.25rem !important;">Test Runs</div>
            <div class="page-subheading" style="font-size: 0.8rem;">
              <span v-if="testCase">
                <span class="font-weight-bold">{{ testCase.test_case_code || '#' + testCase.id }}</span>
                &middot; {{ testCase.title }}
              </span>
              <span v-else-if="!loading">Test case not found</span>
            </div>
          </div>
        </div>
      </v-col>
      <v-col cols="12" md="4" class="d-flex justify-md-end">
        <v-btn
          color="#0f766e"
          class="text-white text-capitalize font-weight-bold rounded-lg"
          prepend-icon="mdi-play-circle-outline"
          elevation="2"
          :disabled="!testCase"
          @click="startRun"
        >Run Test</v-btn>
      </v-col>
    </v-row>

    <v-alert v-if="error" type="error" variant="tonal" density="compact" closable class="mb-3 rounded-lg" @click:close="error = ''">
      {{ error }}
    </v-alert>

    <!-- Summary -->
    <v-row dense class="mb-3">
      <v-col v-for="s in summary" :key="s.label" cols="6" md="3">
        <v-card variant="outlined" class="rounded-lg pa-3 summary-card" :class="{ 'summary-active': statusFilter === s.filter }" @click="statusFilter = s.filter">
          <div class="text-caption text-grey font-weight-bold">{{ s.label }}</div>
          <div class="text-h5 font-weight-bold" :style="{ color: s.color }">{{ s.value }}</div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Filters -->
    <v-card variant="outlined" class="rounded-lg pa-3 mb-3" elevation="0">
      <v-row dense align="center">
        <v-col cols="12" md="6">
          <v-text-field
            v-model="search"
            label="Search run ID or tester"
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            clearable
            hide-details
          ></v-text-field>
        </v-col>
        <v-col cols="8" md="4">
          <v-select
            v-model="statusFilter"
            :items="statusOptions"
            label="Status"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>
        <v-col cols="4" md="2" class="d-flex justify-end">
          <v-btn variant="text" size="small" class="text-capitalize" :disabled="!hasFilters" @click="clearFilters">Clear</v-btn>
        </v-col>
      </v-row>
    </v-card>

    <!-- Table -->
    <v-card variant="outlined" class="rounded-lg pa-0 overflow-hidden main-table-card" elevation="1">
      <v-table hover density="compact" class="runs-table">
        <thead>
          <tr class="table-header-row">
            <th>RUN ID</th>
            <th>TESTER</th>
            <th>STATUS</th>
            <th>STARTED</th>
            <th>COMPLETED</th>
            <th class="text-center" style="width: 120px;">ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="r in pagedRuns" :key="r.id" class="run-row" @click="openReport(r)">
            <td class="py-1 font-weight-bold">{{ runCode(r.id) }}</td>
            <td class="py-1">{{ r.user_name || 'User #' + r.executed_by }}</td>
            <td class="py-1">
              <v-chip size="x-small" variant="flat" label class="font-weight-bold text-white" :color="statusColor(r.run_status)">
                {{ r.run_status || 'Not Started' }}
              </v-chip>
            </td>
            <td class="py-1 text-caption">{{ formatDateTime(r.started_at) }}</td>
            <td class="py-1 text-caption">{{ formatDateTime(r.completed_at) }}</td>
            <td class="text-center py-1" @click.stop>
              <v-btn
                icon
                variant="tonal"
                size="x-small"
                style="width: 22px; height: 22px;"
                class="mr-1"
                color="teal-darken-2"
                :disabled="!isFinished(r)"
                :title="isFinished(r) ? 'View run report' : 'The report is available after the run is submitted'"
                @click="openReport(r)"
              ><v-icon size="14">mdi-file-chart-outline</v-icon></v-btn>
              <v-btn
                icon
                variant="tonal"
                size="x-small"
                style="width: 22px; height: 22px;"
                color="red"
                title="Delete run"
                @click="confirmDelete(r)"
              ><v-icon size="14">mdi-delete-outline</v-icon></v-btn>
            </td>
          </tr>
          <tr v-if="!loading && filteredRuns.length === 0">
            <td colspan="6" class="text-center text-grey py-6">
              {{ runs.length === 0 ? 'No runs yet. Click Run Test to start the first one.' : 'No runs match your filters.' }}
            </td>
          </tr>
          <tr v-if="loading">
            <td colspan="6" class="text-center py-6"><v-progress-circular indeterminate size="24" width="3" color="#0f766e" /></td>
          </tr>
        </tbody>
      </v-table>

      <div v-if="filteredRuns.length > 0" class="d-flex align-center justify-space-between flex-wrap pa-3 table-footer" style="gap: 8px;">
        <div class="text-caption">
          Showing <b>{{ rangeStart }}-{{ rangeEnd }}</b> of <b>{{ filteredRuns.length }}</b> runs
        </div>
        <div class="d-flex align-center" style="gap: 12px;">
          <v-select v-model="pageSize" :items="[5, 10, 25]" density="compact" variant="outlined" hide-details style="width: 90px;" />
          <v-pagination v-model="page" :length="pageCount" :total-visible="5" density="compact" />
        </div>
      </div>
    </v-card>

    <!-- Tester picker -->
    <v-dialog v-model="pickerDialog" max-width="480">
      <v-card class="rounded-xl overflow-hidden">
        <v-card-title class="pa-4 text-white picker-header">
          <div class="text-subtitle-1 font-weight-bold">Who is running this test?</div>
          <div class="text-caption" style="opacity: 0.85;">Pick the tester. The run is recorded under this person.</div>
        </v-card-title>
        <v-card-text class="pa-4">
          <v-alert v-if="!assignedTesters.length" type="info" variant="tonal" density="compact" class="mb-3 rounded-lg">
            No tester is assigned to this test case yet, so all users are listed.
          </v-alert>
          <div
            v-for="t in pickerTesters"
            :key="t.user_id"
            class="tester-card d-flex align-center pa-3 mb-2 rounded-lg"
            @click="runAs(t)"
          >
            <v-avatar size="34" color="#0f766e" class="mr-3 text-white font-weight-bold" style="font-size: 0.8rem;">{{ initials(t.user_name) }}</v-avatar>
            <div>
              <div class="font-weight-bold text-body-2">{{ t.user_name }}</div>
              <div class="text-caption text-grey">{{ t.department || 'No department' }}</div>
            </div>
            <v-spacer />
            <v-icon size="18" color="#0f766e">mdi-play-circle-outline</v-icon>
          </div>
        </v-card-text>
        <v-card-actions class="pa-3">
          <v-spacer />
          <v-btn variant="text" class="text-capitalize" @click="pickerDialog = false">Cancel</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Delete confirm -->
    <v-dialog v-model="deleteDialog" max-width="420">
      <v-card class="rounded-xl pa-2">
        <v-card-title class="text-subtitle-1 font-weight-bold">Delete {{ deleteTarget ? runCode(deleteTarget.id) : 'run' }}?</v-card-title>
        <v-card-text class="text-body-2">
          This removes the run together with its recorded step results. It cannot be undone.
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" class="text-capitalize" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="red" variant="flat" class="text-capitalize text-white" :loading="deleting" @click="deleteRun">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const API = 'https://localhost:7049/api'
const route = useRoute()
const router = useRouter()
const testCaseId = Number(route.params.id)

const loading = ref(true)
const error = ref('')
const testCase = ref(null)
const runs = ref([])
const users = ref([])
const assignments = ref([])

const getList = (url) => fetch(url).then((r) => (r.ok ? r.json() : [])).catch(() => [])

const loadRuns = async () => {
  const data = await getList(`${API}/TestRuns/by-testcase/${testCaseId}`)
  runs.value = Array.isArray(data) ? data : []
}

const loadAll = async () => {
  loading.value = true
  try {
    const caseRes = await fetch(`${API}/TestCases/${testCaseId}`).catch(() => null)
    testCase.value = caseRes && caseRes.ok ? await caseRes.json() : null
    if (!testCase.value) error.value = 'Test case not found or the API cannot be reached.'
    const [, u, a] = await Promise.all([
      loadRuns(),
      getList(`${API}/Users`),
      getList(`${API}/TestAssignments`),
    ])
    users.value = Array.isArray(u) ? u : []
    assignments.value = Array.isArray(a) ? a : []
  } finally {
    loading.value = false
  }
}

onMounted(loadAll)

// ---------- helpers ----------
const runCode = (id) => 'RUN-' + String(id).padStart(3, '0')
const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')
const initials = (name = '') => {
  const p = String(name).trim().split(/\s+/).filter(Boolean)
  return ((p[0]?.[0] || '') + (p[1]?.[0] || '')).toUpperCase() || '?'
}
const statusColor = (s) => {
  switch (s) {
    case 'Passed': return 'success'
    case 'Failed': return 'error'
    case 'Blocked': return 'warning'
    case 'In Progress': return 'indigo-darken-2'
    default: return 'grey'
  }
}
const isFinished = (r) => ['Passed', 'Failed', 'Blocked'].includes(r.run_status)
const goBack = () => router.push({ name: 'TestCases' })

// ---------- summary + filters ----------
const count = (s) => runs.value.filter((r) => r.run_status === s).length
const summary = computed(() => [
  { label: 'Total runs', value: runs.value.length, color: '#0f766e', filter: 'All' },
  { label: 'Passed', value: count('Passed'), color: '#2e7d32', filter: 'Passed' },
  { label: 'Failed', value: count('Failed') + count('Blocked'), color: '#c62828', filter: 'Failed' },
  { label: 'In progress', value: count('In Progress'), color: '#4f46e5', filter: 'In Progress' },
])

const search = ref('')
const statusFilter = ref('All')
const statusOptions = ['All', 'In Progress', 'Passed', 'Failed', 'Blocked']
const hasFilters = computed(() => !!(search.value || '').trim() || statusFilter.value !== 'All')
const clearFilters = () => { search.value = ''; statusFilter.value = 'All' }

const filteredRuns = computed(() => {
  const q = (search.value || '').trim().toLowerCase()
  return runs.value.filter((r) => {
    if (statusFilter.value !== 'All' && r.run_status !== statusFilter.value) return false
    if (q && !`${runCode(r.id)} ${r.user_name || ''}`.toLowerCase().includes(q)) return false
    return true
  })
})

const page = ref(1)
const pageSize = ref(10)
const pageCount = computed(() => Math.max(1, Math.ceil(filteredRuns.value.length / pageSize.value)))
const pagedRuns = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filteredRuns.value.slice(start, start + pageSize.value)
})
const rangeStart = computed(() => (filteredRuns.value.length ? (page.value - 1) * pageSize.value + 1 : 0))
const rangeEnd = computed(() => Math.min(page.value * pageSize.value, filteredRuns.value.length))
watch([search, statusFilter, pageSize], () => { page.value = 1 })
watch(pageCount, (n) => { if (page.value > n) page.value = n })

// ---------- start a run ----------
const pickerDialog = ref(false)

const userById = (id) => users.value.find((u) => u.id === id)

const assignedTesters = computed(() => {
  const seen = new Set()
  return assignments.value
    .filter((a) => a.test_case_id === testCaseId)
    .map((a) => {
      const u = userById(a.user_id)
      return u ? { user_id: u.id, user_name: u.full_name, department: a.department || u.department || '' } : null
    })
    .filter((t) => t && !seen.has(t.user_id) && seen.add(t.user_id))
})

const pickerTesters = computed(() =>
  assignedTesters.value.length
    ? assignedTesters.value
    : users.value.map((u) => ({ user_id: u.id, user_name: u.full_name, department: u.department || '' }))
)

const runAs = (tester) => {
  pickerDialog.value = false
  router.push({
    name: 'TestExecution',
    params: { id: testCaseId },
    query: {
      run_as: tester.user_id,
      run_as_name: tester.user_name,
      department: tester.department || '',
    },
  })
}

const startRun = () => {
  const list = pickerTesters.value
  if (!list.length) {
    error.value = 'There are no users in the system to run this test as. Add a user first.'
    return
  }
  // one possible tester: no need to ask
  if (list.length === 1) return runAs(list[0])
  pickerDialog.value = true
}

// ---------- report ----------
const openReport = (r) => {
  if (!isFinished(r)) return
  router.push({ name: 'TestRunReport', params: { id: testCaseId, runId: r.id } })
}

// ---------- delete ----------
const deleteDialog = ref(false)
const deleteTarget = ref(null)
const deleting = ref(false)

const confirmDelete = (r) => { deleteTarget.value = r; deleteDialog.value = true }

const deleteRun = async () => {
  if (!deleteTarget.value) return
  deleting.value = true
  try {
    const res = await fetch(`${API}/TestRuns/${deleteTarget.value.id}`, { method: 'DELETE' })
    if (!res.ok && res.status !== 404) {
      error.value = `Could not delete the run (HTTP ${res.status}).`
    } else {
      await loadRuns()
    }
  } catch (e) {
    error.value = 'Cannot connect to the API.'
  } finally {
    deleting.value = false
    deleteDialog.value = false
    deleteTarget.value = null
  }
}
</script>

<style scoped>
.page-heading {
  font-weight: 700;
  letter-spacing: -0.02em;
  color: rgb(var(--v-theme-on-surface));
}
.page-subheading { color: rgba(var(--v-theme-on-surface), 0.7); }

.table-header-row { background: linear-gradient(90deg, #760f6f 0%, #641f61 100%); }
.runs-table th {
  font-size: 0.75rem !important;
  color: #ffffff !important;
  text-transform: uppercase;
  font-weight: 700;
  background: transparent !important;
}
.main-table-card { border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.run-row { cursor: pointer; }
.run-row:hover { background: rgba(118, 15, 111, 0.06); }
.table-footer { border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }

.summary-card { cursor: pointer; transition: border-color 0.15s; }
.summary-card:hover,
.summary-active { border-color: #0f766e !important; }

.picker-header { background: linear-gradient(90deg, #760f6f 0%, #641f61 100%); }
.tester-card {
  cursor: pointer;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  transition: background-color 0.15s, border-color 0.15s;
}
.tester-card:hover { background: rgba(15, 118, 110, 0.08); border-color: #0f766e; }
</style>