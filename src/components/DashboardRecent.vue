<template>
  <v-card variant="outlined" class="rounded-xl pa-4">
    <div class="text-h6 font-weight-bold mb-3">Recent Test Cases</div>
    <v-list density="compact">
      <v-list-item v-for="item in recentCases" :key="item.id" class="px-0">
        <template v-slot:prepend>
          <v-chip size="x-small" class="mr-2">{{ item.test_case_code }}</v-chip>
        </template>
        <v-list-item-title class="font-weight-medium">{{ item.title }}</v-list-item-title>
        <template v-slot:append>
          <span class="text-caption text-grey">{{ item.status }}</span>
        </template>
      </v-list-item>
    </v-list>
  </v-card>
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

const recentCases = computed(() => testCases.value.slice(0, 5))
</script>