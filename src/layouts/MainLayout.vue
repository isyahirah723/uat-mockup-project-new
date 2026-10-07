<template>
  <v-app :theme="themeName">
    
    <v-navigation-drawer 
      v-model="drawer" 
      app 
      :style="drawerStyle" 
      class="text-white"
      width="260"
    >
      <div class="d-flex align-center pa-4 sidebar-header">
        <div class="sidebar-logo d-flex align-center justify-center mr-3">
          <v-icon color="white" size="20">mdi-clipboard-check-outline</v-icon>
        </div>
        <div>
          <div class="text-subtitle-1 font-weight-bold" style="line-height: 1.1;">UAT TESTING</div>
          <div class="text-caption sidebar-subtitle">Test Management</div>
        </div>
      </div>

      <v-list density="compact" nav class="mt-2 px-2">
        <div class="sidebar-eyebrow">WORKSPACE</div>
        <v-list-item prepend-icon="mdi-view-dashboard" title="Dashboard" to="/" exact value="dashboard" class="sidebar-item rounded-lg mb-1"></v-list-item>
        <v-list-item prepend-icon="mdi-format-list-checks" title="Test Cases" to="/test-cases" value="testcases" class="sidebar-item rounded-lg mb-1"></v-list-item>
        <v-list-item prepend-icon="mdi-comment-text-multiple-outline" title="Feedback" to="/feedback" value="feedback" class="sidebar-item rounded-lg mb-1"></v-list-item>

        <div class="sidebar-eyebrow mt-4">SYSTEM</div>
        <v-list-item prepend-icon="mdi-history" title="Audit Log" to="/audit-log" value="auditlog" class="sidebar-item rounded-lg mb-1"></v-list-item>
        <v-list-item prepend-icon="mdi-book-open-page-variant-outline" title="User Guide" to="/user-guide" value="userguide" class="sidebar-item rounded-lg mb-1"></v-list-item>
        <v-list-item prepend-icon="mdi-cog" title="Settings" to="/settings" value="settings" class="sidebar-item rounded-lg mb-1"></v-list-item>
        <v-list-item prepend-icon="mdi-account" title="Profile" to="/profile" value="profile" class="sidebar-item rounded-lg mb-1"></v-list-item>
      </v-list>
    </v-navigation-drawer>

    
    <v-app-bar app flat border-b>
      <v-app-bar-nav-icon @click="drawer = !drawer"></v-app-bar-nav-icon>
      <v-app-bar-title class="font-weight-bold">UAT Management System</v-app-bar-title>
    </v-app-bar>

    
    <v-main :style="mainStyle">
      <router-view />
    </v-main>
  </v-app>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const drawer = ref(true)
const themeName = ref('light')


onMounted(() => {
  const saved = localStorage.getItem('userSettings')
  if (saved) {
    try {
      const parsed = JSON.parse(saved)
      themeName.value = parsed.darkMode ? 'dark' : 'light'
    } catch (e) {
      console.error('Error reading theme settings:', e)
    }
  }
})


window.__toggleTheme = (isDark) => {
  themeName.value = isDark ? 'dark' : 'light'
}


const drawerStyle = computed(() => {
  return themeName.value === 'dark' 
    ? { 
        backgroundImage: 'linear-gradient(160deg, #4a1470 0%, #6d1f9e 35%, #9b27b0 70%, #c13fc9 100%)',
        borderRight: '1px solid rgba(255,255,255,0.18)'
      } 
    : { 
        backgroundImage: 'linear-gradient(160deg, #17081f 0%, #3a1052 30%, #7a1a8c 65%, #ab1fae 100%)',
        borderRight: '1px solid rgba(255,255,255,0.1)'
      }
})


const mainStyle = computed(() => {
  return themeName.value === 'dark'
    ? { backgroundColor: '#0f172a' }
    : { backgroundColor: '#f8fafc' }
})
</script>

<style scoped>
.sidebar-header {
  border-bottom: 1px solid rgba(255, 255, 255, 0.15);
}

.sidebar-logo {
  width: 36px;
  height: 36px;
  border-radius: 10px;
  background: linear-gradient(135deg, #7a1a8c 0%, #d125c9 100%);
  box-shadow: 0 3px 10px rgba(171, 31, 174, 0.45);
  flex-shrink: 0;
}

.sidebar-subtitle {
  color: rgba(255, 255, 255, 0.8);
}

.sidebar-eyebrow {
  padding: 8px 12px 4px;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 1px;
  color: rgba(255, 255, 255, 0.7);
}

.sidebar-item {
  color: #ffffff !important;
  transition: background-color 0.15s ease, color 0.15s ease;
}

/* Buang opacity default Vuetify & besarkan font */
.sidebar-item :deep(.v-list-item-title) {
  font-size: 0.95rem;
  font-weight: 500;
  letter-spacing: 0.2px;
  opacity: 1 !important;
  color: #ffffff;
}

.sidebar-item :deep(.v-icon) {
  color: #ffffff !important;
  opacity: 0.9 !important;
}

.sidebar-item :deep(.v-list-item__overlay) {
  display: none;
}

.sidebar-item:hover {
  background-color: rgba(255, 255, 255, 0.14);
}

.sidebar-item.v-list-item--active {
  background: rgba(255, 255, 255, 0.24) !important;
  box-shadow: inset 0 0 0 1px rgba(255, 255, 255, 0.35);
}

.sidebar-item.v-list-item--active :deep(.v-list-item-title) {
  font-weight: 700;
}

.sidebar-item.v-list-item--active :deep(.v-icon) {
  opacity: 1 !important;
}
</style>