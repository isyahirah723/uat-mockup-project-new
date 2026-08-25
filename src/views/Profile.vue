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
              v-model="profileSettings.password"
              label="Password"
              type="password"
              placeholder="Enter password"
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
import { ref, reactive } from 'vue'

const saving = ref(false)
const savedData = JSON.parse(localStorage.getItem('userSettings') || '{}')

const profileSettings = reactive({
  fullName: savedData.profile?.fullName || 'intan',
  email: savedData.profile?.email || 'intan@uat-system.com',
  role: savedData.profile?.role || 'Design',
  password: ''
})

const snackbar = ref({
  show: false,
  title: '',
  message: '',
  color: 'success'
})

const saveProfile = () => {
  saving.value = true
  savedData.profile = profileSettings
  localStorage.setItem('userSettings', JSON.stringify(savedData))

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