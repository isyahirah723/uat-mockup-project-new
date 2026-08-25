<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <v-row class="mb-4" align="center">
      <v-col cols="12" sm="6" class="d-flex">
        <v-select v-model="statusFilter" :items="['All', 'Passed', 'Failed', 'Pending']" label="Status" density="compact" variant="outlined" hide-details class="mr-2"></v-select>
        <v-select v-model="priorityFilter" :items="['All', 'Low', 'Medium', 'High', 'Critical']" label="Priority" density="compact" variant="outlined" hide-details></v-select>
      </v-col>
      <v-col cols="12" sm="6" class="text-end">
        <v-btn color="#1976d2" class="text-white rounded-lg" prepend-icon="mdi-plus" @click="openAdd">New Test Case</v-btn>
      </v-col>
    </v-row>
    <v-card variant="outlined" class="rounded-xl">
      <v-data-table :headers="headers" :items="filteredItems" class="elevation-0">
        <template v-slot:item.actions="{ item }">
          <v-icon size="small" color="blue" class="mr-2" @click="openEdit(item)">mdi-pencil</v-icon>
          <v-icon size="small" color="red" @click="deleteItem(item.id)">mdi-delete</v-icon>
        </template>
      </v-data-table>
    </v-card>
    <TestCaseDrawer
      v-model="drawer"
      :initial-data="activeFormData"
      :is-edit="isEdit"
      @save="handleSave"
    />
  </v-container>
</template>

<script setup>
import { ref, computed } from 'vue'
import { useTestCaseStore } from '../stores/testCaseStore'
import TestCaseDrawer from '../components/TestCaseDrawer.vue'

const store = useTestCaseStore()
const statusFilter = ref('All')
const priorityFilter = ref('All')

const drawer = ref(false)
const isEdit = ref(false)
const activeFormData = ref({})

const headers = [
  { title: 'ID', key: 'testCaseId' },
  { title: 'Title', key: 'title' },
  { title: 'Priority', key: 'priority' },
  { title: 'Status', key: 'status' },
  { title: 'Actions', key: 'actions', sortable: false }
]

const filteredItems = computed(() => {
  return store.testCases.filter(item => {
    const statusMatch = statusFilter.value === 'All' || item.status === statusFilter.value
    const priorityMatch = priorityFilter.value === 'All' || item.priority === priorityFilter.value
    return statusMatch && priorityMatch
  })
})

const openAdd = () => {
  isEdit.value = false
  activeFormData.value = { title: '', priority: 'Medium', status: 'Draft', testDate: new Date().toISOString().substr(0, 10) }
  drawer.value = true
}

const openEdit = (item) => {
  isEdit.value = true
  activeFormData.value = { ...item }
  drawer.value = true
}

const handleSave = (data) => {
  if (isEdit.value) {
    store.updateTestCase(data)
  } else {
    store.addTestCase({ id: Date.now(), ...data })
  }
}

const deleteItem = (id) => {
  if (confirm('Delete this test case?')) store.deleteTestCase(id)
}
</script>