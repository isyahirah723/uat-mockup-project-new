<template>
  <v-container class="pa-6 mx-auto lab-page" style="max-width: 1280px;">
    <v-row class="mb-4" align="center">
      <v-col cols="12">
        <div class="d-flex" style="gap: 12px;">
          <div class="accent-bar"></div>
          <div>
            <div class="text-h5 page-heading d-flex align-center">
              <v-icon color="teal-darken-2" class="mr-2">mdi-comment-text-multiple-outline</v-icon>Feedback
            </div>
            <div class="page-subheading">Defects and observations logged across every test run</div>
            <div v-if="apiLoading" class="text-caption" style="color: #0f766e;">Loading data from the server</div>
          </div>
        </div>
      </v-col>
    </v-row>

    <v-tabs v-model="activeTab" color="#0f766e" class="mb-4">
      <v-tab value="defects">Defects ({{ defects.length }})</v-tab>
      <v-tab value="overall">Overall Feedback ({{ feedbacks.length }})</v-tab>
    </v-tabs>

    <v-window v-model="activeTab">
      
      <v-window-item value="defects">
        <v-row class="mb-4" align="center">
          <v-col cols="12" md="7" class="d-flex flex-wrap" style="gap: 8px;">
            <v-chip
              v-for="s in severityOptions"
              :key="s"
              size="small"
              :color="getSeverityColor(s)"
              :variant="activeSeverity === s ? 'flat' : 'tonal'"
              label
              class="font-weight-bold"
              style="cursor: pointer;"
              @click="activeSeverity = activeSeverity === s ? null : s"
            >
              {{ s }} · {{ countBySeverity(s) }}
            </v-chip>
          </v-col>
          <v-col cols="12" md="5">
            <v-text-field
              v-model="defectSearch"
              label="Search test case, step, or ticket ID"
              variant="outlined"
              density="compact"
              color="teal-darken-2"
              prepend-inner-icon="mdi-magnify"
              hide-details
              clearable
            ></v-text-field>
          </v-col>
        </v-row>

        <v-card variant="outlined" class="rounded-lg pa-0 overflow-hidden themed-card">
          <v-table hover class="defects-table">
            <thead>
              <tr class="table-header-row">
                <th class="font-weight-bold" style="width: 130px;">ID</th>
                <th class="font-weight-bold" style="width: 190px;">Title</th>
                <th class="font-weight-bold">Step & Actual Result</th>
                <th class="font-weight-bold">Severity</th>
                <th class="font-weight-bold">Ticket</th>
                <th class="font-weight-bold">Reported By</th>
                <th class="font-weight-bold">Date</th>
                <th class="font-weight-bold" style="width: 64px;"></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in pagedDefects" :key="d.id">
                <td class="py-3">
                  <span class="id-text mono">{{ d.test_case_code || '-' }}</span>
                </td>
                <td class="py-3">
                  <div class="text-body-2 font-weight-bold">{{ d.title || '-' }}</div>
                </td>
                <td class="py-3">
                  <div class="text-body-2">{{ d.step_name || '-' }}</div>
                  <div class="text-caption text-red-darken-2 mt-1" v-if="d.actual_result">
                    <strong>Actual:</strong> {{ d.actual_result }}
                  </div>
                  <div v-if="attachmentCount(d)" class="mt-1">
                    <v-chip size="x-small" variant="tonal" color="purple" label prepend-icon="mdi-paperclip">
                      {{ attachmentCount(d) }} attachment{{ attachmentCount(d) === 1 ? '' : 's' }}
                    </v-chip>
                  </div>
                </td>
                <td>
                  <v-chip size="small" :color="getSeverityColor(d.severity)" label class="font-weight-bold">
                    {{ d.severity || 'Unset' }}
                  </v-chip>
                </td>
                <td><span class="mono text-body-2">{{ d.ticket_id || '-' }}</span></td>
                <td><span class="text-body-2">{{ d.reported_by || 'Unknown' }}</span></td>
                <td><span class="text-caption text-grey">{{ formatDateTime(d.dt_created) }}</span></td>
                <td>
                  <v-btn icon variant="tonal" size="x-small" color="teal-darken-2" title="View details" @click="openDefectDetail(d)">
                    <v-icon size="small">mdi-eye-outline</v-icon>
                  </v-btn>
                </td>
              </tr>

              <tr v-if="filteredDefects.length === 0 && !apiLoading">
                <td colspan="8" class="text-center text-grey py-8">
                  {{ defects.length === 0 ? 'No defects logged yet.' : 'No defects match your filter.' }}
                </td>
              </tr>
            </tbody>
          </v-table>

          <div class="table-footer" v-if="filteredDefects.length > 0">
            <div class="d-flex align-center footer-rows">
              <span class="text-caption text-grey-darken-1 mr-2">Rows per page</span>
              <v-select
                v-model="defectPageSize"
                :items="defectPageSizes"
                variant="outlined"
                density="compact"
                hide-details
                color="teal-darken-2"
                class="page-size-select"
              ></v-select>
            </div>
            <span class="text-caption text-grey-darken-1 footer-range">{{ defectRange }}</span>
            <v-pagination
              v-model="defectPage"
              :length="defectPageCount"
              :total-visible="5"
              density="comfortable"
              size="small"
              rounded="lg"
              active-color="teal-darken-2"
            ></v-pagination>
          </div>
        </v-card>
      </v-window-item>

      <!-- ===================== OVERALL FEEDBACK TAB (Card Layout) ===================== -->
      <v-window-item value="overall">
        <v-row class="mb-4" align="center">
          <v-col cols="12" md="7" class="d-flex flex-wrap" style="gap: 8px;">
            <v-chip
              v-for="r in [5, 4, 3, 2, 1]"
              :key="r"
              size="small"
              :color="activeRating === r ? 'amber' : 'grey'"
              :variant="activeRating === r ? 'flat' : 'tonal'"
              label
              class="font-weight-bold"
              style="cursor: pointer;"
              @click="activeRating = activeRating === r ? null : r"
            >
              {{ r }}★ · {{ countByRating(r) }}
            </v-chip>
          </v-col>
          <v-col cols="12" md="5">
            <v-text-field
              v-model="feedbackSearch"
              label="Search test case or comment"
              variant="outlined"
              density="compact"
              color="teal-darken-2"
              prepend-inner-icon="mdi-magnify"
              hide-details
              clearable
            ></v-text-field>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12" md="6" lg="4" v-for="f in pagedFeedbacks" :key="f.id">
            <v-card variant="outlined" class="rounded-lg pa-4 h-100 d-flex flex-column justify-space-between themed-card feedback-card">
              <div>
                <div class="d-flex justify-space-between align-center mb-2">
                  <span class="id-text mono">{{ f.test_case_code || '-' }}</span>
                  <v-chip size="small" color="amber" variant="tonal" label class="font-weight-bold">
                    {{ f.rating != null ? f.rating + '★' : '-' }}
                  </v-chip>
                </div>
                <div class="text-subtitle-2 font-weight-bold mb-2">{{ f.test_case_title || '-' }}</div>
                <p class="text-body-2 text-grey-darken-3 mb-4" style="white-space: normal;">
                  "{{ f.comment || 'No comments provided.' }}"
                </p>
              </div>
              <div class="d-flex justify-space-between align-center text-caption text-grey pt-3 border-top">
                <span>Oleh: <strong class="text-purple-darken-3">{{ f.user_name || 'Unknown' }}</strong></span>
                <span>{{ formatDateTime(f.created_at) }}</span>
              </div>
            </v-card>
          </v-col>

          <v-col cols="12" v-if="filteredFeedbacks.length === 0 && !apiLoading">
            <v-card variant="outlined" class="text-center text-grey py-8 rounded-lg">
              {{ feedbacks.length === 0 ? 'No overall feedback submitted yet.' : 'No feedback matches your filter.' }}
            </v-card>
          </v-col>
        </v-row>

        <v-card variant="outlined" class="rounded-lg overflow-hidden themed-card mt-4" v-if="filteredFeedbacks.length > 0">
          <div class="table-footer">
            <div class="d-flex align-center footer-rows">
              <span class="text-caption text-grey-darken-1 mr-2">Cards per page</span>
              <v-select
                v-model="feedbackPageSize"
                :items="feedbackPageSizes"
                variant="outlined"
                density="compact"
                hide-details
                color="teal-darken-2"
                class="page-size-select"
              ></v-select>
            </div>
            <span class="text-caption text-grey-darken-1 footer-range">{{ feedbackRange }}</span>
            <v-pagination
              v-model="feedbackPage"
              :length="feedbackPageCount"
              :total-visible="5"
              density="comfortable"
              size="small"
              rounded="lg"
              active-color="teal-darken-2"
            ></v-pagination>
          </div>
        </v-card>
      </v-window-item>
    </v-window>

    <!-- ===================== DEFECT DETAIL DIALOG ===================== -->
    <v-dialog v-model="detailDialog" max-width="560">
      <v-card class="rounded-lg" v-if="selectedDefect">
        <v-card-title class="pa-4 text-white dialog-header text-wrap">
          <div class="d-flex align-start justify-space-between" style="gap: 12px;">
            <div style="min-width: 0;">
              <div class="text-subtitle-1 font-weight-bold" style="white-space: normal; overflow-wrap: anywhere; line-height: 1.35;">{{ selectedDefect.test_case_code }} — {{ selectedDefect.step_name }}</div>
              <div class="text-caption" style="opacity: 0.8;">
                Reported by {{ selectedDefect.reported_by || 'Unknown' }} · {{ formatDateTime(selectedDefect.dt_created) }}
              </div>
            </div>
            <v-btn icon variant="text" size="small" color="white" class="flex-shrink-0" @click="detailDialog = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>
        </v-card-title>

        <v-card-text class="pa-4">
          <div class="d-flex mb-4" style="gap: 8px;">
            <v-chip size="small" :color="getSeverityColor(selectedDefect.severity)" label class="font-weight-bold">
              {{ selectedDefect.severity || 'Unset' }}
            </v-chip>
            <v-chip v-if="selectedDefect.ticket_id" size="small" variant="tonal" color="grey" label>
              Ticket: {{ selectedDefect.ticket_id }}
            </v-chip>
          </div>

          <div class="text-caption font-weight-bold mb-1" style="color: #760f6f;">ACTUAL RESULT</div>
          <div class="mb-4">{{ selectedDefect.actual_result || '-' }}</div>

          <div class="text-caption font-weight-bold mb-1" style="color: #760f6f;">COMMENTS</div>
          <div>{{ selectedDefect.comments || '-' }}</div>

          <template v-if="selectedAttachments.length">
            <div class="text-caption font-weight-bold mb-2 mt-4" style="color: #760f6f;">
              ATTACHMENTS ({{ selectedAttachments.length }})
            </div>
            <div class="att-list">
              <template v-for="a in selectedAttachments" :key="a.id">
                <a v-if="a.is_image" :href="a.url" target="_blank" rel="noopener" class="att-thumb" :title="a.file_name">
                  <img :src="a.url" :alt="a.file_name" />
                </a>
                <a v-else :href="a.url" target="_blank" rel="noopener" class="att-file">
                  <v-icon size="14">mdi-paperclip</v-icon> {{ a.file_name }}
                </a>
              </template>
            </div>
          </template>
        </v-card-text>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, computed, watch, onMounted } from 'vue'
import { loadAttachmentMap } from '@/utils/runReportAttachments'

const API_BASE = 'https://localhost:7049/api'

const apiLoading = ref(false)
const activeTab = ref('defects')


const defects = ref([])
const attachmentsByStep = ref({})
const stepKey = (d) => String(d.execution_step_id ?? d.id)
const attachmentsFor = (d) => attachmentsByStep.value[stepKey(d)] || []
const attachmentCount = (d) => attachmentsFor(d).length
const defectSearch = ref('')
const activeSeverity = ref(null)
const severityOptions = ['Low', 'Medium', 'High', 'Critical']

const getSeverityColor = (severity) => {
  switch (severity) {
    case 'Critical': return 'red'
    case 'High': return 'orange'
    case 'Medium': return 'amber'
    case 'Low': return 'blue-grey'
    default: return 'grey'
  }
}

const countBySeverity = (severity) =>
  defects.value.filter((d) => d.severity === severity).length

const filteredDefects = computed(() => {
  let list = defects.value

  if (activeSeverity.value) {
    list = list.filter((d) => d.severity === activeSeverity.value)
  }

  if (defectSearch.value && defectSearch.value.trim()) {
    const q = defectSearch.value.trim().toLowerCase()
    list = list.filter((d) =>
      (d.test_case_code || '').toLowerCase().includes(q) ||
      (d.title || '').toLowerCase().includes(q) ||
      (d.step_name || '').toLowerCase().includes(q) ||
      (d.ticket_id || '').toLowerCase().includes(q) ||
      (d.reported_by || '').toLowerCase().includes(q) ||
      (d.actual_result || '').toLowerCase().includes(q)
    )
  }

  return list
})

// ---- Defects pagination ----
const defectPage = ref(1)
const defectPageSize = ref(10)
const defectPageSizes = [5, 10, 25, 50]
const defectPageCount = computed(() =>
  Math.max(1, Math.ceil(filteredDefects.value.length / defectPageSize.value))
)
const pagedDefects = computed(() => {
  const start = (defectPage.value - 1) * defectPageSize.value
  return filteredDefects.value.slice(start, start + defectPageSize.value)
})
const defectRange = computed(() => {
  const total = filteredDefects.value.length
  if (!total) return '0 of 0'
  const start = (defectPage.value - 1) * defectPageSize.value + 1
  const end = Math.min(defectPage.value * defectPageSize.value, total)
  return `${start}–${end} of ${total}`
})
watch([defectSearch, activeSeverity, defectPageSize], () => { defectPage.value = 1 })
watch(defectPageCount, (n) => { if (defectPage.value > n) defectPage.value = n })

const fetchDefects = async () => {
  try {
    const res = await fetch(`${API_BASE}/ExecutionSteps/defects`)
    defects.value = res.ok ? await res.json() : []
  } catch (err) {
    console.error('Gagal tarik senarai defect:', err)
    defects.value = []
  }
 
  attachmentsByStep.value = Object.fromEntries(await loadAttachmentMap(defects.value.map(stepKey)))
}

const detailDialog = ref(false)
const selectedDefect = ref(null)
const selectedAttachments = computed(() => (selectedDefect.value ? attachmentsFor(selectedDefect.value) : []))
const openDefectDetail = (defect) => {
  selectedDefect.value = defect
  detailDialog.value = true
}


const feedbacks = ref([])
const feedbackSearch = ref('')
const activeRating = ref(null)

const countByRating = (rating) =>
  feedbacks.value.filter((f) => f.rating === rating).length

const filteredFeedbacks = computed(() => {
  let list = feedbacks.value

  if (activeRating.value) {
    list = list.filter((f) => f.rating === activeRating.value)
  }

  if (feedbackSearch.value && feedbackSearch.value.trim()) {
    const q = feedbackSearch.value.trim().toLowerCase()
    list = list.filter((f) =>
      (f.test_case_code || '').toLowerCase().includes(q) ||
      (f.test_case_title || '').toLowerCase().includes(q) ||
      (f.comment || '').toLowerCase().includes(q)
    )
  }

  return list
})

// ---- Overall feedback pagination ----
const feedbackPage = ref(1)
const feedbackPageSize = ref(9)
const feedbackPageSizes = [6, 9, 12, 24]
const feedbackPageCount = computed(() =>
  Math.max(1, Math.ceil(filteredFeedbacks.value.length / feedbackPageSize.value))
)
const pagedFeedbacks = computed(() => {
  const start = (feedbackPage.value - 1) * feedbackPageSize.value
  return filteredFeedbacks.value.slice(start, start + feedbackPageSize.value)
})
const feedbackRange = computed(() => {
  const total = filteredFeedbacks.value.length
  if (!total) return '0 of 0'
  const start = (feedbackPage.value - 1) * feedbackPageSize.value + 1
  const end = Math.min(feedbackPage.value * feedbackPageSize.value, total)
  return `${start}–${end} of ${total}`
})
watch([feedbackSearch, activeRating, feedbackPageSize], () => { feedbackPage.value = 1 })
watch(feedbackPageCount, (n) => { if (feedbackPage.value > n) feedbackPage.value = n })

const fetchFeedbacks = async () => {
  try {
    const res = await fetch(`${API_BASE}/TestFeedbacks`)
    feedbacks.value = res.ok ? await res.json() : []
  } catch (err) {
    console.error('Failed to retrieve overall feedback:', err)
    feedbacks.value = []
  }
}

const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')

onMounted(async () => {
  apiLoading.value = true
  await Promise.all([fetchDefects(), fetchFeedbacks()])
  apiLoading.value = false
})
</script>

<style scoped>
.table-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  flex-wrap: wrap;
  gap: 8px 20px;
  padding: 8px 16px;
  border-top: 1px solid rgba(118, 15, 111, 0.12);
  background: rgba(118, 15, 111, 0.03);
}
.footer-rows { flex-shrink: 0; }
.page-size-select { width: 84px; }
.footer-range { white-space: nowrap; }

@import url('https://fonts.googleapis.com/css2?family=IBM+Plex+Sans:wght@400;500;600;700&family=IBM+Plex+Mono:wght@400;500;600;700&display=swap');

.lab-page {
  font-family: 'IBM Plex Sans', sans-serif;
}

.mono {
  font-family: 'IBM Plex Mono', monospace;
  letter-spacing: 0.01em;
}

.page-heading {
  font-family: 'IBM Plex Sans', sans-serif;
  font-weight: 600;
  letter-spacing: -0.01em;
  color: #101828;
}

.page-subheading {
  color: #667085;
  font-size: 0.875rem;
  margin-top: 2px;
}

.accent-bar {
  width: 4px;
  border-radius: 4px;
  background: linear-gradient(180deg, #670e5f 0%, #e987d4 100%);
  align-self: stretch;
}

.themed-card {
  border-color: #e9d5ff !important;
}

.table-header-row {
  background: linear-gradient(90deg, #760f6f 0%, #641f61 100%);
}
.table-header-row th {
  color: #ffffff !important;
  font-size: 0.75rem !important;
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.defects-table :deep(tbody tr:hover) {
  background-color: #fdf4ff !important;
}

.feedback-card:hover {
  border-color: #d946ef !important;
  box-shadow: 0 3px 10px rgba(118, 15, 111, 0.12);
}

.id-text {
  font-size: 0.82rem;
  font-weight: 700;
  color: #1e293b;
  white-space: nowrap;
}

.att-list { display: flex; flex-wrap: wrap; gap: 10px; }
.att-thumb img {
  height: 120px; max-width: 220px; object-fit: cover; display: block;
  border-radius: 8px; border: 1px solid #e9d5ff; background: #fff;
}
.att-thumb:hover img { border-color: #d946ef; box-shadow: 0 3px 10px rgba(118, 15, 111, 0.18); }
.att-file {
  display: inline-flex; align-items: center; gap: 4px; font-size: 0.8rem; font-weight: 600;
  color: #760f6f; background: #fdf4ff; border: 1px solid #e9d5ff; border-radius: 6px;
  padding: 5px 10px; text-decoration: none;
}
.att-file:hover { background: #fae8ff; }

.dialog-header {
  background: linear-gradient(135deg, #670e5f 0%, #e987d4 100%);
}

.border-top {
  border-top: 1px solid rgba(0, 0, 0, 0.06);
}
</style>