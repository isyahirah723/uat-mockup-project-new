<template>
  <div class="detail-page">

    <!-- Header bar -->
    <div class="page-bar elevation-1">
      <div class="d-flex align-center" style="gap: 12px;">
        <v-btn icon variant="tonal" size="small" class="back-btn" title="Back to Test Cases" @click="goBack">
          <v-icon size="20">mdi-arrow-left</v-icon>
        </v-btn>
        <div>
          <div class="page-title font-weight-bold">Test Case Details</div>
          <div v-if="item" class="page-crumb">
            <span class="crumb-code">{{ item.test_case_code || '#' + item.id }}</span>
            <span class="crumb-sep">/</span>
            <span>{{ item.title }}</span>
          </div>
        </div>
      </div>
      <div v-if="item" class="d-flex align-center" style="gap: 8px;">
        <v-btn
          color="red-darken-1" variant="tonal" size="small"
          class="text-capitalize font-weight-bold rounded-lg"
          :loading="approving" @click="approve(false)"
        >Not Approve</v-btn>
        <v-btn
          color="green-darken-1" variant="tonal" size="small"
          class="text-capitalize font-weight-bold rounded-lg"
          :loading="approving" @click="approve(true)"
        >Approve</v-btn>
      </div>
    </div>

    <div class="page-body">

      <div v-if="loading" class="text-center py-12">
        <v-progress-circular indeterminate color="primary" size="48" width="4" />
      </div>

      <v-alert v-else-if="error" type="error" variant="tonal" density="compact" class="rounded-lg">
        {{ error }}
      </v-alert>

      <template v-else-if="item">

        <!-- Overview -->
        <div class="card mb-5">
          <div class="card-title">Overview</div>
          <div class="case-title mb-4">{{ item.title }}</div>
          <div class="info-grid">
            <div>
              <div class="info-label">ID</div>
              <div class="info-value text-teal">{{ item.test_case_code || '#' + item.id }}</div>
            </div>
            <div>
              <div class="info-label">Department</div>
              <div class="info-value">{{ item.test_department || '-' }}</div>
            </div>
            <div>
              <div class="info-label">Version Tag</div>
              <div class="info-value">{{ item.version_tag || '-' }}</div>
            </div>
            <div>
              <div class="info-label">Status</div>
              <v-chip size="x-small" variant="flat" :color="statusColor(item.status)" label class="font-weight-bold text-white">
                {{ item.status || 'Draft' }}
              </v-chip>
            </div>
            <div>
              <div class="info-label">Priority</div>
              <v-chip size="x-small" variant="flat" :color="priorityColor(item.priority)" label class="font-weight-bold text-white">
                {{ item.priority || 'Medium' }}
              </v-chip>
            </div>
            <div>
              <div class="info-label mb-1">Assigned Testers</div>
              <div v-if="assigned.length" class="d-flex flex-wrap" style="gap: 4px;">
                <v-chip v-for="a in assigned" :key="a.id" size="small" variant="tonal" color="teal-darken-3" class="font-weight-medium">
                  <v-icon start size="12">mdi-account</v-icon>
                  {{ userNameById[a.user_id] || a.user_name || ('User #' + a.user_id) }}
                </v-chip>
              </div>
              <span v-else class="text-caption text-grey">Unassigned</span>
            </div>
          </div>
        </div>

        <!-- Description -->
        <div class="card mb-5">
          <div class="card-title">Description</div>
          <div class="desc-text">{{ item.test_description || 'No description' }}</div>
        </div>

        <!-- Steps -->
        <div class="card mb-5">
          <div class="card-title">Test Steps ({{ steps.length }})</div>
          <div v-if="steps.length">
            <div v-for="(step, idx) in steps" :key="idx" class="step-box">
              <div class="d-flex align-start justify-space-between" style="gap: 12px;">
                <div class="step-text"><span class="step-num">#{{ idx + 1 }}</span> {{ step.description }}</div>
                <v-chip size="x-small" variant="flat" class="text-white" :color="stepColor(step)">
                  {{ step.pass_fail || step.passFail || 'N/A' }}
                </v-chip>
              </div>
              <div class="step-sub mt-1">Expected: {{ step.expected || '-' }}</div>
              <div v-if="step.actual" class="step-sub">Actual: {{ step.actual }}</div>
            </div>
          </div>
          <div v-else class="text-grey text-caption">No steps provided.</div>
        </div>

        <!-- Feedback -->
        <div class="card mb-5">
          <div class="card-title">Feedback ({{ feedbacks.length }})</div>
          <div v-if="feedbacks.length">
            <div v-for="(fb, idx) in feedbacks" :key="idx" class="step-box">
              <div class="d-flex align-center justify-space-between">
                <span class="font-weight-bold text-caption">{{ fb.user_name || fb.user || 'Anonymous' }}</span>
                <span class="text-amber-darken-2 font-weight-bold text-caption">Rating: {{ fb.rating || 5 }}/5</span>
              </div>
              <div class="text-body-2 mt-1">{{ fb.comment }}</div>
            </div>
          </div>
          <div v-else class="text-grey text-caption">No feedback</div>
        </div>

        <!-- Approval -->
        <div class="card">
          <div class="card-title">Admin Approval</div>
          <div class="d-flex align-center" style="gap: 12px;">
            <v-chip size="small" variant="flat" :color="approvalColor(item.approval_status)" label class="font-weight-bold text-white">
              {{ item.approval_status || 'Pending Review' }}
            </v-chip>
            <span v-if="item.approved_at" class="text-caption text-grey">decided {{ formatDateTime(item.approved_at) }}</span>
          </div>
        </div>

      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const API = 'https://localhost:7049/api'
const route = useRoute()
const router = useRouter()
const testCaseId = route.params.id

const loading = ref(true)
const error = ref('')
const item = ref(null)
const users = ref([])
const assignments = ref([])
const approving = ref(false)

const goBack = () => router.push({ path: '/test-cases' })

const userNameById = computed(() => {
  const map = {}
  users.value.forEach((u) => { map[u.id] = u.full_name })
  return map
})
const assigned = computed(() => assignments.value.filter((a) => String(a.test_case_id) === String(testCaseId)))
const steps = computed(() => item.value?.steps || [])
const feedbacks = computed(() => item.value?.feedbacks || [])

const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')

const priorityColor = (p) => {
  if (p === 'Critical') return '#e11d48'
  if (p === 'High') return '#ea580c'
  if (p === 'Medium') return '#0284c7'
  return '#64748b'
}
const statusColor = (s) => {
  if (s === 'Passed') return '#16a34a'
  if (s === 'Failed') return '#dc2626'
  if (s === 'Pending') return '#d97706'
  return '#475569'
}
const approvalColor = (s) => {
  if (s === 'Approved') return '#059669'
  if (s === 'Not Approved') return '#dc2626'
  return '#64748b'
}
const stepColor = (s) => {
  const v = s.pass_fail || s.passFail
  if (v === 'Pass') return 'green'
  if (v === 'Fail') return 'red'
  return 'grey'
}

const approve = async (approved) => {
  if (!item.value) return
  approving.value = true
  try {
    const adminId = Number(localStorage.getItem('uat_user_id')) || null
    const res = await fetch(`${API}/TestCases/${item.value.id}/approve`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ approved, approvedBy: adminId }),
    })
    if (res.ok) {
      const result = await res.json()
      item.value = { ...item.value, approval_status: result.approval_status, approved_at: result.approved_at }
    } else {
      alert('Failed to update approval status.')
    }
  } catch (e) {
    console.error('Error updating approval:', e)
    alert('Error: Cannot connect to the API.')
  } finally {
    approving.value = false
  }
}

onMounted(async () => {
  try {
    // Same list endpoint the Test Cases page uses (it includes steps + feedbacks)
    const [casesRes, usersRes, assignRes] = await Promise.all([
      fetch(`${API}/TestCases`),
      fetch(`${API}/Users`),
      fetch(`${API}/TestAssignments`),
    ])
    if (!casesRes.ok) throw new Error('Failed to load test cases')
    const all = await casesRes.json()
    const found = all.find((t) => String(t.id) === String(testCaseId))
    if (!found) throw new Error('Test case not found')
    item.value = found
    users.value = usersRes.ok ? await usersRes.json() : []
    assignments.value = assignRes.ok ? await assignRes.json() : []
  } catch (e) {
    console.error(e)
    error.value = e.message || 'Failed to load test case'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.detail-page {
  background: rgba(var(--v-theme-on-surface), 0.05); min-height: 100vh; color: rgb(var(--v-theme-on-surface));
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
.case-title { font-size: 1.1rem; font-weight: 700; color: var(--acc-teal-deep, #134e4a); overflow-wrap: anywhere; }
.info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px 24px; }
.info-label { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.05em; text-transform: uppercase; color: rgba(var(--v-theme-on-surface), 0.5); margin-bottom: 3px; }
.info-value { font-size: 0.88rem; font-weight: 600; color: rgb(var(--v-theme-on-surface)); overflow-wrap: anywhere; }
.desc-text { font-size: 0.9rem; line-height: 1.5; white-space: pre-wrap; overflow-wrap: anywhere; }

.step-box {
  padding: 12px 14px; margin-bottom: 8px; border-radius: 10px;
  background: rgba(15, 118, 110, 0.1); border: 1px solid rgba(15, 118, 110, 0.3);
}
.step-box:last-child { margin-bottom: 0; }
.step-text { font-size: 0.9rem; overflow-wrap: anywhere; }
.step-num { font-weight: 700; color: var(--acc-teal-deep, #134e4a); margin-right: 4px; }
.step-sub { font-size: 0.78rem; color: rgba(var(--v-theme-on-surface), 0.7); overflow-wrap: anywhere; }
</style>