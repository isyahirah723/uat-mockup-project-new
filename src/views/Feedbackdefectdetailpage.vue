<template>
  <div class="defect-page">

    <!-- Header bar -->
    <div class="page-bar elevation-1">
      <div class="d-flex align-center" style="gap: 12px;">
        <v-btn icon variant="tonal" size="small" class="back-btn" title="Back to Feedback" @click="goBack">
          <v-icon size="20">mdi-arrow-left</v-icon>
        </v-btn>
        <div>
          <div class="page-title font-weight-bold">Defect Details</div>
          <div v-if="defect" class="page-crumb">
            <span class="crumb-code">{{ defect.test_case_code || '-' }}</span>
            <span class="crumb-sep">/</span>
            <span>{{ defect.title || defect.step_name }}</span>
          </div>
        </div>
      </div>
      <v-chip v-if="defect" :color="severityColor(defect.severity)" size="small" label class="font-weight-bold">
        {{ defect.severity || 'Unset' }}
      </v-chip>
    </div>

    <div class="page-body">

      <div v-if="loading" class="text-center py-12">
        <v-progress-circular indeterminate color="primary" size="48" width="4" />
      </div>

      <v-alert v-else-if="error" type="error" variant="tonal" density="compact" class="rounded-lg">
        {{ error }}
      </v-alert>

      <template v-else-if="defect">

        <!-- Hero -->
        <div class="hero mb-5">
          <div class="hero-title">{{ defect.test_case_code }} — {{ defect.step_name }}</div>
          <div class="hero-sub">
            Reported by {{ defect.reported_by || 'Unknown' }} · {{ formatDateTime(defect.dt_created) }}
          </div>
        </div>

        <!-- Overview -->
        <div class="card mb-5">
          <div class="card-title">Overview</div>
          <div class="info-grid">
            <div>
              <div class="info-label">Test Case ID</div>
              <div class="info-value mono">{{ defect.test_case_code || '-' }}</div>
            </div>
            <div>
              <div class="info-label">Title</div>
              <div class="info-value">{{ defect.title || '-' }}</div>
            </div>
            <div>
              <div class="info-label">Severity</div>
              <v-chip size="small" :color="severityColor(defect.severity)" label class="font-weight-bold">
                {{ defect.severity || 'Unset' }}
              </v-chip>
            </div>
            <div>
              <div class="info-label">Ticket</div>
              <div class="info-value mono">{{ defect.ticket_id || '-' }}</div>
            </div>
            <div>
              <div class="info-label">Reported By</div>
              <div class="info-value">{{ defect.reported_by || 'Unknown' }}</div>
            </div>
            <div>
              <div class="info-label">Date</div>
              <div class="info-value">{{ formatDateTime(defect.dt_created) }}</div>
            </div>
          </div>
        </div>

        <!-- Step -->
        <div class="card mb-5">
          <div class="card-title">Step</div>
          <div class="text-block">{{ defect.step_name || '-' }}</div>
        </div>

        <!-- Actual result -->
        <div class="card mb-5">
          <div class="card-title">Actual Result</div>
          <div class="text-block">{{ defect.actual_result || '-' }}</div>
        </div>

        <!-- Comments -->
        <div class="card mb-5">
          <div class="card-title">Comments</div>
          <div class="text-block">{{ defect.comments || '-' }}</div>
        </div>

        <!-- Attachments -->
        <div v-if="attachments.length" class="card">
          <div class="card-title">Attachments ({{ attachments.length }})</div>
          <div class="att-list">
            <template v-for="a in attachments" :key="a.id">
              <a v-if="a.is_image" :href="a.url" target="_blank" rel="noopener" class="att-thumb" :title="a.file_name">
                <img :src="a.url" :alt="a.file_name" />
              </a>
              <a v-else :href="a.url" target="_blank" rel="noopener" class="att-file">
                <v-icon size="14">mdi-paperclip</v-icon> {{ a.file_name }}
              </a>
            </template>
          </div>
        </div>

      </template>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { loadAttachmentMap } from '@/utils/runReportAttachments'

const API_BASE = 'https://localhost:7049/api'
const route = useRoute()
const router = useRouter()
const defectId = route.params.id

const loading = ref(true)
const error = ref('')
const defect = ref(null)
const attachments = ref([])

const goBack = () => router.push({ name: 'Feedback' })
const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')

const severityColor = (severity) => {
  switch (severity) {
    case 'Critical': return 'red'
    case 'High': return 'orange'
    case 'Medium': return 'amber'
    case 'Low': return 'blue-grey'
    default: return 'grey'
  }
}

onMounted(async () => {
  try {
   
    const res = await fetch(`${API_BASE}/ExecutionSteps/defects`)
    if (!res.ok) throw new Error('Failed to load defects')
    const all = await res.json()
    const found = all.find((d) => String(d.id) === String(defectId))
    if (!found) throw new Error('Defect not found')
    defect.value = found

    const key = String(found.execution_step_id ?? found.id)
    const map = Object.fromEntries(await loadAttachmentMap([key]))
    attachments.value = map[key] || []
  } catch (e) {
    console.error(e)
    error.value = e.message || 'Failed to load defect'
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.defect-page {
  background: rgba(var(--v-theme-on-surface), 0.05); min-height: 100vh; color: rgb(var(--v-theme-on-surface));
  font-family: 'Inter', 'Segoe UI', -apple-system, sans-serif;
}
.page-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 14px 28px; background: rgb(var(--v-theme-surface)); border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}
.page-title { font-size: 1.15rem; color: rgb(var(--v-theme-on-surface)); }
.page-crumb { font-size: 0.82rem; color: rgba(var(--v-theme-on-surface), 0.7); display: flex; gap: 6px; align-items: center; }
.crumb-code { font-weight: 700; color: var(--acc-purple, #760f6f); }
.crumb-sep { color: #cbd5e1; }
.back-btn { border-radius: 8px; }
.page-body { max-width: 960px; margin: 0 auto; padding: 24px 28px 48px; }

.hero {
  background: linear-gradient(135deg, #760f6f, #641f61);
  color: #fff; border-radius: 12px; padding: 18px 24px;
}
.hero-title { font-size: 1.05rem; font-weight: 700; line-height: 1.35; overflow-wrap: anywhere; }
.hero-sub { font-size: 0.8rem; opacity: 0.85; margin-top: 4px; }

.card {
  background: rgb(var(--v-theme-surface)); border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); border-radius: 12px; padding: 20px 24px;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
}
.card-title {
  font-size: 0.95rem; font-weight: 700; color: var(--acc-purple, #760f6f); margin-bottom: 14px;
  padding-bottom: 10px; border-bottom: 2px solid #0f766e;
}
.info-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(200px, 1fr)); gap: 16px 24px; }
.info-label { font-size: 0.68rem; font-weight: 700; letter-spacing: 0.05em; text-transform: uppercase; color: rgba(var(--v-theme-on-surface), 0.5); margin-bottom: 3px; }
.info-value { font-size: 0.88rem; font-weight: 600; color: rgb(var(--v-theme-on-surface)); overflow-wrap: anywhere; }
.mono { font-family: ui-monospace, 'Cascadia Mono', Consolas, monospace; }
.text-block { font-size: 0.92rem; line-height: 1.55; white-space: pre-wrap; overflow-wrap: anywhere; }

.att-list { display: flex; flex-wrap: wrap; gap: 10px; }
.att-thumb img { height: 140px; max-width: 260px; object-fit: cover; border-radius: 8px; border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); background: rgb(var(--v-theme-surface)); display: block; }
.att-file {
  display: inline-flex; align-items: center; gap: 4px; font-size: 0.8rem; font-weight: 600;
  color: var(--acc-purple, #760f6f); background: rgb(var(--v-theme-surface)); border: 1px solid rgba(168, 85, 247, 0.30); border-radius: 6px; padding: 5px 12px; text-decoration: none;
}
.att-file:hover { background: rgba(168, 85, 247, 0.10); }
</style>