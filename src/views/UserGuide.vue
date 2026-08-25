<template>
  <v-container class="pa-6 mx-auto" style="max-width: 1100px;">
    <!-- HEADER -->
    <div class="d-flex align-center mb-6">
      <div class="guide-header-icon d-flex align-center justify-center mr-4">
        <v-icon color="white" size="26">mdi-book-open-page-variant-outline</v-icon>
      </div>
      <div>
        <div class="text-caption text-grey font-weight-bold letter-spacing-1">HELP</div>
        <div class="text-h5 font-weight-bold">User Guide</div>
        <div class="text-caption text-grey">How to use every part of the UAT Management System</div>
      </div>
    </div>

    
    <v-card variant="outlined" class="rounded-xl pa-3 mb-6">
      <div class="d-flex flex-wrap ga-2">
        <v-chip
          v-for="section in sections"
          :key="section.id"
          :prepend-icon="section.icon"
          variant="tonal"
          color="primary"
          size="small"
          class="rounded-lg"
          @click="scrollTo(section.id)"
        >
          {{ section.title }}
        </v-chip>
      </div>
    </v-card>

    
    <v-expansion-panels v-model="openPanels" multiple variant="accordion" class="rounded-xl guide-panels">
      <v-expansion-panel
        v-for="(section, index) in sections"
        :key="section.id"
        :id="section.id"
        :value="index"
        class="rounded-lg mb-2 guide-panel"
      >
        <v-expansion-panel-title>
          <div class="d-flex align-center">
            <v-icon color="primary" class="mr-3">{{ section.icon }}</v-icon>
            <div>
              <div class="font-weight-bold">{{ section.title }}</div>
              <div class="text-caption text-grey">{{ section.tagline }}</div>
            </div>
          </div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <div
            v-for="(step, i) in section.steps"
            :key="i"
            class="d-flex align-start guide-step"
          >
            <v-icon size="18" color="success" class="mr-2 mt-1">mdi-check-circle-outline</v-icon>
            <div>
              <div class="font-weight-medium">{{ step.title }}</div>
              <div class="text-body-2 text-grey">{{ step.detail }}</div>
            </div>
          </div>

          <div v-if="section.tip" class="guide-tip mt-3">
            <v-icon size="16" color="primary" class="mr-1">mdi-lightbulb-on-outline</v-icon>
            {{ section.tip }}
          </div>
        </v-expansion-panel-text>
      </v-expansion-panel>
    </v-expansion-panels>

   
    <v-card variant="outlined" class="rounded-xl pa-4 mt-6 d-flex align-center justify-space-between flex-wrap ga-3">
      <div>
        <div class="font-weight-bold">Still need help?</div>
        <div class="text-caption text-grey">Reach the support team directly.</div>
      </div>
      <v-btn color="primary" variant="tonal" prepend-icon="mdi-help-circle" @click="$emit('open-help')">
        Contact Support
      </v-btn>
    </v-card>
  </v-container>
</template>

<script setup>
import { ref } from 'vue'

defineEmits(['open-help'])


const openPanels = ref([0, 1, 2, 3, 4, 5, 6])

const sections = [
  {
    id: 'guide-dashboard',
    title: 'Dashboard',
    tagline: 'Your at-a-glance view of testing progress',
    icon: 'mdi-view-dashboard',
    steps: [
      { title: 'Check overall status', detail: 'See how many test cases are passed, failed, or pending at a glance.' },
      { title: 'Spot active cycles', detail: 'Ongoing test cycles are highlighted so you know what needs attention now.' },
      { title: 'Jump to details', detail: 'Click any summary card to go straight to the related Test Cases or Test Cycles page.' }
    ]
  },
  {
    id: 'guide-testcases',
    title: 'Test Cases',
    tagline: 'Create, assign, and track individual test cases',
    icon: 'mdi-format-list-checks',
    steps: [
      { title: 'Create a test case', detail: 'Click "New Test Case", fill in the title, steps, and expected result.' },
      { title: 'Assign to someone', detail: 'Open a test case and set the "Assigned To" field to the person responsible.' },
      { title: 'Update the status', detail: 'Mark a case as Passed, Failed, or Pending as testing progresses.' },
      { title: 'Export results', detail: 'Use the Excel or PDF button to download a copy of your test cases.' }
    ],
    tip: 'Keep expected results specific — it makes Pass/Fail decisions faster and less subjective.'
  },
  {
    id: 'guide-testcycles',
    title: 'Test Cycles',
    tagline: 'Group test cases into a testing round',
    icon: 'mdi-sync',
    steps: [
      { title: 'Create a cycle', detail: 'Give it a name and version tag (e.g. Sprint 25, v2.0.0) plus a start and end date.' },
      { title: 'Attach test cases', detail: 'Add the test cases that belong to this round of testing.' },
      { title: 'Track cycle progress', detail: 'See how many attached test cases are done versus outstanding.' }
    ]
  },
  {
    id: 'guide-feedback',
    title: 'Feedback',
    tagline: 'Collect comments and ratings on test results',
    icon: 'mdi-comment-text-multiple-outline',
    steps: [
      { title: 'Submit feedback', detail: 'Leave a rating and comment against a specific test case.' },
      { title: 'Review feedback received', detail: 'Browse feedback left by other testers or stakeholders.' }
    ]
  },
  {
    id: 'guide-auditlog',
    title: 'Audit Log',
    tagline: 'See who did what, and when',
    icon: 'mdi-history',
    steps: [
      { title: 'Open the log', detail: 'Go to Settings → Audit Log → View Audit Logs to see recorded activity.' },
      { title: 'Read an entry', detail: 'Each row shows the timestamp, user, action type, and details of what changed.' }
    ]
  },
  {
    id: 'guide-settings',
    title: 'Settings',
    tagline: 'Configure the system to your preference',
    icon: 'mdi-cog',
    steps: [
      { title: 'General', detail: 'Set the system name, language, and timezone.' },
      { title: 'Dark Mode', detail: 'Toggle between light and dark theme across the whole system.' },
      { title: 'Profile', detail: 'Update your name, contact details, and password from the Profile tab.' }
    ]
  },
  {
    id: 'guide-profile',
    title: 'Profile',
    tagline: 'Manage your personal account details',
    icon: 'mdi-account',
    steps: [
      { title: 'Edit your details', detail: 'Update your full name, email, department, phone, and bio.' },
      { title: 'Change your password', detail: 'Enter a new password in Settings → Profile; leave it blank to keep your current one.' }
    ]
  }
]

const scrollTo = (id) => {
  const el = document.getElementById(id)
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'start' })
  }
}
</script>

<style scoped>
.letter-spacing-1 {
  letter-spacing: 1px;
}

.guide-header-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  background: linear-gradient(135deg, #4338ca 0%, #6366f1 100%);
  box-shadow: 0 4px 12px rgba(67, 56, 202, 0.25);
  flex-shrink: 0;
}

.guide-panel {
  border: 1px solid #e2e8f0;
}

.guide-step {
  padding: 6px 0;
}

.guide-tip {
  background-color: rgba(99, 102, 241, 0.08);
  border-radius: 8px;
  padding: 8px 12px;
  font-size: 13px;
  display: flex;
  align-items: center;
}
</style>