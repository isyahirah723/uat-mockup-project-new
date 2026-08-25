<template>
  <v-row class="mb-4">
    <v-col cols="12" sm="3" v-for="stat in stats" :key="stat.title">
      <v-card variant="outlined" class="pa-4 rounded-xl">
        <div class="text-caption text-grey">{{ stat.title }}</div>
        <div class="text-h4 font-weight-bold my-1" :class="stat.color">{{ stat.value }}</div>
      </v-card>
    </v-col>
  </v-row>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { testCaseService } from '@/services/testCaseService'

const testCases = ref([])

const fetchTestCases = async () => {
  try {
    const res = await testCaseService.getAll()
    testCases.value = res.data
  } catch (error) {
    console.error('Error fetching test cases:', error)
  }
}

onMounted(() => {
  fetchTestCases()
})

const stats = computed(() => [
  { title: 'Total Test Cases', value: testCases.value.length, color: 'text-indigo' },
  { title: 'Passed', value: testCases.value.filter(t => t.status === 'Passed').length, color: 'text-green' },
  { title: 'Failed', value: testCases.value.filter(t => t.status === 'Failed').length, color: 'text-red' },
  { title: 'Pending', value: testCases.value.filter(t => t.status === 'Pending').length, color: 'text-orange' }
])
</script>