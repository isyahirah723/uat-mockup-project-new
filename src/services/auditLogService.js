import api from './api'

export const auditLogService = {
  getAll: () => api.get('/AuditLog'),
  getById: (id) => api.get(`/AuditLog/${id}`),
  create: (data) => api.post('/AuditLog', data)
}