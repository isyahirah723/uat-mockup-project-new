<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <!-- HEADER -->
    <div class="d-flex align-center mb-8">
      <div class="settings-header-icon d-flex align-center justify-center mr-4">
        <v-icon color="white" size="26">mdi-cog</v-icon>
      </div>
      <div>
        <div class="text-caption text-grey font-weight-bold letter-spacing-1">SETTINGS</div>
        <div class="text-h5 font-weight-bold">System Settings</div>
        <div class="text-caption text-grey">Manage general, appearance, notification and account preferences</div>
      </div>
    </div>

    <v-row>
      <v-col cols="12" md="3">
        <v-card variant="outlined" class="rounded-xl pa-2 settings-nav-card">
          <v-list density="compact" nav class="bg-transparent">
            <v-list-item
              v-for="item in menuItems"
              :key="item.value"
              :value="item.value"
              :active="activeMenu === item.value"
              @click="activeMenu = item.value"
              :prepend-icon="item.icon"
              :title="item.title"
              color="primary"
              class="rounded-lg mb-1 settings-nav-item"
            ></v-list-item>
          </v-list>
        </v-card>
      </v-col>

      
      <v-col cols="12" md="9">
        <v-card variant="outlined" class="rounded-xl">
          <v-card-text class="pa-6">

            
            <div v-if="activeMenu === 'general'">
              <div class="text-h6 font-weight-bold mb-4">General Settings</div>
              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="generalSettings.systemName"
                    label="System Name"
                    variant="outlined"
                    density="compact"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-select
                    v-model="generalSettings.language"
                    :items="['English', 'Bahasa Melayu']"
                    label="System Language"
                    variant="outlined"
                    density="compact"
                  ></v-select>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-select
                    v-model="generalSettings.timezone"
                    :items="['Asia/Kuala_Lumpur (+08:00)', 'UTC (+00:00)']"
                    label="Timezone"
                    variant="outlined"
                    density="compact"
                  ></v-select>
                </v-col>
              </v-row>
            </div>

            
            <div v-else-if="activeMenu === 'darkmode'">
              <div class="text-h6 font-weight-bold mb-4">Appearance / Theme</div>
              <v-row>
                <v-col cols="12">
                  <v-switch
                    v-model="isDarkMode"
                    label="Enable Dark Theme"
                    color="primary"
                    inset
                    @update:model-value="toggleDarkMode"
                  ></v-switch>
                  <div class="text-caption text-grey mt-2">
                    {{ isDarkMode ? '🌙 Dark mode is active across all pages' : '☀️ Light mode is active across all pages' }}
                  </div>
                </v-col>
              </v-row>
            </div>

           
            <div v-else-if="activeMenu === 'profile'">
              <div class="text-h6 font-weight-bold mb-4">User Profile</div>
              
              <v-row class="mb-4">
                <v-col cols="12" md="4" class="text-center">
                  <v-avatar
                    size="120"
                    class="mb-2 profile-avatar"
                    style="font-size: 48px;"
                  >
                    {{ profileSettings.fullName?.charAt(0)?.toUpperCase() || 'U' }}
                  </v-avatar>
                  <div class="text-subtitle-1 font-weight-bold">{{ profileSettings.fullName }}</div>
                  <div class="text-caption text-grey">{{ profileSettings.role }}</div>
                </v-col>
                <v-col cols="12" md="8">
                  <v-row>
                    <v-col cols="6">
                      <div class="text-caption text-grey">Email</div>
                      <div class="font-weight-medium">{{ profileSettings.email }}</div>
                    </v-col>
                    <v-col cols="6">
                      <div class="text-caption text-grey">Department</div>
                      <div class="font-weight-medium">{{ profileSettings.department }}</div>
                    </v-col>
                    <v-col cols="6">
                      <div class="text-caption text-grey">Phone</div>
                      <div class="font-weight-medium">{{ profileSettings.phone || '-' }}</div>
                    </v-col>
                    <v-col cols="6">
                      <div class="text-caption text-grey">Location</div>
                      <div class="font-weight-medium">{{ profileSettings.location || '-' }}</div>
                    </v-col>
                  </v-row>
                </v-col>
              </v-row>

              <v-divider class="my-4"></v-divider>

              <v-row>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="profileSettings.fullName"
                    label="Full Name"
                    variant="outlined"
                    density="compact"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="profileSettings.email"
                    label="Email Address"
                    variant="outlined"
                    density="compact"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="profileSettings.role"
                    label="Role / Position"
                    variant="outlined"
                    density="compact"
                    readonly
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="profileSettings.department"
                    label="Department"
                    variant="outlined"
                    density="compact"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="profileSettings.phone"
                    label="Phone Number"
                    variant="outlined"
                    density="compact"
                    placeholder="+60 12-345 6789"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="profileSettings.location"
                    label="Location"
                    variant="outlined"
                    density="compact"
                    placeholder="Kuala Lumpur, Malaysia"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-textarea
                    v-model="profileSettings.bio"
                    label="Bio / About"
                    variant="outlined"
                    density="compact"
                    rows="3"
                    placeholder="Tell us about yourself..."
                  ></v-textarea>
                </v-col>
                <v-col cols="12">
                  <v-text-field
                    v-model="profileSettings.password"
                    label="New Password"
                    type="password"
                    variant="outlined"
                    density="compact"
                    placeholder="Leave blank to keep current password"
                  ></v-text-field>
                </v-col>
              </v-row>
            </div>

           
            <div v-else-if="activeMenu === 'audit'">
              <div class="text-h6 font-weight-bold mb-4">Audit Log Settings</div>
              <v-row>
                <v-col cols="12">
                  <v-switch
                    v-model="auditSettings.logUserActivity"
                    label="Log User Activities"
                    color="primary"
                    inset
                    @update:model-value="onAuditToggle"
                  ></v-switch>
                  <div class="text-caption text-grey mt-1">
                    {{ auditSettings.logUserActivity ? '✅ User activities are being logged' : '❌ User activities are not being logged' }}
                  </div>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-select
                    v-model="auditSettings.retentionPeriod"
                    :items="['30 Days', '60 Days', '90 Days', '1 Year']"
                    label="Log Retention Period"
                    variant="outlined"
                    density="compact"
                    :disabled="!auditSettings.logUserActivity"
                  ></v-select>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-btn
                    color="primary"
                    variant="outlined"
                    @click="viewAuditLogs"
                    :disabled="!auditSettings.logUserActivity"
                  >
                    <v-icon left size="small" class="mr-1">mdi-history</v-icon>
                    View Audit Logs
                  </v-btn>
                </v-col>
              </v-row>
            </div>

          </v-card-text>

         
          <v-divider></v-divider>
          <v-card-actions class="pa-4">
            <v-spacer></v-spacer>
            <v-btn 
              color="primary" 
              class="px-6 rounded-lg text-capitalize"
              @click="saveAllSettings"
              :loading="saving"
            >
              <v-icon left size="small" class="mr-1">mdi-content-save</v-icon>
              SAVE SETTINGS
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

    
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

    
    <v-dialog v-model="auditDialog" max-width="800px">
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">Audit Log</span>
          <v-btn icon variant="text" size="small" @click="auditDialog = false" class="float-right">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text class="pa-4">
          <v-data-table
            :headers="auditHeaders"
            :items="auditLogs"
            :loading="auditLogsLoading"
            density="compact"
            class="elevation-0"
          >
            <template v-slot:item.dtCreated="{ item }">
              {{ formatDate(item.dtCreated) }}
            </template>
            <template v-slot:item.action="{ item }">
              <v-chip size="x-small" :color="getAuditActionColor(item.action)" label>
                {{ item.action }}
              </v-chip>
            </template>
          </v-data-table>
        </v-card-text>
        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn color="primary" variant="text" @click="auditDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    
    <v-dialog v-model="helpDialog" max-width="500px">
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
          <div class="text-subtitle-1 font-weight-bold mb-2">📧 Email Support</div>
          <div class="text-body-2 mb-4">
            <a href="mailto:support@uat-system.com" style="color: #4338ca; text-decoration: none;">
              support@uat-system.com
            </a>
          </div>

          <div class="text-subtitle-1 font-weight-bold mb-2">📞 Phone Support</div>
          <div class="text-body-2 mb-4">+60 12-345 6789</div>

          <div class="text-subtitle-1 font-weight-bold mb-2">🕐 Working Hours</div>
          <div class="text-body-2 mb-4">Monday - Friday, 9:00 AM - 6:00 PM (MYT)</div>

          <v-divider class="my-4"></v-divider>

          <div class="text-subtitle-1 font-weight-bold mb-2">📋 FAQ</div>
          <v-list density="compact" style="background: transparent;">
            <v-list-item 
              v-for="(faq, index) in faqList" 
              :key="index"
              class="mb-2 rounded-lg"
              style="border: 1px solid #e2e8f0;"
            >
              <template v-slot:prepend>
                <v-icon color="primary" size="small">mdi-help-circle-outline</v-icon>
              </template>
              <v-list-item-title class="text-body-2 font-weight-bold">{{ faq.question }}</v-list-item-title>
              <v-list-item-subtitle class="text-caption text-grey">{{ faq.answer }}</v-list-item-subtitle>
            </v-list-item>
          </v-list>
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
  </v-container>
</template>

<script setup>
import { ref, reactive, watch } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'


const API_BASE_URL = 'https://localhost:7049/api'
const router = useRouter()
const activeMenu = ref('general')
const saving = ref(false)
const auditDialog = ref(false)
const helpDialog = ref(false)
const logoutDialog = ref(false)

function readSavedSettings() {
  try {
    const raw = localStorage.getItem('userSettings')
    if (!raw) return {}
    const parsed = JSON.parse(raw)
    return (parsed && typeof parsed === 'object') ? parsed : {}
  } catch (err) {
    console.warn('userSettings dalam localStorage rosak, guna default. Detail:', err)
    return {}
  }
}

const savedData = readSavedSettings()
const isDarkMode = ref(savedData.darkMode ?? false)


watch(isDarkMode, (newVal) => {
  const settings = readSavedSettings()
  settings.darkMode = newVal
  localStorage.setItem('userSettings', JSON.stringify(settings))

  if (typeof window.__toggleTheme === 'function') {
    window.__toggleTheme(newVal)
  }
}, { immediate: true })


const toggleDarkMode = (val) => {
  isDarkMode.value = val
  showNotification(
    val ? '🌙 Dark Mode Enabled' : '☀️ Light Mode Enabled',
    val ? 'Dark theme is now active across all pages' : 'Light theme is now active across all pages',
    'primary',
    val ? 'moon-waning-crescent' : 'weather-sunny'
  )
}


const menuItems = [
  { title: 'General', value: 'general', icon: 'mdi-cog' },
  { title: 'Dark Mode', value: 'darkmode', icon: 'mdi-theme-light-dark' },
  { title: 'Profile', value: 'profile', icon: 'mdi-account' },
  { title: 'Audit Log', value: 'audit', icon: 'mdi-history' }
]


const generalSettings = reactive({
  systemName: savedData.general?.systemName || 'UAT Management System',
  language: savedData.general?.language || 'English',
  timezone: savedData.general?.timezone || 'Asia/Kuala_Lumpur (+08:00)'
})


const profileSettings = reactive({
  fullName: savedData.profile?.fullName || 'System Administrator',
  email: savedData.profile?.email || 'admin@uat-system.com',
  role: savedData.profile?.role || 'UAT Tester / Admin',
  department: savedData.profile?.department || 'IT',
  phone: savedData.profile?.phone || '',
  location: savedData.profile?.location || '',
  bio: savedData.profile?.bio || '',
  password: ''
})


const auditSettings = reactive({
  logUserActivity: savedData.audit?.logUserActivity ?? true,
  retentionPeriod: savedData.audit?.retentionPeriod || '90 Days'
})


const auditHeaders = [
  { title: 'Timestamp', key: 'dtCreated' },
  { title: 'User', key: 'crtUserId' },
  { title: 'Action', key: 'action' },
  { title: 'Details', key: 'details' }
]

const auditLogs = ref([])
const auditLogsLoading = ref(false)


const faqList = [
  { question: 'How to create a new test case?', answer: 'Go to Test Cases page and click "New Test Case" button.' },
  { question: 'How to assign a test case to someone?', answer: 'Edit the test case and select the user in "Assigned To" field.' },
  { question: 'How to change system theme?', answer: 'Go to Settings → Dark Mode and toggle the switch.' },
  { question: 'How to export test cases?', answer: 'Go to Test Cases page and click "Excel" or "PDF" button.' }
]


const snackbar = ref({
  show: false,
  title: '',
  message: '',
  color: 'success',
  icon: 'check-circle'
})


const onAuditToggle = (val) => {
  showNotification(
    val ? '📋 Audit Log Enabled' : '📋 Audit Log Disabled',
    val ? 'User activities are being logged' : 'User activities are not being logged',
    val ? 'success' : 'warning',
    val ? 'history' : 'history-off'
  )
}


const viewAuditLogs = async () => {
  auditDialog.value = true
  auditLogsLoading.value = true
  try {
    const response = await axios.get(`${API_BASE_URL}/AuditLog`)
    auditLogs.value = response.data
  } catch (err) {
    console.error('Gagal ambil audit logs dari database:', err)
    showNotification(
      '⚠️ Gagal Muat Audit Log',
      'Tak dapat sambung ke server. Sila cuba lagi.',
      'error',
      'alert-circle'
    )
  } finally {
    auditLogsLoading.value = false
  }
}


const showHelpSupport = () => {
  helpDialog.value = true
}


const showLogoutDialog = () => {
  logoutDialog.value = true
}


const confirmLogout = () => {
  logoutDialog.value = false
  localStorage.removeItem('userSettings')
  localStorage.removeItem('userToken')
  showNotification(
    '👋 Logged Out', 
    'You have been logged out successfully', 
    'warning', 
    'logout'
  )
  
}


const saveAllSettings = async () => {
  saving.value = true

  const allSettings = {
    general: generalSettings,
    darkMode: isDarkMode.value,
    profile: profileSettings,
    audit: auditSettings
  }

  localStorage.setItem('userSettings', JSON.stringify(allSettings))

  try {
    await axios.post(`${API_BASE_URL}/AuditLog`, {
      crtUserId: profileSettings.fullName || 'Admin',
      action: 'Update',
      details: 'System settings updated'
    })
  } catch (err) {
    console.error('Gagal simpan audit log ke database:', err)
  }

  setTimeout(() => {
    saving.value = false
    showNotification(
      '✅ Settings Saved',
      'All system settings updated successfully',
      'success',
      'check-circle'
    )
  }, 600)
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

// Format Date
const formatDate = (date) => {
  if (!date) return '-'
  const d = new Date(date)
  return d.toLocaleString('en-US', {
    year: 'numeric',
    month: 'short',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}


const getAuditActionColor = (action) => {
  const colors = {
    Login: 'success',
    Logout: 'grey',
    Create: 'primary',
    Update: 'warning',
    Delete: 'error',
    Export: 'info',
    Import: 'info'
  }
  return colors[action] || 'grey'
}


</script>

<style scoped>
.letter-spacing-1 {
  letter-spacing: 1px;
}

.settings-header-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, #4338ca 0%, #6366f1 100%);
  box-shadow: 0 4px 12px rgba(67, 56, 202, 0.25);
  flex-shrink: 0;
}

.settings-nav-card {
  border-color: #e2e8f0;
}

.settings-nav-item {
  transition: background-color 0.15s ease;
}

.v-list-item {
  margin-bottom: 4px;
}
.v-list-item--active {
  background-color: #e8f0fe !important;
  color: #4338ca !important;
  font-weight: 600;
}

.profile-avatar {
  background: linear-gradient(135deg, #4338ca 0%, #6366f1 100%) !important;
  color: #fff !important;
  box-shadow: 0 0 0 4px rgba(99, 102, 241, 0.15);
}

.border-t {
  border-top: 1px solid #e2e8f0;
}
.float-right {
  float: right;
}
.cursor-pointer {
  cursor: pointer;
}
</style>