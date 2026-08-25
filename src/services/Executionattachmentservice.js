import api from './api'

export const executionAttachmentService = {
  getByStep: (executionStepId) => api.get(`/ExecutionAttachments/by-step/${executionStepId}`),

  upload: (executionStepId, file) => {
    const formData = new FormData()
    formData.append('file', file)
    return api.post(`/ExecutionAttachments/Upload/${executionStepId}`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
  },

  remove: (id) => api.delete(`/ExecutionAttachments/${id}`)
}