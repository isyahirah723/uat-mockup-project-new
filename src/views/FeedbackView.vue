<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1000px;">
    <div class="mb-6">
      <div class="text-caption text-grey font-weight-bold">// FEEDBACK & EXECUTION</div>
      <div class="d-flex align-center justify-space-between">
        <div>
          <div class="text-h5 font-weight-bold">
            {{ currentTestCase ? currentTestCase.title : 'Test Case Not Found' }}
          </div>
          <div class="text-caption text-grey">
            Test Case ID: {{ currentTestCase?.testCaseId || route.params.id }}
          </div>
        </div>
        <v-btn color="grey-darken-1" variant="text" prepend-icon="mdi-arrow-left" @click="$router.push('/test-cases')">
          Back to Test Cases
        </v-btn>
      </div>
    </div>
    <v-card variant="flat" color="surface" class="rounded-xl pa-6 border">
      <div class="d-flex align-center justify-space-between mb-4">
        <div class="text-h6 font-weight-bold">Workflow Steps & Feedback</div>
        <v-btn color="primary" size="small" prepend-icon="mdi-plus" @click="addStep">
          Add Step
        </v-btn>
      </div>

      <v-form ref="uatForm" v-model="isFormValid" @submit.prevent="submitUAT">
        <div v-for="(step, index) in workflowSteps" :key="step.id" class="mb-6 pa-4 rounded-lg" style="border: 1px solid #e2e8f0;">
          
          <!-- STEP HEADER -->
          <div class="d-flex align-center justify-space-between mb-2">
            <div>
              <div class="text-overline text-primary font-weight-bold">
                Step {{ step.sequenceOrder }}: {{ step.requiredRole || 'Tester' }}
              </div>
              <div class="text-subtitle-1 font-weight-bold">{{ step.stepName || 'New Step' }}</div>
            </div>
            <div class="d-flex align-center">
              <v-chip :color="getStatusColor(step.executionStatus)" font-weight-bold class="text-uppercase mr-2">
                {{ step.executionStatus || 'Pending' }}
              </v-chip>
              <v-btn icon size="small" color="red" variant="text" @click="removeStep(index)" v-if="workflowSteps.length > 1">
                <v-icon size="small">mdi-delete</v-icon>
              </v-btn>
            </div>
          </div>

          <v-divider class="my-3"></v-divider>
          <div class="mb-4">
            <div class="text-subtitle-2 font-weight-bold mb-1">1. Workflow Decision</div>
            <v-radio-group
              v-model="step.executionStatus"
              inline
              :rules="[v => !!v || 'Please choose an action']"
              class="mt-0"
            >
              <v-radio label="Approve" value="Approve" color="success"></v-radio>
              <v-radio label="Need Info" value="Need Info" color="warning"></v-radio>
              <v-radio label="Reject" value="Reject" color="error"></v-radio>
            </v-radio-group>
          </div>
          <div>
            <div class="d-flex align-center justify-space-between mb-2">
              <div class="text-subtitle-2 font-weight-bold">2. Test Feedback / Defect</div>
              <v-switch
                v-model="step.hasDefect"
                label="Log Defect / Issue"
                color="error"
                hide-details
                density="compact"
                inset
              ></v-switch>
            </div>

            <v-expand-transition>
              <div v-if="step.hasDefect || step.executionStatus === 'Reject'">
                <v-row class="mt-2">
                  <v-col cols="12" md="6">
                    <v-select
                      v-model="step.feedback.severity"
                      :items="severityOptions"
                      label="Severity"
                      variant="outlined"
                      density="compact"
                      :rules="[v => !step.hasDefect || !!v || 'Severity required']"
                    ></v-select>
                  </v-col>
                  <v-col cols="12" md="6">
                    <v-text-field
                      v-model="step.feedback.ticketId"
                      label="Ticket ID (Optional)"
                      placeholder="cth: JIRA-1234"
                      variant="outlined"
                      density="compact"
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-textarea
                      v-model="step.feedback.actualResult"
                      label="Actual Result"
                      placeholder="Explain what happened during the test..."
                      variant="outlined"
                      rows="2"
                      density="compact"
                      :rules="[v => !step.hasDefect || !!v || 'Actual Result required']"
                    ></v-textarea>
                  </v-col>
                  <v-col cols="12">
                    <v-textarea
                      v-model="step.feedback.comments"
                      label="Comments / Additional Notes"
                      placeholder="Include additional information..."
                      variant="outlined"
                      rows="2"
                      density="compact"
                    ></v-textarea>
                  </v-col>
                  <v-col cols="12">
                    <v-file-input
                      v-model="step.feedback.attachments"
                      label="Attachments (Screenshots / Logs)"
                      variant="outlined"
                      density="compact"
                      prepend-icon="mdi-paperclip"
                      multiple
                      show-size
                    ></v-file-input>
                  </v-col>
                </v-row>
              </div>
            </v-expand-transition>
          </div>
          
        </div>
       
        <div class="d-flex justify-end gap-2 mt-6">
          <v-btn color="grey-lighten-1" variant="text" @click="resetForm">Reset</v-btn>
          <v-btn color="primary" type="submit" :disabled="!isFormValid" :loading="loading">
            
          </v-btn>
        </div>
      </v-form>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { testCaseService } from '@/services/testCaseService'
import { testFeedbackService } from '@/services/testFeedbackService'
import { executionStepService } from '@/services/executionStepService'
import { executionAttachmentService } from '@/services/executionAttachmentService'

const route = useRoute()
const router = useRouter()

const uatForm = ref(null)
const isFormValid = ref(false)
const loading = ref(false)
const pageLoading = ref(false)
const currentTestCase = ref(null)

const fetchTestCase = async () => {
  const id = route.params.id
  if (!id) return
  pageLoading.value = true
  try {
    const res = await testCaseService.getById(id)
    currentTestCase.value = res.data
  } catch (error) {
    console.error('Error fetching test case:', error)
    currentTestCase.value = null
  } finally {
    pageLoading.value = false
  }
}

const workflowSteps = ref([])
const initSteps = () => {
  if (currentTestCase.value && currentTestCase.value.steps && currentTestCase.value.steps.length > 0) {
    
    workflowSteps.value = currentTestCase.value.steps.map((s, index) => ({
      id: Date.now() + index,
      runId: currentTestCase.value.test_case_code || 'RUN-001',
      stepName: s.description || `Step ${index + 1}`,
      requiredRole: 'Tester',
      executionStatus: s.passFail === 'Pass' ? 'Approve' : s.passFail === 'Fail' ? 'Reject' : 'Pending',
      sequenceOrder: index + 1,
      hasDefect: s.passFail === 'Fail',
      feedback: {
        severity: 'minor',
        actualResult: s.actual || '',
        comments: '',
        attachments: [],
        ticketId: ''
      }
    }))
  } else {
    workflowSteps.value = [{
      id: Date.now(),
      runId: currentTestCase.value?.test_case_code || route.params.id || 'RUN-001',
      stepName: 'Default Execution Step',
      requiredRole: 'Tester',
      executionStatus: 'Pending',
      sequenceOrder: 1,
      hasDefect: false,
      feedback: {
        severity: 'minor',
        actualResult: '',
        comments: '',
        attachments: [],
        ticketId: ''
      }
    }]
  }
}

onMounted(async () => {
  await fetchTestCase()
  initSteps()
})

const severityOptions = [
  { title: 'Minor', value: 'minor' },
  { title: 'Major', value: 'major' },
  { title: 'Critical', value: 'critical' }
]
const addStep = () => {
  const newId = Date.now()
  workflowSteps.value.push({
    id: newId,
    runId: route.params.id || 'RUN-001',
    stepName: `Step ${workflowSteps.value.length + 1}`,
    requiredRole: '',
    executionStatus: 'Pending',
    sequenceOrder: workflowSteps.value.length + 1,
    hasDefect: false,
    feedback: {
      severity: 'minor',
      actualResult: '',
      comments: '',
      attachments: [],
      ticketId: ''
    }
  })
}

const removeStep = (index) => {
  if (workflowSteps.value.length > 1) {
    workflowSteps.value.splice(index, 1)
    workflowSteps.value.forEach((step, i) => {
      step.sequenceOrder = i + 1
    })
  }
}

const getStatusColor = (status) => {
  switch (status?.toLowerCase()) {
    case 'approve': return 'success'
    case 'reject': return 'error'
    case 'need info': return 'warning'
    default: return 'grey'
  }
}

const submitUAT = async () => {
  const { valid } = await uatForm.value.validate()
  if (!valid) return

  if (!currentTestCase.value) {
    alert('Test case not found - cannot submit.')
    return
  }

  loading.value = true

  
  const lastStep = workflowSteps.value[workflowSteps.value.length - 1]
  let finalStatus = 'Pending'
  if (lastStep) {
    finalStatus = lastStep.executionStatus === 'Approve' ? 'Passed' : lastStep.executionStatus === 'Reject' ? 'Failed' : 'Pending'
    const hasNeedInfo = workflowSteps.value.some(s => s.executionStatus === 'Need Info')
    if (hasNeedInfo) {
      finalStatus = 'Pending'
    }
  }
  currentTestCase.value.status = finalStatus

  try {
    await testCaseService.update(currentTestCase.value.id, currentTestCase.value)
  } catch (error) {
    console.error('Error updating test case status:', error)
    alert('Failed to update test case status.')
    loading.value = false
    return
  }

  const userName = 'QA Tester'

  const feedbackPayload = workflowSteps.value.map(step => {
    const rating = step.executionStatus === 'Approve' ? 5
      : step.executionStatus === 'Need Info' ? 3
      : step.executionStatus === 'Reject' ? 1
      : null

    const commentParts = []
    if (step.hasDefect || step.executionStatus === 'Reject') {
      if (step.feedback.severity) commentParts.push(`Severity: ${step.feedback.severity}`)
      if (step.feedback.ticketId) commentParts.push(`Ticket: ${step.feedback.ticketId}`)
      if (step.feedback.actualResult) commentParts.push(`Actual: ${step.feedback.actualResult}`)
      if (step.feedback.comments) commentParts.push(step.feedback.comments)
    }

    return {
      id: 0,
      user_name: userName,
      rating,
      comment: commentParts.length ? commentParts.join(' | ') : `Step "${step.stepName}" - ${step.executionStatus}`,
      created_at: new Date().toISOString()
    }
  })

  const executionStepsPayload = workflowSteps.value.map(step => ({
    id: 0,
    run_id: currentTestCase.value.test_case_code || null,
    step_name: step.stepName,
    required_role: step.requiredRole,
    execution_status: step.executionStatus,
    sequence_order: step.sequenceOrder,
    has_defect: step.hasDefect || step.executionStatus === 'Reject',
    severity: step.feedback.severity || null,
    ticket_id: step.feedback.ticketId || null,
    actual_result: step.feedback.actualResult || null,
    comments: step.feedback.comments || null,
    crt_user_id: userName
  }))

  try {
    const [, executionRes] = await Promise.all([
      testFeedbackService.saveBatch(currentTestCase.value.id, feedbackPayload),
      executionStepService.saveBatch(currentTestCase.value.id, executionStepsPayload)
    ])

   
    const savedSteps = executionRes.data?.steps || []
    const uploadPromises = []

    for (const step of workflowSteps.value) {
      const files = step.feedback.attachments
      if (!files || files.length === 0) continue

      const matchedStep = savedSteps.find(s => s.sequence_order === step.sequenceOrder)
      if (!matchedStep) continue

      for (const file of files) {
        uploadPromises.push(executionAttachmentService.upload(matchedStep.id, file))
      }
    }

    if (uploadPromises.length > 0) {
      await Promise.all(uploadPromises)
    }

    alert('UAT Submission Successful!')
    router.push('/test-cases')
  } catch (error) {
    console.error('Error submitting UAT:', error)
    alert('Failed to submit UAT feedback. Please try again.')
  } finally {
    loading.value = false
  }
}

const resetForm = () => {
  uatForm.value?.reset()
  workflowSteps.value.forEach(step => {
    step.executionStatus = 'Pending'
    step.hasDefect = false
  })
}
</script>