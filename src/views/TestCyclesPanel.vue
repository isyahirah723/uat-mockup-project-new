<template>
  <div class="cycles-panel">
   
    <div class="d-flex align-center justify-space-between mb-3">
      <div>
        <div class="text-subtitle-1 font-weight-bold">Test Cycles</div>
        <div class="text-caption text-grey">Testing windows that Test Cases are grouped under</div>
        <div v-if="apiLoading" class="text-caption text-blue mt-1">Loading data from the server.</div>
      </div>
      <v-btn
        color="#0f766e"
        size="small"
        class="text-white rounded-lg text-capitalize font-weight-bold btn-glow"
        elevation="2"
        prepend-icon="mdi-plus-circle"
        @click="openCreateDialog"
      >
        New Test Cycle
      </v-btn>
    </div>
    
    <v-card variant="outlined" class="rounded-lg pa-0 overflow-hidden main-table-card" elevation="1">
      <v-table hover density="compact" class="cycle-table">
        <thead>
          <tr class="table-header-row">
            <th class="font-weight-bold">CYCLE ID</th>
            <th class="font-weight-bold">VERSION TAG</th>
            <th class="font-weight-bold">ASSIGNED TO</th>
            <th class="font-weight-bold">AUTO RULE</th>
            <th class="font-weight-bold text-center" style="width: 120px;">ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="item in paginatedCycles" :key="item.id">
          <tr class="cycle-row">
            <td class="py-1 text-no-wrap">
              <v-btn
                icon
                variant="text"
                size="x-small"
                class="mr-1"
                :title="isExpanded(item.id) ? 'Hide test cases' : 'Show test cases in this cycle'"
                @click="toggleExpand(item.id)"
              >
                <v-icon size="18">{{ isExpanded(item.id) ? 'mdi-chevron-down' : 'mdi-chevron-right' }}</v-icon>
              </v-btn>
              <span class="id-text id-link font-weight-bold" @click="openCycleDetail(item)">{{ item.cycle_code }}</span>
            </td>

            <td class="py-1">
              <v-chip size="x-small" variant="flat" color="#e0f2fe" class="text-light-blue-darken-4 font-weight-bold rounded-lg">
                <v-icon start size="12">mdi-sync</v-icon>
                {{ item.name }}
              </v-chip>
            </td>

            <td class="py-1">
              <span class="text-caption font-weight-medium">{{ item.assigned_to_name }}</span>
            </td>

            <td class="py-1">
              <v-chip size="x-small" variant="tonal" color="purple-darken-1" label class="font-weight-bold">
                {{ autoRuleLabel(item.auto_assign_rule) }}
              </v-chip>
            </td>

            <td class="text-center py-1">
              <div class="action-buttons-group">
                <v-btn icon variant="tonal" size="x-small" color="teal-darken-2" title="View" style="width: 22px; height: 22px;" @click="openCycleDetail(item)">
                  <v-icon size="14">mdi-eye-outline</v-icon>
                </v-btn>
                <v-btn icon variant="tonal" size="x-small" color="blue-darken-1" title="Edit" style="width: 22px; height: 22px;" @click="openEditDialog(item)">
                  <v-icon size="14">mdi-pencil-outline</v-icon>
                </v-btn>
                <v-btn icon variant="tonal" size="x-small" color="red-darken-1" title="Delete" style="width: 22px; height: 22px;" @click="confirmDelete(item)">
                  <v-icon size="14">mdi-delete-outline</v-icon>
                </v-btn>
              </div>
            </td>
          </tr>

          <tr v-if="isExpanded(item.id)" class="expand-row">
            <td colspan="5" class="py-3 pr-4 bg-slate-50" style="padding-left: 58px !important;">
              <div v-if="casesOf(item.id).length === 0" class="text-caption text-grey">No test cases in this cycle yet.</div>
              <div v-else>
                <div
                  v-for="tc in casesOf(item.id)"
                  :key="tc.id"
                  class="d-flex align-center py-1"
                  style="gap: 10px;"
                >
                  <span class="id-text font-weight-bold">{{ tc.test_case_code || tc.id }}</span>
                  <span class="text-body-2 flex-grow-1">{{ tc.title }}</span>
                  <v-chip size="x-small" label variant="tonal" :color="caseStatusColor(tc.status)" class="font-weight-bold">
                    {{ tc.status || 'Draft' }}
                  </v-chip>
                </div>
              </div>
            </td>
          </tr>
          </template>

          <tr v-if="cyclesList.length === 0">
            <td colspan="5" class="text-center text-grey pa-8">No test cycles available.</td>
          </tr>
        </tbody>
      </v-table>

      <div
        v-if="cyclesList.length > 0"
        class="d-flex flex-wrap justify-space-between align-center pa-2 px-3 border-t bg-slate-50"
        style="row-gap: 8px;"
      >
        <div class="text-caption text-grey-darken-1 font-weight-medium" style="font-size: 0.75rem;">
          Showing <span class="text-teal-darken-3 font-weight-bold">{{ pageStart }}-{{ pageEnd }}</span> of
          <span class="text-teal-darken-3 font-weight-bold">{{ cyclesList.length }}</span> records
        </div>

        <div class="d-flex align-center" style="gap: 12px;">
          <v-select
            v-model="itemsPerPage"
            :items="itemsPerPageOptions"
            label="Rows per page"
            variant="outlined"
            density="compact"
            hide-details
            style="min-width: 150px; max-width: 150px;"
          ></v-select>

          <v-pagination
            v-model="currentPage"
            :length="totalPages"
            :total-visible="5"
            density="compact"
            active-color="#0f766e"
          ></v-pagination>
        </div>
      </div>
    </v-card>

  
    <v-dialog v-model="dialog" max-width="900px" persistent scrollable>
      <v-card class="rounded-xl overflow-hidden cycles-panel">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between dialog-header">
          <span class="text-h6 font-weight-bold d-flex align-center">
            <v-icon class="mr-2">{{ isEditing ? 'mdi-pencil-box-outline' : 'mdi-plus-box-outline' }}</v-icon>
            {{ isEditing ? 'Edit Test Cycle' : 'New Test Cycle' }}
          </span>
          <v-btn icon variant="text" size="small" @click="dialog = false">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-6" style="max-height: 65vh; overflow-y: auto;">
          <div class="testmo-edit-layout">
            <!-- Main column -->
            <div class="testmo-main-col">
              <div class="d-flex align-center mb-3" style="gap: 10px;">
                <div class="testmo-icon-badge"><v-icon size="16" color="white">mdi-tag-outline</v-icon></div>
                <span class="text-caption font-weight-bold text-slate-500" style="text-transform: uppercase; letter-spacing: 0.04em;">Version Tag *</span>
              </div>
              <v-combobox
                v-model="cycleForm.name"
                :items="versionTagOptions"
                variant="outlined"
                density="comfortable"
                placeholder="Choose a version or type a new one..."
                clearable
                hide-details
                class="mb-4"
              ></v-combobox>

              <div class="d-flex align-center mb-3" style="gap: 10px;">
                <div class="testmo-icon-badge"><v-icon size="16" color="white">mdi-identifier</v-icon></div>
                <span class="text-caption font-weight-bold text-slate-500" style="text-transform: uppercase; letter-spacing: 0.04em;">Cycle ID</span>
              </div>
              <v-text-field
                v-model="cycleForm.cycleId"
                variant="outlined"
                density="comfortable"
                placeholder="Auto-generated by system"
                readonly
                hide-details
                class="mb-4"
              ></v-text-field>

              <div class="d-flex align-center mb-3" style="gap: 10px;">
                <div class="testmo-icon-badge"><v-icon size="16" color="white">mdi-account-outline</v-icon></div>
                <span class="text-caption font-weight-bold text-slate-500" style="text-transform: uppercase; letter-spacing: 0.04em;">Assigned To</span>
              </div>
              <v-select
                v-model="cycleForm.assignedTo"
                :items="users"
                item-title="full_name"
                item-value="id"
                variant="outlined"
                density="comfortable"
                placeholder="Unassigned"
                clearable
                hide-details
              ></v-select>
            </div>

            <!-- Properties panel -->
            <div class="testmo-props-col">
              <div class="testmo-props-field">
                <span class="testmo-props-label">Status</span>
                <v-select
                  v-model="cycleForm.status"
                  :items="['Active', 'Completed']"
                  variant="outlined"
                  density="compact"
                  hide-details
                ></v-select>
              </div>

              <div class="testmo-props-field">
                <span class="testmo-props-label">Department</span>
                <v-select
                  v-model="cycleForm.department"
                  :items="departmentOptions"
                  variant="outlined"
                  density="compact"
                  placeholder="Choose Department Code"
                  clearable
                  hide-details
                ></v-select>
              </div>

              <div class="testmo-props-field">
                <span class="testmo-props-label">Start Date</span>
                <v-text-field v-model="cycleForm.dtStart" type="date" variant="outlined" density="compact" hide-details></v-text-field>
              </div>

              <div class="testmo-props-field">
                <span class="testmo-props-label">End Date</span>
                <v-text-field v-model="cycleForm.dtEnd" type="date" variant="outlined" density="compact" hide-details></v-text-field>
              </div>

              <div class="testmo-props-field">
                <span class="testmo-props-label">Created Date</span>
                <v-text-field v-model="cycleForm.createdDate" type="date" variant="outlined" density="compact" disabled hide-details></v-text-field>
              </div>

              <div class="testmo-props-field">
                <span class="testmo-props-label">Auto-Assign Rule</span>
                <v-select
                  v-model="cycleForm.autoAssignRule"
                  :items="autoAssignOptions"
                  variant="outlined"
                  density="compact"
                  placeholder="Choose rule"
                  clearable
                  hide-details
                ></v-select>
              </div>
            </div>
          </div>
        </v-card-text>

        <v-card-actions class="pa-4 border-t bg-slate-50">
          <v-spacer></v-spacer>
          <v-btn variant="tonal" color="grey-darken-2" class="text-capitalize font-weight-bold rounded-lg mr-2" @click="dialog = false">Cancel</v-btn>
          <v-btn
            variant="flat"
            color="#0f766e"
            class="px-6 rounded-lg text-capitalize font-weight-bold btn-glow save-btn"
            :loading="saving"
            :disabled="saving"
            @click="saveCycle"
          >
            {{ isEditing ? 'Update Test Cycle' : 'Save Test Cycle' }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- DELETE CONFIRMATION -->
    <v-dialog v-model="deleteDialog" max-width="400px">
      <v-card class="rounded-xl pa-2 cycles-panel">
        <v-card-title class="font-weight-bold">Delete Test Cycle?</v-card-title>
        <v-card-text>
          Are you sure you want to delete <b>"{{ deleteTarget?.name }}"</b>?
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="deleteDialog = false">Cancel</v-btn>
          <v-btn color="error" variant="flat" @click="handleDelete">Delete</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()

// Test cases come from the parent (TestCasepage.vue)
const props = defineProps({
  testCases: { type: Array, default: () => [] }
})

// 'changed'    -> a cycle was created / updated / deleted (parent refreshes its cycle list)
// 'view-cases' -> user clicked a cycle's case count (parent switches tab + filters by cycle)
const emit = defineEmits(['changed', 'view-cases'])

const API_URL = 'https://localhost:7049/api/TestCycles'

const apiCycles = ref([])
const apiLoading = ref(false)
const users = ref([])

const fetchUsers = async () => {
  try {
    const res = await fetch('https://localhost:7049/api/Users')
    if (res.ok) users.value = await res.json()
  } catch (error) {
    console.error('Failed to load users:', error)
  }
}

const userName = (id) => users.value.find(u => u.id === id)?.full_name || null
const saving = ref(false)

// logged-in user id from localStorage; null lets the API fall back to an existing user
const currentUserId = () => Number(localStorage.getItem('uat_user_id')) || null

// local date (not UTC) as YYYY-MM-DD
const today = () => {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

const formatDate = (dateStr) => {
  if (!dateStr) return '-'
  return dateStr.split('T')[0]
}

const fetchCyclesFromAPI = async () => {
  apiLoading.value = true
  try {
    const response = await fetch(API_URL)
    if (!response.ok) throw new Error('Failed to fetch data from the API')
    apiCycles.value = await response.json()
  } catch (error) {
    console.error('Error API:', error)
  } finally {
    apiLoading.value = false
  }
}

// ---------- Cycle ID: CY + YYMMDD + 3-digit running number (e.g. CY260928001) ----------
const generateCycleCode = () => {
  const d = new Date()
  const prefix = `CY${String(d.getFullYear()).slice(-2)}${String(d.getMonth() + 1).padStart(2, '0')}${String(d.getDate()).padStart(2, '0')}`

  const used = apiCycles.value
    .map(c => c.cycle_code || '')
    .filter(code => code.startsWith(prefix) && /^\d+$/.test(code.slice(prefix.length)))
    .map(code => parseInt(code.slice(prefix.length), 10))

  const next = (used.length ? Math.max(...used) : 0) + 1
  return prefix + String(next).padStart(3, '0')
}

const cyclesList = computed(() => {
  return apiCycles.value.map(cycle => ({
    ...cycle,
    cycle_code: cycle.cycle_code || '-',
    name: cycle.name || '-',
    department: cycle.department || '-',
    status: cycle.status || 'Active',
    auto_assign_rule: cycle.auto_assign_rule || 'none',
    created_by_name: cycle.created_by_name || userName(cycle.created_by) || 'System',
    assigned_to_name: cycle.assigned_to_name || userName(cycle.assigned_to) || 'Unassigned',
    dt_start_formatted: formatDate(cycle.dt_start),
    dt_end_formatted: formatDate(cycle.dt_end),
    created_date_formatted: formatDate(cycle.created_date)
  }))
})

// ---------- Pagination ----------
const itemsPerPageOptions = [5, 10, 20, 50]
const itemsPerPage = ref(10)
const currentPage = ref(1)

const totalPages = computed(() => Math.max(1, Math.ceil(cyclesList.value.length / itemsPerPage.value)))
const paginatedCycles = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  return cyclesList.value.slice(start, start + itemsPerPage.value)
})
const pageStart = computed(() => (cyclesList.value.length === 0 ? 0 : (currentPage.value - 1) * itemsPerPage.value + 1))
const pageEnd = computed(() => Math.min(currentPage.value * itemsPerPage.value, cyclesList.value.length))

watch(itemsPerPage, () => { currentPage.value = 1 })
watch(totalPages, (n) => { if (currentPage.value > n) currentPage.value = n })

// Test cases that belong to a cycle (for progress + expandable row)
const casesOf = (cycleId) =>
  props.testCases.filter(tc => String(tc.cycle_id) === String(cycleId))

const caseStatusColor = (status) => {
  if (status === 'Passed') return 'success'
  if (status === 'Failed') return 'error'
  if (status === 'Pending') return 'warning'
  return 'grey'
}

// which cycle rows are expanded
const expandedIds = ref([])
const isExpanded = (id) => expandedIds.value.includes(id)
const toggleExpand = (id) => {
  expandedIds.value = isExpanded(id) ? expandedIds.value.filter(x => x !== id) : [...expandedIds.value, id]
}

const versionTagOptions = ['v2.0.0 Pre-Launch UAT', 'Sprint 25 Regression', 'v1.5.0 Production']

const departmentOptions = [
  'PEM', 'ADGM', 'PMM', 'SMHR', 'PB8', 'PB7', 'PB4', 'PB10',
  'MOTC', 'SOTC', 'AOE', 'PQA', 'SEIT', 'MGMT', 'OTR', 'SOIT',
  'SOVS', 'PAPR', 'PMGM', 'PLFG', 'SOMT', 'SMMG', 'PB6', 'PRND',
  'SETH', 'SEPC', 'SETT', 'PRDA', 'AIT', 'SOPA', 'SEGH', 'PLTB',
  'PLTA', 'SEPH', 'SOGT', 'PB3', 'AMCA', 'PAPK', 'PB2', 'PTM',
  'ARPD', 'PQC', 'SEVN', 'SOIP', 'SOVN', 'SEKA', 'PLOG', 'METH',
  'SOKA', 'PLRM', 'AHR', 'AACT'
]

const autoAssignOptions = [
  { title: 'Default (Latest Active Cycle)', value: 'default_latest' },
  { title: 'By Module / Feature', value: 'by_module' },
  { title: 'By Priority', value: 'by_priority' },
  { title: 'By Department', value: 'by_department' },
  { title: 'No Auto-Assign (Manual)', value: 'none' }
]

const autoRuleLabels = {
  default_latest: 'Latest Active',
  by_module: 'By Module',
  by_priority: 'By Priority',
  by_department: 'By Department',
  none: 'Manual'
}
const autoRuleLabel = (rule) => autoRuleLabels[rule] || rule || 'Manual'

// ---------- Dialog / form ----------
const dialog = ref(false)
const isEditing = ref(false)
const deleteDialog = ref(false)
const deleteTarget = ref(null)

// Cycle details now open on their own page instead of a popup
const openCycleDetail = (item) => router.push({ name: 'TestCycleDetail', params: { id: item.id } })

const emptyForm = () => ({
  id: null,
  cycleId: '',
  name: '',
  department: '',
  dtStart: today(),
  dtEnd: today(),
  status: 'Active',
  createdDate: today(),
  autoAssignRule: 'none',
  createdBy: null,
  assignedTo: null,
  raw: null
})

const cycleForm = ref(emptyForm())

const openCreateDialog = async () => {
  isEditing.value = false
  // refresh first so the running number is based on the latest data
  await fetchCyclesFromAPI()
  cycleForm.value = emptyForm()
  cycleForm.value.cycleId = generateCycleCode()
  dialog.value = true
}

const openEditDialog = (item) => {
  isEditing.value = true
  cycleForm.value = {
    id: item.id,
    cycleId: item.cycle_code,
    name: item.name === '-' ? '' : item.name,
    department: item.department === '-' ? '' : item.department,
    dtStart: item.dt_start ? item.dt_start.split('T')[0] : today(),
    dtEnd: item.dt_end ? item.dt_end.split('T')[0] : today(),
    status: item.status || 'Active',
    createdDate: item.created_date ? item.created_date.split('T')[0] : today(),
    autoAssignRule: item.auto_assign_rule || 'none',
    createdBy: item.created_by ?? null,
    assignedTo: item.assigned_to ?? null,
    // untouched original record, so fields this form doesn't edit (module, priority, ...) aren't wiped on update
    raw: apiCycles.value.find(c => c.id === item.id) || null
  }
  dialog.value = true
}

const saveCycle = async () => {
  if (!cycleForm.value.name) {
    alert('Please select a Version Tag!')
    return
  }

  saving.value = true
  try {
    // re-check the running number right before saving (someone else may have created a cycle meanwhile)
    if (!isEditing.value) {
      await fetchCyclesFromAPI()
      cycleForm.value.cycleId = generateCycleCode()
    }

    const payload = {
      cycle_code: cycleForm.value.cycleId,
      name: cycleForm.value.name,
      department: cycleForm.value.department || null,
      dt_start: cycleForm.value.dtStart || null,
      dt_end: cycleForm.value.dtEnd || null,
      status: cycleForm.value.status || 'Active',
      auto_assign_rule: cycleForm.value.autoAssignRule || null,
      created_by: cycleForm.value.createdBy ?? currentUserId(),
      assigned_to: cycleForm.value.assignedTo ?? null
    }

    if (isEditing.value) payload.id = cycleForm.value.id

    const url = isEditing.value ? `${API_URL}/${cycleForm.value.id}` : API_URL

    const response = await fetch(url, {
      method: isEditing.value ? 'PUT' : 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(isEditing.value ? { ...(cycleForm.value.raw || {}), ...payload } : payload)
    })

    if (response.ok) {
      alert(isEditing.value ? 'Successfully updated!' : 'Successfully saved!')
      dialog.value = false
      await fetchCyclesFromAPI()
      emit('changed')
    } else {
      const errorText = await response.text()
      alert(`Failed to save: ${errorText}`)
    }
  } catch (error) {
    console.error('Error saving cycle:', error)
    alert('Error: Cannot connect to the API.')
  } finally {
    saving.value = false
  }
}

const confirmDelete = (item) => {
  deleteTarget.value = item
  deleteDialog.value = true
}

const handleDelete = async () => {
  if (!deleteTarget.value) return

  try {
    const response = await fetch(`${API_URL}/${deleteTarget.value.id}`, { method: 'DELETE' })

    if (response.ok) {
      alert('Successfully deleted!')
      deleteDialog.value = false
      await fetchCyclesFromAPI()
      emit('changed')
    } else {
      alert('Failed to delete.')
    }
  } catch (error) {
    console.error('Error deleting:', error)
    alert('Error: Cannot connect to the API.')
  }
}

const getStatusColor = (status) => {
  if (status === 'Active') return 'success'
  if (status === 'Completed') return 'info'
  return 'grey'
}

onMounted(() => {
  fetchUsers()
  fetchCyclesFromAPI()
})
</script>

<style scoped>
.cycles-panel,
.cycles-panel * {
  font-family: Arial, Helvetica, sans-serif !important;
}

.cursor-pointer { cursor: pointer; }

.cases-chip { transition: all 0.15s ease; }
.cases-chip.cursor-pointer:hover {
  transform: translateY(-1px);
  box-shadow: 0 3px 8px rgba(15, 118, 110, 0.35);
}

.id-text {
  color: rgb(var(--v-theme-on-surface));
  font-size: 12px;
  letter-spacing: 0.3px;
  white-space: nowrap;
}

.dept-tag {
  background-color: rgba(var(--v-theme-on-surface), 0.05);
  color: rgb(var(--v-theme-on-surface));
  padding: 3px 8px;
  border-radius: 6px;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.table-header-row {
  background: linear-gradient(90deg, #760f6f 0%, #641f61 100%);
}

.cycle-table th {
  font-size: 0.75rem !important;
  color: #ffffff !important;
  text-transform: uppercase;
  font-weight: 700;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.cycle-row {
  transition: all 0.2s ease;
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.cycle-row:hover {
  background-color: rgba(15, 118, 110, 0.1) !important;
  transform: translateY(-1px);
}

.main-table-card { border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.bg-slate-50 { background-color: rgba(var(--v-theme-on-surface), 0.05); }
.border-t { border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }

.action-buttons-group {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 4px;
}

.btn-glow { transition: all 0.2s ease-in-out; }
.btn-glow:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(15, 118, 110, 0.3) !important;
}

.dialog-header {
  background: linear-gradient(135deg, #670e5f 0%, #e987d4 100%);
}

.testmo-edit-layout {
  display: grid;
  grid-template-columns: 1fr 260px;
  gap: 28px;
}

@media (max-width: 640px) {
  .testmo-edit-layout { grid-template-columns: 1fr; }
}

.testmo-main-col { min-width: 0; }

.testmo-icon-badge {
  width: 26px;
  height: 26px;
  border-radius: 8px;
  background: linear-gradient(135deg, #0f766e 0%, #115e59 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.testmo-props-col {
  border-left: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  padding-left: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.testmo-props-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.view-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px 24px;
}

.testmo-props-label {
  font-size: 0.65rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: rgba(var(--v-theme-on-surface), 0.5);
  font-weight: 700;
}
.id-link { cursor: pointer; }
.id-link:hover { text-decoration: underline; color: var(--acc-teal, #0f766e); }

/* Solid, always-readable Save/Update button (was washing out to pale mint) */
.save-btn {
  background-color: #0f766e !important;
  color: #ffffff !important;
  opacity: 1 !important;
}
.save-btn:hover { background-color: #0d5f59 !important; }
.save-btn.v-btn--disabled {
  background-color: #0f766e !important;
  color: #ffffff !important;
  opacity: 0.65 !important;
}
</style>