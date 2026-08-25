<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <div class="d-flex align-center justify-space-between mb-4">
      <div>
        <div class="text-caption text-grey font-weight-bold">// AUDIT</div>
        <div class="text-h5 font-weight-bold">Audit Log History</div>
      </div>
    </div>

    <v-card variant="outlined" class="rounded-xl">
      <v-data-table
        :headers="headers"
        :items="logs"
        :loading="loading"
        class="elevation-0"
        :items-per-page="10"
      >
        <template v-slot:item.dtCreated="{ item }">
          <span class="text-caption font-weight-medium">
            {{ formatDateTime(item.dtCreated || item.timestamp) }}
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

          <div v-else>
            <v-chip size="small" :color="getActionColor(item.action)" variant="flat" class="mr-2">
              {{ item.action || '-' }}
            </v-chip>
            <span class="text-caption">{{ item.title || '-' }}</span>
          </div>
        </template>

        <template v-slot:item.crtUserId="{ item }">
          <span class="text-caption font-weight-medium">{{ item.crtUserId || item.user || 'System' }}</span>
        </template>

        <template v-slot:no-data>
          <div class="text-center text-grey pa-6">No audit logs yet.</div>
        </template>
      </v-data-table>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { auditLogService } from '@/services/auditLogService'

const logs = ref([])
const loading = ref(false)

const headers = [
  { title: 'Run ID', key: 'runId', align: 'start' },
  { title: 'Status Changes', key: 'statusChanges', align: 'start' },
  { title: 'User ID', key: 'crtUserId', align: 'start' },
  { title: 'Timestamp', key: 'dtCreated', align: 'start' }
]
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

onMounted(async () => {
  loading.value = true
  try {
    const res = await auditLogService.getAll()
    logs.value = res.data
  } catch (err) {
    console.error('Failed to load audit logs:', err)
  } finally {
    loading.value = false
  }
})
</script>