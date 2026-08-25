import api from './api'

export const testCaseService = {
  getAll: () => api.get('/TestCases'),
  getById: (id) => api.get(`/TestCases/${id}`),
  create: (data) => api.post('/TestCases', data),
  update: (id, data) => api.put(`/TestCases/${id}`, data),
  remove: (id) => api.delete(`/TestCases/${id}`)
}