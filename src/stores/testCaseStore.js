import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export const useTestCaseStore = defineStore('testCaseStore', () => {

  const testCases = ref([])

  
  const addAuditLog = (action, title, runId = null, user = 'System') => {
    try {
      const stored = localStorage.getItem('auditLogs')
      const logs = stored ? JSON.parse(stored) : []
      
      const newLog = {
        id: Date.now(),
        action: action || 'UNKNOWN',
        title: title || 'Untitled Item',
        runId: runId || '-',
        user: user || 'System',
        timestamp: new Date().toLocaleString()
      }
      
      logs.unshift(newLog)
      localStorage.setItem('auditLogs', JSON.stringify(logs))
    } catch (error) {
      console.error('Failed to save audit log:', error)
    }
  }

  
  const addTestCase = (newCase) => {
    if (!newCase.id) newCase.id = Date.now()
    if (!newCase.title) newCase.title = 'Untitled Test Case'
    testCases.value.unshift(newCase)
    addAuditLog('CREATE', newCase.title, newCase.testCaseId || newCase.id)
  }

  const updateTestCase = (updatedCase) => {
    const index = testCases.value.findIndex(tc => tc.id === updatedCase.id)
    if (index !== -1) {
      const oldTitle = testCases.value[index].title
      testCases.value[index] = { ...updatedCase }
      addAuditLog('UPDATE', updatedCase.title || oldTitle, updatedCase.testCaseId || updatedCase.id)
      return true
    }
    return false
  }

  const deleteTestCase = (id) => {
    const index = testCases.value.findIndex(tc => tc.id === id)
    if (index !== -1) {
      const title = testCases.value[index].title || 'Untitled Item'
      const runId = testCases.value[index].testCaseId || id
      testCases.value.splice(index, 1)
      addAuditLog('DELETE', title, runId)
      return true
    }
    return false
  }

  const totalCases = computed(() => testCases.value.length)
  const passedCases = computed(() => testCases.value.filter(tc => tc.status === 'Passed').length)
  const failedCases = computed(() => testCases.value.filter(tc => tc.status === 'Failed').length)
  const pendingCases = computed(() => testCases.value.filter(tc => tc.status === 'Pending' || tc.status === 'Draft').length)

  const lowCount = computed(() => testCases.value.filter(t => t.priority === 'Low').length)
  const mediumCount = computed(() => testCases.value.filter(t => t.priority === 'Medium').length)
  const highCount = computed(() => testCases.value.filter(t => t.priority === 'High').length)
  const criticalCount = computed(() => testCases.value.filter(t => t.priority === 'Critical').length)

  return {
    testCases,
    addTestCase, updateTestCase, deleteTestCase,
    totalCases, passedCases, failedCases, pendingCases,
    lowCount, mediumCount, highCount, criticalCount,
    addAuditLog
  }
})