<template>
  <v-container fluid class="pa-6" style="min-height: 100vh;">
    <!-- Top Bar: Title & Filters / Actions -->
    <v-row class="mb-6" align="center">
      <v-col cols="12" md="5">
        <div class="text-caption text-grey font-weight-bold">// OVERVIEW</div>
        <div class="text-h5 font-weight-bold">UAT Performance Dashboard</div>
      </v-col>
      
      <v-col cols="12" md="7" class="d-flex justify-md-end align-center flex-wrap gap-2">
        <!-- Dynamic Date Range based on Test Cases -->
        <v-btn variant="outlined" prepend-icon="mdi-calendar-range" class="rounded-lg text-none mr-2 mb-2 mb-sm-0" size="small">
          Date range - {{ dynamicDateRange }}
        </v-btn>

        <!-- Filters Button -->
        <v-btn variant="outlined" prepend-icon="mdi-filter-variant" class="rounded-lg text-none mr-2 mb-2 mb-sm-0" size="small">
          Filters
        </v-btn>

        <!-- New Test Case Button -->
        <v-btn color="primary" class="rounded-lg text-white text-capitalize mr-3 mb-2 mb-sm-0" elevation="2" size="small" @click="openNewTestCase">
          <v-icon left class="mr-1">mdi-plus</v-icon> New Test Case
        </v-btn>

        <!-- User Profile Avatar -->
        <v-menu offset-y :close-on-content-click="false" location="bottom end">
          <template v-slot:activator="{ props }">
            <v-avatar
              color="primary"
              size="40"
              class="font-weight-bold text-white cursor-pointer mb-2 mb-sm-0"
              v-bind="props"
            >
              <v-img v-if="userProfile.avatar" :src="userProfile.avatar" alt="avatar" cover></v-img>
              <span v-else>{{ userInitials }}</span>
            </v-avatar>
          </template>
          <v-card width="300" class="rounded-xl pa-4" variant="outlined">
            <div class="d-flex align-center mb-3 pb-3 border-b">
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
            <v-list density="compact" style="background: transparent;">
              <v-list-item
                v-for="item in profileMenuItems"
                :key="item.value"
                @click="handleProfileAction(item.value)"
                :prepend-icon="item.icon"
                :title="item.title"
                class="rounded-lg"
              ></v-list-item>
            </v-list>
            <v-divider class="my-2"></v-divider>
            <v-btn color="error" variant="text" size="small" block class="text-capitalize" @click="handleLogout">
              <v-icon left size="small" class="mr-1">mdi-logout</v-icon> Logout
            </v-btn>
          </v-card>
        </v-menu>
      </v-col>
    </v-row>

    <!-- Top Summary Cards Row -->
    <v-row class="mb-6">
      <!-- Card 1: Total Test Cases -->
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 stat-card" variant="outlined" @click="filterByStatus('All')">
          <div class="d-flex justify-space-between align-start">
            <div class="text-subtitle-2 text-grey font-weight-bold">Total Test Cases</div>
            <v-icon color="primary">mdi-chart-line</v-icon>
          </div>
          <div class="text-h3 font-weight-bold text-primary my-1">{{ totalCases }}</div>
          <div class="text-caption text-success font-weight-bold mb-3">
            <v-icon size="small" color="success">mdi-arrow-up</v-icon> Active System Records
          </div>
          <div class="mt-2">
            <v-sparkline
              :model-value="[0, 2, 5, 3, 8, 5, 9, 7, totalCases]"
              color="indigo-accent-4"
              height="45"
              padding="4"
              smooth
              stroke-linecap="round"
              auto-draw
            ></v-sparkline>
          </div>
        </v-card>
      </v-col>

      <!-- Card 2: Test Health -->
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 stat-card" variant="outlined">
          <div class="d-flex justify-space-between align-start">
            <div class="text-subtitle-2 text-grey font-weight-bold">Test Health</div>
            <v-icon color="success">mdi-checkbox-marked-circle-outline</v-icon>
          </div>
          <div class="d-flex align-center justify-space-between my-2">
            <div>
              <div class="text-h3 font-weight-bold text-success">{{ passRate }}%</div>
              <div class="text-caption text-grey font-weight-medium">Pass Rate / Overall Status</div>
            </div>
            <v-progress-circular
              :model-value="passRate"
              :size="70"
              :width="8"
              color="success"
            >
              <span class="text-caption font-weight-bold">{{ passRate }}%</span>
            </v-progress-circular>
          </div>
          <div class="d-flex justify-space-between text-caption text-grey mt-4 pt-2 border-t">
            <span class="cursor-pointer" @click="filterByStatus('Passed')"><v-badge dot color="success" inline></v-badge> Pass ({{ passedCases }})</span>
            <span class="cursor-pointer" @click="filterByStatus('Failed')"><v-badge dot color="error" inline></v-badge> Fail ({{ failedCases }})</span>
            <span class="cursor-pointer" @click="filterByStatus('Pending')"><v-badge dot color="warning" inline></v-badge> Pending ({{ pendingCases }})</span>
          </div>
        </v-card>
      </v-col>

      <!-- Card 3: Total Passed -->
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 stat-card" variant="outlined" @click="filterByStatus('Passed')">
          <div class="d-flex justify-space-between align-start">
            <div class="text-subtitle-2 text-grey font-weight-bold">Total Passed</div>
            <v-icon color="indigo">mdi-chart-bar</v-icon>
          </div>
          <div class="text-h3 font-weight-bold text-indigo my-1">{{ passedCases }}</div>
          <div class="text-caption text-grey font-weight-medium mb-3">Successful test execution milestone</div>
          <div class="d-flex align-end justify-space-between pt-2 px-2" style="height: 50px;">
            <div class="bg-indigo-lighten-4 rounded-t" style="width: 14px; height: 30px;"></div>
            <div class="bg-indigo-lighten-3 rounded-t" style="width: 14px; height: 40px;"></div>
            <div class="bg-indigo-lighten-2 rounded-t" style="width: 14px; height: 35px;"></div>
            <div class="bg-indigo-darken-1 rounded-t" style="width: 14px; height: 50px;"></div>
            <div class="bg-indigo-lighten-4 rounded-t" style="width: 14px; height: 20px;"></div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Priority Performance Table -->
    <v-row class="mb-6">
      <v-col cols="12">
        <v-card class="pa-5 rounded-xl" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-1">Test Case Performance by Priority</div>
          <div class="text-caption text-grey mb-4">Detailed execution status breakdown across priority metrics</div>
          
          <v-table density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">Priority</th>
                <th class="text-grey font-weight-bold">Total Cases</th>
                <th class="text-grey font-weight-bold">Execution Rate</th>
                <th class="text-grey font-weight-bold">Defects Found</th>
                <th class="text-grey font-weight-bold">Completion Status</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="pItem in priorityTableData" :key="pItem.priority">
                <td class="font-weight-bold">
                  <v-chip size="small" :color="pItem.color" label class="mr-2 font-weight-bold">
                    <v-icon start size="small">{{ pItem.icon }}</v-icon> {{ pItem.priority }}
                  </v-chip>
                </td>
                <td class="font-weight-bold">{{ pItem.count }}</td>
                <td style="width: 25%;">
                  <div class="d-flex align-center">
                    <span class="text-caption font-weight-bold mr-2">{{ pItem.rate }}%</span>
                    <v-progress-linear :model-value="pItem.rate" :color="pItem.color" height="8" rounded></v-progress-linear>
                  </div>
                </td>
                <td class="font-weight-bold text-error">{{ pItem.defects }}</td>
                <td>
                  <v-chip size="x-small" variant="tonal" :color="pItem.statusColor" class="font-weight-bold">
                    {{ pItem.statusText }}
                  </v-chip>
                </td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>

    <!-- Bottom Analytics Section -->
    <v-row class="mb-6">
      <v-col cols="12" md="8">
        <v-card class="pa-5 rounded-xl h-100" variant="outlined">
          <div class="d-flex justify-space-between align-center mb-4">
            <div>
              <div class="text-subtitle-1 font-weight-bold">Test Execution over time</div>
              <div class="text-caption text-grey">Daily tracking of pass, fail, and pending test trends</div>
            </div>
            <div class="d-flex gap-2">
              <v-chip size="x-small" color="success" variant="outlined">Pass</v-chip>
              <v-chip size="x-small" color="error" variant="outlined">Fail</v-chip>
              <v-chip size="x-small" color="warning" variant="outlined">Pending</v-chip>
            </div>
          </div>
          
          <div class="my-4">
            <v-sparkline
              :model-value="[10, 15, 25, 22, 45, 38, 60, 52, 75, 68, totalCases > 0 ? totalCases * 10 : 90]"
              color="primary"
              height="110"
              padding="12"
              smooth
              line-width="3"
              fill
              auto-draw
            ></v-sparkline>
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 d-flex flex-column justify-space-between" variant="outlined">
          <div>
            <div class="text-subtitle-1 font-weight-bold">Defects by Severity</div>
            <div class="text-caption text-grey mb-4">Distribution ratio of logged issues</div>
          </div>
          
          <div class="d-flex justify-center align-center my-auto py-2">
            <v-progress-circular
              :model-value="75"
              :size="130"
              :width="18"
              color="deep-purple-accent-3"
              class="font-weight-bold"
            >
              <div class="text-center">
                <div class="text-h5 font-weight-bold">{{ failedCases + 3 }}</div>
                <div class="text-caption text-grey">Total Issues</div>
              </div>
            </v-progress-circular>
          </div>

          <div class="d-flex justify-space-around text-caption pt-3 border-t">
            <div><v-badge dot color="error" inline></v-badge> Critical</div>
            <div><v-badge dot color="warning" inline></v-badge> High</div>
            <div><v-badge dot color="primary" inline></v-badge> Medium</div>
            <div><v-badge dot color="grey" inline></v-badge> Low</div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Recent Test Cases -->
    <v-row>
      <v-col cols="12">
        <v-card class="pa-5 rounded-xl" variant="outlined">
          <div class="d-flex justify-space-between align-center mb-4">
            <div>
              <div class="text-subtitle-1 font-weight-bold">Recent Test Cases</div>
              <div class="text-caption text-grey">Latest test cases and their assigned testers</div>
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
                <th class="text-grey font-weight-bold">TESTERS</th>
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
                  <div v-if="testersFor(item.id).length" class="d-flex flex-column py-1" style="row-gap: 4px;">
                    <div v-for="name in testersFor(item.id)" :key="name" class="d-flex align-center">
                      <v-avatar color="primary" size="24" class="mr-2 text-white font-weight-bold" style="font-size: 10px;">
                        {{ initialsOf(name) }}
                      </v-avatar>
                      <span class="text-body-2 font-weight-medium">{{ name }}</span>
                    </div>
                  </div>
                  <span v-else class="text-caption text-grey">Not assigned</span>
                </td>
              </tr>
              <tr v-if="recentCases.length === 0">
                <td colspan="5" class="text-center text-grey py-6">
                  No test cases found. Click "New Test Case" to create one.
                </td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>

    <!-- Profile Edit Dialog -->
    <v-dialog v-model="editProfileDialog" max-width="480px" persistent>
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">
            <v-icon color="white" class="mr-2">mdi-account-edit</v-icon> Edit Profile
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
            <input ref="avatarInput" type="file" accept="image/*" class="d-none" @change="onAvatarChange" />
          </div>
          <v-text-field label="Full Name" v-model="profileForm.name" variant="outlined" density="compact" class="mb-3"></v-text-field>
          <v-text-field label="Email" v-model="profileForm.email" variant="outlined" density="compact" disabled class="mb-3"></v-text-field>
          <v-select label="Role" :items="roleOptions" v-model="profileForm.role" variant="outlined" density="compact" class="mb-3"></v-select>
          <v-text-field label="Department" v-model="profileForm.department" variant="outlined" density="compact"></v-text-field>
        </v-card-text>
        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn variant="text" class="text-capitalize" @click="editProfileDialog = false">Cancel</v-btn>
          <v-btn color="indigo-accent-4" class="text-white text-capitalize px-6 rounded-lg" @click="saveProfile">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Logout Dialog -->
    <v-dialog v-model="logoutDialog" max-width="400px">
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4" style="background-color: #dc2626;">
          <span class="text-h6 font-weight-bold"><v-icon color="white" class="mr-2">mdi-logout</v-icon> Confirm Logout</span>
        </v-card-title>
        <v-card-text class="pa-6">
          <div class="text-body-1">Are you sure you want to logout?</div>
        </v-card-text>
        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn variant="text" @click="logoutDialog = false">Cancel</v-btn>
          <v-btn color="error" class="text-white" @click="confirmLogout">Logout</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </v-container>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { testCaseService } from '@/services/testCaseService'

const router = useRouter()
const testCases = ref([])
const loadingTestCases = ref(false)

const userProfile = ref({
  name: 'Intan Syahirah',
  email: 'intan@kotrapharma.com',
  role: 'UAT Tester / Admin',
  department: 'IT',
  avatar: ''
})

const userInitials = computed(() => {
  const name = userProfile.value.name || 'User'
  return name.split(' ').map(w => w.charAt(0)).join('').toUpperCase().slice(0, 2)
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
const profileForm = ref({ name: '', email: '', role: '', department: '', avatar: '' })
const logoutDialog = ref(false)

const handleProfileAction = (action) => {
  if (action === 'profile') {
    profileForm.value = { ...userProfile.value }
    editProfileDialog.value = true
  } else if (action === 'settings') {
    router.push('/settings')
  } else if (action === 'mycases') {
    router.push('/test-cases?assigned=me')
  }
}

const triggerAvatarUpload = () => avatarInput.value?.click()

const onAvatarChange = (e) => {
  const file = e.target.files[0]
  if (!file) return
  const reader = new FileReader()
  reader.onload = () => { profileForm.value.avatar = reader.result }
  reader.readAsDataURL(file)
}

const saveProfile = () => {
  userProfile.value = { ...userProfile.value, ...profileForm.value }
  editProfileDialog.value = false
}

const handleLogout = () => { logoutDialog.value = true }
const confirmLogout = () => {
  logoutDialog.value = false
  router.push('/login')
}

const filterByStatus = (status) => {
  if (status === 'All') router.push('/test-cases')
  else router.push({ path: '/test-cases', query: { status } })
}

const openNewTestCase = () => { router.push('/test-cases') }

const totalCases = computed(() => testCases.value?.length || 0)
const passedCases = computed(() => testCases.value?.filter(c => c.status === 'Passed').length || 0)
const failedCases = computed(() => testCases.value?.filter(c => c.status === 'Failed').length || 0)
const pendingCases = computed(() => testCases.value?.filter(c => c.status === 'Pending' || c.status === 'Draft').length || 0)

const passRate = computed(() => {
  if (totalCases.value === 0) return 0
  return Math.round((passedCases.value / totalCases.value) * 100)
})

// Dynamic Date Range calculation based on actual test cases created
const dynamicDateRange = computed(() => {
  if (!testCases.value || testCases.value.length === 0) {
    return 'No test cases yet'
  }
  const dates = testCases.value
    .map(c => c.testDate || c.executionDate || c.created_at)
    .filter(Boolean)
    .map(d => new Date(d))
    .sort((a, b) => a - b)

  if (dates.length === 0) return 'Recent'

  const options = { month: 'short', day: 'numeric', year: 'numeric' }
  const startDate = dates[0].toLocaleDateString('en-US', options)
  const endDate = dates[dates.length - 1].toLocaleDateString('en-US', options)

  return startDate === endDate ? startDate : `${startDate} - ${endDate}`
})

const priorityCount = computed(() => {
  const counts = { Low: 0, Medium: 0, High: 0, Critical: 0 }
  testCases.value?.forEach(c => {
    if (counts[c.priority] !== undefined) counts[c.priority]++
  })
  return counts
})

const priorityTableData = computed(() => [
  { priority: 'Critical', count: priorityCount.value.Critical, rate: totalCases.value ? Math.round((priorityCount.value.Critical / totalCases.value) * 100) : 0, defects: 2, color: 'error', icon: 'mdi-alert-circle', statusText: priorityCount.value.Critical > 0 ? 'In Progress' : 'Not Started', statusColor: priorityCount.value.Critical > 0 ? 'warning' : 'grey' },
  { priority: 'High', count: priorityCount.value.High, rate: totalCases.value ? Math.round((priorityCount.value.High / totalCases.value) * 100) : 0, defects: 1, color: 'warning', icon: 'mdi-alert', statusText: priorityCount.value.High > 0 ? 'In Progress' : 'Not Started', statusColor: priorityCount.value.High > 0 ? 'warning' : 'grey' },
  { priority: 'Medium', count: priorityCount.value.Medium, rate: totalCases.value ? Math.round((priorityCount.value.Medium / totalCases.value) * 100) : 0, defects: 3, color: 'primary', icon: 'mdi-information', statusText: priorityCount.value.Medium > 0 ? 'Active' : 'Not Started', statusColor: 'primary' },
  { priority: 'Low', count: priorityCount.value.Low, rate: totalCases.value ? Math.round((priorityCount.value.Low / totalCases.value) * 100) : 0, defects: 0, color: 'grey', icon: 'mdi-arrow-down', statusText: 'Not Started', statusColor: 'grey' }
])

const recentCases = computed(() => {
  if (!testCases.value) return []
  return [...testCases.value].reverse().slice(0, 5)
})

const getPriorityColor = (p) => p === 'Critical' ? 'red' : p === 'High' ? 'orange' : p === 'Medium' ? 'primary' : 'grey'
const getStatusColor = (s) => s === 'Passed' ? 'success' : s === 'Failed' ? 'error' : s === 'Pending' ? 'warning' : 'grey'

// Tester sebenar datang dari TestAssignments + Users (bukan medan assigned_to)
const assignments = ref([])
const users = ref([])

const userName = (u) => u?.name || u?.full_name || u?.fullName || u?.username || ''

const testersFor = (testCaseId) => {
  const names = assignments.value
    .filter((a) => a.test_case_id === testCaseId)
    .map((a) => {
      const u = users.value.find((x) => x.id === a.user_id)
      return userName(u) || userName(a.user)
    })
    .filter(Boolean)
  return [...new Set(names)]
}

const initialsOf = (name = '') => {
  const parts = String(name).trim().split(/\s+/).filter(Boolean)
  return ((parts[0]?.[0] || '') + (parts[1]?.[0] || '')).toUpperCase() || '?'
}

const fetchAssignees = async () => {
  const [a, u] = await Promise.allSettled([
    fetch('/api/TestAssignments').then((r) => (r.ok ? r.json() : [])),
    fetch('/api/Users').then((r) => (r.ok ? r.json() : []))
  ])
  assignments.value = a.status === 'fulfilled' && Array.isArray(a.value) ? a.value : []
  users.value = u.status === 'fulfilled' && Array.isArray(u.value) ? u.value : []
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
  fetchTestCases()
  fetchAssignees()
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
.cursor-pointer { cursor: pointer; }
.border-b { border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.border-t { border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
</style>