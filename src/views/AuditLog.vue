<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <div class="d-flex align-center justify-space-between mb-6 flex-wrap" style="row-gap: 12px;">
      <div class="d-flex align-center">
        <div class="audit-header-icon d-flex align-center justify-center mr-4">
          <v-icon color="white" size="26">mdi-history</v-icon>
        </div>
        <div>
          <div class="text-caption text-grey font-weight-bold letter-spacing-1">// AUDIT</div>
          <div class="text-h5 font-weight-bold">Audit Log History</div>
          <div class="text-caption text-grey">Track every change made across the system</div>
        </div>
      </div>

      <v-btn
        color="primary"
        variant="tonal"
        class="rounded-lg text-capitalize"
        prepend-icon="mdi-refresh"
        :loading="loading"
        @click="fetchLogs"
      >
        Refresh
      </v-btn>
    </div>

    <v-row class="mb-6">
      <v-col cols="6" sm="3">
        <v-card variant="outlined" class="pa-4 rounded-xl stat-card">
          <div class="text-h4 font-weight-bold">{{ logs.length }}</div>
          <div class="text-caption text-grey font-weight-medium mt-1">Total Logs</div>
        </v-card>
      </v-col>
      <v-col cols="6" sm="3">
        <v-card variant="outlined" class="pa-4 rounded-xl stat-card">
          <div class="text-h4 font-weight-bold text-success">{{ actionCount('CREATE') }}</div>
          <div class="text-caption text-grey font-weight-medium mt-1">Created</div>
        </v-card>
      </v-col>
      <v-col cols="6" sm="3">
        <v-card variant="outlined" class="pa-4 rounded-xl stat-card">
          <div class="text-h4 font-weight-bold text-info">{{ actionCount('UPDATE') }}</div>
          <div class="text-caption text-grey font-weight-medium mt-1">Updated</div>
        </v-card>
      </v-col>
      <v-col cols="6" sm="3">
        <v-card variant="outlined" class="pa-4 rounded-xl stat-card">
          <div class="text-h4 font-weight-bold text-error">{{ actionCount('DELETE') }}</div>
          <div class="text-caption text-grey font-weight-medium mt-1">Deleted</div>
        </v-card>
      </v-col>
    </v-row>

    <v-card variant="outlined" class="pa-4 rounded-xl mb-4">
      <v-row density="compact" align="center">
        <v-col cols="12" sm="6">
          <v-text-field
            v-model="searchQuery"
            placeholder="Search by Run ID, User, or Title..."
            prepend-inner-icon="mdi-magnify"
            variant="outlined"
            density="compact"
            hide-details
            clearable
          ></v-text-field>
        </v-col>
        <v-col cols="12" sm="4">
          <v-select
            v-model="actionFilter"
            :items="['All', 'CREATE', 'UPDATE', 'DELETE']"
            label="Filter Action"
            variant="outlined"
            density="compact"
            hide-details
          ></v-select>
        </v-col>
        <v-col cols="12" sm="2" class="text-end">
          <v-btn variant="text" size="small" color="grey-darken-1" @click="clearFilters">
            Reset
          </v-btn>
        </v-col>
      </v-row>
    </v-card>

    <v-card variant="outlined" class="rounded-xl overflow-hidden">
      <v-data-table
        :headers="headers"
        :items="filteredLogs"
        :loading="loading"
        class="elevation-0 audit-table"
        :items-per-page="10"
        :items-per-page-options="[10, 25, 50, 100]"
      >
        <template v-slot:item.dtCreated="{ item }">
          <span class="text-caption font-weight-medium">
            {{ formatDateTime(item.dtCreated || item.timestamp) }}
          </span>
        </template>

        <template v-slot:item.runId="{ item }">
          <span class="text-caption font-weight-bold text-indigo-darken-3">
            {{ item.runId || '-' }}
          </span>
        </template>

        <template v-slot:item.statusChanges="{ item }">
          <div v-if="item.statusOld || item.statusNew" class="d-flex align-center">
            <v-chip size="x-small" :color="getStatusColor(item.statusOld)" variant="outlined" class="mr-1">
              {{ item.statusOld || '-' }}
            </v-chip>
            <v-icon size="small" class="mx-1">mdi-arrow-right</v-icon>
            <v-chip size="x-small" :color="getStatusColor(item.statusNew)" variant="flat">
              {{ item.statusNew || '-' }}
            </v-chip>
          </div>

          <div v-else class="d-flex align-center flex-wrap" style="row-gap: 4px;">
            <v-chip size="small" :color="getActionColor(item.action)" variant="flat" class="mr-2" :prepend-icon="getActionIcon(item.action)">
              {{ item.action || '-' }}
            </v-chip>
            <span class="text-caption">{{ item.title || '-' }}</span>
          </div>

          <div v-if="item.details" class="text-caption text-grey mt-1 text-truncate" style="max-width: 320px;">
            {{ item.details }}
          </div>
        </template>

        <template v-slot:item.crtUserId="{ item }">
          <div class="d-flex align-center">
            <v-avatar size="24" color="indigo-lighten-4" class="mr-2">
              <span class="text-caption font-weight-bold text-indigo-darken-3">
                {{ (item.crtUserId || item.user || 'S').charAt(0).toUpperCase() }}
              </span>
            </v-avatar>
            <span class="text-caption font-weight-medium">{{ item.crtUserId || item.user || 'System' }}</span>
          </div>
        </template>

        <template v-slot:no-data>
          <div class="text-center text-grey pa-10">
            <v-icon size="40" color="grey-lighten-1" class="mb-2">mdi-history</v-icon>
            <div class="text-body-2">No audit logs found.</div>
            <div class="text-caption" v-if="searchQuery || actionFilter !== 'All'">Try adjusting your search or filter.</div>
          </div>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { auditLogService } from '@/services/auditLogService'

const logs = ref([])
const loading = ref(false)
const searchQuery = ref('')
const actionFilter = ref('All')

const headers = [
  { title: 'Run ID', key: 'runId', align: 'start' },
  { title: 'Status Changes', key: 'statusChanges', align: 'start' },
  { title: 'User ID', key: 'crtUserId', align: 'start' },
  { title: 'Timestamp', key: 'dtCreated', align: 'start' }
]

const filteredLogs = computed(() => {
  return logs.value.filter(log => {
    const matchesAction = actionFilter.value === 'All' || log.action === actionFilter.value

    const query = searchQuery.value?.toLowerCase().trim()
    const matchesSearch = !query ||
      (log.runId && log.runId.toLowerCase().includes(query)) ||
      (log.title && log.title.toLowerCase().includes(query)) ||
      ((log.crtUserId || log.user) && (log.crtUserId || log.user).toLowerCase().includes(query))

    return matchesAction && matchesSearch
  })
})

const actionCount = (action) => logs.value.filter(l => l.action === action).length

const clearFilters = () => {
  searchQuery.value = ''
  actionFilter.value = 'All'
}

const getStatusColor = (status) => {
  if (status === 'Passed' || status === 'Approved') return 'success'
  if (status === 'Failed' || status === 'Rejected') return 'error'
  if (status === 'In Progress' || status === 'Pending') return 'warning'
  if (status === 'Draft') return 'grey'
  return 'info'
}
const getActionColor = (action) => {
  if (action === 'CREATE') return 'success'
  if (action === 'UPDATE') return 'info'
  if (action === 'DELETE') return 'error'
  return 'grey'
}
const getActionIcon = (action) => {
  if (action === 'CREATE') return 'mdi-plus-circle-outline'
  if (action === 'UPDATE') return 'mdi-pencil-outline'
  if (action === 'DELETE') return 'mdi-delete-outline'
  return 'mdi-information-outline'
}
const formatDateTime = (isoString) => {
  if (!isoString) return '-'
  try {
    const date = new Date(isoString)
    return date.toLocaleString('en-MY', {
      day: '2-digit', month: '2-digit', year: 'numeric',
      hour: '2-digit', minute: '2-digit', second: '2-digit'
    })
  } catch {
    return isoString
  }
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

onMounted(fetchLogs)
</script>

<style scoped>
.letter-spacing-1 {
  letter-spacing: 1px;
}

.audit-header-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, #4338ca 0%, #6366f1 100%);
  box-shadow: 0 4px 12px rgba(67, 56, 202, 0.25);
  flex-shrink: 0;
}

.stat-card {
  transition: all 0.2s ease-in-out;
}
.stat-card:hover {
  transform: translateY(-4px);
  border-color: #3b82f6 !important;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}

.audit-table :deep(th) {
  font-size: 0.75rem !important;
  color: #64748b !important;
  text-transform: uppercase;
  background-color: #f8fafc;
}
</style>