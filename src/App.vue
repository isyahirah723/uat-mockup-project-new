<template>
  <v-app>
    <router-view />
  </v-app>
</template>

<script setup>
import { onMounted } from 'vue'
import { useTheme } from 'vuetify'

const theme = useTheme()
const toggleTheme = (isDark) => {
  theme.global.name.value = isDark ? 'dark' : 'light'
}


window.__toggleTheme = toggleTheme

onMounted(() => {
  const saved = localStorage.getItem('userSettings')
  if (saved) {
    try {
      const parsed = JSON.parse(saved)
      const isDark = parsed.darkMode ?? true
      toggleTheme(isDark)
    } catch (e) {
      console.error('Error reading theme settings:', e)
    }
  } else {
    toggleTheme(true) 
  }
})
</script>

<style>
html, body {
  margin: 0;
  padding: 0;
  height: 100%;
  overflow-y: auto;
}
</style>