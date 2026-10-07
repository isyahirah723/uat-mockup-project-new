import { createApp } from 'vue'
import App from './App.vue'
import router from './router'

// Vuetify Setup
import 'vuetify/styles'
import '@mdi/font/css/materialdesignicons.css'
import { createVuetify } from 'vuetify'
import * as components from 'vuetify/components'
import * as directives from 'vuetify/directives'

// Global dark-mode polish (must come after 'vuetify/styles')
import './assets/dark-theme.css'

// Read the saved theme so pages outside MainLayout (e.g. login) start in the right theme.
// MainLayout.vue itself switches the theme at runtime via window.__toggleTheme.
const readSavedDarkMode = () => {
  try {
    const raw = localStorage.getItem('userSettings')
    return raw ? JSON.parse(raw)?.darkMode === true : false
  } catch {
    return false
  }
}

const vuetify = createVuetify({
  components,
  directives,
  theme: {
    defaultTheme: readSavedDarkMode() ? 'dark' : 'light',
    themes: {
      // Layered dark surfaces: page (#0f172a) < cards (#1e293b) so test cases never sink into the background
      dark: {
        colors: {
          background: '#0f172a',
          surface: '#1e293b',
          'surface-bright': '#334155',
          'surface-light': '#273449'
        }
      }
    }
  }
})

const app = createApp(App)

app.use(router)
app.use(vuetify)

app.mount('#app')