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

    <!-- ROLE SWITCH -->
    <v-card variant="outlined" class="rounded-xl pa-3 mb-4">
      <div class="d-flex align-center flex-wrap ga-3">
        <span class="text-caption font-weight-bold text-grey-darken-1">I AM A</span>
        <v-btn-toggle v-model="role" density="comfortable" color="primary" variant="outlined" divided class="rounded-lg">
          <v-btn value="tester" size="small" class="text-capitalize" prepend-icon="mdi-account-check-outline">Tester</v-btn>
          <v-btn value="admin" size="small" class="text-capitalize" prepend-icon="mdi-shield-account-outline">Admin</v-btn>
        </v-btn-toggle>
        <span class="text-caption text-grey">{{ roleHint }}</span>
      </div>
    </v-card>

    <!-- QUICK JUMP -->
    <v-card v-if="visibleSections.length" variant="outlined" class="rounded-xl pa-3 mb-6">
      <div class="d-flex flex-wrap ga-2">
        <v-chip
          v-for="section in visibleSections"
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

    <!-- SECTIONS -->
    <v-expansion-panels v-if="visibleSections.length" v-model="openPanels" multiple variant="accordion" class="rounded-xl guide-panels">
      <v-expansion-panel
        v-for="section in visibleSections"
        :key="section.id"
        :id="section.id"
        :value="section.id"
        class="rounded-lg mb-2 guide-panel"
      >
        <v-expansion-panel-title>
          <div class="d-flex align-center">
            <v-icon color="primary" class="mr-3">{{ section.icon }}</v-icon>
            <div>
              <div class="font-weight-bold d-flex align-center flex-wrap" style="gap: 6px;">
                {{ section.title }}
                <v-chip
                  v-for="r in section.roles.filter((x) => x === role)"
                  :key="r"
                  size="x-small"
                  label
                  :color="r === 'admin' ? 'purple' : 'teal'"
                  variant="tonal"
                  class="font-weight-bold"
                >{{ r === 'admin' ? 'Admin' : 'Tester' }}</v-chip>
              </div>
              <div class="text-caption text-grey">{{ section.tagline }}</div>
            </div>
          </div>
        </v-expansion-panel-title>
        <v-expansion-panel-text>
          <div
            v-for="(step, i) in stepsFor(section)"
            :key="i"
            class="d-flex align-start guide-step"
          >
            <v-icon size="18" color="success" class="mr-2 mt-1">mdi-check-circle-outline</v-icon>
            <div>
              <div class="font-weight-medium d-flex align-center flex-wrap" style="gap: 6px;">
                <template v-if="section.flow">{{ i + 1 }}.&nbsp;</template>{{ step.title }}
              </div>
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
import { ref, computed } from 'vue'

defineEmits(['open-help'])

const role = ref(null)

const roleHint = computed(() => {
  if (role.value === 'tester') return 'Run your assigned test cases, log defects and submit feedback.'
  if (role.value === 'admin') return 'Create test cases and cycles, review results and approve.'
  return 'Pick your role to see the guide.'
})

// roles: who the section is for. A step may carry its own `role` to be shown only for that role.
const sections = [
  {
    id: 'guide-overview',
    title: 'How It Works',
    tagline: 'The full flow from test case to approval',
    icon: 'mdi-map-marker-path',
    roles: ['admin', 'tester'],
    flow: true,
    steps: [
      { title: 'Admin creates a Test Case', detail: 'A Test Case holds the title, description and the ordered Test Steps with their Expected Results. It is placed in a Test Cycle (a testing round).', role: 'admin' },
      { title: 'Testers are assigned', detail: 'Each test case is assigned to its testers (the default testers are added automatically). Every tester runs the same steps independently.', role: 'admin' },
      { title: 'Run your own Test Run', detail: 'Open Test Cases, find the test case assigned to you and click Start. You get your own Run ID (RUN-001, RUN-002 …) and your own results. Other testers\' results never overwrite yours.', role: 'tester' },
      { title: 'Log defects as you go', detail: 'Do each step one at a time. When a step fails, log a defect for it; a ticket ID is generated for you.', role: 'tester' },
      { title: 'Submit the run and get your report', detail: 'After the last step, give an overall result, a rating and your feedback, then submit. You can download the run report as a PDF.', role: 'tester' },
      { title: 'Admin reviews and approves', detail: 'The admin checks each tester\'s results, defects and feedback in Feedback and the test case details, then chooses Approve or Not Approve.', role: 'admin' }
    ],
    tip: 'Menu: Dashboard, Test Cases (with a Test Cycles tab), Feedback, Audit Log, User Guide, Settings and Profile.'
  },
  {
    id: 'guide-dashboard',
    title: 'Dashboard',
    tagline: 'Your at-a-glance view of testing progress',
    icon: 'mdi-view-dashboard',
    roles: ['admin', 'tester'],
    steps: [
      { title: 'Check overall status', detail: 'The top cards show Total Test Cases, the Pass Rate (with Pass, Fail and Pending counts) and Total Passed.' },
      { title: 'Jump to a filtered list', detail: 'Click the Total Test Cases or Total Passed card, or the Pass, Fail or Pending count, to open the Test Cases page already filtered by that status.' },
      { title: 'Compare priorities', detail: 'Test Case Performance by Priority shows how many test cases you have at each priority: Critical, High, Medium and Low.' },
      { title: 'See the latest test cases', detail: 'Recent Test Cases lists the five newest ones with their ID, department, priority, status and assigned testers. Click View All to open the full list.' },
      { title: 'Create a test case quickly', detail: 'Click New Test Case at the top right to go to the Test Cases page.', role: 'admin' },
      { title: 'Use your profile menu', detail: 'Click your avatar at the top right to edit your profile, open Settings, open My Test Cases or log out.' }
    ]
  },
  {
    id: 'guide-run',
    title: 'Running a Test',
    tagline: 'Execute a test case step by step',
    icon: 'mdi-play-circle-outline',
    roles: ['tester'],
    steps: [
      { title: 'Open your test case', detail: 'Go to Test Cases and click the row of the test case (or the people icon "View tester runs"). This opens the Runs & results page.' },
      { title: 'Start your run', detail: 'Find your own name in the list and click Start. If you already have a run, the button says Continue (unfinished) or Run again (finished).' },
      { title: 'Execute one step at a time', detail: 'Read the step and its Expected Result, choose Pass or Fail, then click Submit Step. The next step stays locked until the current one is saved. Use the Previous and Next arrows to review steps you already did.' },
      { title: 'Log a defect when a step fails', detail: 'Fill in Actual Result, Defect Description, Affected / Impact and Comment (all required). Severity and a screenshot or log file are optional. The Ticket ID is generated automatically. You cannot submit the run until every failed or blocked step has a defect logged.' },
      { title: 'Submit the run', detail: 'After the last step, set the Overall Result, give a Rating (1 to 5) and write your Overall Feedback, then click Submit Test Run.' },
      { title: 'See your result', detail: 'Back on the Runs & results page, click a finished run to open its report page and download the Test Case Run Report as a PDF.' }
    ],
    tip: 'Each tester has their own run. Never worry about overwriting someone else\'s results.'
  },
  {
    id: 'guide-testcases',
    title: 'Test Cases',
    tagline: 'Create, assign, and manage test cases',
    icon: 'mdi-format-list-checks',
    roles: ['admin', 'tester'],
    steps: [
      { title: 'Create a test case', detail: 'Click New Test Case. On the Header tab enter the title (the Test Case ID is generated for you, e.g. TC261007001). On the Description tab describe the test, and on the Steps tab add each step with its Expected Result.', role: 'admin' },
      { title: 'Choose the cycle', detail: 'A new test case is placed in the newest Test Cycle automatically. You can pick another cycle from the list or type a new name. The Version Tag follows the selected cycle.', role: 'admin' },
      { title: 'Set the details', detail: 'On the right side set State, Priority, Department and Module / Feature.', role: 'admin' },
      { title: 'Assigned testers', detail: 'Testers are assigned automatically. Remove a tester only if they have no recorded run yet; otherwise the system tells you the tester could not be unassigned.', role: 'admin' },
      { title: 'Edit or delete', detail: 'Use the pencil icon to edit and the bin icon to delete a test case.', role: 'admin' },
      { title: 'View details', detail: 'Click the eye icon to open the Test Case Details page: overview, description, steps, feedback and the admin approval status.' },
      { title: 'See the flow', detail: 'Click the arrow beside the ID to expand Test Case → Assignment → Run, with each tester\'s latest run. Testers can start or re-run from there.' },
      { title: 'Export', detail: 'Use the Excel or PDF button at the top to download a copy of the test case list.', role: 'admin' }
    ],
    tip: 'The list is ordered by Test Case ID, with the first one created at the top. Keep expected results specific; it makes Pass/Fail decisions faster.'
  },
  {
    id: 'guide-testcycles',
    title: 'Test Cycles',
    tagline: 'Group test cases into a testing round',
    icon: 'mdi-sync',
    roles: ['admin'],
    steps: [
      { title: 'Open the tab', detail: 'In Test Cases, switch to the Test Cycles tab.' },
      { title: 'Create a cycle', detail: 'Click New Test Cycle. Pick the Version Tag (required), then set Assigned To, Status, Department, Start and End date and the Auto-Assign Rule. The Cycle ID is generated for you (e.g. CY261005001).' },
      { title: 'New test cases go to the newest cycle', detail: 'When you create a cycle, test cases created afterwards join it automatically. Existing test cases stay in their original cycle.' },
      { title: 'View a cycle', detail: 'Click the Cycle ID or the eye icon to open the Test Cycle Details page with the cycle information and its test cases.' },
      { title: 'Peek at the test cases', detail: 'Click the arrow beside a Cycle ID to list the test cases inside it without leaving the page.' }
    ]
  },
  {
    id: 'guide-feedback',
    title: 'Feedback',
    tagline: 'Defects and overall feedback from every run',
    icon: 'mdi-comment-text-multiple-outline',
    roles: ['admin', 'tester'],
    steps: [
      { title: 'Defects tab', detail: 'Every defect logged during test runs: test case, step, actual result, severity, ticket, who reported it and when. Filter by severity or search by test case, step, ticket or reporter.' },
      { title: 'Open a defect', detail: 'Click the eye icon to open the Defect Details page with the actual result, comments and any screenshots or files.' },
      { title: 'Overall Feedback tab', detail: 'The overall rating and comment each tester left when submitting a run.' },
      { title: 'Use it to improve', detail: 'Admins use defects and feedback to fix issues or update the test case or system, then run again.', role: 'admin' }
    ]
  },
  {
    id: 'guide-approval',
    title: 'Approval',
    tagline: 'Sign off a test case after review',
    icon: 'mdi-check-decagram-outline',
    roles: ['admin'],
    steps: [
      { title: 'Review the results', detail: 'Open the test case with the eye icon and check the steps, each tester\'s results and the feedback.' },
      { title: 'Decide', detail: 'Click Approve or Not Approve at the top of the Test Case Details page. The status and the decision time are shown under Admin Approval.' },
      { title: 'Check the status', detail: 'The approval status of a test case is shown in its details page and reflected in the Runs & results page.' }
    ]
  },
  {
    id: 'guide-auditlog',
    title: 'Audit Log',
    tagline: 'See who did what, and when',
    icon: 'mdi-history',
    roles: ['admin'],
    steps: [
      { title: 'Open the log', detail: 'Click Audit Log in the left menu.' },
      { title: 'Find an entry', detail: 'Search by Run ID, filter by user, and click Reset to clear the filters.' },
      { title: 'Read an entry', detail: 'Each row shows the name, module, action, who did it and when. Click the eye icon for the Additional Details panel and use Previous / Next to move between entries.' },
      { title: 'Copy a Run ID', detail: 'Use the copy button in the details panel to copy the Run ID.' }
    ]
  },
  {
    id: 'guide-settings',
    title: 'Settings',
    tagline: 'Configure the system to your preference',
    icon: 'mdi-cog',
    roles: ['admin', 'tester'],
    steps: [
      { title: 'General', detail: 'Set the system name, language and timezone.', role: 'admin' },
      { title: 'Email Notifications', detail: 'Turn notifications on and set the SMTP host, port, username, password, from address and default recipient.', role: 'admin' },
      { title: 'Dark Mode', detail: 'Open the Dark Mode tab and switch on Enable Dark Theme. The theme applies to all pages straight away and is remembered the next time you open the system.' },
      { title: 'Profile', detail: 'Update your name, email, role, department, phone, location and bio. Enter a new password only if you want to change it; leave it blank to keep the current one.' },
      { title: 'Audit Log', detail: 'Choose whether user activities are logged and how long logs are kept.', role: 'admin' }
    ],
    tip: 'Click Save Settings to keep changes to General, Email Notifications and the other tabs.'
  },
  {
    id: 'guide-profile',
    title: 'Profile',
    tagline: 'Manage your personal account details',
    icon: 'mdi-account',
    roles: ['admin', 'tester'],
    steps: [
      { title: 'Edit your details', detail: 'Update your full name, email address and role / position.' },
      { title: 'Change your password', detail: 'Enter a new password to change it; leave it blank to keep your current one.' }
    ]
  }
]

const visibleSections = computed(() =>
  role.value ? sections.filter((s) => s.roles.includes(role.value)) : []
)

// Steps for the selected role: untagged steps are for everyone
const stepsFor = (section) =>
  section.steps.filter((st) => !st.role || st.role === role.value)

const openPanels = ref(sections.map((s) => s.id))

const scrollTo = (id) => {
  if (!openPanels.value.includes(id)) openPanels.value = [...openPanels.value, id]
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
  border: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
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