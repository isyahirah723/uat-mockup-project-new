import api from './api'

export const executionStepService = {
  getAll: () => api.get('/ExecutionSteps'),
  getByTestCase: (testCaseId) => api.get(`/ExecutionSteps/by-test-case/${testCaseId}`),
  create: (data) => api.post('/ExecutionSteps', data),
  saveBatch: (testCaseId, steps) => api.post(`/ExecutionSteps/SaveBatch/${testCaseId}`, steps)
}