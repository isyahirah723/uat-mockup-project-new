<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1280px;">
    <div class="d-flex align-center justify-space-between mb-6">
      <div>
        <div class="text-caption text-grey font-weight-bold">// PROFILE</div>
        <div class="text-h5 font-weight-bold">User Profile</div>
      </div>
    </div>

    <v-card variant="outlined" class="rounded-xl">
      <v-card-text class="pa-6">
        <div class="d-flex flex-column align-center mb-6">
          <v-avatar size="90" color="primary" class="font-weight-bold text-white mb-3">
            <v-img v-if="profileSettings.avatar" :src="profileSettings.avatar" alt="avatar" cover></v-img>
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

        <div class="text-h6 font-weight-bold mb-4">Personal Details</div>
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
              v-model="profileSettings.password"
              label="Password"
              type="password"
              placeholder="Leave blank to keep current password"
              variant="outlined"
              density="compact"
            ></v-text-field>
          </v-col>
        </v-row>
      </v-card-text>
      <v-divider></v-divider>
      <v-card-actions class="pa-4">
        <v-spacer></v-spacer>
        <v-btn 
          color="primary" 
          class="px-6 rounded-lg text-capitalize"
          @click="saveProfile"
          :loading="saving"
          prepend-icon="mdi-content-save"
        >
          Save Changes
        </v-btn>
      </v-card-actions>
    </v-card>

    <v-snackbar
      v-model="snackbar.show"
      :color="snackbar.color"
      :timeout="3000"
      location="top end"
      variant="tonal"
      class="rounded-lg"
    >
      <div class="d-flex align-center">
        <v-icon :color="snackbar.color" class="mr-2">mdi-check-circle</v-icon>
        <div>
          <div class="font-weight-bold">{{ snackbar.title }}</div>
          <div class="text-caption">{{ snackbar.message }}</div>
        </div>
      </div>
    </v-snackbar>
  </v-container>
</template>

<script setup>
import { ref, reactive, computed } from 'vue'

const saving = ref(false)
const savedData = JSON.parse(localStorage.getItem('userSettings') || '{}')

const roleOptions = [
  'UAT Manager / QA Lead',
  'Business Tester',
  'Compliance Officer / Auditor',
  'UAT Tester / Admin'
]

const profileSettings = reactive({
  fullName: savedData.profile?.fullName || 'Intan Syafiqah',
  email: savedData.profile?.email || 'intan@kotrapharma.com',
  role: savedData.profile?.role || 'UAT Tester / Admin',
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

const avatarInput = ref(null)

const triggerAvatarUpload = () => {
  avatarInput.value?.click()
}

const onAvatarChange = (e) => {
  const file = e.target.files[0]
  if (!file) return

  if (!file.type.startsWith('image/')) {
    snackbar.value = {
      show: true,
      title: 'Invalid File',
      message: 'Please choose an image file.',
      color: 'error'
    }
    return
  }

  const reader = new FileReader()
  reader.onload = () => {
    profileSettings.avatar = reader.result
  }
  reader.readAsDataURL(file)
}

const snackbar = ref({
  show: false,
  title: '',
  message: '',
  color: 'success'
})

const saveProfile = () => {
  saving.value = true

  const { password, ...profileToSave } = profileSettings
  savedData.profile = {
    ...(savedData.profile || {}),
    ...profileToSave
  }

  if (password) {
    savedData.profile.password = password
  }

  localStorage.setItem('userSettings', JSON.stringify(savedData))
  profileSettings.password = ''

  setTimeout(() => {
    saving.value = false
    snackbar.value = {
      show: true,
      title: 'Profile Updated',
      message: 'Your profile details have been saved successfully.',
      color: 'success'
    }
  }, 500)
}
</script>