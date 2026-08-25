<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <v-row class="mb-6" align="center">
      <v-col cols="12" sm="6">
        <div>
          <div class="text-caption text-grey font-weight-bold">// REPOSITORY</div>
          <div class="text-h5 font-weight-bold">Test Cases</div>
          <div v-if="apiLoading" class="text-caption text-blue">Loading data from the server</div>
        </div>
      </v-col>

      <v-col cols="12" sm="6" class="d-flex justify-end align-center">
        <v-btn color="#00c853" class="rounded-lg mr-2 text-white" prepend-icon="mdi-microsoft-excel" @click="exportExcel">
          Excel
        </v-btn>
        <v-btn color="#d32f2f" class="rounded-lg mr-2 text-white" prepend-icon="mdi-file-pdf-box" @click="exportPDF">
          PDF
        </v-btn>
        <v-btn color="#4338ca" class="text-white rounded-lg text-capitalize" elevation="0" @click="openCreateDialog">
          <v-icon left size="small" class="mr-1">mdi-plus</v-icon> New Test Case
        </v-btn>
      </v-col>
    </v-row>

    <v-card variant="outlined" class="pa-4 rounded-xl mb-6">
      <v-row density="compact" align="center">
        <v-col cols="12" sm="4">
          <v-text-field
            v-model="searchQuery"
            placeholder="Search Test Title, ID, or Assigned..."
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            hide-details
            clearable
          ></v-text-field>
        </v-col>

        <v-col cols="12" sm="3">
          <v-select
            v-model="statusFilter"
            :items="['All', 'Passed', 'Failed', 'Pending', 'Draft']"
            label="Filter Status"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>

        <v-col cols="12" sm="3">
          <v-select
            v-model="priorityFilter"
            :items="['All', 'Low', 'Medium', 'High', 'Critical']"
            label="Filter Priority"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>

        <v-col cols="12" sm="3">
          <v-select
            v-model="cycleFilter"
            label="Filter Cycle"
            :items="cycleOptions"
            item-title="name"
            item-value="id"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>

        <v-col cols="12" sm="3" class="text-end">
          <v-btn variant="text" size="small" color="grey-darken-1" @click="clearFilters">
            Reset Filter
          </v-btn>
        </v-col>
      </v-row>
    </v-card>

    <v-card variant="outlined" class="rounded-xl pa-0 overflow-hidden">
      <v-table hover class="test-case-table">
        <thead>
          <tr style="background-color: #f8fafc;">
            <th class="font-weight-bold">ID / TITLE</th>
            <th class="font-weight-bold">CYCLE</th>
            <th class="font-weight-bold">DEPT</th>
            <th class="font-weight-bold">PRIORITY</th>
            <th class="font-weight-bold">STATUS</th>
            <th class="font-weight-bold">ASSIGNED TO</th>
            <th class="font-weight-bold text-center" style="width: 160px;">ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in filteredTestCases" :key="item.id">
            <td class="py-3">
              <div class="font-weight-bold text-body-2 text-indigo-darken-3">
                {{ item.test_case_code || '#' + item.id }}
              </div>
              <div class="text-body-2 font-weight-medium">{{ item.title }}</div>
              <div class="text-caption text-truncate" style="max-width: 300px;">
                {{ item.test_description || 'Tiada penerangan' }}
              </div>
            </td>

            <td>
              <v-chip size="x-small" variant="tonal" color="indigo">
                {{ getCycleName(item.cycle_id) }}
              </v-chip>
            </td>

            <td>
              <span class="text-caption font-weight-bold">{{ item.test_department || '-' }}</span>
            </td>

            <td>
              <v-chip size="small" :color="getPriorityColor(item.priority)" label class="font-weight-bold">
                {{ item.priority || 'Medium' }}
              </v-chip>
            </td>

            <td>
              <v-chip size="small" :color="getStatusColor(item.status)" label class="font-weight-bold">
                {{ item.status || 'Draft' }}
              </v-chip>
            </td>

            <td>
              <div class="text-caption font-weight-medium">{{ item.assigned_to || 'Unassigned' }}</div>
            </td>

            <td class="text-center">
              <v-btn icon variant="text" size="small" color="blue" title="Edit" @click="openEditDialog(item)">
                <v-icon size="small">mdi-pencil-outline</v-icon>
              </v-btn>

              <v-btn icon variant="text" size="small" color="green" title="View Details" @click="openViewDialog(item)">
                <v-icon size="small">mdi-eye-outline</v-icon>
              </v-btn>

              <v-btn icon variant="text" size="small" color="purple" title="Feedback" @click="goToFeedback(item.id)">
                <v-icon size="small">mdi-message-text-outline</v-icon>
              </v-btn>

              <v-btn icon variant="text" size="small" color="orange" title="Export PDF" @click="exportSinglePDF(item)">
                <v-icon size="small">mdi-file-pdf-box</v-icon>
              </v-btn>

              <v-btn icon variant="text" size="small" color="red" title="Delete" @click="confirmDelete(item.id)">
                <v-icon size="small">mdi-delete-outline</v-icon>
              </v-btn>
            </td>
          </tr>

          <tr v-if="filteredTestCases.length === 0 && !apiLoading">
            <td colspan="7" class="text-center text-grey py-8">
              No Test Case records found.
            </td>
          </tr>
        </tbody>
      </v-table>
    </v-card>

    <v-dialog v-model="dialog" max-width="900px" persistent>
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">{{ isEdit ? 'Edit Test Case' : 'New Test Case' }}</span>
          <v-btn icon variant="text" size="small" @click="dialog = false">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-6">
          <v-tabs v-model="activeTab" color="indigo-accent-4" class="mb-4 border-b">
            <v-tab value="header">Header</v-tab>
            <v-tab value="description">Description</v-tab>
            <v-tab value="steps">Steps</v-tab>
            <v-tab value="feedback">Feedback</v-tab>
          </v-tabs>

          <v-window v-model="activeTab">
            <v-window-item value="header">
              <v-row density="compact">
                <v-col cols="12" sm="8">
                  <v-text-field label="Test Title *" v-model="form.title" variant="outlined" density="compact"></v-text-field>
                </v-col>
                <v-col cols="12" sm="4">
                  <v-text-field
                    label="Test Case ID"
                    v-model="form.testCaseId"
                    variant="outlined"
                    density="compact"
                    placeholder="Auto-generated by system"
                    :disabled="!isEdit"
                  ></v-text-field>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-select
                    label="Department"
                    :items="departmentOptions"
                    v-model="form.testDepartment"
                    variant="outlined"
                    density="compact"
                    @update:model-value="onDeptChange"
                  ></v-select>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-select
                    label="Version Tag"
                    :items="versionTagOptions"
                    v-model="form.versionTag"
                    variant="outlined"
                    density="compact"
                    placeholder="Choose Version Tag"
                    clearable
                  >
                  </v-select>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-select
                    label="Module / Feature"
                    :items="moduleOptions"
                    v-model="form.module"
                    variant="outlined"
                    density="compact"
                    clearable
                  >
                  </v-select>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-select
                    label="Test Cycle"
                    :items="testCycles"
                    item-title="name"
                    item-value="id"
                    v-model="form.cycleId"
                    variant="outlined"
                    density="compact"
                    clearable
                  >
                  </v-select>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-select label="Priority" :items="['Low', 'Medium', 'High', 'Critical']" v-model="form.priority" variant="outlined" density="compact"></v-select>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-select label="Status" :items="['Draft', 'Passed', 'Failed', 'Pending']" v-model="form.status" variant="outlined" density="compact"></v-select>
                </v-col>

                <v-col cols="12" sm="4">
                  <v-text-field label="Assigned To" v-model="form.assignedTo" variant="outlined" density="compact"></v-text-field>
                </v-col>
              </v-row>
            </v-window-item>

            <v-window-item value="description">
              <v-textarea label="Test Description" v-model="form.testDescription" variant="outlined" density="compact" rows="3"></v-textarea>
              <v-textarea label="Test Dependencies" v-model="form.dependencies" variant="outlined" density="compact" rows="2" class="mt-2"></v-textarea>
              <v-textarea label="Test Conditions" v-model="form.conditions" variant="outlined" density="compact" rows="2" class="mt-2"></v-textarea>
              <v-textarea label="Test Control" v-model="form.control" variant="outlined" density="compact" rows="2" class="mt-2"></v-textarea>
            </v-window-item>

            <v-window-item value="steps">
              <div class="d-flex justify-space-between align-center mb-3">
                <div class="text-subtitle-2 font-weight-bold">Test Steps</div>
                <v-btn size="small" color="indigo-accent-4" prepend-icon="mdi-plus" class="text-capitalize" @click="addStep">Add Step</v-btn>
              </div>
              <div v-for="(step, idx) in form.steps" :key="idx" class="mb-3 pa-3 rounded-lg" style="border: 1px solid #e2e8f0;">
                <div class="d-flex align-center justify-space-between mb-2">
                  <span class="font-weight-bold text-caption">STEP {{ idx + 1 }}</span>
                  <v-btn icon size="x-small" variant="text" color="red" @click="removeStep(idx)"><v-icon>mdi-delete</v-icon></v-btn>
                </div>
                <v-row dense>
                  <v-col cols="12" md="8">
                    <v-text-field v-model="step.description" label="Step Description" variant="outlined" density="compact" hide-details></v-text-field>
                  </v-col>
                  <v-col cols="12" md="4">
                    <v-text-field v-model="step.expected" label="Expected Result" variant="outlined" density="compact" hide-details></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="6" class="mt-2">
                    <v-select v-model="step.passFail" :items="['Pass', 'Fail', 'N/A']" label="Result" variant="outlined" density="compact" hide-details></v-select>
                  </v-col>
                  <v-col cols="12" sm="6" class="mt-2">
                    <v-text-field v-model="step.actual" label="Actual Result" variant="outlined" density="compact" hide-details></v-text-field>
                  </v-col>
                </v-row>
              </div>
              <div v-if="!form.steps || form.steps.length === 0" class="text-center text-grey py-4 text-caption">No step added. Click 'Add Step' above.</div>
            </v-window-item>

            <v-window-item value="feedback">
              <div class="d-flex justify-space-between align-center mb-3">
                <div class="text-subtitle-2 font-weight-bold">Feedback</div>
                <v-btn size="small" color="indigo-accent-4" prepend-icon="mdi-plus" class="text-capitalize" @click="addFeedback">Add Feedback</v-btn>
              </div>
              <div v-for="(fb, idx) in form.feedbacks" :key="idx" class="mb-3 pa-3 rounded-lg" style="border: 1px solid #e2e8f0;">
                <div class="d-flex align-center justify-space-between mb-2">
                  <span class="font-weight-bold text-caption">FEEDBACK #{{ idx + 1 }}</span>
                  <v-btn icon size="x-small" variant="text" color="red" @click="removeFeedback(idx)"><v-icon>mdi-delete</v-icon></v-btn>
                </div>
                <v-row dense>
                  <v-col cols="12" md="6">
                    <v-text-field v-model="fb.user" label="User Name" variant="outlined" density="compact" hide-details></v-text-field>
                  </v-col>
                  <v-col cols="12" md="6">
                    <v-select v-model="fb.rating" :items="[1,2,3,4,5]" label="Rating" variant="outlined" density="compact" hide-details></v-select>
                  </v-col>
                  <v-col cols="12" class="mt-2">
                    <v-textarea v-model="fb.comment" label="Comment" variant="outlined" density="compact" rows="2" hide-details></v-textarea>
                  </v-col>
                </v-row>
              </div>
              <div v-if="!form.feedbacks || form.feedbacks.length === 0" class="text-center text-grey py-4 text-caption">No feedback. Click 'Add Feedback' above.</div>
            </v-window-item>
          </v-window>
        </v-card-text>

        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="dialog = false" class="text-capitalize">Cancel</v-btn>
          <v-btn color="indigo-accent-4" class="px-6 rounded-lg text-capitalize text-white" :loading="saving" :disabled="saving" @click="saveTestCase">Save Test Case</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

   
    <v-dialog v-model="viewDialog" max-width="800px">
      <v-card class="rounded-xl" v-if="viewItem">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">Test Case Details</span>
          <v-btn icon variant="text" size="small" @click="viewDialog = false"><v-icon color="white">mdi-close</v-icon></v-btn>
        </v-card-title>

        <v-card-text class="pa-6">
          <div class="mb-4">
            <div class="text-caption text-grey font-weight-bold">TITLE</div>
            <div class="text-h6 font-weight-bold text-indigo-darken-4">{{ viewItem.title }}</div>
          </div>
          
          <v-row class="mb-2">
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">ID</div>
              <div>{{ viewItem.test_case_code || '#' + viewItem.id }}</div>
            </v-col>
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">DEPARTMENT</div>
              <div>{{ viewItem.test_department || '-' }}</div>
            </v-col>
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">VERSION TAG</div>
              <div>{{ viewItem.version_tag || '-' }}</div>
            </v-col>
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">STATUS</div>
              <v-chip size="x-small" :color="getStatusColor(viewItem.status)" label class="font-weight-bold">
                {{ viewItem.status || 'Draft' }}
              </v-chip>
            </v-col>
          </v-row>

          <v-divider class="my-4"></v-divider>
          
          <div class="mb-4">
            <div class="text-caption text-grey font-weight-bold">DESCRIPTION</div>
            <div class="text-body-2">{{ viewItem.test_description || 'Tiada penerangan' }}</div>
          </div>

          <v-divider class="my-4"></v-divider>
          
          <div class="text-subtitle-2 font-weight-bold mb-2">TEST STEPS ({{ viewItem.steps ? viewItem.steps.length : 0 }})</div>
          <div v-if="viewItem.steps && viewItem.steps.length > 0">
            <div v-for="(step, idx) in viewItem.steps" :key="idx" class="mb-2 pa-3 rounded-lg" style="border: 1px solid #e2e8f0; background-color: #f8fafc;">
              <div class="d-flex align-center justify-space-between">
                <div><span class="font-weight-bold mr-2">#{{ idx+1 }}</span> {{ step.description }}</div>
                <v-chip size="x-small" :color="step.pass_fail === 'Pass' || step.passFail === 'Pass' ? 'green' : step.pass_fail === 'Fail' || step.passFail === 'Fail' ? 'red' : 'grey'">
                  {{ step.pass_fail || step.passFail || 'N/A' }}
                </v-chip>
              </div>
              <div class="text-caption text-grey mt-1">Expected: {{ step.expected || '-' }}</div>
              <div class="text-caption text-grey" v-if="step.actual">Actual: {{ step.actual }}</div>
            </div>
          </div>
          <div v-else class="text-grey text-caption">no steps provided.</div>

          <v-divider class="my-4"></v-divider>
          
          <div class="text-subtitle-2 font-weight-bold mb-2">FEEDBACK ({{ viewItem.feedbacks ? viewItem.feedbacks.length : 0 }})</div>
          <div v-if="viewItem.feedbacks && viewItem.feedbacks.length > 0">
            <div v-for="(fb, idx) in viewItem.feedbacks" :key="idx" class="mb-2 pa-3 rounded-lg" style="border: 1px solid #e2e8f0;">
              <div class="d-flex align-center justify-space-between">
                <span class="font-weight-bold text-caption">{{ fb.user_name || fb.user || 'Anonymous' }}</span>
                <span class="text-amber-darken-2">Rating: {{ fb.rating || 5 }}/5</span>
              </div>
              <div class="text-body-2 mt-1">{{ fb.comment }}</div>
            </div>
          </div>
          <div v-else class="text-grey text-caption">no feedback</div>
        </v-card-text>

        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn color="indigo-accent-4" variant="text" @click="viewDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import * as XLSX from 'xlsx' 
import { jsPDF } from 'jspdf'
import 'jspdf-autotable'
import { kotraLogoBase64 } from '@/assets/kotraLogo'

const router = useRouter()
const route = useRoute()
const apiTestCases = ref([])
const apiLoading = ref(false)
const testCycles = ref([])

const fetchFromAPI = async () => {
  apiLoading.value = true
  try {
    const response = await fetch('https://localhost:7049/api/TestCases')
    if (!response.ok) throw new Error('Failed to connect to the API')
    apiTestCases.value = await response.json()
  } catch (error) {
    console.error('Error API:', error)
  } finally {
    apiLoading.value = false
  }
}

const versionTagOptions = [
  'v1.0.0 Production',
  'v1.5.0 Production',
  'v2.0.0 Pre-Launch UAT',
  'Sprint 25 Regression'
]

const searchQuery = ref('')
const statusFilter = ref('All')
const priorityFilter = ref('All')
const cycleFilter = ref('All')
const dialog = ref(false)
const isEdit = ref(false)
const activeTab = ref('header')
const viewDialog = ref(false)
const viewItem = ref(null)

const departmentOptions = [
  'PEM', 'ADGM', 'PMM', 'SMHR', 'PB8', 'PB7', 'PB4', 'PB10', 
  'MOTC', 'SOTC', 'AOE', 'PQA', 'SEIT', 'MGMT', 'OTR', 'SOIT', 
  'SOVS', 'PAPR', 'PMGM', 'PLFG', 'SOMT', 'SMMG', 'PB6', 'PRND', 
  'SETH', 'SEPC', 'SETT', 'PRDA', 'AIT', 'SOPA', 'SEGH', 'PLTB', 
  'PLTA', 'SEPH', 'SOGT', 'PB3', 'AMCA', 'PAPK', 'PB2', 'PTM', 
  'ARPD', 'PQC', 'SEVN', 'SOIP', 'SOVN', 'SEKA', 'PLOG', 'METH', 
  'SOKA', 'PLRM', 'AHR', 'AACT'
]

const moduleOptions = [
  'Login / Authentication',
  'User Management',
  'Dashboard',
  'Reporting',
  'Payment / Billing',
  'Inventory',
  'Order Management',
  'Customer Service',
  'API Integration',
  'Mobile App',
  'Admin Panel',
  'Notifications',
  'Search / Filter',
  'File Upload / Download',
  'Other'
]

const form = ref({
  id: null,
  title: '',
  testCaseId: '',
  testDepartment: '',
  versionTag: '',
  module: '',
  cycleId: null,
  priority: 'Medium',
  status: 'Draft',
  assignedTo: '',
  testDescription: '',
  dependencies: '',
  conditions: '',
  control: '',
  steps: [],
  feedbacks: []
})

const cycleOptions = computed(() => {
  const options = [{ id: 'All', name: 'All' }]
  if (testCycles.value.length > 0) {
    testCycles.value.forEach(cycle => {
      options.push({ id: cycle.id, name: cycle.name })
    })
  }
  return options
})

const filteredTestCases = computed(() => {
  const sourceData = apiTestCases.value
  
  return sourceData.filter(tc => {
    const matchesSearch = searchQuery.value === '' || 
      (tc.title && tc.title.toLowerCase().includes(searchQuery.value.toLowerCase())) ||
      (tc.test_case_code && tc.test_case_code.toLowerCase().includes(searchQuery.value.toLowerCase())) ||
      (tc.assigned_to && tc.assigned_to.toLowerCase().includes(searchQuery.value.toLowerCase()))
    
    const matchesStatus = statusFilter.value === 'All' || tc.status === statusFilter.value
    const matchesPriority = priorityFilter.value === 'All' || tc.priority === priorityFilter.value
    const matchesCycle = cycleFilter.value === 'All' || String(tc.cycle_id) === String(cycleFilter.value)

    return matchesSearch && matchesStatus && matchesPriority && matchesCycle
  })
})

const getCycleName = (cycleId) => {
  if (!cycleId) return '-'
  const cycle = testCycles.value.find(c => String(c.id) === String(cycleId))
  return cycle?.name || cycleId
}

const onDeptChange = (dept) => {
  
}

const clearFilters = () => {
  searchQuery.value = ''
  statusFilter.value = 'All'
  priorityFilter.value = 'All'
  cycleFilter.value = 'All'
}

const openCreateDialog = () => {
  isEdit.value = false
  form.value = {
    id: null,
    title: '',
    testCaseId: '',
    testDepartment: '',
    versionTag: '',
    module: '',
    cycleId: null,
    priority: 'Medium',
    status: 'Draft',
    assignedTo: '',
    testDescription: '',
    dependencies: '',
    conditions: '',
    control: '',
    steps: [],
    feedbacks: []
  }
  activeTab.value = 'header'
  dialog.value = true
}

const openEditDialog = (item) => {
  isEdit.value = true
  form.value = {
    id: item.id,
    title: item.title || '',
    testCaseId: item.test_case_code || '',
    testDepartment: item.test_department || '',
    versionTag: item.version_tag || '',
    module: item.module || '',
    cycleId: item.cycle_id || null,
    priority: item.priority || 'Medium',
    status: item.status || 'Draft',
    assignedTo: item.assigned_to || '',
    testDescription: item.test_description || '',
    dependencies: item.dependencies || '',
    conditions: item.conditions || '',
    control: item.control_notes || '',
    steps: item.steps ? item.steps.map(s => ({
      id: s.id || 0,
      test_case_id: s.test_case_id || item.id || 0,
      step_order: s.step_order || 1,
      description: s.description || '',
      expected: s.expected || '',
      actual: s.actual || '',
      passFail: s.pass_fail || s.passFail || 'N/A'
    })) : [],
    feedbacks: item.feedbacks ? item.feedbacks.map(f => ({
      id: f.id || 0,
      test_case_id: f.test_case_id || item.id || 0,
      user: f.user_name || f.user || '',
      rating: f.rating || 5,
      comment: f.comment || ''
    })) : []
  }
  activeTab.value = 'header'
  dialog.value = true
}

const addStep = () => {
  if (!form.value.steps) form.value.steps = []
  form.value.steps.push({ id: 0, test_case_id: form.value.id || 0, step_order: form.value.steps.length + 1, description: '', expected: '', passFail: 'N/A', actual: '' })
}

const removeStep = (index) => {
  form.value.steps.splice(index, 1)
}

const addFeedback = () => {
  if (!form.value.feedbacks) form.value.feedbacks = []
  form.value.feedbacks.push({ id: 0, test_case_id: form.value.id || 0, user: '', rating: 5, comment: '' })
}

const removeFeedback = (index) => {
  form.value.feedbacks.splice(index, 1)
}

const saving = ref(false)

const saveTestCase = async () => {
  if (saving.value) return // elak double-submit
  if (!form.value.title || !form.value.title.trim()) {
    alert('Please fill in the Test Title!')
    return
  }
  saving.value = true

  const payload = {
    id: form.value.id || 0,
    test_case_code: form.value.testCaseId || null, 
    title: form.value.title,
    test_department: form.value.testDepartment || null,
    version_tag: form.value.versionTag || null,
    module: form.value.module || null,
    cycle_id: form.value.cycleId ? Number(form.value.cycleId) : null,
    priority: form.value.priority || 'Medium',
    status: form.value.status || 'Draft',
    assigned_to: form.value.assignedTo || null,
    test_description: form.value.testDescription || null,
    dependencies: form.value.dependencies || null,
    conditions: form.value.conditions || null,
    control_notes: form.value.control || null,
    created_by: 14,
    steps: (form.value.steps || []).map((step, idx) => ({
      id: step.id || 0,
      test_case_id: form.value.id || 0,
      step_order: idx + 1,
      description: step.description || '',
      expected: step.expected || '',
      actual: step.actual || '',
      pass_fail: step.passFail || 'N/A'
    })),
    feedbacks: (form.value.feedbacks || []).map(fb => ({
      id: fb.id || 0,
      test_case_id: form.value.id || 0,
      user_name: fb.user || '',
      rating: Number(fb.rating) || 5,
      comment: fb.comment || ''
    }))
  }

  try {
    const url = isEdit.value 
      ? `https://localhost:7049/api/TestCases/${form.value.id}` 
      : 'https://localhost:7049/api/TestCases'
    
    const method = isEdit.value ? 'PUT' : 'POST'

    const response = await fetch(url, {
      method: method,
      headers: { 
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(payload)
    })

    if (response.ok) {
      alert(`Test Case, Steps & Feedbacks berjaya ${isEdit.value ? 'dikemaskini' : 'disimpan'} ke SQL Server!`)
      dialog.value = false
      await fetchFromAPI() 
    } else {
      const errorText = await response.text()
      alert(`Gagal simpan (Status ${response.status}): ${errorText}`)
    }
  } catch (error) {
    console.error('Error saving to API:', error)
    alert('Error: Cannot connect to the API. Make sure Backend is running.')
  } finally {
    saving.value = false
  }
}

const confirmDelete = async (id) => {
  if (!confirm('Are you sure you want to delete this Test Case?')) return

  try {
    const response = await fetch(`https://localhost:7049/api/TestCases/${id}`, {
      method: 'DELETE'
    })

    if (response.ok) {
      alert('The Test Case has been successfully deleted from SQL Server!')
      await fetchFromAPI()
    } else {
      alert('Failed to delete Test Case from the API.')
    }
  } catch (error) {
    console.error('Error deleting:', error)
    alert('Error: Cannot connect to the API.')
  }
}

const openViewDialog = (item) => {
  viewItem.value = JSON.parse(JSON.stringify(item))
  viewDialog.value = true
}

const goToFeedback = (testCaseId) => {
  router.push(`/feedback/${testCaseId}`)
}

const getPriorityColor = (p) => {
  if (p === 'Critical') return 'red'
  if (p === 'High') return 'orange'
  if (p === 'Medium') return 'blue'
  return 'grey'
}

const getStatusColor = (s) => {
  if (s === 'Passed') return 'green'
  if (s === 'Failed') return 'red'
  if (s === 'Pending') return 'orange'
  return 'grey'
}

const exportExcel = () => {
  const rows = filteredTestCases.value.map(tc => ({
    'Test Case ID': tc.test_case_code || ('#' + tc.id),
    'Title': tc.title || '',
    'Cycle': getCycleName(tc.cycle_id),
    'Department': tc.test_department || '',
    'Priority': tc.priority || '',
    'Status': tc.status || '',
    'Assigned To': tc.assigned_to || 'Unassigned',
    'Description': tc.test_description || ''
  }))

  if (rows.length === 0) {
    alert('No Test Cases to export.')
    return
  }

  const worksheet = XLSX.utils.json_to_sheet(rows)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Test Cases')
  XLSX.writeFile(workbook, `TestCases_${new Date().toISOString().substr(0, 10)}.xlsx`)
}

const exportPDF = () => {
  if (filteredTestCases.value.length === 0) {
    alert('No Test Cases to export.')
    return
  }

  const doc = new jsPDF({ orientation: 'landscape' })

  const logoX = 14
  const logoY = 8
  const logoW = 16
  const logoH = 19
  doc.addImage(kotraLogoBase64, 'PNG', logoX, logoY, logoW, logoH)

  const textX = logoX + logoW + 4
  doc.setFontSize(14)
  doc.setFont(undefined, 'bold')
  doc.text('Test Cases Report', textX, 13)
  doc.setFont(undefined, 'normal')

  doc.setFontSize(8)
  doc.setTextColor(71, 85, 105)
  doc.text('KOTRA PHARMA (M) SDN BHD', textX, 18)
  doc.setFontSize(7)
  doc.text('1, 2 & 3, Jalan TTC 12, Cheng Industrial Estate, 75250 Melaka, Malaysia', textX, 22)

  doc.setTextColor(0, 0, 0)
  doc.setFontSize(9)
  doc.text(`Generated: ${new Date().toLocaleString()}`, textX, 27)

  doc.setDrawColor(150)
  doc.line(14, logoY + logoH + 3, doc.internal.pageSize.getWidth() - 14, logoY + logoH + 3)

  doc.autoTable({
    startY: logoY + logoH + 8,
    head: [['ID', 'Title', 'Cycle', 'Dept', 'Priority', 'Status', 'Assigned To']],
    body: filteredTestCases.value.map(tc => [
      tc.test_case_code || ('#' + tc.id),
      tc.title || '',
      getCycleName(tc.cycle_id),
      tc.test_department || '-',
      tc.priority || '-',
      tc.status || '-',
      tc.assigned_to || 'Unassigned'
    ]),
    styles: { fontSize: 8 },
    headStyles: { fillColor: [30, 41, 59] }
  })

  doc.save(`TestCases_${new Date().toISOString().substr(0, 10)}.pdf`)
}

const exportSinglePDF = (item) => {
  const doc = new jsPDF()
  const pageWidth = doc.internal.pageSize.getWidth()
  const margin = 12
  const usableWidth = pageWidth - margin * 2

  const drawField = (x, y, w, h, label, value) => {
    doc.setDrawColor(180)
    doc.rect(x, y, w, h)
    doc.setFillColor(241, 245, 249)
    doc.rect(x, y, w, 5, 'F')
    doc.rect(x, y, w, 5)
    doc.setFontSize(6)
    doc.setTextColor(71, 85, 105)
    doc.text(String(label).toUpperCase(), x + 1.5, y + 3.5)
    doc.setFontSize(9)
    doc.setTextColor(15, 23, 42)
    const text = value ? String(value) : '-'
    const lines = doc.splitTextToSize(text, w - 3)
    doc.text(lines, x + 1.5, y + 8)
  }

  
  const measureFieldHeight = (value, w, minHeight = 12) => {
    doc.setFontSize(9)
    const text = value ? String(value) : '-'
    const lines = doc.splitTextToSize(text, w - 3)
    const neededHeight = 8 + (lines.length * 4) + 2 
    return Math.max(minHeight, neededHeight)
  }

  const formatDate = (d) => d ? new Date(d).toLocaleDateString() : '-'

  
  const logoX = margin
  const logoY = 8
  const logoW = 16
  const logoH = 19 
  doc.addImage(kotraLogoBase64, 'PNG', logoX, logoY, logoW, logoH)

  const textX = logoX + logoW + 4
  doc.setFontSize(13)
  doc.setTextColor(15, 23, 42)
  doc.setFont(undefined, 'bold')
  doc.text('TEST CASE PLANNING AND EXECUTION TEMPLATE', textX, 13)
  doc.setFont(undefined, 'normal')

  doc.setFontSize(8)
  doc.setTextColor(71, 85, 105)
  doc.text('KOTRA PHARMA (M) SDN BHD', textX, 18)
  doc.setFontSize(7)
  doc.text('1, 2 & 3, Jalan TTC 12, Cheng Industrial Estate, 75250 Melaka, Malaysia', textX, 22)

  doc.setDrawColor(150)
  doc.line(margin, logoY + logoH + 3, pageWidth - margin, logoY + logoH + 3)

  let y = 34
  const row1Widths = [usableWidth * 0.38, usableWidth * 0.14, usableWidth * 0.16, usableWidth * 0.16, usableWidth * 0.16]
  let x = margin
  drawField(x, y, row1Widths[0], 12, 'Test Title', item.title); x += row1Widths[0]
  drawField(x, y, row1Widths[1], 12, 'Priority', item.priority); x += row1Widths[1]
  drawField(x, y, row1Widths[2], 12, 'Test Case ID', item.test_case_code || ('#' + item.id)); x += row1Widths[2]
  drawField(x, y, row1Widths[3], 12, 'Test Number', item.test_number); x += row1Widths[3]
  drawField(x, y, row1Widths[4], 12, 'Test Date', formatDate(item.test_date))

  
  y += 12
  const row2Widths = [usableWidth * 0.28, usableWidth * 0.24, usableWidth * 0.24, usableWidth * 0.24]
  x = margin
  drawField(x, y, row2Widths[0], 12, 'Test Cycle', getCycleName(item.cycle_id)); x += row2Widths[0]
  drawField(x, y, row2Widths[1], 12, 'Department', item.test_department); x += row2Widths[1]
  drawField(x, y, row2Widths[2], 12, 'Assigned To', item.assigned_to || 'Unassigned'); x += row2Widths[2]
  drawField(x, y, row2Widths[3], 12, 'Execution Date', formatDate(item.execution_date))

  
  y += 12
  const row3Widths = [usableWidth * 0.34, usableWidth * 0.22, usableWidth * 0.22, usableWidth * 0.22]
  x = margin
  const row3Height = Math.max(
    measureFieldHeight(item.test_description, row3Widths[0]),
    measureFieldHeight(item.dependencies, row3Widths[1]),
    measureFieldHeight(item.conditions, row3Widths[2]),
    measureFieldHeight(item.control_notes, row3Widths[3])
  )
  drawField(x, y, row3Widths[0], row3Height, 'Test Description', item.test_description); x += row3Widths[0]
  drawField(x, y, row3Widths[1], row3Height, 'Dependencies', item.dependencies); x += row3Widths[1]
  drawField(x, y, row3Widths[2], row3Height, 'Conditions', item.conditions); x += row3Widths[2]
  drawField(x, y, row3Widths[3], row3Height, 'Control Notes', item.control_notes)

  y += row3Height + 6

  // --- STEPS TABLE ---
  doc.setFontSize(10)
  doc.setFont(undefined, 'bold')
  doc.setTextColor(15, 23, 42)
  doc.text('Test Steps', margin, y)
  doc.setFont(undefined, 'normal')
  y += 3

  const steps = item.steps && item.steps.length > 0 ? item.steps : []
  doc.autoTable({
    startY: y,
    head: [['Step ID', 'Step Description', 'Expected Results', 'Actual Results', 'Pass / Fail']],
    body: steps.length > 0
      ? steps.map((s, idx) => [
          idx + 1,
          s.description || '-',
          s.expected || '-',
          s.actual || '-',
          s.pass_fail || s.passFail || 'N/A'
        ])
      : [['-', 'No steps recorded', '-', '-', '-']],
    styles: { fontSize: 8, cellPadding: 2 },
    headStyles: { fillColor: [30, 41, 59] },
    columnStyles: { 0: { cellWidth: 15 }, 4: { cellWidth: 22 } }
  })

  let finalY = doc.lastAutoTable.finalY + 8

  // --- FEEDBACK (jika ada) ---
  if (item.feedbacks && item.feedbacks.length > 0) {
    doc.setFontSize(10)
    doc.setFont(undefined, 'bold')
    doc.text('Feedback', margin, finalY)
    doc.setFont(undefined, 'normal')
    doc.autoTable({
      startY: finalY + 3,
      head: [['User', 'Rating', 'Comment']],
      body: item.feedbacks.map(f => [
        f.user_name || f.user || 'Anonymous',
        `${f.rating || 5}/5`,
        f.comment || '-'
      ]),
      styles: { fontSize: 8, cellPadding: 2 },
      headStyles: { fillColor: [30, 41, 59] }
    })
  }

  doc.save(`${item.test_case_code || 'TestCase_' + item.id}.pdf`)
}

onMounted(async () => {
  await fetchFromAPI()

  try {
    const cycleRes = await fetch('https://localhost:7049/api/TestCycles')
    if (cycleRes.ok) {
      const data = await cycleRes.json()
      const uniqueCycles = data.filter((item, index, self) =>
        index === self.findIndex((t) => t.name === item.name)
      )
      
      testCycles.value = uniqueCycles
    }
  } 
  catch (err) {
    console.error('Gagal tarik Test Cycles dari API:', err)
    if (testCycles.value.length === 0) {
      testCycles.value = [
        { id: 23, name: 'v2.0.0 Pre-Launch UAT' },
        { id: 25, name: 'Sprint 25 Regression' },
        { id: 35, name: 'v1.5.0 Production' },
        { id: 38, name: 'v1.0.0 Production' }
      ]
    }
  }

  if (route.query.status) statusFilter.value = route.query.status
  if (route.query.priority) priorityFilter.value = route.query.priority
})
</script>

<style scoped>
.test-case-table th {
  font-size: 0.75rem !important;
  color: #64748b !important;
  text-transform: uppercase;
}

.border-b {
  border-bottom: 1px solid #e2e8f0;
}

.border-t {
  border-top: 1px solid #e2e8f0;
}
</style>