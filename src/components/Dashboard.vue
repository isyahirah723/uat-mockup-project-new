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
              <v-img v-if="userProfile.avatar" :src="userProfile.avatar" alt="avatar" cover></v-img>
              <span v-else>{{ userInitials }}</span>
            </v-avatar>
          </template>
          <v-card width="300" class="rounded-xl pa-4" variant="outlined">
            <!-- User Info -->
            <div class="d-flex align-center mb-3 pb-3" style="border-bottom: 1px solid rgba(0,0,0,0.08);">
              <v-avatar color="primary" size="50" class="mr-3 font-weight-bold text-white">
                <v-img v-if="userProfile.avatar" :src="userProfile.avatar" alt="avatar" cover></v-img>
                <span v-else>{{ userInitials }}</span>
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
    <v-dialog v-model="editProfileDialog" max-width="480px" persistent>
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">
            <v-icon color="white" class="mr-2">mdi-account-edit</v-icon>
            Edit Profile
          </span>
          <v-btn icon variant="text" size="small" @click="editProfileDialog = false">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-6">
          <div class="d-flex flex-column align-center mb-6">
            <v-avatar size="90" color="primary" class="font-weight-bold text-white mb-3">
              <v-img v-if="profileForm.avatar" :src="profileForm.avatar" alt="avatar" cover></v-img>
              <span v-else class="text-h5">{{ userInitials }}</span>
            </v-avatar>
            <v-btn size="small" variant="outlined" prepend-icon="mdi-camera" class="text-capitalize" @click="triggerAvatarUpload">
              Change Photo
            </v-btn>
            <input
              ref="avatarInput"
              type="file"
              accept="image/*"
              class="d-none"
              @change="onAvatarChange"
            />
          </div>

          <v-text-field
            label="Full Name"
            v-model="profileForm.name"
            variant="outlined"
            density="compact"
            class="mb-3"
          ></v-text-field>

          <v-text-field
            label="Email"
            v-model="profileForm.email"
            variant="outlined"
            density="compact"
            disabled
            class="mb-3"
          ></v-text-field>

          <v-select
            label="Jawatan / Role"
            :items="roleOptions"
            v-model="profileForm.role"
            variant="outlined"
            density="compact"
            class="mb-3"
          ></v-select>

          <v-text-field
            label="Department"
            v-model="profileForm.department"
            variant="outlined"
            density="compact"
          ></v-text-field>
        </v-card-text>

        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn variant="text" class="text-capitalize" @click="editProfileDialog = false">Cancel</v-btn>
          <v-btn color="indigo-accent-4" class="text-white text-capitalize px-6 rounded-lg" @click="saveProfile">Save</v-btn>
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
  department: 'IT',
  avatar: ''
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
]

const roleOptions = [
  'UAT Manager / QA Lead',
  'Business Tester',
  'Compliance Officer / Auditor',
  'UAT Tester / Admin'
]

const editProfileDialog = ref(false)
const avatarInput = ref(null)
const profileForm = ref({
  name: '',
  email: '',
  role: '',
  department: '',
  avatar: ''
})

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
      openEditProfile()
      break
    case 'settings':
      router.push('/settings')
      showNotification('⚙️ Settings', 'Redirecting to settings...', 'info', 'cog')
      break
    case 'mycases':
      router.push('/test-cases?assigned=me')
      showNotification('📋 My Test Cases', 'Showing your assigned test cases...', 'info', 'clipboard-text')
      break
    default:
      break
  }
}

const openEditProfile = () => {
  profileForm.value = { ...userProfile.value }
  editProfileDialog.value = true
}

const triggerAvatarUpload = () => {
  avatarInput.value?.click()
}

const onAvatarChange = (e) => {
  const file = e.target.files[0]
  if (!file) return

  if (!file.type.startsWith('image/')) {
    showNotification('⚠️ Invalid File', 'Please choose an image file.', 'error', 'alert-circle')
    return
  }

  const reader = new FileReader()
  reader.onload = () => {
    profileForm.value.avatar = reader.result
  }
  reader.readAsDataURL(file)
}

const saveProfile = () => {
  userProfile.value = { ...userProfile.value, ...profileForm.value }

  const saved = localStorage.getItem('userSettings')
  let settings = {}
  try {
    settings = saved ? JSON.parse(saved) : {}
  } catch (e) {
    settings = {}
  }

  settings.profile = {
    ...(settings.profile || {}),
    fullName: userProfile.value.name,
    email: userProfile.value.email,
    role: userProfile.value.role,
    department: userProfile.value.department,
    avatar: userProfile.value.avatar
  }
  localStorage.setItem('userSettings', JSON.stringify(settings))

  editProfileDialog.value = false
  showNotification('✅ Profile Updated', 'Your profile has been saved.', 'success', 'account-check')
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
    department: '',
    avatar: ''
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
        userProfile.value.avatar = data.profile.avatar || userProfile.value.avatar
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