import { createRouter, createWebHistory } from 'vue-router'
import MainLayout from '../layouts/MainLayout.vue'
import Dashboard from '../components/Dashboard.vue'
import TestCasepage from '../views/TestCasepage.vue'
import TestCaseDetailPage from '../views/Testcasedetailpage.vue'
import TestRunsPage from '../views/TestRunsPage.vue'
import TestExecutionPage from '../views/TestExecutionPage.vue'
import TestRunReportPage from '../views/TestRunReportPage.vue'
import TestCycleDetailPage from '../views/Testcycledetailpage.vue'
import FeedbackView from '../views/FeedbackView.vue'
import FeedbackDefectDetailPage from '../views/Feedbackdefectdetailpage.vue'
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
        { path: 'test-cases/:id', name: 'TestCaseDetail', component: TestCaseDetailPage },
        { path: 'test-cases/:id/runs', name: 'TestCaseRuns', component: TestRunsPage },
        { path: 'test-cases/:id/execute', name: 'TestExecution', component: TestExecutionPage },
        { path: 'test-cases/:id/runs/:runId/report', name: 'TestRunReport', component: TestRunReportPage },
        { path: 'test-cycles', redirect: { path: '/test-cases', query: { tab: 'cycles' } } },
        { path: 'test-cycles/:id', name: 'TestCycleDetail', component: TestCycleDetailPage },
        { 
          path: 'feedback/:id?',   
          name: 'Feedback', 
          component: FeedbackView 
        },
        { path: 'feedback/defect/:id', name: 'FeedbackDefectDetail', component: FeedbackDefectDetailPage },
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