import { createVuetify } from 'vuetify'
import 'vuetify/styles'
import { aliases, mdi } from 'vuetify/iconsets/mdi'

export default createVuetify({
  icons: {
    defaultSet: 'mdi',
    aliases,
    sets: {
      mdi,
    },
  },
  theme: {
    defaultTheme: 'light',
    themes: {
      light: {
        colors: {
          primary: '#4338ca',
          secondary: '#6b7280',
          success: '#16a67b',
          error: '#e4543a',
          warning: '#e9a93a',
          background: '#f8fafc',
          surface: '#ffffff',
        },
      },
      dark: {
        colors: {
          primary: '#818cf8',
          secondary: '#9ca3af',
          success: '#34d399',
          error: '#f87171',
          warning: '#fbbf24',
          background: '#0f172a',
          surface: '#1e293b',
          'on-surface': '#f1f5f9',
          'on-background': '#f1f5f9',
        },
      },
    },
  },
})