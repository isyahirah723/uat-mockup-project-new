import api from './api'

export const testFeedbackService = {
  getAll: () => api.get('/TestFeedbacks'),
  getByTestCase: (testCaseId) => api.get(`/TestFeedbacks/by-test-case/${testCaseId}`),
  create: (data) => api.post('/TestFeedbacks', data),
  saveBatch: (testCaseId, feedbacks) => api.post(`/TestFeedbacks/SaveBatch/${testCaseId}`, feedbacks)
}