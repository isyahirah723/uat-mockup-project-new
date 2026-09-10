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

            
            <div v-else-if="activeMenu === 'email'">
              <div class="text-h6 font-weight-bold mb-4">Email Notifications</div>
              <v-row>
                <v-col cols="12">
                  <v-switch
                    v-model="emailSettings.emailEnabled"
                    label="Enable Email Notifications"
                    color="primary"
                    inset
                  ></v-switch>
                  <div class="text-caption text-grey mt-1 mb-2">
                    {{ emailSettings.emailEnabled ? '✅ System will send email notifications via SMTP' : '❌ Email notifications are turned off' }}
                  </div>
                </v-col>
              </v-row>

              <v-row :class="{ 'disabled-section': !emailSettings.emailEnabled }">
                <v-col cols="12" sm="8">
                  <v-text-field
                    v-model="emailSettings.smtpHost"
                    label="SMTP Host"
                    placeholder="smtp.gmail.com"
                    variant="outlined"
                    density="compact"
                    :disabled="!emailSettings.emailEnabled"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="4">
                  <v-text-field
                    v-model.number="emailSettings.smtpPort"
                    label="SMTP Port"
                    type="number"
                    placeholder="587"
                    variant="outlined"
                    density="compact"
                    :disabled="!emailSettings.emailEnabled"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="emailSettings.smtpUser"
                    label="SMTP Username"
                    variant="outlined"
                    density="compact"
                    :disabled="!emailSettings.emailEnabled"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="emailSettings.smtpPassword"
                    label="SMTP Password"
                    type="password"
                    placeholder="Leave blank to keep current password"
                    variant="outlined"
                    density="compact"
                    :disabled="!emailSettings.emailEnabled"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-switch
                    v-model="emailSettings.smtpSecure"
                    label="Use Secure Connection (SSL/TLS)"
                    color="primary"
                    inset
                    :disabled="!emailSettings.emailEnabled"
                  ></v-switch>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="emailSettings.fromEmail"
                    label="From Email"
                    placeholder="noreply@kotrapharma.com"
                    variant="outlined"
                    density="compact"
                    :disabled="!emailSettings.emailEnabled"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" sm="6">
                  <v-text-field
                    v-model="emailSettings.toEmail"
                    label="Default Notification Recipient"
                    placeholder="qa-team@kotrapharma.com"
                    variant="outlined"
                    density="compact"
                    :disabled="!emailSettings.emailEnabled"
                  ></v-text-field>
                </v-col>
                <v-col cols="12">
                  <v-btn
                    color="primary"
                    variant="outlined"
                    size="small"
                    prepend-icon="mdi-email-fast-outline"
                    :disabled="!emailSettings.emailEnabled"
                    :loading="testingEmail"
                    @click="sendTestEmail"
                  >
                    Send Test Email
                  </v-btn>
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
                  <v-avatar size="120" class="mb-2 profile-avatar" style="font-size: 48px;">
                    <v-img v-if="profileSettings.avatar" :src="profileSettings.avatar" alt="avatar" cover></v-img>
                    <span v-else>{{ userInitials }}</span>
                  </v-avatar>
                  <div>
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
                  <div class="text-subtitle-1 font-weight-bold mt-2">{{ profileSettings.fullName }}</div>
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
                  <v-select
                    v-model="profileSettings.role"
                    label="Role / Position"
                    :items="roleOptions"
                    variant="outlined"
                    density="compact"
                  ></v-select>
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
  </v-container>
</template>

<script setup>
import { ref, reactive, watch, onMounted, computed } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'


const API_BASE_URL = 'https://localhost:7049/api'
const router = useRouter()
const activeMenu = ref('general')
const saving = ref(false)
const profileLoading = ref(false)
const auditDialog = ref(false)

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
  { title: 'Email Notifications', value: 'email', icon: 'mdi-email-outline' },
  { title: 'Dark Mode', value: 'darkmode', icon: 'mdi-theme-light-dark' },
  { title: 'Profile', value: 'profile', icon: 'mdi-account' },
  { title: 'Audit Log', value: 'audit', icon: 'mdi-history' }
]


const generalSettings = reactive({
  systemName: savedData.general?.systemName || 'UAT Management System',
  language: savedData.general?.language || 'English',
  timezone: savedData.general?.timezone || 'Asia/Kuala_Lumpur (+08:00)'
})


const emailSettings = reactive({
  emailEnabled: savedData.email?.emailEnabled ?? false,
  smtpHost: savedData.email?.smtpHost || '',
  smtpPort: savedData.email?.smtpPort || 587,
  smtpUser: savedData.email?.smtpUser || '',
  smtpPassword: '',
  smtpSecure: savedData.email?.smtpSecure ?? true,
  fromEmail: savedData.email?.fromEmail || '',
  toEmail: savedData.email?.toEmail || ''
})

const testingEmail = ref(false)

const sendTestEmail = async () => {
  testingEmail.value = true
  try {
    await axios.post(`${API_BASE_URL}/SystemSettings/test-email`)
    showNotification('📧 Test Email Sent', `A test email was sent to ${emailSettings.toEmail || 'the configured recipient'}.`, 'success', 'email-check')
  } catch (err) {
    console.error('Gagal hantar test email:', err)
    showNotification('⚠️ Test Email Failed', 'Could not send test email. Please check your SMTP settings.', 'error', 'email-alert')
  } finally {
    testingEmail.value = false
  }
}


const profileSettings = reactive({
  fullName: savedData.profile?.fullName || 'System Administrator',
  email: savedData.profile?.email || 'admin@kotrapharma.com',
  role: savedData.profile?.role || 'UAT Tester / Admin',
  department: savedData.profile?.department || 'IT',
  phone: savedData.profile?.phone || '',
  location: savedData.profile?.location || '',
  bio: savedData.profile?.bio || '',
  avatar: savedData.profile?.avatar || '',
  password: ''
})

const userInitials = computed(() => {
  const name = profileSettings.fullName || 'User'
  return name
    .split(' ')
    .map(word => word.charAt(0))
    .join('')
    .toUpperCase()
    .slice(0, 2)
})

const roleOptions = [
  'UAT Manager / QA Lead',
  'Business Tester',
  'Compliance Officer / Auditor',
  'UAT Tester / Admin'
]

const avatarInput = ref(null)

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
    profileSettings.avatar = reader.result
  }
  reader.readAsDataURL(file)
}

const loadSystemSettingsFromServer = async () => {
  profileLoading.value = true
  try {
    const response = await axios.get(`${API_BASE_URL}/SystemSettings`)
    if (response.data) {
      Object.assign(generalSettings, {
        systemName: response.data.systemName ?? generalSettings.systemName,
        language: response.data.language ?? generalSettings.language,
        timezone: response.data.timezone ?? generalSettings.timezone
      })
      Object.assign(emailSettings, {
        emailEnabled: response.data.emailEnabled ?? emailSettings.emailEnabled,
        smtpHost: response.data.smtpHost ?? emailSettings.smtpHost,
        smtpPort: response.data.smtpPort ?? emailSettings.smtpPort,
        smtpUser: response.data.smtpUser ?? emailSettings.smtpUser,
        smtpSecure: response.data.smtpSecure ?? emailSettings.smtpSecure,
        fromEmail: response.data.fromEmail ?? emailSettings.fromEmail,
        toEmail: response.data.toEmail ?? emailSettings.toEmail
        // smtpPassword deliberately not populated from the server response
      })
      Object.assign(auditSettings, {
        logUserActivity: response.data.logUserActivity ?? auditSettings.logUserActivity,
        retentionPeriod: response.data.retentionPeriod ?? auditSettings.retentionPeriod
      })
    }
  } catch (err) {
    console.warn('Tak dapat tarik system settings dari server, guna data local/default. Detail:', err)
  } finally {
    profileLoading.value = false
  }
}


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



const saveAllSettings = async () => {
  saving.value = true

  const allSettings = {
    general: generalSettings,
    email: emailSettings,
    darkMode: isDarkMode.value,
    profile: profileSettings,
    audit: auditSettings
  }

  localStorage.setItem('userSettings', JSON.stringify(allSettings))

  let settingsSavedToDb = true
  try {
    const payload = {
      systemName: generalSettings.systemName,
      language: generalSettings.language,
      timezone: generalSettings.timezone,
      emailEnabled: emailSettings.emailEnabled,
      smtpHost: emailSettings.smtpHost,
      smtpPort: emailSettings.smtpPort,
      smtpUser: emailSettings.smtpUser,
      smtpSecure: emailSettings.smtpSecure,
      fromEmail: emailSettings.fromEmail,
      toEmail: emailSettings.toEmail,
      logUserActivity: auditSettings.logUserActivity,
      retentionPeriod: auditSettings.retentionPeriod
    }
    // Only send smtpPassword if the user actually typed a new one,
    // so an empty field doesn't wipe out the saved password.
    if (emailSettings.smtpPassword) {
      payload.smtpPassword = emailSettings.smtpPassword
    }

    await axios.put(`${API_BASE_URL}/SystemSettings`, payload)
  } catch (err) {
    settingsSavedToDb = false
    console.error('Gagal simpan system settings ke database, kekal tersimpan di local sahaja:', err)
  }

  try {
    await axios.post(`${API_BASE_URL}/AuditLog`, {
      crtUserId: profileSettings.fullName || 'Admin',
      action: 'Update',
      details: 'System settings updated'
    })
  } catch (err) {
    console.error('Gagal simpan audit log ke database:', err)
  }

  emailSettings.smtpPassword = ''
  profileSettings.password = ''

  setTimeout(() => {
    saving.value = false
    if (settingsSavedToDb) {
      showNotification(
        '✅ Settings Saved',
        'All system settings updated and synced to the database.',
        'success',
        'check-circle'
      )
    } else {
      showNotification(
        '⚠️ Saved Locally Only',
        'Settings saved on this device, but syncing to the database failed. Please check your connection.',
        'warning',
        'alert-circle'
      )
    }
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

onMounted(() => {
  loadSystemSettingsFromServer()
})

</script>

<style scoped>
.disabled-section {
  opacity: 0.6;
}

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