<template>
  <v-container fluid class="pa-6" style="min-height: 100vh;">
    <v-row class="mb-6" align="center">
      <v-col cols="12" sm="6">
        <div class="text-caption text-grey font-weight-bold">// OVERVIEW</div>
        <div class="text-h4 font-weight-bold">Dashboard</div>
      </v-col>
      <v-col cols="12" sm="6" class="d-flex justify-end align-center">
        <v-btn color="primary" class="rounded-lg text-white text-capitalize mr-3" elevation="2" @click="openNewTestCase">
          <v-icon left class="mr-1">mdi-plus</v-icon> New Test Case
        </v-btn>
        

        <v-menu v-model="showNotifications" offset-y :close-on-content-click="false" location="bottom end">
          <template v-slot:activator="{ props }">
            <v-btn icon v-bind="props" class="mr-3" variant="tonal">
              <v-badge :content="notifications.length" color="red" v-if="notifications.length > 0">
                <v-icon>mdi-bell-outline</v-icon>
              </v-badge>
              <v-icon v-else>mdi-bell-outline</v-icon>
            </v-btn>
          </template>
          <v-card width="340" class="rounded-xl pa-2" variant="outlined">
            <v-card-title class="d-flex justify-space-between align-center text-subtitle-1 font-weight-bold border-b pb-2 mb-2">
              <span>Notifications</span>
              <v-chip size="x-small" color="primary" label>{{ notifications.length }} New</v-chip>
            </v-card-title>
            <v-card-text class="pa-0" style="max-height: 300px; overflow-y: auto;">
              <v-list density="compact" style="background: transparent;">
                <v-list-item
                  v-for="(notif, idx) in notifications"
                  :key="idx"
                  class="mb-2 rounded-lg"
                  variant="tonal"
                >
                  <template v-slot:prepend>
                    <v-avatar :color="notif.color" size="28" class="mr-2">
                      <v-icon size="small" color="white">{{ notif.icon }}</v-icon>
                    </v-avatar>
                  </template>
                  <v-list-item-title class="text-caption font-weight-bold">
                    {{ notif.title }}
                  </v-list-item-title>
                  <v-list-item-subtitle class="text-caption text-grey">
                    {{ notif.message }}
                  </v-list-item-subtitle>
                </v-list-item>
                <div v-if="notifications.length === 0" class="text-center text-grey py-4 text-caption">
                  no new notifications
                </div>
              </v-list>
            </v-card-text>
            <v-card-actions class="pt-2 border-t">
              <v-spacer></v-spacer>
              <v-btn size="x-small" variant="text" color="primary" @click="clearNotifications">Clear All</v-btn>
            </v-card-actions>
          </v-card>
        </v-menu>

        <v-menu offset-y :close-on-content-click="false" location="bottom end">
          <template v-slot:activator="{ props }">
            <v-avatar
              color="primary"
              size="40"
              class="font-weight-bold text-white cursor-pointer"
              v-bind="props"
              style="cursor: pointer;"
            >
              {{ userInitials }}
            </v-avatar>
          </template>
          <v-card width="300" class="rounded-xl pa-4" variant="outlined">
            <!-- User Info -->
            <div class="d-flex align-center mb-3 pb-3" style="border-bottom: 1px solid rgba(0,0,0,0.08);">
              <v-avatar color="primary" size="50" class="mr-3 font-weight-bold text-white">
                {{ userInitials }}
              </v-avatar>
              <div>
                <div class="text-subtitle-1 font-weight-bold">{{ userProfile.name }}</div>
                <div class="text-caption text-grey">{{ userProfile.email }}</div>
                <div class="text-caption text-grey">{{ userProfile.role }}</div>
              </div>
            </div>
            <!-- Menu Items -->
            <v-list density="compact" style="background: transparent;">
              <v-list-item
                v-for="item in profileMenuItems"
                :key="item.value"
                @click="handleProfileAction(item.value)"
                :prepend-icon="item.icon"
                :title="item.title"
                class="rounded-lg"
                style="cursor: pointer;"
              ></v-list-item>
            </v-list>
            <!-- Logout Button -->
            <v-divider class="my-2"></v-divider>
            <v-btn
              color="error"
              variant="text"
              size="small"
              block
              class="text-capitalize"
              @click="handleLogout"
            >
              <v-icon left size="small" class="mr-1">mdi-logout</v-icon>
              Logout
            </v-btn>
          </v-card>
        </v-menu>
      </v-col>
    </v-row>

  
    <v-row class="mb-6">
      <v-col cols="12" sm="6" md="3">
        <v-card
          class="pa-4 rounded-xl stat-card cursor-pointer"
          variant="outlined"
          @click="filterByStatus('All')"
        >
          <div class="d-flex justify-space-between align-start">
            <div>
              <div class="text-h3 font-weight-bold text-primary">{{ totalCases }}</div>
              <div class="text-caption text-grey mt-1 font-weight-medium">Total test cases</div>
            </div>
            <v-icon color="primary" size="28">mdi-clipboard-text-outline</v-icon>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card
          class="pa-4 rounded-xl stat-card cursor-pointer"
          variant="outlined"
          @click="filterByStatus('Passed')"
        >
          <div class="d-flex justify-space-between align-start">
            <div>
              <div class="text-h3 font-weight-bold text-success">{{ passedCases }}</div>
              <div class="text-caption text-grey mt-1 font-weight-medium">Passed</div>
            </div>
            <v-icon color="success" size="28">mdi-check-circle-outline</v-icon>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card
          class="pa-4 rounded-xl stat-card cursor-pointer"
          variant="outlined"
          @click="filterByStatus('Failed')"
        >
          <div class="d-flex justify-space-between align-start">
            <div>
              <div class="text-h3 font-weight-bold text-error">{{ failedCases }}</div>
              <div class="text-caption text-grey mt-1 font-weight-medium">Failed</div>
            </div>
            <v-icon color="error" size="28">mdi-close-circle-outline</v-icon>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" sm="6" md="3">
        <v-card
          class="pa-4 rounded-xl stat-card cursor-pointer"
          variant="outlined"
          @click="filterByStatus('Pending')"
        >
          <div class="d-flex justify-space-between align-start">
            <div>
              <div class="text-h3 font-weight-bold text-warning">{{ pendingCases }}</div>
              <div class="text-caption text-grey mt-1 font-weight-medium">Pending</div>
            </div>
            <v-icon color="warning" size="28">mdi-clock-outline</v-icon>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mb-6">
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold">Test Health</div>
          <div class="text-caption text-grey mb-6">Share of pass / fail / pending results</div>
          <div class="d-flex justify-center align-center my-6">
            <v-progress-circular
              :model-value="passRate"
              :size="160"
              :width="16"
              color="success"
              style="border-radius: 50%;"
            >
              <div class="text-center">
                <div class="text-h4 font-weight-bold">{{ passRate }}%</div>
                <div class="text-caption text-grey" style="font-size: 0.65rem !important;">PASS RATE</div>
              </div>
            </v-progress-circular>
          </div>
          <div class="mt-6">
            <div class="d-flex justify-space-between align-center py-2 border-b cursor-pointer" @click="filterByStatus('Passed')">
              <div class="d-flex align-center">
                <v-badge dot color="success" inline class="mr-2"></v-badge>
                <span class="text-body-2">Passed</span>
              </div>
              <span class="font-weight-bold">{{ passedCases }}</span>
            </div>
            <div class="d-flex justify-space-between align-center py-2 border-b cursor-pointer" @click="filterByStatus('Failed')">
              <div class="d-flex align-center">
                <v-badge dot color="error" inline class="mr-2"></v-badge>
                <span class="text-body-2">Failed</span>
              </div>
              <span class="font-weight-bold">{{ failedCases }}</span>
            </div>
            <div class="d-flex justify-space-between align-center py-2 cursor-pointer" @click="filterByStatus('Pending')">
              <div class="d-flex align-center">
                <v-badge dot color="warning" inline class="mr-2"></v-badge>
                <span class="text-body-2">Pending</span>
              </div>
              <span class="font-weight-bold">{{ pendingCases }}</span>
            </div>
          </div>
        </v-card>
      </v-col>
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold">By Priority</div>
          <div class="text-caption text-grey mb-6">Where testing effort is concentrated</div>
          <div class="d-flex flex-column justify-space-around h-75">
            <div>
              <div class="d-flex justify-space-between text-caption font-weight-bold mb-1">
                <span>Low</span>
                <span>{{ priorityCount.Low }}</span>
              </div>
              <v-progress-linear :model-value="getPriorityPercent('Low')" color="grey" height="8" rounded></v-progress-linear>
            </div>
            <div>
              <div class="d-flex justify-space-between text-caption font-weight-bold mb-1">
                <span>Medium</span>
                <span>{{ priorityCount.Medium }}</span>
              </div>
              <v-progress-linear :model-value="getPriorityPercent('Medium')" color="primary" height="8" rounded></v-progress-linear>
            </div>
            <div>
              <div class="d-flex justify-space-between text-caption font-weight-bold mb-1">
                <span>High</span>
                <span>{{ priorityCount.High }}</span>
              </div>
              <v-progress-linear :model-value="getPriorityPercent('High')" color="warning" height="8" rounded></v-progress-linear>
            </div>
            <div>
              <div class="d-flex justify-space-between text-caption font-weight-bold mb-1">
                <span>Critical</span>
                <span>{{ priorityCount.Critical }}</span>
              </div>
              <v-progress-linear :model-value="getPriorityPercent('Critical')" color="error" height="8" rounded></v-progress-linear>
            </div>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold">Top Testers</div>
          <div class="text-caption text-grey mb-6">Most active designers & executors</div>
          <v-list density="compact" style="background: transparent;">
            <v-list-item v-for="(tester, index) in topTesters" :key="index" class="px-0 mb-3">
              <template v-slot:prepend>
                <v-avatar color="primary" size="36" class="mr-3 text-caption font-weight-bold text-white">
                  {{ tester.name.charAt(0).toUpperCase() }}
                </v-avatar>
              </template>
              <v-list-item-title class="text-body-2 font-weight-bold">{{ tester.name }}</v-list-item-title>
              <v-list-item-subtitle class="text-caption text-grey">{{ tester.role || 'QA Engineer' }}</v-list-item-subtitle>
              <template v-slot:append>
                <v-chip size="small" variant="tonal" class="font-weight-bold">
                  {{ tester.count }} cases
                </v-chip>
              </template>
            </v-list-item>
          </v-list>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <v-card class="pa-5 rounded-xl" variant="outlined">
          <div class="d-flex justify-space-between align-center mb-4">
            <div>
              <div class="text-subtitle-1 font-weight-bold">Recent Test Cases</div>
              <div class="text-caption text-grey">Latest added or executed test cases</div>
            </div>
            <v-btn variant="text" color="primary" size="small" class="text-capitalize" @click="filterByStatus('All')">
              View All <v-icon size="small" class="ml-1">mdi-arrow-right</v-icon>
            </v-btn>
          </div>
          <v-table density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">ID / TITLE</th>
                <th class="text-grey font-weight-bold">DEPT</th>
                <th class="text-grey font-weight-bold">PRIORITY</th>
                <th class="text-grey font-weight-bold">STATUS</th>
                <th class="text-grey font-weight-bold">ASSIGNED TO</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in recentCases" :key="item.id">
                <td class="py-3">
                  <div class="font-weight-bold text-primary text-caption">
                    {{ item.test_case_code || '#' + item.id }}
                  </div>
                  <div class="text-body-2 font-weight-medium">{{ item.title }}</div>
                </td>
                <td>
                  <span class="text-caption font-weight-bold">{{ item.test_department || '-' }}</span>
                </td>
                <td>
                  <v-chip size="x-small" :color="getPriorityColor(item.priority)" label class="font-weight-bold">
                    {{ item.priority || 'Medium' }}
                  </v-chip>
                </td>
                <td>
                  <v-chip size="x-small" :color="getStatusColor(item.status)" label class="font-weight-bold">
                    {{ item.status || 'Draft' }}
                  </v-chip>
                </td>
                <td>
                  <span class="text-caption text-grey">{{ item.assigned_to || 'Unassigned' }}</span>
                </td>
              </tr>
              <tr v-if="recentCases.length === 0">
                <td colspan="5" class="text-center text-grey py-6">
                  No recent test cases found
                </td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>
    <v-dialog v-model="helpDialog" max-width="550px">
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">
            <v-icon color="white" class="mr-2">mdi-help-circle</v-icon>
            Help & Support
          </span>
          <v-btn icon variant="text" size="small" @click="helpDialog = false">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text class="pa-6">
          <div class="text-subtitle-1 font-weight-bold mb-3">Quick Actions</div>
          <v-row>
            <v-col cols="6" v-for="item in helpQuickActions" :key="item.action">
              <v-card
                class="rounded-xl pa-3 cursor-pointer"
                variant="outlined"
                @click="handleHelpItem(item.action)"
                style="text-align: center; cursor: pointer;"
                hover
              >
                <v-icon :color="item.color" size="32" class="mb-1">{{ item.icon }}</v-icon>
                <div class="text-caption font-weight-bold">{{ item.title }}</div>
                <div class="text-caption text-grey" style="font-size: 0.65rem;">{{ item.subtitle }}</div>
              </v-card>
            </v-col>
          </v-row>

          <v-divider class="my-4"></v-divider>
          <div class="text-subtitle-1 font-weight-bold mb-3">Contact Us</div>
          
          <div class="d-flex align-center mb-2">
            <v-icon color="primary" size="20" class="mr-2">mdi-email</v-icon>
            <div>
              <div class="text-body-2 font-weight-bold">Email Support</div>
              <a href="mailto:support@uat-system.com" style="color: #4338ca; text-decoration: none; font-size: 0.9rem;">
                support@uat-system.com
              </a>
            </div>
          </div>

          <div class="d-flex align-center mb-2">
            <v-icon color="success" size="20" class="mr-2">mdi-phone</v-icon>
            <div>
              <div class="text-body-2 font-weight-bold">Phone Support</div>
              <a href="tel:+60123456789" style="color: #16a34a; text-decoration: none; font-size: 0.9rem;">
                +60 12-345 6789
              </a>
            </div>
          </div>

          <div class="d-flex align-center mb-2">
            <v-icon color="warning" size="20" class="mr-2">mdi-clock-outline</v-icon>
            <div>
              <div class="text-body-2 font-weight-bold">Working Hours</div>
              <div class="text-caption text-grey">Monday - Friday, 8:00 AM - 5:30 PM</div>
            </div>
          </div>

          <v-divider class="my-4"></v-divider>

          <!-- FAQ -->
          <div class="text-subtitle-1 font-weight-bold mb-3">📋 FAQ</div>
          
          <v-expansion-panels variant="accordion">
            <v-expansion-panel
              v-for="(faq, index) in faqList"
              :key="index"
              class="mb-2 rounded-lg"
              style="border: 1px solid #e2e8f0;"
            >
              <v-expansion-panel-title class="text-body-2 font-weight-medium">
                <v-icon color="primary" size="small" class="mr-2">mdi-help-circle-outline</v-icon>
                {{ faq.question }}
              </v-expansion-panel-title>
              <v-expansion-panel-text class="text-caption text-grey">
                {{ faq.answer }}
              </v-expansion-panel-text>
            </v-expansion-panel>
          </v-expansion-panels>
        </v-card-text>
        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn color="primary" variant="text" @click="helpDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <v-dialog v-model="logoutDialog" max-width="400px">
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4" style="background-color: #dc2626;">
          <span class="text-h6 font-weight-bold">
            <v-icon color="white" class="mr-2">mdi-logout</v-icon>
            Confirm Logout
          </span>
        </v-card-title>
        <v-card-text class="pa-6">
          <div class="text-body-1">Are you sure you want to logout?</div>
          <div class="text-caption text-grey mt-1">You will need to login again to access the system.</div>
        </v-card-text>
        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="logoutDialog = false">Cancel</v-btn>
          <v-btn color="error" class="text-white" @click="confirmLogout">Logout</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    
    <v-snackbar
      v-model="snackbar.show"
      :color="snackbar.color"
      :timeout="3000"
      location="top end"
      variant="tonal"
      class="rounded-lg"
    >
      <div class="d-flex align-center">
        <v-icon :color="snackbar.color" class="mr-2">mdi-{{ snackbar.icon }}</v-icon>
        <div>
          <div class="font-weight-bold">{{ snackbar.title }}</div>
          <div class="text-caption">{{ snackbar.message }}</div>
        </div>
      </div>
      <template v-slot:actions>
        <v-btn icon variant="text" size="small" @click="snackbar.show = false">
          <v-icon>mdi-close</v-icon>
        </v-btn>
      </template>
    </v-snackbar>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { testCaseService } from '@/services/testCaseService'

const router = useRouter()
const testCases = ref([])
const loadingTestCases = ref(false)
const showNotifications = ref(false)
const notifications = ref([
  {
    title: 'Test Case Failed',
    message: 'Test ID #AMCA102 failed during UAT.',
    color: 'red',
    icon: 'mdi-alert-circle-outline'
  },
  {
    title: 'New Cycle Created',
    message: 'CY-008 was generated for Sprint 25.',
    color: 'blue',
    icon: 'mdi-sync'
  },
  {
    title: 'Test Pending Review',
    message: '3 Test Cases are waiting for approval.',
    color: 'amber',
    icon: 'mdi-clock-outline'
  }
])
const userProfile = ref({
  name: 'Intan Syafiqah',
  email: 'intan@kotrapharma.com',
  role: 'UAT Tester / Admin',
  department: 'IT'
})
const userInitials = computed(() => {
  const name = userProfile.value.name || 'User'
  return name
    .split(' ')
    .map(word => word.charAt(0))
    .join('')
    .toUpperCase()
    .slice(0, 2)
})

const profileMenuItems = [
  { title: 'My Profile', value: 'profile', icon: 'mdi-account' },
  { title: 'Settings', value: 'settings', icon: 'mdi-cog' },
  { title: 'My Test Cases', value: 'mycases', icon: 'mdi-clipboard-text' },
  { title: 'Help & Support', value: 'help', icon: 'mdi-help-circle' },
]
const helpQuickActions = [
  { 
    title: 'User Guide', 
    subtitle: 'Learn the basics',
    icon: 'mdi-book-open-variant', 
    color: 'primary',
    action: 'guide' 
  },
  { 
    title: 'Report Bug', 
    subtitle: 'Report an issue',
    icon: 'mdi-bug', 
    color: 'error',
    action: 'bug' 
  },
  { 
    title: 'Feature Request', 
    subtitle: 'Suggest new feature',
    icon: 'mdi-lightbulb', 
    color: 'warning',
    action: 'feature' 
  },
  { 
    title: 'Email Support', 
    subtitle: 'Send us an email',
    icon: 'mdi-email', 
    color: 'success',
    action: 'email' 
  },
]
const faqList = [
  { 
    question: 'How to create a new test case?', 
    answer: 'Go to Test Cases page and click "New Test Case" button.' 
  },
  { 
    question: 'How to assign a test case to someone?', 
    answer: 'Edit the test case and select the user in "Assigned To" field.' 
  },
  { 
    question: 'How to change system theme?', 
    answer: 'Go to Settings → Dark Mode and toggle the switch.' 
  },
  { 
    question: 'How to export test cases?', 
    answer: 'Go to Test Cases page and click "Excel" or "PDF" button.' 
  }
]
const helpDialog = ref(false)
const logoutDialog = ref(false)
const snackbar = ref({
  show: false,
  title: '',
  message: '',
  color: 'success',
  icon: 'check-circle'
})

const handleProfileAction = (action) => {
  switch (action) {
    case 'profile':
      router.push('/settings')
      showNotification('👤 My Profile', 'Redirecting to profile...', 'info', 'account')
      break
    case 'settings':
      router.push('/settings')
      showNotification('⚙️ Settings', 'Redirecting to settings...', 'info', 'cog')
      break
    case 'mycases':
      router.push('/test-cases?assigned=me')
      showNotification('📋 My Test Cases', 'Showing your assigned test cases...', 'info', 'clipboard-text')
      break
    case 'help':
      helpDialog.value = true
      showNotification('❓ Help & Support', 'Opening help center...', 'info', 'help-circle')
      break
    default:
      break
  }
}

const handleHelpItem = (action) => {
  helpDialog.value = false
  
  switch (action) {
    case 'guide':
      router.push('/user-guide')
      showNotification('📖 User Guide', 'Opening user guide...', 'info', 'book-open-variant')
      break
    case 'bug':
      showNotification('🐛 Report Bug', 'Opening bug report form...', 'warning', 'bug')
      break
    case 'feature':
      showNotification('💡 Feature Request', 'Opening feature request form...', 'info', 'lightbulb')
      break
    case 'email':
      window.location.href = 'mailto:support@uat-system.com?subject=Help%20Support%20Request'
      showNotification('📧 Email Support', 'Opening email client...', 'success', 'email')
      break
    default:
      break
  }
}
const handleLogout = () => {
  logoutDialog.value = true
}

const confirmLogout = () => {
  logoutDialog.value = false
  localStorage.removeItem('userSettings')
  localStorage.removeItem('userToken')
  localStorage.removeItem('theme')

  userProfile.value = {
    name: '',
    email: '',
    role: '',
    department: ''
  }
  
  showNotification(
    '👋 Logged Out', 
    'You have been logged out successfully', 
    'warning', 
    'logout'
  )
}
const showNotification = (title, message, color = 'success', icon = 'check-circle') => {
  snackbar.value = {
    show: true,
    title: title,
    message: message,
    color: color,
    icon: icon
  }
}


const loadUserProfile = () => {
  const saved = localStorage.getItem('userSettings')
  if (saved) {
    try {
      const data = JSON.parse(saved)
      if (data.profile) {
        userProfile.value.name = data.profile.fullName || userProfile.value.name
        userProfile.value.email = data.profile.email || userProfile.value.email
        userProfile.value.role = data.profile.role || userProfile.value.role
        userProfile.value.department = data.profile.department || userProfile.value.department
      }
    } catch (e) {
      console.error('Error loading profile:', e)
    }
  }
}
const filterByStatus = (status) => {
  if (status === 'All') {
    router.push('/test-cases')
  } else {
    router.push({ path: '/test-cases', query: { status: status } })
  }
}

const openNewTestCase = () => {
  router.push('/test-cases')
}

const clearNotifications = () => {
  notifications.value = []
}


const totalCases = computed(() => testCases.value?.length || 0)
const passedCases = computed(() => testCases.value?.filter(c => c.status === 'Passed').length || 0)
const failedCases = computed(() => testCases.value?.filter(c => c.status === 'Failed').length || 0)
const pendingCases = computed(() => testCases.value?.filter(c => c.status === 'Pending' || c.status === 'Draft').length || 0)

const passRate = computed(() => {
  if (totalCases.value === 0) return 0
  return Math.round((passedCases.value / totalCases.value) * 100)
})

const priorityCount = computed(() => {
  const counts = { Low: 0, Medium: 0, High: 0, Critical: 0 }
  testCases.value?.forEach(c => {
    if (counts[c.priority] !== undefined) {
      counts[c.priority]++
    }
  })
  return counts
})

const getPriorityPercent = (p) => {
  if (totalCases.value === 0) return 0
  return (priorityCount.value[p] / totalCases.value) * 100
}

const topTesters = computed(() => {
  const testerMap = {}
  testCases.value?.forEach(c => {
    const name = c.assigned_to || 'Unassigned'
    testerMap[name] = (testerMap[name] || 0) + 1
  })
  return Object.keys(testerMap).map(name => ({
    name,
    count: testerMap[name],
    role: 'QA Engineer'
  }))
})

const recentCases = computed(() => {
  if (!testCases.value) return []
  return [...testCases.value].reverse().slice(0, 5)
})


const getPriorityColor = (p) => {
  if (p === 'Critical') return 'red'
  if (p === 'High') return 'orange'
  if (p === 'Medium') return 'primary'
  return 'grey'
}

const getStatusColor = (s) => {
  if (s === 'Passed') return 'success'
  if (s === 'Failed') return 'error'
  if (s === 'Pending') return 'warning'
  return 'grey'
}

const fetchTestCases = async () => {
  loadingTestCases.value = true
  try {
    const res = await testCaseService.getAll()
    testCases.value = res.data
  } catch (error) {
    console.error('Error fetching test cases:', error)
  } finally {
    loadingTestCases.value = false
  }
}

onMounted(() => {
  loadUserProfile()
  fetchTestCases()
})
</script>

<style scoped>
.stat-card {
  transition: all 0.2s ease-in-out;
}
.stat-card:hover {
  transform: translateY(-4px);
  border-color: #3b82f6 !important;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}
.cursor-pointer {
  cursor: pointer;
}
.border-b {
  border-bottom: 1px solid rgba(0, 0, 0, 0.08);
}
.border-t {
  border-top: 1px solid rgba(0, 0, 0, 0.08);
}
:deep(.v-theme--dark) .border-b {
  border-bottom-color: rgba(255, 255, 255, 0.08);
}
:deep(.v-theme--dark) .border-t {
  border-top-color: rgba(255, 255, 255, 0.08);
}
:deep(.v-theme--dark) .stat-card:hover {
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.4);
}
</style>