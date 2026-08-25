import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import Dashboard from '../components/Dashboard.vue'
import TestCasepage from '../views/TestCasepage.vue'
import TestCycles from '../views/TestCycles.vue'
import FeedbackView from '../views/FeedbackView.vue'
import TemplateForm from '../views/TemplateForm.vue'
import AuditLog from '../views/AuditLog.vue'
import Settings from '../views/Settings.vue'
import Profile from '../views/Profile.vue'
import UserGuide from '../views/UserGuide.vue'  

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: MainLayout,
      children: [
        { path: '', name: 'Dashboard', component: Dashboard },
        { path: 'test-cases', name: 'TestCases', component: TestCasepage },
        { path: 'test-cycles', name: 'TestCycles', component: TestCycles },
        { 
          path: 'feedback/:id',   
          name: 'Feedback', 
          component: FeedbackView 
        },
        { path: 'template-form', name: 'TemplateForm', component: TemplateForm },
        { path: 'audit-log', name: 'AuditLog', component: AuditLog },
        { path: 'settings', name: 'Settings', component: Settings },
        { path: 'profile', name: 'Profile', component: Profile },
        { path: 'user-guide', name: 'UserGuide', component: UserGuide }  
      ]
    }
  ]
})

export default router