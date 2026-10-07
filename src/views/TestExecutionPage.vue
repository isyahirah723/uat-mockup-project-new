<template>
  <div class="execution-layout">
    <!-- Top Navigation / Header Bar -->
    <div class="exec-header-bar elevation-1">
      <div class="d-flex align-center" style="gap: 14px;">
        <v-btn icon variant="flat" size="small" class="back-btn-styled" @click="goBack" title="Back to Runs">
          <v-icon size="20" color="white">mdi-arrow-left</v-icon>
        </v-btn>
        <div>
          <div class="exec-page-title font-weight-bold">Test Execution Workspace</div>
          <div v-if="activeCase" class="exec-crumb">
            <span class="crumb-code font-weight-bold">{{ activeCase.test_case_code || ('#' + activeCase.id) }}</span>
            <span class="crumb-sep">/</span>
            <span class="text-slate-600">{{ activeCase.title }}</span>
          </div>
        </div>
      </div>
      <div class="d-flex align-center" style="gap: 12px;">
        <v-chip size="small" :color="getStatusColor(runStatusLabel)" variant="flat" class="font-weight-bold text-white px-4 py-2 shadow-sm">
          <v-icon start size="small">mdi-circle-medium</v-icon> {{ runStatusLabel }}
        </v-chip>
      </div>
    </div>

    <!-- Main Content Container -->
    <div class="exec-main-container">
      
      <!-- Info hero card -->
      <div class="info-hero-card mb-6 elevation-2 pa-4 px-6 rounded-xl">
        <div class="d-flex align-center justify-between flex-wrap" style="gap: 20px;">
          <div class="d-flex align-center" style="gap: 16px;">
            <div class="hero-icon-box">
              <v-icon color="white" size="24">mdi-clipboard-text-clock-outline</v-icon>
            </div>
            <div>
              <div class="text-caption text-indigo-darken-3 font-weight-bold uppercase-label">Test Run ID #{{ activeRun?.id || '...' }}</div>
              <div class="text-subtitle-1 font-weight-bold text-slate-900">{{ activeCase?.title || 'Loading test case...' }}</div>
            </div>
          </div>
          
          <div class="d-flex align-center flex-wrap" style="gap: 24px;">
            <div class="hero-meta-badge">
              <v-icon size="small" color="indigo" class="mr-1">mdi-account-outline</v-icon>
              <span>Tester: <b>{{ activeCase?.run_as_name || '-' }}</b></span>
            </div>
            <div class="hero-meta-badge">
              <v-icon size="small" color="indigo" class="mr-1">mdi-tag-outline</v-icon>
              <span>Dept: <b>{{ activeCase?.department || '-' }}</b></span>
            </div>
            <div class="hero-meta-badge">
              <v-icon size="small" color="indigo" class="mr-1">mdi-clock-outline</v-icon>
              <span>{{ formatDateTime(activeRun?.started_at) }}</span>
            </div>
          </div>
        </div>
      </div>

      <v-alert v-if="runError" type="error" variant="tonal" density="compact" closable class="mb-5 rounded-lg" @click:close="runError = ''">
        {{ runError }}
      </v-alert>

      <!-- If Run is Completed View -->
      <div v-if="runCompleted" class="completion-box elevation-3 text-center py-12 px-6 rounded-2xl bg-surface">
        <v-icon size="72" color="#10b981" class="mb-4">mdi-check-decagram</v-icon>
        <div class="text-h5 font-weight-bold text-slate-900 mb-2">Test Run Submitted Successfully!</div>
        <div class="text-body-1 text-grey-darken-1 mb-2">
          {{ activeCase?.test_case_code }} &mdash; {{ activeCase?.title }}
        </div>
        <div class="text-caption text-grey mb-6 d-flex align-center justify-center" style="gap: 8px;">
          Tester: <b>{{ activeCase?.run_as_name }}</b> &middot; Overall Result:
          <v-chip size="small" :color="getStatusColor(overallResult)" variant="flat" class="text-white font-weight-bold">
            {{ overallResult }}
          </v-chip>
        </div>
        <div class="d-flex justify-center" style="gap: 12px;">
          <v-btn color="#b91c1c" class="text-white rounded-lg font-weight-bold px-6 text-capitalize" elevation="0" prepend-icon="mdi-file-pdf-box" @click="generateReport">
            Download PDF Report
          </v-btn>
          <v-btn color="indigo-darken-4" class="text-white rounded-lg font-weight-bold px-6 text-capitalize" elevation="0" @click="goBack">
            Back to Runs
          </v-btn>
        </div>
      </div>

      <!-- Active Execution Steps View -->
      <template v-else>
        <div v-if="stepsLoading" class="text-center py-16 bg-surface rounded-xl elevation-1">
          <v-progress-circular indeterminate color="indigo-darken-3" size="48" width="4" />
          <div class="text-subtitle-2 text-grey-darken-1 mt-3">Loading test execution steps...</div>
        </div>

        <template v-else>
          <!-- Description & Instruction Alerts -->
          <div class="mb-5">
            <v-card v-if="activeCase?.test_description" flat class="pa-4 mb-3 rounded-xl description-card-modern">
              <div class="d-flex align-center" style="gap: 12px;">
                <v-icon color="indigo-darken-2" size="22">mdi-information</v-icon>
                <div class="text-body-2 text-indigo-darken-4 font-weight-medium">{{ activeCase.test_description }}</div>
              </div>
            </v-card>

            <v-card flat class="pa-4 rounded-xl instruction-card-modern">
              <div class="d-flex align-center" style="gap: 12px;">
                <v-icon color="teal-darken-3" size="22">mdi-format-list-numbered-rtl</v-icon>
                <div class="text-body-2 font-weight-medium text-teal-darken-4">
                  <b>Execution guide:</b> Complete one step at a time. Record Pass or Fail and submit it before the next step unlocks.
                </div>
              </div>
            </v-card>
          </div>

          <!-- Step Progress Timeline Indicator -->
          <div class="step-timeline-wrapper elevation-2 pa-5 mb-6 rounded-xl bg-surface">
            <div class="d-flex align-center justify-between mb-2 px-2">
              <span class="text-caption font-weight-bold text-slate-500 uppercase-label">Test Step Progress</span>
              <span class="text-caption font-weight-bold text-indigo-darken-3">Step {{ viewingStepIndex + 1 }} of {{ steps.length }}</span>
            </div>
            <div class="d-flex align-center justify-center flex-wrap" style="gap: 8px;">
              <template v-for="(step, idx) in steps" :key="'dot-' + step.test_step_id">
                <div
                  class="step-timeline-node d-flex align-center justify-center"
                  :class="{
                    'node-done': idx < currentStepIndex && step.execution_status !== 'Failed' && step.execution_status !== 'Blocked',
                    'node-fail': idx < currentStepIndex && step.execution_status === 'Failed',
                    'node-blocked': idx < currentStepIndex && step.execution_status === 'Blocked',
                    'node-current': idx === currentStepIndex,
                    'node-locked': idx > currentStepIndex,
                    'node-viewing': idx === viewingStepIndex,
                  }"
                  @click="idx <= currentStepIndex ? (viewingStepIndex = idx) : null"
                  :title="`Step ${idx + 1}: ${step.step_name}`"
                >
                  <v-icon v-if="idx < currentStepIndex && (step.execution_status === 'Failed' || step.execution_status === 'Blocked')" size="14" color="white">mdi-alert</v-icon>
                  <v-icon v-else-if="idx < currentStepIndex" size="14" color="white">mdi-check</v-icon>
                  <v-icon v-else-if="idx > currentStepIndex" size="12" color="grey">mdi-lock</v-icon>
                  <span v-else class="font-weight-bold">{{ idx + 1 }}</span>
                </div>
                <div v-if="idx < steps.length - 1" class="step-timeline-connector" :class="{ 'connector-done': idx < currentStepIndex }"></div>
              </template>
            </div>
          </div>

          <!-- Active Step Card -->
          <v-card v-if="activeStep" flat class="pa-7 rounded-2xl bg-surface elevation-3 border-subtle mb-6 active-step-box">
            <div class="d-flex align-center justify-space-between mb-4 pb-3 border-bottom-subtle">
              <div class="d-flex align-center" style="gap: 14px;">
                <div class="step-badge-num">{{ viewingStepIndex + 1 }}</div>
                <div>
                  <div class="text-h6 font-weight-bold text-slate-900">{{ activeStep.step_name }}</div>
                  <div class="text-caption text-grey-darken-1">Step {{ viewingStepIndex + 1 }} of {{ steps.length }}</div>
                </div>
              </div>

              <div>
                <v-chip v-if="activeStep.stage === 'pending'" size="small" color="grey-lighten-3" variant="flat" class="font-weight-bold text-slate-700 px-3">
                  <v-icon start size="small">mdi-clock-outline</v-icon> Not Executed
                </v-chip>
                <v-chip v-else-if="activeStep.stage === 'executing'" size="small" color="indigo" variant="flat" class="font-weight-bold text-white px-3">
                  <v-icon start size="small">mdi-play-circle-outline</v-icon> Executing
                </v-chip>
                <v-chip v-else size="small" :color="getExecutionStatusColor(activeStep.execution_status)" variant="flat" class="font-weight-bold text-white px-3">
                  {{ activeStep.execution_status }} &mdash; Recorded
                </v-chip>
                <v-chip v-if="activeStep.ticket_id" size="small" color="red-darken-2" variant="tonal" label class="font-weight-bold ml-2">
                  <v-icon start size="small">mdi-ticket-outline</v-icon>{{ activeStep.ticket_id }}
                </v-chip>
              </div>
            </div>

            <div class="expected-box pa-4 rounded-xl mb-6">
              <div class="text-caption font-weight-bold text-indigo-darken-3 mb-1 uppercase-label">Expected Result</div>
              <div class="text-body-2 text-slate-800 font-weight-medium">{{ activeStep.expected || 'No expected result defined.' }}</div>
            </div>

            <!-- Stage 1: Pending (Execute Button) -->
            <div v-if="activeStep.stage === 'pending'" class="d-flex justify-end">
              <v-btn color="indigo-darken-4" class="text-white font-weight-bold rounded-xl px-8 py-3 text-capitalize elevation-2" @click="executeStep(activeStep)">
                <v-icon size="small" class="mr-2">mdi-play</v-icon> Execute This Step
              </v-btn>
            </div>

            <!-- Stage 2: Executing (Select Result & Submit Step) -->
            <v-row v-else-if="activeStep.stage === 'executing'" density="comfortable">
              <v-col cols="12" sm="6">
                <v-select
                  v-model="activeStep.execution_status"
                  label="Result: Pass or Fail *"
                  :items="statusOptions"
                  variant="outlined"
                  density="comfortable"
                  hide-details
                  class="rounded-xl"
                ></v-select>
              </v-col>
              <v-col cols="12" class="d-flex align-center justify-end mt-2">
                <v-btn
                  color="indigo-darken-4"
                  class="text-white font-weight-bold rounded-xl px-8 py-3 text-capitalize elevation-2"
                  :disabled="!activeStep.execution_status"
                  :loading="activeStep.saving"
                  @click="submitStep(activeStep, viewingStepIndex)"
                >
                  Submit Step
                </v-btn>
              </v-col>
            </v-row>

            <!-- Stage 3: Done (Edit option) -->
            <div v-else class="d-flex align-center justify-space-between pt-2">
              <div class="text-caption text-grey-darken-1">
                Recorded result: <b :class="activeStep.execution_status === 'Passed' ? 'text-success' : 'text-error'">{{ activeStep.execution_status }}</b>
              </div>
              <v-btn variant="outlined" color="indigo-darken-3" size="small" class="font-weight-bold rounded-lg text-capitalize px-4" @click="editStep(activeStep)">
                <v-icon size="small" class="mr-1">mdi-pencil</v-icon> Edit Result
              </v-btn>
            </div>
          </v-card>

          <!-- Step Navigation Controls (Previous / Next) -->
          <div class="d-flex align-center justify-between mb-8">
            <v-btn variant="tonal" color="slate-darken-2" class="font-weight-bold text-capitalize px-5 py-2 rounded-xl" :disabled="viewingStepIndex === 0" @click="viewingStepIndex--">
              <v-icon size="small" class="mr-1">mdi-arrow-left</v-icon> Previous Step
            </v-btn>
            <v-btn variant="tonal" color="slate-darken-2" class="font-weight-bold text-capitalize px-5 py-2 rounded-xl" :disabled="viewingStepIndex >= currentStepIndex || viewingStepIndex >= steps.length - 1" @click="viewingStepIndex++">
              Next Step <v-icon size="small" class="ml-1">mdi-arrow-right</v-icon>
            </v-btn>
          </div>

          <!-- Overall Submission Section -->
          <template v-if="allStepsSaved">
            <v-divider class="my-8"></v-divider>

            <!-- Defect logging panel (inline cards, no dialog) -->
            <template v-if="defectRequiredSteps.length > 0">
              <div class="text-subtitle-1 font-weight-bold text-slate-900 mb-3">Defect Log &mdash; Failed / Blocked Steps</div>
              
              <div v-for="fs in defectRequiredSteps" :key="'defect-' + fs.test_step_id" class="mb-4">
                
                <!-- Shown while the defect form is open for this step -->
                <v-card v-if="activeDefectStepId === fs.test_step_id" flat class="pa-6 rounded-2xl bg-surface elevation-3 border-error mb-4">
                  <div class="d-flex align-center justify-between mb-4 pb-2 border-bottom-subtle">
                    <div class="d-flex align-center" style="gap: 10px;">
                      <v-icon color="error" size="24">mdi-alert-circle</v-icon>
                      <div>
                        <div class="text-subtitle-1 font-weight-bold text-slate-900">Log Defect: Step {{ steps.indexOf(fs) + 1 }}</div>
                        <div class="text-caption text-grey-darken-1">{{ fs.step_name }}</div>
                      </div>
                    </div>
                    <v-btn variant="text" size="small" color="grey" @click="activeDefectStepId = null">Close</v-btn>
                  </div>

                  <v-textarea v-model="defectForm.actual_result" label="Actual Result *" variant="outlined" density="comfortable" rows="2" class="mb-4 rounded-xl"></v-textarea>
                  <v-textarea v-model="defectForm.description" label="Defect Description *" variant="outlined" density="comfortable" rows="2" class="mb-4 rounded-xl"></v-textarea>
                  <v-textarea v-model="defectForm.affected_impact" label="Affected / Impact *" variant="outlined" density="comfortable" rows="2" class="mb-4 rounded-xl" hint="Which module, feature or business process is affected, and how" persistent-hint></v-textarea>
                  <v-select v-model="defectForm.severity" label="Severity" :items="['Low', 'Medium', 'High', 'Critical']" variant="outlined" density="comfortable" class="mb-4 rounded-xl"></v-select>
                  <v-text-field v-model="defectForm.ticket_id" label="Ticket ID" readonly placeholder="Auto-generated" persistent-placeholder hint="Assigned automatically when the step is marked Failed" persistent-hint variant="outlined" density="comfortable" prepend-inner-icon="mdi-ticket-outline" class="mb-4 rounded-xl"></v-text-field>
                  <v-file-input v-model="defectForm.attachment" multiple label="Upload Screenshot / Log" variant="outlined" density="comfortable" prepend-icon="" prepend-inner-icon="mdi-paperclip" class="mb-4 rounded-xl"></v-file-input>
                  <v-textarea v-model="defectForm.comments" label="Comment *" variant="outlined" density="comfortable" rows="2" class="rounded-xl mb-4"></v-textarea>

                  <div class="d-flex justify-end" style="gap: 10px;">
                    <v-btn variant="outlined" color="grey-darken-1" class="text-capitalize rounded-xl px-5" @click="activeDefectStepId = null">Cancel</v-btn>
                    <v-btn color="#b91c1c" class="text-white font-weight-bold rounded-xl px-6 text-capitalize elevation-2" :disabled="!defectFormValid" :loading="defectSaving" @click="saveDefect(fs)">
                      Save Defect
                    </v-btn>
                  </div>
                </v-card>

                <!-- Summary row shown when the defect form is closed -->
                <div v-else class="pa-4 rounded-xl bg-surface elevation-2 border-subtle d-flex align-center justify-space-between flex-wrap" style="gap: 12px;">
                  <div class="d-flex align-center" style="gap: 12px;">
                    <span class="text-h5">{{ fs.execution_status === 'Failed' ? '🔴' : '🟠' }}</span>
                    <div>
                      <div class="font-weight-bold text-slate-900">Step {{ steps.indexOf(fs) + 1 }}: {{ fs.step_name }}</div>
                      <div class="text-caption" :class="fs.has_defect ? 'text-success font-weight-bold' : 'text-error'">
                        {{ fs.has_defect ? '✓ Defect logged' : '⚠ No defect logged yet' }}
                      </div>
                      <div v-if="fs.ticket_id" class="text-caption font-weight-bold" style="color: var(--acc-red-deep, #b91c1c);">{{ fs.ticket_id }}</div>
                    </div>
                  </div>
                  <v-btn
                    size="small"
                    :color="fs.has_defect ? 'grey-darken-2' : '#b91c1c'"
                    :variant="fs.has_defect ? 'outlined' : 'flat'"
                    class="font-weight-bold rounded-lg text-capitalize text-white px-4"
                    elevation="0"
                    @click="openInlineDefect(fs)"
                  >
                    <v-icon size="small" class="mr-1">{{ fs.has_defect ? 'mdi-pencil' : 'mdi-alert-circle-outline' }}</v-icon>
                    {{ fs.has_defect ? 'Edit Defect' : 'Log Defect' }}
                  </v-btn>
                </div>

              </div>

              <div v-if="!allDefectsLogged" class="text-caption text-error font-weight-bold mt-2 mb-6">
                ⚠️ Log a defect for every failed or blocked step before you can submit this run.
              </div>
            </template>

            <!-- Final Feedback Card -->
            <v-card flat class="pa-7 rounded-2xl bg-surface elevation-3 border-subtle mb-6">
              <div class="text-subtitle-1 font-weight-bold text-slate-900 mb-4">Overall Feedback & Submission</div>
              <v-row density="comfortable">
                <v-col cols="12" sm="6">
                  <v-select
                    v-model="overallResult"
                    label="Overall Result *"
                    :items="['Passed', 'Failed']"
                    variant="outlined"
                    density="comfortable"
                    hide-details
                    class="rounded-xl mb-4"
                  ></v-select>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-select
                    v-model="feedbackRating"
                    label="Rating (1 to 5)"
                    :items="[1, 2, 3, 4, 5]"
                    variant="outlined"
                    density="comfortable"
                    hide-details
                    class="rounded-xl mb-4"
                  ></v-select>
                </v-col>
                <v-col cols="12">
                  <v-textarea
                    v-model="feedbackComment"
                    label="Overall Feedback"
                    variant="outlined"
                    density="comfortable"
                    rows="3"
                    hide-details
                    class="rounded-xl"
                  ></v-textarea>
                </v-col>
              </v-row>
            </v-card>

            <!-- Action Bar -->
            <div class="d-flex align-center justify-end" style="gap: 14px;">
              <v-btn variant="tonal" color="slate" class="font-weight-bold px-6 py-3 text-capitalize rounded-xl" @click="goBack">
                Cancel
              </v-btn>
              <v-btn
                color="indigo-darken-4"
                class="text-white font-weight-bold px-8 py-3 text-capitalize rounded-xl elevation-3"
                :disabled="!allDefectsLogged"
                :loading="submitting"
                @click="submitRun"
              >
                Submit Test Run
              </v-btn>
            </div>
          </template>
        </template>
      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { generateRunReportPdf } from '@/utils/runReportPdf'
import { withAttachments, embedImages } from '@/utils/runReportAttachments'

const API_BASE = 'https://localhost:7049/api'

const emit = defineEmits(['completed'])
const route = useRoute()
const router = useRouter()
const goBack = () => router.push({ name: 'TestCaseRuns', params: { id: route.params.id } })

const stepsLoading = ref(false)
const activeCase = ref(null)
const activeRun = ref(null)
const activeAssignmentId = ref(null)
const steps = ref([])
const viewingStepIndex = ref(0)
const submitting = ref(false)
const runError = ref('')
const runCompleted = ref(false)

const feedbackRating = ref(5)
const feedbackComment = ref('')
const overallResult = ref('Passed')

// Inline defect management (no pop-up dialog)
const activeDefectStepId = ref(null)
const defectSaving = ref(false)
const defectForm = ref({
  actual_result: '',
  description: '',
  affected_impact: '',
  severity: 'Medium',
  ticket_id: '',
  attachment: null,
  comments: '',
})

const defectFormValid = computed(() =>
  !!defectForm.value.actual_result?.trim() &&
  !!defectForm.value.description?.trim() &&
  !!defectForm.value.affected_impact?.trim() &&
  !!defectForm.value.comments?.trim()
)

const statusOptions = ['Passed', 'Failed', 'Blocked', 'N/A']

const getStatusColor = (status) => {
  switch (status) {
    case 'Passed': return 'success'
    case 'Failed': return 'error'
    case 'Blocked': return 'warning'
    case 'In Progress': return 'indigo-darken-2'
    default: return 'grey'
  }
}

const getExecutionStatusColor = (status) => {
  switch (status) {
    case 'Passed': return 'success'
    case 'Failed': return 'error'
    case 'Blocked': return 'warning'
    case 'N/A': return 'grey'
    default: return 'indigo'
  }
}

const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')

const parseJsonSafe = async (res) => {
  if (res.status === 204) return null
  const text = await res.text()
  if (!text) return null
  try { return JSON.parse(text) } catch { return null }
}

const allStepsSaved = computed(() =>
  steps.value.length > 0 && steps.value.every((s) => s.stage === 'done')
)

const currentStepIndex = computed(() => {
  const idx = steps.value.findIndex((s) => s.stage !== 'done')
  return idx === -1 ? steps.value.length : idx
})

const activeStep = computed(() => steps.value[viewingStepIndex.value])
const runStatusLabel = computed(() => activeRun.value?.run_status || 'In Progress')
const defectRequiredSteps = computed(() =>
  steps.value.filter((s) => s.execution_status === 'Failed' || s.execution_status === 'Blocked')
)
const allDefectsLogged = computed(() => defectRequiredSteps.value.every((s) => s.has_defect))

const derivedOverallStatus = computed(() => {
  if (steps.value.some((s) => s.execution_status === 'Failed')) return 'Failed'
  if (steps.value.some((s) => s.execution_status === 'Blocked')) return 'Blocked'
  return 'Passed'
})

watch(allStepsSaved, (val) => {
  if (val) overallResult.value = derivedOverallStatus.value
})

const ensureAssignment = async (testCase) => {
  try {
    const res = await fetch(`${API_BASE}/TestAssignments`)
    const all = res.ok ? await res.json() : []
    const existing = all.find((a) => a.test_case_id === testCase.id && a.user_id === testCase.run_as)

    if (existing) {
      if (testCase.department && existing.department !== testCase.department) {
        await fetch(`${API_BASE}/TestAssignments/${existing.id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ ...existing, department: testCase.department }),
        })
      }
      return existing.id
    }

    const createdRes = await fetch(`${API_BASE}/TestAssignments`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        test_case_id: testCase.id,
        user_id: testCase.run_as,
        department: testCase.department || null,
        status: 'Pending',
      }),
    })
    const created = await parseJsonSafe(createdRes)
    return created?.id || null
  } catch (err) {
    console.error('Failed to ensure test assignment:', err)
    return null
  }
}

const open = async (payload) => {
  activeCase.value = payload
  runCompleted.value = false
  stepsLoading.value = true
  runError.value = ''
  feedbackRating.value = 5
  feedbackComment.value = ''
  overallResult.value = 'Passed'

  try {
    const assignmentId = await ensureAssignment(payload)
    activeAssignmentId.value = assignmentId

    const runRes = await fetch(`${API_BASE}/TestRuns`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        test_case_id: payload.id,
        test_assignment_id: assignmentId,
        executed_by: payload.run_as,
        run_status: 'In Progress',
        started_at: new Date().toISOString(),
      }),
    })
    if (!runRes.ok) {
      const body = await runRes.text().catch(() => '')
      throw new Error(`Could not create test run (HTTP ${runRes.status}). ${body.slice(0, 300)}`)
    }
    activeRun.value = await parseJsonSafe(runRes)
    if (!activeRun.value?.id) throw new Error('Test run was created but the server returned no run id.')

    const stepsRes = await fetch(`${API_BASE}/TestSteps?testCaseId=${payload.id}`)
    const templateSteps = stepsRes.ok ? await stepsRes.json() : []

    steps.value = templateSteps
      .sort((a, b) => (a.step_order || 0) - (b.step_order || 0))
      .map((s) => ({
        test_step_id: s.id,
        step_name: s.description,
        sequence_order: s.step_order,
        expected: s.expected,
        stage: 'pending',
        execution_status: null,
        execution_step_id: null,
        ticket_id: null,
        saving: false,
        has_defect: false,
        defect: null,
      }))
    viewingStepIndex.value = 0
  } catch (err) {
    console.error('Failed to start test run:', err)
    runError.value = err.message || 'Failed to start test run.'
  } finally {
    stepsLoading.value = false
  }
}

onMounted(async () => {
  const id = route.params.id
  let tc = null
  try {
    const res = await fetch(`${API_BASE}/TestCases/${id}`)
    if (res.ok) tc = await res.json()
  } catch (err) {
    console.error('Failed to load test case:', err)
  }
  await open({
    id: tc?.id ?? Number(id),
    test_case_code: tc?.test_case_code,
    title: tc?.title,
    test_description: tc?.test_description,
    run_as: Number(route.query.run_as) || route.query.run_as,
    run_as_name: route.query.run_as_name,
    department: route.query.department || null,
  })
})

const executeStep = (step) => { step.stage = 'executing' }
const editStep = (step) => { step.stage = 'executing' }

const submitStep = async (step, idx) => {
  if (!step.execution_status) return
  step.saving = true
  runError.value = ''
  const wasHeadOfQueue = idx === currentStepIndex.value
  try {
    if (!activeRun.value?.id) throw new Error('No active test run.')
    const payload = {
      test_case_id: activeCase.value.id,
      run_id_fk: activeRun.value.id,
      step_name: step.step_name,
      sequence_order: step.sequence_order,
      execution_status: step.execution_status,
      actual_result: step.defect?.actual_result || null,
      has_defect: step.has_defect || false,
      severity: step.defect?.severity || null,
      ticket_id: step.defect?.ticket_id || step.ticket_id || null,
      comments: step.defect?.comments || null,
      required_role: step.required_role || null,
      crt_user_id: String(activeCase.value.run_as || ''),
    }

    let res
    if (step.execution_step_id) {
      res = await fetch(`${API_BASE}/ExecutionSteps/${step.execution_step_id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      })
    } else {
      res = await fetch(`${API_BASE}/ExecutionSteps`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      })
    }

    if (res.ok) {
      const saved = await parseJsonSafe(res)
      if (saved?.id) step.execution_step_id = saved.id
      // the API assigns DEF-001, DEF-002 ... as soon as a step is saved as Failed
      if (saved?.ticket_id) step.ticket_id = saved.ticket_id
      step.stage = 'done'

      if (activeRun.value?.id) {
        activeRun.value.run_status = 'In Progress'
        fetch(`${API_BASE}/TestRuns/${activeRun.value.id}`, {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ ...activeRun.value, run_status: 'In Progress' }),
        }).catch((e) => console.error('Failed to update run status:', e))
      }

      if (step.execution_status !== 'Failed' && step.execution_status !== 'Blocked') {
        step.has_defect = false
        step.defect = null
      }

      if (wasHeadOfQueue && typeof idx === 'number' && idx < steps.value.length - 1) {
        viewingStepIndex.value = idx + 1
      }
    } else {
      const body = await res.text().catch(() => '')
      runError.value = `Could not save step (HTTP ${res.status}). ${body.slice(0, 300)}`
    }
  } catch (err) {
    runError.value = err.message || 'Failed to save step.'
  } finally {
    step.saving = false
  }
}

const openInlineDefect = (step) => {
  activeDefectStepId.value = step.test_step_id
  defectForm.value = {
    actual_result: step.defect?.actual_result || '',
    description: step.defect?.description || '',
    affected_impact: step.defect?.affected_impact || '',
    severity: step.defect?.severity || 'Medium',
    ticket_id: step.defect?.ticket_id || step.ticket_id || '',
    attachment: null,
    comments: step.defect?.comments || '',
  }
}

const saveDefect = async (step) => {
  if (!step || !defectFormValid.value) return
  defectSaving.value = true
  runError.value = ''
  try {
    const defectRes = await fetch(`${API_BASE}/ExecutionSteps/${step.execution_step_id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        test_case_id: activeCase.value.id,
        run_id_fk: activeRun.value.id,
        step_name: step.step_name,
        sequence_order: step.sequence_order,
        execution_status: step.execution_status,
        actual_result: defectForm.value.actual_result,
        has_defect: true,
        description: defectForm.value.description,
        affected_impact: defectForm.value.affected_impact,
        severity: defectForm.value.severity,
        ticket_id: defectForm.value.ticket_id,
        comments: defectForm.value.comments,
        required_role: step.required_role || null,
        crt_user_id: String(activeCase.value.run_as || ''),
      }),
    })
    if (!defectRes.ok) {
      const body = await defectRes.text().catch(() => '')
      throw new Error(`Could not save defect (HTTP ${defectRes.status}). ${body.slice(0, 300)}`)
    }
    // the API assigns the ticket number (DEF-001, DEF-002 ...) the first time a defect is saved
    const savedStep = await parseJsonSafe(defectRes)
    const ticketId = savedStep?.ticket_id || defectForm.value.ticket_id || ''

    
    const att = defectForm.value.attachment
    const files = (Array.isArray(att) ? att : att ? [att] : []).filter((f) => f instanceof File)

    let uploadError = ''
    for (const file of files) {
      const formData = new FormData()
      formData.append('execution_step_id', step.execution_step_id)
      formData.append('file', file)
      const upRes = await fetch(`${API_BASE}/ExecutionAttachments/upload`, { method: 'POST', body: formData })
      if (!upRes.ok) {
        const body = await upRes.text().catch(() => '')
        uploadError = `Defect saved, but "${file.name}" was not uploaded (HTTP ${upRes.status}). ${body.slice(0, 200)}`
        break
      }
    }

    step.has_defect = true
    step.ticket_id = ticketId || step.ticket_id
    step.defect = {
      actual_result: defectForm.value.actual_result,
      description: defectForm.value.description,
      affected_impact: defectForm.value.affected_impact,
      severity: defectForm.value.severity,
      ticket_id: ticketId,
      comments: defectForm.value.comments,
      attachment: files.map((f) => f.name).join(', ') || step.defect?.attachment || null,
    }
    activeDefectStepId.value = null
    if (uploadError) runError.value = uploadError
  } catch (err) {
    runError.value = err.message || 'Failed to save defect.'
  } finally {
    defectSaving.value = false
  }
}

const submitRun = async () => {
  submitting.value = true
  runError.value = ''
  try {
    const completedAtIso = new Date().toISOString()
    const runPut = await fetch(`${API_BASE}/TestRuns/${activeRun.value.id}`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        ...activeRun.value,
        run_status: overallResult.value,
        completed_at: completedAtIso,
      }),
    })
    if (!runPut.ok) {
      const body = await runPut.text().catch(() => '')
      throw new Error(`Could not close test run (HTTP ${runPut.status}). ${body.slice(0, 300)}`)
    }
    const fbRes = await fetch(`${API_BASE}/TestFeedbacks`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        test_case_id: activeCase.value.id,
        run_id: activeRun.value.id,
        user_name: activeCase.value.run_as_name,
        rating: feedbackRating.value,
        comment: feedbackComment.value,
      }),
    })
    if (!fbRes.ok) {
      const body = await fbRes.text().catch(() => '')
      throw new Error(`Run closed, but feedback was not saved (HTTP ${fbRes.status}). ${body.slice(0, 300)}`)
    }

    if (activeAssignmentId.value) {
      await fetch(`${API_BASE}/TestAssignments/${activeAssignmentId.value}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status: 'Completed', department: activeCase.value.department }),
      })
    }

    if (activeRun.value) {
      activeRun.value.run_status = overallResult.value
      activeRun.value.completed_at = completedAtIso
    }
    runCompleted.value = true
    emit('completed', activeCase.value.id)
  } catch (err) {
    runError.value = err.message || 'Failed to submit test run.'
  } finally {
    submitting.value = false
  }
}


const generateReport = async () => {
  
  const stepsForPdf = await embedImages(await withAttachments(steps.value))
  generateRunReportPdf({
    testCase: activeCase.value,
    run: activeRun.value,
    testerName: activeCase.value?.run_as_name,
    department: activeCase.value?.department,
    steps: stepsForPdf,
    feedbackComment: feedbackComment.value,
  })
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');

.execution-layout {
  background: rgb(var(--v-theme-background));
  min-height: 100vh;
  font-family: 'Inter', sans-serif;
  color: rgb(var(--v-theme-on-surface));
}

.exec-header-bar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 32px;
  background: rgb(var(--v-theme-surface));
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.exec-page-title {
  font-size: 1.2rem;
  color: rgb(var(--v-theme-on-surface));
}

.exec-crumb {
  font-size: 0.85rem;
  color: rgba(var(--v-theme-on-surface), 0.7);
  display: flex;
  gap: 6px;
  align-items: center;
}

.crumb-code {
  color: var(--acc-indigo, #4f46e5);
}

.back-btn-styled {
  background: linear-gradient(135deg, #4f46e5, #3730a3);
  border-radius: 10px;
  box-shadow: 0 4px 10px rgba(79, 70, 229, 0.2);
}

.exec-main-container {
  max-width: 1040px;
  margin: 0 auto;
  padding: 32px 20px 60px;
}

.info-hero-card {
  background: linear-gradient(135deg, rgba(79, 70, 229, 0.16) 0%, rgba(168, 85, 247, 0.14) 100%);
  border: 1px solid rgba(79, 70, 229, 0.30);
  border-radius: 16px;
}

.hero-icon-box {
  width: 48px;
  height: 48px;
  background: #4f46e5;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 12px rgba(79, 70, 229, 0.3);
}

.hero-meta-badge {
  background: rgba(var(--v-theme-on-surface), 0.08);
  padding: 6px 12px;
  border-radius: 8px;
  font-size: 0.82rem;
  color: rgb(var(--v-theme-on-surface));
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  display: flex;
  align-items: center;
}

.description-card-modern {
  background: rgba(59, 130, 246, 0.10);
  border: 1px solid rgba(59, 130, 246, 0.30);
}

.instruction-card-modern {
  background: rgba(34, 197, 94, 0.10);
  border: 1px solid rgba(34, 197, 94, 0.30);
}

.step-timeline-wrapper {
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
  background: rgb(var(--v-theme-surface));
}

.step-timeline-node {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  font-size: 0.85rem;
  font-weight: 700;
  background-color: rgba(var(--v-theme-on-surface), 0.05);
  color: rgba(var(--v-theme-on-surface), 0.7);
  cursor: pointer;
  transition: all 0.2s ease;
  box-shadow: 0 2px 4px rgba(0,0,0,0.04);
}

.node-done {
  background-color: #10b981;
  color: #ffffff;
}

.node-fail {
  background-color: #ef4444;
  color: #ffffff;
}

.node-blocked {
  background-color: #f59e0b;
  color: #ffffff;
}

.node-current {
  background-color: #4f46e5;
  color: #ffffff;
  border: 2px solid #ffffff;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.3);
}

.node-locked {
  background-color: rgba(var(--v-theme-on-surface), 0.05);
  color: #cbd5e1;
  cursor: not-allowed;
}

.node-viewing {
  transform: scale(1.1);
}

.step-timeline-connector {
  width: 28px;
  height: 3px;
  background-color: rgba(148, 163, 184, 0.3);
  border-radius: 2px;
}

.connector-done {
  background-color: #10b981;
}

.border-subtle {
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.border-error {
  border: 2px solid #ef4444;
}

.border-bottom-subtle {
  border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.border-top-subtle {
  border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.step-badge-num {
  width: 40px;
  height: 40px;
  background: rgba(79, 70, 229, 0.16);
  color: var(--acc-indigo, #4f46e5);
  font-weight: 700;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 1.1rem;
}

.expected-box {
  background: rgba(var(--v-theme-on-surface), 0.05);
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.uppercase-label {
  letter-spacing: 0.05em;
  font-size: 0.7rem;
}

.completion-box {
  background: rgb(var(--v-theme-surface));
  border-radius: 20px;
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

/* ===== Dark mode: terangkan teks yang terlalu gelap ===== */
.v-theme--dark .exec-page-title {
  color: #ffffff;
}

.v-theme--dark .exec-crumb {
  color: rgba(255, 255, 255, 0.85);
}

.v-theme--dark .crumb-code {
  color: #a5b4fc;
}

.v-theme--dark .text-indigo-darken-3 {
  color: #a5b4fc !important;
}

.v-theme--dark .text-indigo-darken-4 {
  color: #e0e7ff !important;
}

.v-theme--dark .text-grey-darken-1,
.v-theme--dark .text-grey {
  color: rgba(255, 255, 255, 0.78) !important;
}

.v-theme--dark .text-slate-500 {
  color: #cbd5e1 !important;
}

.v-theme--dark .hero-meta-badge {
  background: rgba(255, 255, 255, 0.1);
  color: #ffffff;
  border-color: rgba(255, 255, 255, 0.25);
}

.v-theme--dark .description-card-modern,
.v-theme--dark .instruction-card-modern {
  color: #f1f5f9;
}

.v-theme--dark .node-locked {
  color: #94a3b8;
}

.v-theme--dark .step-badge-num {
  color: #c7d2fe;
  background: rgba(99, 102, 241, 0.3);
}
</style>