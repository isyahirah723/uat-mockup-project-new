<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <!-- Header -->
    <div class="d-flex align-start justify-space-between mb-4 flex-wrap" style="row-gap: 12px;">
      <div class="d-flex align-center">
        <div class="header-accent mr-3"></div>
        <div>
          <div class="text-h6 font-weight-bold d-flex align-center">
            <v-icon color="teal-darken-2" class="mr-2">mdi-history</v-icon>Audit Logs
          </div>
          <div class="text-caption text-medium-emphasis">
            Track and monitor all system activities, user actions, and data changes across your account
          </div>
        </div>
      </div>
      <v-btn
        color="#0f766e"
        variant="flat"
        size="small"
        class="text-capitalize text-white font-weight-bold"
        prepend-icon="mdi-refresh"
        :loading="loading"
        @click="refreshAll"
      >
        Refresh
      </v-btn>
    </div>

    <!-- Filters + table -->
    <v-card variant="outlined" class="pa-3 audit-card" elevation="1">
      <v-row dense class="mb-2">
        <v-col cols="12" md="3">
          <v-text-field
            v-model="searchQuery"
            placeholder="Search by Run ID"
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            color="teal-darken-2"
            hide-details
            clearable
          />
        </v-col>
        <v-col cols="6" md="2">
          <v-select
            v-model="userFilter"
            :items="userOptions"
            placeholder="Select users"
            variant="outlined"
            density="compact"
            color="teal-darken-2"
            hide-details
            clearable
          />
        </v-col>
        <v-col cols="6" md="2">
          <v-select
            v-model="actionFilter"
            :items="actionOptions"
            variant="outlined"
            density="compact"
            color="teal-darken-2"
            hide-details
            :prefix="'Action - '"
          />
        </v-col>
        <v-col cols="12" md="3">
          <v-select
            v-model="dateFilter"
            :items="dateOptions"
            item-title="title"
            item-value="value"
            variant="outlined"
            density="compact"
            color="teal-darken-2"
            hide-details
          />
        </v-col>
        <v-col cols="12" md="2" class="d-flex align-center">
          <v-btn variant="text" size="small" color="purple-darken-2" class="font-weight-bold" @click="clearFilters">Reset</v-btn>
        </v-col>
      </v-row>

      <v-data-table
        :headers="headers"
        :items="filteredLogs"
        :loading="loading"
        class="audit-table"
        density="comfortable"
        :items-per-page="20"
        :items-per-page-options="[10, 20, 50, 100]"
        hover
        @click:row="(_, { item }) => openDrawer(item)"
      >
        <!-- ID -->
        <template #item.runId="{ item }">
          <span class="text-body-2 run-id-text font-weight-bold">{{ item.runId || '-' }}</span>
        </template>

        <!-- Title -->
        <template #item.title="{ item }">
          <span class="text-body-2 font-weight-medium">{{ item.title || '-' }}</span>
        </template>

        <!-- Module -->
        <template #item.module="{ item }">
          <div class="d-flex align-center text-body-2">
            <v-icon size="16" class="mr-2" color="teal-darken-2">mdi-clipboard-check-outline</v-icon>
            {{ moduleOf(item) }}
          </div>
        </template>

        <!-- Action -->
        <template #item.action="{ item }">
          <div class="d-flex align-center text-body-2" :class="`text-${actionColorOf(item)}`">
            <v-icon size="16" class="mr-1">{{ actionIconOf(item) }}</v-icon>
            {{ actionLabelOf(item) }}
          </div>
        </template>

        <!-- Done By -->
        <template #item.crtUserId="{ item }">
          <div class="d-flex align-center">
            <v-avatar size="28" :color="avatarColor(userOf(item))" class="mr-2">
              <span class="text-caption font-weight-bold">{{ initials(userOf(item)) }}</span>
            </v-avatar>
            <div>
              <div class="text-body-2 font-weight-medium">{{ userOf(item) }}</div>
              <div class="text-caption text-medium-emphasis">{{ roleOf(userOf(item)) }}</div>
            </div>
          </div>
        </template>

        <!-- Date & Time -->
        <template #item.dtCreated="{ item }">
          <div>
            <div class="text-body-2 font-weight-medium">{{ formatDate(dateOf(item)) }}</div>
            <div class="text-caption text-medium-emphasis">at {{ formatTime(dateOf(item)) }}</div>
          </div>
        </template>

        <!-- View button -->
        <template #item.actions="{ item }">
          <v-btn
            icon="mdi-eye-outline"
            variant="tonal"
            color="teal-darken-2"
            size="small"
            density="comfortable"
            aria-label="View details"
            @click.stop="openDrawer(item)"
          />
        </template>

        <template #no-data>
          <div class="text-center text-grey pa-10">
            <v-icon size="40" color="grey-lighten-1" class="mb-2">mdi-history</v-icon>
            <div class="text-body-2">No audit logs found.</div>
            <div v-if="hasActiveFilters" class="text-caption">Try adjusting your search or filters.</div>
          </div>
        </template>
      </v-data-table>
    </v-card>

    <!-- Details drawer -->
    <v-navigation-drawer
      v-model="drawer"
      location="right"
      temporary
      width="440"
      class="audit-drawer"
    >
      <div v-if="selected">
        <div class="d-flex align-center justify-space-between px-5 py-3 drawer-header">
          <div class="d-flex align-center text-body-2 font-weight-bold">
            <v-icon size="16" class="mr-2">mdi-information-outline</v-icon>
            Additional Details
          </div>
          <v-btn icon="mdi-close" variant="tonal" size="small" density="comfortable" aria-label="Close" @click="closeDrawer" />
        </div>

        <div class="px-5 py-4 drawer-body">
          <div class="detail-block">
            <div class="detail-label">Module</div>
            <div class="detail-value">{{ moduleOf(selected) }}</div>
          </div>

          <div class="detail-block">
            <div class="detail-label">Run ID</div>
            <div class="detail-value d-flex align-center">
              <span class="mr-2">{{ selected.runId || '-' }}</span>
              <v-btn
                v-if="selected.runId"
                :icon="copied ? 'mdi-check' : 'mdi-content-copy'"
                variant="text"
                size="x-small"
                density="comfortable"
                aria-label="Copy Run ID"
                @click="copyRunId"
              />
            </div>
          </div>

          <div class="detail-block">
            <div class="detail-label">Title</div>
            <div class="detail-value">{{ selected.title || '-' }}</div>
          </div>

          <div class="detail-block">
            <div class="detail-label">Action</div>
            <div class="detail-value" :class="`text-${actionColorOf(selected)}`">
              {{ actionLabelOf(selected) }}
            </div>
          </div>

          <div v-if="!isFeedback(selected) && (selected.statusOld || selected.statusNew)" class="detail-block">
            <div class="detail-label">Status change</div>
            <div class="d-flex align-center">
              <v-chip size="x-small" :color="getStatusColor(selected.statusOld)" variant="outlined" class="mr-1">
                {{ selected.statusOld || '-' }}
              </v-chip>
              <v-icon size="small" class="mx-1">mdi-arrow-right</v-icon>
              <v-chip size="x-small" :color="getStatusColor(selected.statusNew)" variant="flat">
                {{ selected.statusNew || '-' }}
              </v-chip>
            </div>
          </div>

          <div class="detail-block">
            <div class="detail-label">Date of change</div>
            <div class="detail-value">{{ formatFull(dateOf(selected)) }}</div>
          </div>

          <div class="detail-block">
            <div class="detail-label">Modified by</div>
            <div class="d-flex align-center mt-1">
              <v-avatar size="30" :color="avatarColor(userOf(selected))" class="mr-3">
                <span class="text-caption font-weight-bold">{{ initials(userOf(selected)) }}</span>
              </v-avatar>
              <div>
                <div class="text-body-2 font-weight-medium">{{ userOf(selected) }}</div>
                <div class="text-caption text-medium-emphasis">{{ roleOf(userOf(selected)) }}</div>
              </div>
            </div>
          </div>

          <div v-if="isFeedback(selected)" class="detail-block">
            <div class="detail-label">Rating</div>
            <v-rating
              :model-value="Number(selected.statusNew)"
              readonly
              density="compact"
              size="small"
              color="amber"
              active-color="amber"
            />
          </div>

          <div class="detail-block">
            <div class="detail-label">{{ isFeedback(selected) ? 'Feedback' : 'Description' }}</div>
            <div class="detail-text">{{ selected.details || '-' }}</div>
          </div>
        </div>
      </div>

      <template #append>
        <div v-if="selected" class="d-flex ga-2 px-5 py-3 drawer-footer">
            <v-btn
              size="small"
              variant="outlined"
              color="purple-darken-2"
              prepend-icon="mdi-arrow-left"
              class="text-capitalize"
              :disabled="selectedIndex <= 0"
              @click="goPrev"
            >
              Previous
            </v-btn>
            <v-btn
              size="small"
              variant="outlined"
              color="purple-darken-2"
              append-icon="mdi-arrow-right"
              class="text-capitalize"
              :disabled="selectedIndex >= filteredLogs.length - 1"
              @click="goNext"
            >
              Next
            </v-btn>
        </div>
      </template>
    </v-navigation-drawer>
  </v-container>
</template>

<script setup>
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue'
import { auditLogService } from '@/services/auditLogService'

const API = 'https://localhost:7049/api'
const logs = ref([])
const testCases = ref([])
const testCycles = ref([])
const loading = ref(false)

// filters
const searchQuery = ref('')
const userFilter = ref(null)
const actionFilter = ref('All')
const dateFilter = ref(60)

const actionOptions = ['All', 'CREATE', 'UPDATE', 'DELETE']
const dateOptions = [
  { title: 'Last 7 days', value: 7 },
  { title: 'Last 30 days', value: 30 },
  { title: 'Last 60 days', value: 60 },
  { title: 'Last 90 days', value: 90 },
  { title: 'All time', value: 0 }
]

// drawer
const drawer = ref(false)
const selected = ref(null)
const copied = ref(false)

const headers = [
  { title: 'ID', key: 'runId', sortable: false, width: 150 },
  { title: 'Title', key: 'title', sortable: false },
  { title: 'Module', key: 'module', sortable: false },
  { title: 'Action', key: 'action', sortable: false },
  { title: 'Done By', key: 'crtUserId', sortable: false },
  { title: 'Date & Time', key: 'dtCreated' },
  { title: '', key: 'actions', sortable: false, align: 'end', width: 56 }
]

// helpers
// "danish hakim", "Danish Hakim", "danish.hakim" -> "Danish Hakim"
const normalizeUser = (raw) => {
  const s = String(raw ?? '').trim().replace(/[._]+/g, ' ').replace(/\s+/g, ' ')
  if (!s) return 'System'
  return s.toLowerCase().replace(/\b\w/g, (c) => c.toUpperCase())
}
const userOf = (log) => normalizeUser(log.crtUserId || log.user)
const dateOf = (log) => log.dtCreated || log.timestamp

// Hanya pengguna yang terlibat dalam UAT. Tambah/buang nama di sini jika perlu.
const INVOLVED_USERS = ['Danish Hakim', 'Intan Syahirah', 'Realdo Dias', 'Vivian']
const userOptions = INVOLVED_USERS

// Peranan setiap pengguna. Ubah di sini jika ada perubahan.
const USER_ROLES = {
  'Intan Syahirah': 'Admin',
  'Realdo Dias': 'Tester',
  Vivian: 'Tester',
  'Danish Hakim': 'Tester'
}
const roleOf = (name) => USER_ROLES[name] || 'User'

const hasActiveFilters = computed(
  () => !!searchQuery.value || !!userFilter.value || actionFilter.value !== 'All' || dateFilter.value !== 60
)

const filteredLogs = computed(() => {
  const query = searchQuery.value?.toLowerCase().trim()
  const cutoff = dateFilter.value
    ? Date.now() - dateFilter.value * 24 * 60 * 60 * 1000
    : null

  return logs.value.filter((log) => {
    if (actionFilter.value !== 'All' && log.action !== actionFilter.value) return false
    if (userFilter.value && userOf(log) !== userFilter.value) return false
    if (cutoff) {
      const t = new Date(dateOf(log)).getTime()
      if (!isNaN(t) && t < cutoff) return false
    }
    if (query) {
      const haystack = [log.runId, log.title, userOf(log)].filter(Boolean).join(' ').toLowerCase()
      if (!haystack.includes(query)) return false
    }
    return true
  })
})

const clearFilters = () => {
  searchQuery.value = ''
  userFilter.value = null
  actionFilter.value = 'All'
  dateFilter.value = 60
}

// drawer logic
const selectedIndex = computed(() =>
  selected.value ? filteredLogs.value.findIndex((l) => l === selected.value) : -1
)

const openDrawer = (log) => {
  selected.value = log
  copied.value = false
  drawer.value = true
}
const closeDrawer = () => {
  drawer.value = false
}
const goPrev = () => {
  if (selectedIndex.value > 0) openDrawer(filteredLogs.value[selectedIndex.value - 1])
}
const goNext = () => {
  if (selectedIndex.value < filteredLogs.value.length - 1) {
    openDrawer(filteredLogs.value[selectedIndex.value + 1])
  }
}

const copyRunId = async () => {
  try {
    await navigator.clipboard.writeText(selected.value.runId)
    copied.value = true
    setTimeout(() => (copied.value = false), 1500)
  } catch (err) {
    console.error('Copy failed:', err)
  }
}

// Log feedback tester: statusOld kosong, statusNew ialah rating (1-5)
const isFeedback = (log) =>
  !log.statusOld && /^[0-5]$/.test(String(log.statusNew ?? '').trim())

// Module: log test case (admin create / update / delete) ikut module test case tu,
// manakala log tester run test case sentiasa "UAT Run".
const RUN_MODULE = 'UAT Run'
const normKey = (v) => String(v ?? '').trim().toLowerCase()

const moduleByKey = computed(() => {
  const m = new Map()
  for (const tc of testCases.value) {
    if (!tc.module) continue
    if (tc.test_case_code) m.set(normKey(tc.test_case_code), tc.module)
    if (tc.title) m.set(normKey(tc.title), tc.module)
  }
  return m
})

// Log test cycle: Run ID bermula "CY" (cth CY261002001)
const CYCLE_MODULE = 'Test Cycle'
const isCycleLog = (log) => /^cy\d/i.test(String(log.runId ?? '').trim())

const moduleOf = (log) => {
  // Kalau backend dah simpan module sebenar dalam log, guna terus (ini yang paling tepat, termasuk DELETE)
  if (log.module && log.module !== RUN_MODULE) return log.module
  // Tester run test case -> sentiasa UAT Run
  if (isFeedback(log)) return RUN_MODULE

  // Admin create / update / delete test cycle -> module cycle (atau "Test Cycle"), bukan UAT Run
  if (isCycleLog(log)) {
    const cyc = testCycles.value.find((c) => normKey(c.cycle_code) === normKey(log.runId))
    return cyc?.module || CYCLE_MODULE
  }

  const byKey = moduleByKey.value
  const hit = byKey.get(normKey(log.runId)) || byKey.get(normKey(log.title))
  if (hit) return hit

  // title / runId mungkin ada kod test case di dalamnya (cth "Test Case TC261005001")
  const text = `${log.title ?? ''} ${log.runId ?? ''}`.toLowerCase()
  for (const tc of testCases.value) {
    const code = normKey(tc.test_case_code)
    if (tc.module && code && text.includes(code)) return tc.module
  }
  return RUN_MODULE
}

// keyboard: Esc closes, arrows navigate
const onKeydown = (e) => {
  if (!drawer.value) return
  if (e.key === 'Escape') closeDrawer()
  else if (e.key === 'ArrowLeft') goPrev()
  else if (e.key === 'ArrowRight') goNext()
}
onMounted(() => window.addEventListener('keydown', onKeydown))
onBeforeUnmount(() => window.removeEventListener('keydown', onKeydown))

// reset selection if the list changes under it
watch(filteredLogs, () => {
  if (selected.value && selectedIndex.value === -1) drawer.value = false
})

// formatting
const getStatusColor = (status) => {
  if (status === 'Passed' || status === 'Approved') return 'success'
  if (status === 'Failed' || status === 'Rejected') return 'error'
  if (status === 'In Progress' || status === 'Pending') return 'warning'
  if (status === 'Draft') return 'grey'
  return 'info'
}
const getActionColor = (action) => {
  if (action === 'CREATE') return 'success'
  if (action === 'UPDATE') return 'primary'
  if (action === 'DELETE') return 'error'
  return 'grey'
}
const getActionIcon = (action) => {
  if (action === 'CREATE') return 'mdi-plus-circle-outline'
  if (action === 'UPDATE') return 'mdi-pencil-outline'
  if (action === 'DELETE') return 'mdi-delete-outline'
  return 'mdi-information-outline'
}
const formatAction = (action) =>
  action ? action.charAt(0) + action.slice(1).toLowerCase() : '-'

// Log feedback tester dipaparkan sebagai "Run Test", bukan "Create"
const actionColorOf = (log) => (isFeedback(log) ? 'orange-darken-2' : getActionColor(log.action))
const actionIconOf = (log) => (isFeedback(log) ? 'mdi-play-circle-outline' : getActionIcon(log.action))
const actionLabelOf = (log) => (isFeedback(log) ? 'Run Test' : formatAction(log.action))

const initials = (name = '') => {
  const parts = String(name).trim().split(/\s+/).filter(Boolean)
  if (!parts.length) return 'S'
  return (parts[0][0] + (parts[1]?.[0] || '')).toUpperCase()
}
const AVATAR_COLORS = ['orange-lighten-4', 'purple-lighten-4', 'indigo-lighten-4', 'pink-lighten-4', 'cyan-lighten-4', 'blue-lighten-4', 'teal-lighten-4']
const avatarColor = (name = '') => {
  let h = 0
  for (const c of String(name)) h = (h * 31 + c.charCodeAt(0)) >>> 0
  return AVATAR_COLORS[h % AVATAR_COLORS.length]
}

const toDate = (v) => {
  const d = new Date(v)
  return isNaN(d.getTime()) ? null : d
}
const formatDate = (v) => {
  const d = toDate(v)
  return d ? d.toLocaleDateString('en-US', { month: 'short', day: '2-digit', year: 'numeric' }) : '-'
}
const formatTime = (v) => {
  const d = toDate(v)
  return d ? d.toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }) : '-'
}
const formatFull = (v) => {
  const d = toDate(v)
  return d ? `${formatDate(v)} ${formatTime(v)}` : '-'
}


const fetchLogs = async () => {
  loading.value = true
  try {
    const res = await auditLogService.getAll()
    logs.value = res.data
  } catch (err) {
    console.error('Failed to load audit logs:', err)
  } finally {
    loading.value = false
  }
}

const fetchTestCases = async () => {
  try {
    const res = await fetch(`${API}/TestCases`)
    if (res.ok) testCases.value = await res.json()
  } catch (err) {
    console.error('Failed to load test cases for module lookup:', err)
  }
}

const fetchTestCycles = async () => {
  try {
    const res = await fetch(`${API}/TestCycles`)
    if (res.ok) testCycles.value = await res.json()
  } catch (err) {
    console.error('Failed to load test cycles for module lookup:', err)
  }
}


const refreshAll = async () => {
  await Promise.all([fetchTestCases(), fetchTestCycles(), fetchLogs()])
}

onMounted(refreshAll)
</script>

<style scoped>
.audit-card {
  border-radius: 12px;
  border-color: #e9d5ff !important;
}

.header-accent {
  width: 4px;
  align-self: stretch;
  min-height: 44px;
  border-radius: 4px;
  background: linear-gradient(180deg, #670e5f 0%, #e987d4 100%);
}

.audit-table :deep(thead th) {
  font-size: 0.75rem !important;
  font-weight: 700 !important;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #ffffff !important;
  background: linear-gradient(90deg, #760f6f 0%, #641f61 100%) !important;
}
.audit-table :deep(thead th .v-icon),
.audit-table :deep(thead th .v-data-table-header__sort-icon) {
  color: #ffffff !important;
}
.audit-table :deep(thead th:first-child) {
  border-top-left-radius: 8px;
}
.audit-table :deep(thead th:last-child) {
  border-top-right-radius: 8px;
}
.audit-table :deep(tbody tr) {
  cursor: pointer;
}
.audit-table :deep(tbody tr:hover) {
  background-color: #fdf4ff !important;
}

.run-id-text {
  color: #a21caf;
  font-weight: 600;
  letter-spacing: 0.02em;
}

.drawer-header,
.drawer-footer {
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}
.drawer-header {
  background: linear-gradient(135deg, #670e5f 0%, #e987d4 100%);
  color: #ffffff;
  border-bottom: none;
}
.drawer-header :deep(.v-btn) {
  color: #ffffff;
}
.drawer-footer {
  border-bottom: none;
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}

.detail-block {
  margin-bottom: 16px;
}
.detail-label {
  font-size: 0.7rem;
  color: #760f6f;
  font-weight: 600;
  margin-bottom: 2px;
}
.detail-value {
  font-size: 0.8rem;
  font-weight: 500;
  word-break: break-all;
}

.detail-text {
  font-size: 0.8rem;
  line-height: 1.5;
  white-space: pre-wrap;
  word-break: break-word;
  background: #f8fafc;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  padding: 10px 12px;
  margin-top: 4px;
  max-height: 260px;
  overflow: auto;
}
</style>