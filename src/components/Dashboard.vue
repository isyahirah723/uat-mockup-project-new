<template>
  <v-container fluid class="pa-6" style="min-height: 100vh;">
    <!-- Top Bar: Title & Filters / Actions -->
    <v-row class="mb-6" align="center">
      <v-col cols="12" md="5">
        <div class="text-caption text-grey font-weight-bold">// OVERVIEW</div>
        <div class="text-h5 font-weight-bold">UAT Performance Dashboard</div>
      </v-col>

      <v-col cols="12" md="7" class="d-flex justify-md-end align-center flex-wrap gap-2">
        <!-- Date range (presets) -->
        <v-menu location="bottom end">
          <template v-slot:activator="{ props }">
            <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-calendar-range" class="rounded-lg text-none mr-2 mb-2 mb-sm-0" size="small">
              Date range - {{ dateRangeLabel }}
            </v-btn>
          </template>
          <v-list density="compact" min-width="200">
            <v-list-item
              v-for="p in datePresets"
              :key="p.value"
              :active="datePreset === p.value"
              @click="datePreset = p.value"
            >
              <v-list-item-title>{{ p.title }}</v-list-item-title>
            </v-list-item>
          </v-list>
        </v-menu>

        <!-- Filters -->
        <v-menu :close-on-content-click="false" location="bottom end">
          <template v-slot:activator="{ props }">
            <v-btn v-bind="props" variant="outlined" prepend-icon="mdi-filter-variant" class="rounded-lg text-none mr-2 mb-2 mb-sm-0" size="small">
              Filters<span v-if="activeFilterCount"> ({{ activeFilterCount }})</span>
            </v-btn>
          </template>
          <v-card width="320" class="pa-4 rounded-xl">
            <div class="text-caption text-grey font-weight-bold mb-1">Priority</div>
            <v-chip-group v-model="filters.priority" multiple column class="mb-2">
              <v-chip v-for="p in priorityOptions" :key="p" :value="p" filter variant="outlined" size="small">{{ p }}</v-chip>
            </v-chip-group>
            <div class="text-caption text-grey font-weight-bold mb-1">Status</div>
            <v-chip-group v-model="filters.status" multiple column>
              <v-chip v-for="s in statusOptions" :key="s" :value="s" filter variant="outlined" size="small">{{ s }}</v-chip>
            </v-chip-group>
            <div class="d-flex justify-end mt-2">
              <v-btn variant="text" size="small" class="text-capitalize" :disabled="!activeFilterCount && datePreset === 'all'" @click="resetFilters">Reset all</v-btn>
            </div>
          </v-card>
        </v-menu>

        <!-- New Test Case Button -->
        <v-btn color="primary" class="rounded-lg text-white text-capitalize mr-3 mb-2 mb-sm-0" elevation="2" size="small" @click="openNewTestCase">
          <v-icon left class="mr-1">mdi-plus</v-icon> New Test Case
        </v-btn>

        <!-- User Profile Avatar -->
        <v-menu offset-y :close-on-content-click="false" location="bottom end">
          <template v-slot:activator="{ props }">
            <v-avatar
              color="primary"
              size="40"
              class="font-weight-bold text-white cursor-pointer mb-2 mb-sm-0"
              v-bind="props"
            >
              <v-img v-if="userProfile.avatar" :src="userProfile.avatar" alt="avatar" cover></v-img>
              <span v-else>{{ userInitials }}</span>
            </v-avatar>
          </template>
          <v-card width="300" class="rounded-xl pa-4" variant="outlined">
            <div class="d-flex align-center mb-3 pb-3 border-b">
              <v-avatar color="primary" size="50" class="mr-3 font-weight-bold text-white">
                <v-img v-if="userProfile.avatar" :src="userProfile.avatar" alt="avatar" cover></v-img>
                <span v-else>{{ userInitials }}</span>
              </v-avatar>
              <div>
                <div class="text-subtitle-1 font-weight-bold">{{ userProfile.name }}</div>
                <div class="text-caption text-grey">{{ userProfile.email }}</div>
                <div class="text-caption text-grey">{{ userProfile.role }}</div>
              </div>
            </div>
            <v-list density="compact" style="background: transparent;">
              <v-list-item
                v-for="item in profileMenuItems"
                :key="item.value"
                @click="handleProfileAction(item.value)"
                :prepend-icon="item.icon"
                :title="item.title"
                class="rounded-lg"
              ></v-list-item>
            </v-list>
            <v-divider class="my-2"></v-divider>
            <v-btn color="error" variant="text" size="small" block class="text-capitalize" @click="handleLogout">
              <v-icon left size="small" class="mr-1">mdi-logout</v-icon> Logout
            </v-btn>
          </v-card>
        </v-menu>
      </v-col>
    </v-row>

    <!-- Top Summary Cards Row -->
    <v-row class="mb-6">
      <!-- Card 1: Total Test Cases -->
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 stat-card" variant="outlined" @click="filterByStatus('All')">
          <div class="d-flex justify-space-between align-start">
            <div class="text-subtitle-2 text-grey font-weight-bold">Total Test Cases</div>
            <v-icon color="primary">mdi-chart-line</v-icon>
          </div>
          <div class="text-h3 font-weight-bold text-primary my-1">{{ totalCases }}</div>
          <div class="text-caption text-success font-weight-bold mb-3">
            <v-icon size="small" color="success">mdi-arrow-up</v-icon> Active System Records
          </div>
          <div class="mt-2">
            <v-sparkline
              :model-value="[0, 2, 5, 3, 8, 5, 9, 7, totalCases]"
              color="indigo-accent-4"
              height="45"
              padding="4"
              smooth
              stroke-linecap="round"
              auto-draw
            ></v-sparkline>
          </div>
        </v-card>
      </v-col>

      <!-- Card 2: Test Cycle -->
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 stat-card" variant="outlined">
          <div class="d-flex justify-space-between align-start cursor-pointer" @click="router.push('/test-cases?tab=cycles')">
            <div class="text-subtitle-2 text-grey font-weight-bold">Test Cycle</div>
            <v-icon color="deep-purple">mdi-sync</v-icon>
          </div>
          <div class="d-flex align-baseline my-1 cursor-pointer" @click="router.push('/test-cases?tab=cycles')">
            <div class="text-h3 font-weight-bold text-deep-purple">{{ cycles.length }}</div>
            <div class="text-caption text-grey font-weight-medium ml-2">cycle{{ cycles.length === 1 ? '' : 's' }} in total</div>
          </div>

          <div v-if="cycleSummary.length" class="mt-2">
            <div
              v-for="cy in cycleSummary.slice(0, 3)"
              :key="cy.id"
              class="cycle-row"
              @click="openCycle(cy)"
            >
              <div class="d-flex justify-space-between text-caption">
                <span class="font-weight-bold text-truncate" style="max-width: 62%;">{{ cy.name }}</span>
                <span class="text-grey">{{ cy.passed }} / {{ cy.total }} Passed</span>
              </div>
              <v-progress-linear :model-value="cy.rate" color="deep-purple" height="6" rounded class="mt-1"></v-progress-linear>
            </div>
            <div v-if="cycleSummary.length > 3" class="text-caption text-primary cursor-pointer mt-2" @click="router.push('/test-cases?tab=cycles')">
              +{{ cycleSummary.length - 3 }} more cycles
            </div>
          </div>
          <div v-else class="text-caption text-grey mt-4">No test cycles yet. Create one in the Test Cycles tab.</div>
        </v-card>
      </v-col>

      <!-- Card 3: Total Passed -->
      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 stat-card" variant="outlined" @click="filterByStatus('Passed')">
          <div class="d-flex justify-space-between align-start">
            <div class="text-subtitle-2 text-grey font-weight-bold">Total Passed</div>
            <v-icon color="indigo">mdi-chart-bar</v-icon>
          </div>
          <div class="text-h3 font-weight-bold text-indigo my-1">{{ passedCases }}</div>
          <div class="text-caption text-grey font-weight-medium mb-3">Successful test execution milestone</div>
          <div class="d-flex align-end justify-space-between pt-2 px-2" style="height: 50px;">
            <div class="bg-indigo-lighten-4 rounded-t" style="width: 14px; height: 30px;"></div>
            <div class="bg-indigo-lighten-3 rounded-t" style="width: 14px; height: 40px;"></div>
            <div class="bg-indigo-lighten-2 rounded-t" style="width: 14px; height: 35px;"></div>
            <div class="bg-indigo-darken-1 rounded-t" style="width: 14px; height: 50px;"></div>
            <div class="bg-indigo-lighten-4 rounded-t" style="width: 14px; height: 20px;"></div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Priority Performance Table -->
    <v-row class="mb-6">
      <v-col cols="12">
        <v-card class="pa-5 rounded-xl" variant="outlined">
          <div class="text-subtitle-1 font-weight-bold mb-1">Test Case Performance by Priority</div>
          <div class="text-caption text-grey mb-4">Click a priority to list its test cases, or the defect count to see its defects</div>

          <v-table density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">Priority</th>
                <th class="text-grey font-weight-bold">Total Cases</th>
                <th class="text-grey font-weight-bold">Execution Rate</th>
                <th class="text-grey font-weight-bold">Defects Found</th>
                <th class="text-grey font-weight-bold">Completion Status</th>
                <th style="width: 32px;"></th>
              </tr>
            </thead>
            <tbody>
              <template v-for="pItem in priorityTableData" :key="pItem.priority">
                <tr
                  class="clickable-row"
                  :class="{ 'row-open': expandedPriority === pItem.priority }"
                  tabindex="0"
                  @click="togglePriority(pItem.priority)"
                  @keyup.enter="togglePriority(pItem.priority)"
                >
                  <td class="font-weight-bold">
                    <v-chip size="small" :color="pItem.color" label class="mr-2 font-weight-bold">
                      <v-icon start size="small">{{ pItem.icon }}</v-icon> {{ pItem.priority }}
                    </v-chip>
                  </td>
                  <td class="font-weight-bold">{{ pItem.count }}</td>
                  <td style="width: 25%;">
                    <div class="d-flex align-center">
                      <span class="text-caption font-weight-bold mr-2">{{ pItem.rate }}%</span>
                      <v-progress-linear :model-value="pItem.rate" :color="pItem.color" height="8" rounded></v-progress-linear>
                    </div>
                  </td>
                  <td>
                    <span
                      class="font-weight-bold text-error defect-link"
                      :class="{ 'defect-link--active': pItem.defects > 0 }"
                      @click.stop="pItem.defects > 0 && openPriorityDefects(pItem.priority)"
                    >{{ pItem.defects }}</span>
                  </td>
                  <td>
                    <v-chip size="x-small" variant="tonal" :color="pItem.statusColor" class="font-weight-bold">
                      {{ pItem.statusText }}
                    </v-chip>
                  </td>
                  <td>
                    <v-icon size="small" color="grey">{{ expandedPriority === pItem.priority ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
                  </td>
                </tr>
                <tr v-if="expandedPriority === pItem.priority" class="priority-detail">
                  <td colspan="6" class="pa-0">
                    <div class="px-4 py-3" style="background: rgba(var(--v-theme-primary), 0.04);">
                      <div v-if="casesByPriority(pItem.priority).length">
                        <div
                          v-for="tc in casesByPriority(pItem.priority)"
                          :key="tc.id"
                          class="d-flex align-center justify-space-between py-2 detail-case"
                          @click="openQuickView(tc)"
                        >
                          <div>
                            <span class="font-weight-bold text-primary text-caption mr-3">{{ tc.test_case_code || '#' + tc.id }}</span>
                            <span class="text-body-2 font-weight-medium">{{ tc.title }}</span>
                          </div>
                          <div class="d-flex align-center" style="gap: 12px;">
                            <span class="text-caption text-grey">{{ testersFor(tc.id).join(', ') || 'Not assigned' }}</span>
                            <v-chip size="x-small" :color="getStatusColor(tc.status)" label class="font-weight-bold">{{ tc.status || 'Draft' }}</v-chip>
                          </div>
                        </div>
                      </div>
                      <div v-else class="text-caption text-grey py-2">No {{ pItem.priority }} priority test cases.</div>
                    </div>
                  </td>
                </tr>
              </template>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>

    <!-- Bottom Analytics Section -->
    <v-row class="mb-6">
      <v-col cols="12" md="8">
        <v-card class="pa-5 rounded-xl h-100" variant="outlined">
          <div class="d-flex justify-space-between align-center mb-4">
            <div>
              <div class="text-subtitle-1 font-weight-bold">Test Execution over time</div>
              <div class="text-caption text-grey">Test runs per day. Click a day or a dot for details; click a chip to show or hide a line</div>
            </div>
            <div class="d-flex gap-2">
              <v-chip
                v-for="s in SERIES"
                :key="s.key"
                size="x-small"
                :color="s.vcolor"
                :variant="seriesOn[s.key] ? 'flat' : 'outlined'"
                class="cursor-pointer"
                @click="seriesOn[s.key] = !seriesOn[s.key]"
              >{{ s.label }}</v-chip>
            </div>
          </div>

          <div v-if="chart" class="my-2">
            <svg :viewBox="`0 0 ${CHART.w} ${CHART.h}`" width="100%" style="display: block;">
              <!-- grid + y axis -->
              <g v-for="t in chart.ticks" :key="'t' + t.v">
                <line :x1="CHART.l" :x2="CHART.w - CHART.r" :y1="t.y" :y2="t.y" stroke="#e5e7eb" stroke-width="1" />
                <text :x="CHART.l - 8" :y="t.y + 3" text-anchor="end" font-size="10" fill="#9ca3af">{{ t.v }}</text>
              </g>
              <!-- clickable day bands -->
              <rect
                v-for="d in chart.days"
                :key="'b' + d.key"
                class="day-band"
                :x="d.x - chart.bandW / 2"
                :y="CHART.t"
                :width="chart.bandW"
                :height="CHART.h - CHART.t - CHART.b"
                fill="transparent"
                @click="openDayDrill(d)"
              >
                <title>{{ d.label }}: {{ d.pass }} pass, {{ d.fail }} fail, {{ d.pending }} pending</title>
              </rect>
              <!-- lines -->
              <template v-for="ln in chart.lines" :key="ln.key">
                <g v-if="seriesOn[ln.key]">
                  <polyline :points="ln.points" fill="none" :stroke="ln.color" stroke-width="2.5" stroke-linejoin="round" stroke-linecap="round" style="pointer-events: none;" />
                  <circle
                    v-for="dot in ln.dots.filter(x => x.v > 0)"
                    :key="ln.key + dot.day.key"
                    class="chart-dot"
                    :cx="dot.x"
                    :cy="dot.y"
                    r="4.5"
                    :fill="ln.color"
                    stroke="#fff"
                    stroke-width="1.5"
                    @click.stop="openDayDrill(dot.day, ln.key)"
                  >
                    <title>{{ dot.day.label }} - {{ ln.label }}: {{ dot.v }}</title>
                  </circle>
                </g>
              </template>
              <!-- x labels -->
              <template v-for="d in chart.days" :key="'x' + d.key">
                <text v-if="d.showLabel" :x="d.x" :y="CHART.h - 8" text-anchor="middle" font-size="10" fill="#9ca3af" style="pointer-events: none;">{{ d.label }}</text>
              </template>
            </svg>
          </div>
          <div v-else class="d-flex align-center justify-center text-caption text-grey" style="height: 140px;">
            No test runs recorded yet. Runs show up here once a tester executes a test case.
          </div>
        </v-card>
      </v-col>

      <v-col cols="12" md="4">
        <v-card class="pa-5 rounded-xl h-100 d-flex flex-column justify-space-between" variant="outlined">
          <div>
            <div class="text-subtitle-1 font-weight-bold">Defects by Severity</div>
            <div class="text-caption text-grey mb-4">Click a segment or a severity to list its defects</div>
          </div>

          <div class="d-flex justify-center align-center my-auto py-2">
            <div style="position: relative; width: 140px; height: 140px;">
              <svg viewBox="0 0 140 140" width="140" height="140" style="transform: rotate(-90deg);">
                <circle cx="70" cy="70" :r="DONUT_R" fill="none" stroke="#e5e7eb" stroke-width="18" />
                <circle
                  v-for="seg in severitySegments"
                  :key="seg.name"
                  class="donut-seg"
                  cx="70"
                  cy="70"
                  :r="DONUT_R"
                  fill="none"
                  :stroke="seg.color"
                  stroke-width="18"
                  :stroke-dasharray="`${seg.len} ${DONUT_C - seg.len}`"
                  :stroke-dashoffset="-seg.offset"
                  @click="openSeverityDrill(seg.name)"
                >
                  <title>{{ seg.name }}: {{ seg.count }}</title>
                </circle>
              </svg>
              <div class="d-flex flex-column align-center justify-center" style="position: absolute; inset: 0; pointer-events: none;">
                <div class="text-h5 font-weight-bold">{{ defectList.length }}</div>
                <div class="text-caption text-grey">Total Issues</div>
              </div>
            </div>
          </div>

          <div class="d-flex justify-space-around flex-wrap text-caption pt-3 border-t" style="row-gap: 4px;">
            <div
              v-for="sev in severityLegend"
              :key="sev.name"
              class="legend-item"
              :class="{ 'legend-item--active': sev.count > 0 }"
              @click="sev.count > 0 && openSeverityDrill(sev.name)"
            >
              <span class="legend-dot" :style="{ background: sev.color }"></span>{{ sev.name }} ({{ sev.count }})
            </div>
          </div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Recent Test Cases -->
    <v-row>
      <v-col cols="12">
        <v-card class="pa-5 rounded-xl" variant="outlined">
          <div class="d-flex justify-space-between align-center mb-4">
            <div>
              <div class="text-subtitle-1 font-weight-bold">Recent Test Cases</div>
              <div class="text-caption text-grey">Latest test cases and their assigned testers. Click a row for details</div>
            </div>
            <v-btn variant="text" color="primary" size="small" class="text-capitalize" @click="filterByStatus('All')">
              View All <v-icon size="small" class="ml-1">mdi-arrow-right</v-icon>
            </v-btn>
          </div>
          <v-table density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">ID / TITLE</th>
                <th class="text-grey font-weight-bold">DEPT</th>
                <th class="text-grey font-weight-bold">PRIORITY</th>
                <th class="text-grey font-weight-bold">STATUS</th>
                <th class="text-grey font-weight-bold">TESTERS</th>
                <th style="width: 32px;"></th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="item in recentCases"
                :key="item.id"
                class="clickable-row"
                tabindex="0"
                @click="openQuickView(item)"
                @keyup.enter="openQuickView(item)"
              >
                <td class="py-3">
                  <div class="font-weight-bold text-primary text-caption">
                    {{ item.test_case_code || '#' + item.id }}
                  </div>
                  <div class="text-body-2 font-weight-medium">{{ item.title }}</div>
                </td>
                <td>
                  <span class="text-caption font-weight-bold">{{ item.test_department || '-' }}</span>
                </td>
                <td>
                  <v-chip size="x-small" :color="getPriorityColor(item.priority)" label class="font-weight-bold">
                    {{ item.priority || 'Medium' }}
                  </v-chip>
                </td>
                <td>
                  <v-chip size="x-small" :color="getStatusColor(item.status)" label class="font-weight-bold">
                    {{ item.status || 'Draft' }}
                  </v-chip>
                </td>
                <td>
                  <div v-if="testersFor(item.id).length" class="d-flex flex-column py-1" style="row-gap: 4px;">
                    <div v-for="name in testersFor(item.id)" :key="name" class="d-flex align-center">
                      <v-avatar color="primary" size="24" class="mr-2 text-white font-weight-bold" style="font-size: 10px;">
                        {{ initialsOf(name) }}
                      </v-avatar>
                      <span class="text-body-2 font-weight-medium">{{ name }}</span>
                    </div>
                  </div>
                  <span v-else class="text-caption text-grey">Not assigned</span>
                </td>
                <td><v-icon size="small" color="grey">mdi-chevron-right</v-icon></td>
              </tr>
              <tr v-if="recentCases.length === 0">
                <td colspan="6" class="text-center text-grey py-6">
                  No test cases found. Click "New Test Case" to create one.
                </td>
              </tr>
            </tbody>
          </v-table>
        </v-card>
      </v-col>
    </v-row>

    <!-- Drill-down dialog (priority / severity / day) -->
    <v-dialog v-model="drill.open" max-width="900px" scrollable>
      <v-card class="rounded-xl">
        <v-card-title class="d-flex align-center justify-space-between pa-4">
          <div>
            <div class="text-subtitle-1 font-weight-bold">{{ drill.title }}</div>
            <div class="text-caption text-grey">{{ drill.subtitle }}</div>
          </div>
          <v-btn icon variant="text" size="small" @click="drill.open = false"><v-icon>mdi-close</v-icon></v-btn>
        </v-card-title>
        <v-divider></v-divider>

        <v-card-text class="pa-0" style="max-height: 60vh;">
          <!-- Test cases -->
          <v-table v-if="drill.type === 'cases'" density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">ID / TITLE</th>
                <th class="text-grey font-weight-bold">DEPT</th>
                <th class="text-grey font-weight-bold">PRIORITY</th>
                <th class="text-grey font-weight-bold">STATUS</th>
                <th class="text-grey font-weight-bold">TESTERS</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="item in drill.items" :key="item.id" class="clickable-row" @click="openQuickView(item)">
                <td class="py-2">
                  <div class="font-weight-bold text-primary text-caption">{{ item.test_case_code || '#' + item.id }}</div>
                  <div class="text-body-2 font-weight-medium">{{ item.title }}</div>
                </td>
                <td class="text-caption font-weight-bold">{{ item.test_department || '-' }}</td>
                <td>
                  <v-chip size="x-small" :color="getPriorityColor(item.priority)" label class="font-weight-bold">{{ item.priority || 'Medium' }}</v-chip>
                </td>
                <td>
                  <v-chip size="x-small" :color="getStatusColor(item.status)" label class="font-weight-bold">{{ item.status || 'Draft' }}</v-chip>
                </td>
                <td class="text-body-2">{{ testersFor(item.id).join(', ') || 'Not assigned' }}</td>
              </tr>
            </tbody>
          </v-table>

          <!-- Defects -->
          <v-table v-else-if="drill.type === 'defects'" density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">TEST CASE</th>
                <th class="text-grey font-weight-bold">STEP</th>
                <th class="text-grey font-weight-bold">SEVERITY</th>
                <th class="text-grey font-weight-bold">TICKET</th>
                <th class="text-grey font-weight-bold">ACTUAL RESULT</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="d in drill.items" :key="d.key">
                <td class="py-2">
                  <div class="font-weight-bold text-primary text-caption">{{ d.code }}</div>
                  <div class="text-body-2 font-weight-medium">{{ d.title }}</div>
                </td>
                <td class="text-body-2" style="min-width: 140px;">
                  <div v-if="d.stepNo != null || d.stepName">
                    <span v-if="d.stepNo != null" class="font-weight-bold">Step {{ d.stepNo }}</span>
                    <div v-if="d.stepName" class="text-caption text-grey">{{ d.stepName }}</div>
                  </div>
                  <span v-else>-</span>
                </td>
                <td>
                  <v-chip size="x-small" label class="font-weight-bold text-white" :style="{ background: severityColor(d.severity) }">{{ d.severity }}</v-chip>
                </td>
                <td class="text-body-2">{{ d.ticket }}</td>
                <td class="text-body-2" style="max-width: 280px;">{{ d.actual }}</td>
              </tr>
            </tbody>
          </v-table>

          <!-- Runs -->
          <v-table v-else-if="drill.type === 'runs'" density="comfortable" style="background: transparent;">
            <thead>
              <tr>
                <th class="text-grey font-weight-bold">RUN</th>
                <th class="text-grey font-weight-bold">TEST CASE</th>
                <th class="text-grey font-weight-bold">TESTER</th>
                <th class="text-grey font-weight-bold">RESULT</th>
                <th class="text-grey font-weight-bold">STARTED</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="r in drill.items" :key="r.id" class="clickable-row" @click="openRunCase(r)">
                <td class="text-body-2 font-weight-bold">#{{ r.id }}</td>
                <td class="py-2">
                  <div class="font-weight-bold text-primary text-caption">{{ r.code }}</div>
                  <div class="text-body-2 font-weight-medium">{{ r.title }}</div>
                </td>
                <td class="text-body-2">{{ r.who }}</td>
                <td>
                  <v-chip size="x-small" label class="font-weight-bold" :color="outcomeColor(r.outcome)">{{ outcomeLabel(r.outcome) }}</v-chip>
                </td>
                <td class="text-caption">{{ r.when }}</td>
              </tr>
            </tbody>
          </v-table>

          <div v-if="!drill.items.length" class="text-center text-grey py-8">Nothing to show here.</div>
        </v-card-text>

        <template v-if="drill.link">
          <v-divider></v-divider>
          <v-card-actions class="pa-3">
            <v-spacer></v-spacer>
            <v-btn variant="text" class="text-capitalize" @click="drill.open = false">Close</v-btn>
            <v-btn color="primary" variant="tonal" class="text-capitalize" @click="goDrillLink">{{ drill.link.label }}</v-btn>
          </v-card-actions>
        </template>
      </v-card>
    </v-dialog>

    <!-- Quick view for a single test case -->
    <v-dialog v-model="qv.open" max-width="560px">
      <v-card class="rounded-xl" v-if="qv.item">
        <v-card-title class="d-flex align-center justify-space-between pa-4">
          <div>
            <div class="font-weight-bold text-primary text-caption">{{ qv.item.test_case_code || '#' + qv.item.id }}</div>
            <div class="text-subtitle-1 font-weight-bold">{{ qv.item.title }}</div>
          </div>
          <v-btn icon variant="text" size="small" @click="qv.open = false"><v-icon>mdi-close</v-icon></v-btn>
        </v-card-title>
        <v-divider></v-divider>
        <v-card-text class="pa-4">
          <div v-if="qv.item.test_description || qv.item.description" class="text-body-2 mb-4">
            {{ qv.item.test_description || qv.item.description }}
          </div>
          <div class="qv-row"><span class="qv-label">Department</span><span>{{ qv.item.test_department || '-' }}</span></div>
          <div class="qv-row">
            <span class="qv-label">Priority</span>
            <v-chip size="x-small" :color="getPriorityColor(qv.item.priority)" label class="font-weight-bold">{{ qv.item.priority || 'Medium' }}</v-chip>
          </div>
          <div class="qv-row">
            <span class="qv-label">Status</span>
            <v-chip size="x-small" :color="getStatusColor(qv.item.status)" label class="font-weight-bold">{{ qv.item.status || 'Draft' }}</v-chip>
          </div>
          <div v-if="qv.item.approval_status" class="qv-row"><span class="qv-label">Approval</span><span>{{ qv.item.approval_status }}</span></div>
          <div class="qv-row"><span class="qv-label">Testers</span><span>{{ testersFor(qv.item.id).join(', ') || 'Not assigned' }}</span></div>
          <div class="qv-row"><span class="qv-label">Test runs</span><span>{{ runsForCase(qv.item.id) }}</span></div>
          <div class="qv-row"><span class="qv-label">Defects</span><span>{{ defectsForCase(qv.item.id) }}</span></div>
        </v-card-text>
        <v-divider></v-divider>
        <v-card-actions class="pa-3">
          <v-spacer></v-spacer>
          <v-btn variant="text" class="text-capitalize" @click="qv.open = false">Close</v-btn>
          <v-btn color="primary" variant="tonal" class="text-capitalize" @click="goToTestCases">View full details</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Profile Edit Dialog -->
    <v-dialog v-model="editProfileDialog" max-width="480px" persistent>
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between" style="background-color: #1e293b;">
          <span class="text-h6 font-weight-bold">
            <v-icon color="white" class="mr-2">mdi-account-edit</v-icon> Edit Profile
          </span>
          <v-btn icon variant="text" size="small" @click="editProfileDialog = false">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>
        <v-card-text class="pa-6">
          <div class="d-flex flex-column align-center mb-6">
            <v-avatar size="90" color="primary" class="font-weight-bold text-white mb-3">
              <v-img v-if="profileForm.avatar" :src="profileForm.avatar" alt="avatar" cover></v-img>
              <span v-else class="text-h5">{{ userInitials }}</span>
            </v-avatar>
            <v-btn size="small" variant="outlined" prepend-icon="mdi-camera" class="text-capitalize" @click="triggerAvatarUpload">
              Change Photo
            </v-btn>
            <input ref="avatarInput" type="file" accept="image/*" class="d-none" @change="onAvatarChange" />
          </div>
          <v-text-field label="Full Name" v-model="profileForm.name" variant="outlined" density="compact" class="mb-3"></v-text-field>
          <v-text-field label="Email" v-model="profileForm.email" variant="outlined" density="compact" disabled class="mb-3"></v-text-field>
          <v-select label="Role" :items="roleOptions" v-model="profileForm.role" variant="outlined" density="compact" class="mb-3"></v-select>
          <v-text-field label="Department" v-model="profileForm.department" variant="outlined" density="compact"></v-text-field>
        </v-card-text>
        <v-card-actions class="pa-4 border-t">
          <v-spacer></v-spacer>
          <v-btn variant="text" class="text-capitalize" @click="editProfileDialog = false">Cancel</v-btn>
          <v-btn color="indigo-accent-4" class="text-white text-capitalize px-6 rounded-lg" @click="saveProfile">Save</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Logout Dialog -->
    <v-dialog v-model="logoutDialog" max-width="400px">
      <v-card class="rounded-xl">
        <v-card-title class="text-white pa-4" style="background-color: #dc2626;">
          <span class="text-h6 font-weight-bold"><v-icon color="white" class="mr-2">mdi-logout</v-icon> Confirm Logout</span>
        </v-card-title>
        <v-card-text class="pa-6">
          <div class="text-body-1">Are you sure you want to logout?</div>
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
import { ref, reactive, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { testCaseService } from '@/services/testCaseService'

const router = useRouter()
const testCases = ref([])
const loadingTestCases = ref(false)

const userProfile = ref({
  name: 'Intan Syahirah',
  email: 'intan@kotrapharma.com',
  role: 'UAT Tester / Admin',
  department: 'IT',
  avatar: ''
})

const userInitials = computed(() => {
  const name = userProfile.value.name || 'User'
  return name.split(' ').map(w => w.charAt(0)).join('').toUpperCase().slice(0, 2)
})

const profileMenuItems = [
  { title: 'My Profile', value: 'profile', icon: 'mdi-account' },
  { title: 'Settings', value: 'settings', icon: 'mdi-cog' },
  { title: 'My Test Cases', value: 'mycases', icon: 'mdi-clipboard-text' },
]

const roleOptions = [
  'UAT Manager / QA Lead',
  'Business Tester',
  'Compliance Officer / Auditor',
  'UAT Tester / Admin'
]

const editProfileDialog = ref(false)
const avatarInput = ref(null)
const profileForm = ref({ name: '', email: '', role: '', department: '', avatar: '' })
const logoutDialog = ref(false)

const handleProfileAction = (action) => {
  if (action === 'profile') {
    profileForm.value = { ...userProfile.value }
    editProfileDialog.value = true
  } else if (action === 'settings') {
    router.push('/settings')
  } else if (action === 'mycases') {
    router.push('/test-cases?assigned=me')
  }
}

const triggerAvatarUpload = () => avatarInput.value?.click()

const onAvatarChange = (e) => {
  const file = e.target.files[0]
  if (!file) return
  const reader = new FileReader()
  reader.onload = () => { profileForm.value.avatar = reader.result }
  reader.readAsDataURL(file)
}

const saveProfile = () => {
  userProfile.value = { ...userProfile.value, ...profileForm.value }
  editProfileDialog.value = false
}

const handleLogout = () => { logoutDialog.value = true }
const confirmLogout = () => {
  logoutDialog.value = false
  router.push('/login')
}

const filterByStatus = (status) => {
  if (status === 'All') router.push('/test-cases')
  else router.push({ path: '/test-cases', query: { status } })
}

const openNewTestCase = () => { router.push('/test-cases') }

/* ------------------------------------------------------------------ */
/* Header filters (date range + priority + status)                     */
/* ------------------------------------------------------------------ */
const priorityOptions = ['Critical', 'High', 'Medium', 'Low']
const statusOptions = ['Passed', 'Failed', 'Pending']
const filters = reactive({ priority: [], status: [] })

const datePresets = [
  { title: 'All time', value: 'all' },
  { title: 'Last 7 days', value: '7' },
  { title: 'Last 30 days', value: '30' },
  { title: 'This month', value: 'month' }
]
const datePreset = ref('all')

const activeFilterCount = computed(() => filters.priority.length + filters.status.length)
const resetFilters = () => {
  filters.priority = []
  filters.status = []
  datePreset.value = 'all'
}

const caseDate = (c) => {
  const raw = c.testDate || c.executionDate || c.created_at
  if (!raw) return null
  const d = new Date(raw)
  return isNaN(d) ? null : d
}

const isPendingStatus = (s) => !s || s === 'Pending' || s === 'Draft'

const filteredCases = computed(() => {
  let cutoff = null
  const now = new Date()
  if (datePreset.value === '7') cutoff = new Date(now.getTime() - 7 * 86400000)
  else if (datePreset.value === '30') cutoff = new Date(now.getTime() - 30 * 86400000)
  else if (datePreset.value === 'month') cutoff = new Date(now.getFullYear(), now.getMonth(), 1)

  return (testCases.value || []).filter((c) => {
    if (filters.priority.length && !filters.priority.includes(c.priority)) return false
    if (filters.status.length) {
      const ok = filters.status.some((s) => (s === 'Pending' ? isPendingStatus(c.status) : c.status === s))
      if (!ok) return false
    }
    if (cutoff) {
      const d = caseDate(c)
      if (d && d < cutoff) return false
    }
    return true
  })
})

const filteredIds = computed(() => new Set(filteredCases.value.map((c) => c.id)))
const caseById = computed(() => Object.fromEntries((testCases.value || []).map((c) => [c.id, c])))

/* ------------------------------------------------------------------ */
/* Summary numbers                                                     */
/* ------------------------------------------------------------------ */
const totalCases = computed(() => filteredCases.value.length)
const passedCases = computed(() => filteredCases.value.filter(c => c.status === 'Passed').length)
const failedCases = computed(() => filteredCases.value.filter(c => c.status === 'Failed').length)
const pendingCases = computed(() => filteredCases.value.filter(c => c.status === 'Pending' || c.status === 'Draft').length)

const passRate = computed(() => {
  if (totalCases.value === 0) return 0
  return Math.round((passedCases.value / totalCases.value) * 100)
})

const dateRangeLabel = computed(() => {
  if (datePreset.value !== 'all') return datePresets.find((p) => p.value === datePreset.value)?.title
  if (!filteredCases.value.length) return 'No test cases yet'
  const dates = filteredCases.value.map(caseDate).filter(Boolean).sort((a, b) => a - b)
  if (!dates.length) return 'Recent'
  const options = { month: 'short', day: 'numeric', year: 'numeric' }
  const startDate = dates[0].toLocaleDateString('en-US', options)
  const endDate = dates[dates.length - 1].toLocaleDateString('en-US', options)
  return startDate === endDate ? startDate : `${startDate} - ${endDate}`
})

const priorityCount = computed(() => {
  const counts = { Low: 0, Medium: 0, High: 0, Critical: 0 }
  filteredCases.value.forEach(c => {
    if (counts[c.priority] !== undefined) counts[c.priority]++
  })
  return counts
})

/* ------------------------------------------------------------------ */
/* Runs, steps and defects (real data)                                 */
/* ------------------------------------------------------------------ */
const runs = ref([])
const execSteps = ref([])
const assignments = ref([])
const users = ref([])
const cycles = ref([])

const userName = (u) => u?.name || u?.full_name || u?.fullName || u?.username || ''

const stepsByRun = computed(() => {
  const map = {}
  execSteps.value.forEach((s) => {
    const k = s.run_id ?? s.test_run_id
    if (k == null) return
    ;(map[k] ||= []).push(s)
  })
  return map
})

const stepStatus = (s) => String(s.execution_status || s.status || '').toLowerCase()

// 'pass' | 'fail' | 'pending'
const runOutcome = (run) => {
  const st = stepsByRun.value[run.id] || []
  const rs = String(run.run_status || '').toLowerCase()
  if (st.some((s) => stepStatus(s).startsWith('fail')) || rs.includes('fail')) return 'fail'
  if (rs.includes('pass')) return 'pass'
  const closed = /complete|closed|done|finish/.test(rs)
  if (closed && st.length && st.every((s) => stepStatus(s).startsWith('pass'))) return 'pass'
  return 'pending'
}

const runDate = (run) => {
  const raw = run.started_at || run.created_at || run.completed_at
  if (!raw) return null
  const d = new Date(raw)
  return isNaN(d) ? null : d
}

const filteredRuns = computed(() => runs.value.filter((r) => filteredIds.value.has(r.test_case_id)))

const whoRan = (run) => {
  if (run.executed_by_name) return run.executed_by_name
  const u = users.value.find((x) => x.id === run.executed_by)
  return userName(u) || (run.executed_by != null ? `User #${run.executed_by}` : '-')
}

const runToRow = (x) => {
  const r = x.r
  const tc = caseById.value[r.test_case_id]
  return {
    id: r.id,
    testCaseId: r.test_case_id,
    code: tc?.test_case_code || `#${r.test_case_id}`,
    title: tc?.title || '-',
    who: whoRan(r),
    outcome: x.o,
    when: x.d ? x.d.toLocaleString('en-GB', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' }) : '-'
  }
}

// A defect is a step flagged as defect, or a step that failed.
const defectList = computed(() => {
  const out = []
  const runMap = Object.fromEntries(runs.value.map((r) => [r.id, r]))
  execSteps.value.forEach((s) => {
    const flagged = s.has_defect === true || s.has_defect === 1 || s.hasDefect === true
    const failed = stepStatus(s).startsWith('fail')
    if (!flagged && !failed) return
    const run = runMap[s.run_id ?? s.test_run_id]
    const tcId = s.test_case_id ?? run?.test_case_id
    if (tcId != null && !filteredIds.value.has(tcId)) return
    const tc = caseById.value[tcId]
    out.push({
      key: s.id ?? `${s.run_id}-${s.step_id}-${out.length}`,
      testCaseId: tcId,
      priority: tc?.priority,
      code: tc?.test_case_code || (tcId != null ? `#${tcId}` : '-'),
      title: tc?.title || '-',
      stepNo: s.sequence_order ?? s.step_number ?? s.step_no ?? null,
      stepName: s.step_name || s.description || '',
      severity: s.severity || s.defect_severity || 'Unrated',
      ticket: s.ticket_id || s.ticketId || s.defect_ticket_id || '-',
      actual: s.actual_result || s.actualResult || s.defect_description || '-'
    })
  })
  return out
})

const defectsByPriority = computed(() => {
  const m = {}
  defectList.value.forEach((d) => { m[d.priority] = (m[d.priority] || 0) + 1 })
  return m
})

const runsForCase = (id) => runs.value.filter((r) => r.test_case_id === id).length
const defectsForCase = (id) => defectList.value.filter((d) => d.testCaseId === id).length

const priorityTableData = computed(() => {
  const total = totalCases.value
  const rate = (n) => (total ? Math.round((n / total) * 100) : 0)
  const dc = defectsByPriority.value
  const c = priorityCount.value
  return [
    { priority: 'Critical', count: c.Critical, rate: rate(c.Critical), defects: dc.Critical || 0, color: 'error', icon: 'mdi-alert-circle', statusText: c.Critical > 0 ? 'In Progress' : 'Not Started', statusColor: c.Critical > 0 ? 'warning' : 'grey' },
    { priority: 'High', count: c.High, rate: rate(c.High), defects: dc.High || 0, color: 'warning', icon: 'mdi-alert', statusText: c.High > 0 ? 'In Progress' : 'Not Started', statusColor: c.High > 0 ? 'warning' : 'grey' },
    { priority: 'Medium', count: c.Medium, rate: rate(c.Medium), defects: dc.Medium || 0, color: 'primary', icon: 'mdi-information', statusText: c.Medium > 0 ? 'Active' : 'Not Started', statusColor: 'primary' },
    { priority: 'Low', count: c.Low, rate: rate(c.Low), defects: dc.Low || 0, color: 'grey', icon: 'mdi-arrow-down', statusText: 'Not Started', statusColor: 'grey' }
  ]
})

/* ------------------------------------------------------------------ */
/* Test Execution over time (SVG line chart)                           */
/* ------------------------------------------------------------------ */
const CHART = { w: 640, h: 180, l: 34, r: 14, t: 14, b: 28 }
const SERIES = [
  { key: 'pass', label: 'Pass', vcolor: 'success', color: '#2e7d32' },
  { key: 'fail', label: 'Fail', vcolor: 'error', color: '#b00020' },
  { key: 'pending', label: 'Pending', vcolor: 'warning', color: '#fb8c00' }
]
const seriesOn = reactive({ pass: true, fail: true, pending: true })

const dayKey = (d) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`

const chart = computed(() => {
  const items = filteredRuns.value
    .map((r) => ({ r, d: runDate(r), o: runOutcome(r) }))
    .filter((x) => x.d)
  if (!items.length) return null

  const dayStart = (d) => new Date(d.getFullYear(), d.getMonth(), d.getDate())
  const end = dayStart(new Date(Math.max(...items.map((x) => x.d))))
  let start = dayStart(new Date(Math.min(...items.map((x) => x.d))))
  const cap = new Date(end)
  cap.setDate(cap.getDate() - 13)
  if (start < cap) start = cap

  const days = []
  for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
    days.push({ date: new Date(d), key: dayKey(d), pass: 0, fail: 0, pending: 0, runs: [] })
  }
  const byKey = Object.fromEntries(days.map((d) => [d.key, d]))
  items.forEach((x) => {
    const day = byKey[dayKey(x.d)]
    if (day) { day[x.o]++; day.runs.push(x) }
  })

  const maxV = Math.max(1, ...days.map((d) => Math.max(d.pass, d.fail, d.pending)))
  const plotW = CHART.w - CHART.l - CHART.r
  const plotH = CHART.h - CHART.t - CHART.b
  const xAt = (i) => (days.length === 1 ? CHART.l + plotW / 2 : CHART.l + (i * plotW) / (days.length - 1))
  const yAt = (v) => CHART.t + plotH - (v / maxV) * plotH

  days.forEach((d, i) => {
    d.x = xAt(i)
    d.label = d.date.toLocaleDateString('en-GB', { day: 'numeric', month: 'short' })
    d.showLabel = days.length <= 8 || i % 2 === 0
  })

  const lines = SERIES.map((s) => ({
    ...s,
    points: days.map((d) => `${d.x},${yAt(d[s.key])}`).join(' '),
    dots: days.map((d) => ({ x: d.x, y: yAt(d[s.key]), v: d[s.key], day: d }))
  }))

  const step = Math.max(1, Math.ceil(maxV / 4))
  const ticks = []
  for (let v = 0; v <= maxV; v += step) ticks.push({ v, y: yAt(v) })

  return { days, lines, ticks, bandW: days.length === 1 ? plotW : plotW / (days.length - 1) }
})

const outcomeLabel = (o) => (o === 'pass' ? 'Pass' : o === 'fail' ? 'Fail' : 'Pending')
const outcomeColor = (o) => (o === 'pass' ? 'success' : o === 'fail' ? 'error' : 'warning')

/* ------------------------------------------------------------------ */
/* Defects by Severity (SVG donut)                                     */
/* ------------------------------------------------------------------ */
const DONUT_R = 52
const DONUT_C = 2 * Math.PI * DONUT_R
const SEVERITY_COLORS = { Critical: '#b71c1c', High: '#f59e0b', Medium: '#1565c0', Low: '#9e9e9e', Unrated: '#78909c' }
const severityColor = (s) => SEVERITY_COLORS[s] || SEVERITY_COLORS.Unrated

const severityCounts = computed(() => {
  const m = {}
  defectList.value.forEach((d) => { m[d.severity] = (m[d.severity] || 0) + 1 })
  return m
})

const severityLegend = computed(() => {
  const names = ['Critical', 'High', 'Medium', 'Low']
  if (severityCounts.value.Unrated) names.push('Unrated')
  return names.map((n) => ({ name: n, count: severityCounts.value[n] || 0, color: severityColor(n) }))
})

const severitySegments = computed(() => {
  const total = defectList.value.length
  if (!total) return []
  let offset = 0
  return severityLegend.value
    .filter((s) => s.count > 0)
    .map((s) => {
      const len = (s.count / total) * DONUT_C
      const seg = { ...s, len, offset }
      offset += len
      return seg
    })
})

/* ------------------------------------------------------------------ */
/* Test cycles + inline priority expand                                */
/* ------------------------------------------------------------------ */
const cycleName = (c) => c.name || c.cycle_name || c.title || c.cycle_code || `Cycle #${c.id}`

const cycleSummary = computed(() =>
  [...cycles.value].reverse().map((c) => {
    const tcs = filteredCases.value.filter((t) => t.cycle_id === c.id)
    const passed = tcs.filter((t) => t.status === 'Passed').length
    return { id: c.id, name: cycleName(c), total: tcs.length, passed, rate: tcs.length ? Math.round((passed / tcs.length) * 100) : 0 }
  })
)

const openCycle = (cy) => router.push({ name: 'TestCycleDetail', params: { id: cy.id } })

const expandedPriority = ref(null)
const togglePriority = (p) => { expandedPriority.value = expandedPriority.value === p ? null : p }
const casesByPriority = (p) => filteredCases.value.filter((c) => c.priority === p)

/* ------------------------------------------------------------------ */
/* Drill-down + quick view                                             */
/* ------------------------------------------------------------------ */
const drill = reactive({ open: false, type: 'cases', title: '', subtitle: '', items: [], link: null })
const qv = reactive({ open: false, item: null })

const openDrill = (cfg) => {
  Object.assign(drill, { type: 'cases', title: '', subtitle: '', items: [], link: null }, cfg, { open: true })
}

const openPriorityDrill = (priority) => {
  const items = filteredCases.value.filter((c) => c.priority === priority)
  openDrill({
    type: 'cases',
    title: `${priority} priority test cases`,
    subtitle: `${items.length} test case${items.length === 1 ? '' : 's'}`,
    items,
    link: { label: 'Open in Test Cases', to: { path: '/test-cases', query: { priority } } }
  })
}

const openPriorityDefects = (priority) => {
  const items = defectList.value.filter((d) => d.priority === priority)
  openDrill({
    type: 'defects',
    title: `${priority} priority defects`,
    subtitle: `${items.length} defect${items.length === 1 ? '' : 's'} found`,
    items,
    link: { label: 'Open Feedback', to: { path: '/feedback' } }
  })
}

const openSeverityDrill = (severity) => {
  const items = defectList.value.filter((d) => d.severity === severity)
  openDrill({
    type: 'defects',
    title: `${severity} defects`,
    subtitle: `${items.length} defect${items.length === 1 ? '' : 's'} logged`,
    items,
    link: { label: 'Open Feedback', to: { path: '/feedback' } }
  })
}

const openDayDrill = (day, outcome = null) => {
  const items = day.runs.filter((x) => !outcome || x.o === outcome).map(runToRow)
  openDrill({
    type: 'runs',
    title: `Test runs on ${day.label}`,
    subtitle: outcome
      ? `${items.length} ${outcomeLabel(outcome).toLowerCase()} run${items.length === 1 ? '' : 's'}`
      : `${items.length} run${items.length === 1 ? '' : 's'}: ${day.pass} pass, ${day.fail} fail, ${day.pending} pending`,
    items
  })
}

const goDrillLink = () => {
  if (!drill.link) return
  drill.open = false
  router.push(drill.link.to)
}

const openQuickView = (item) => {
  qv.item = item
  qv.open = true
}

const goToTestCases = () => {
  const id = qv.item?.id
  qv.open = false
  drill.open = false
  router.push(id != null ? { name: 'TestCaseDetail', params: { id } } : '/test-cases')
}

const openRunCase = (row) => {
  if (row.testCaseId == null) return
  drill.open = false
  router.push({ name: 'TestCaseRuns', params: { id: row.testCaseId } })
}

/* ------------------------------------------------------------------ */
/* Recent test cases                                                   */
/* ------------------------------------------------------------------ */
const recentCases = computed(() => [...filteredCases.value].reverse().slice(0, 5))

const getPriorityColor = (p) => p === 'Critical' ? 'red' : p === 'High' ? 'orange' : p === 'Medium' ? 'primary' : 'grey'
const getStatusColor = (s) => s === 'Passed' ? 'success' : s === 'Failed' ? 'error' : s === 'Pending' ? 'warning' : 'grey'

// Tester sebenar datang dari TestAssignments + Users (bukan medan assigned_to)
const testersFor = (testCaseId) => {
  const names = assignments.value
    .filter((a) => a.test_case_id === testCaseId)
    .map((a) => {
      const u = users.value.find((x) => x.id === a.user_id)
      return userName(u) || userName(a.user)
    })
    .filter(Boolean)
  return [...new Set(names)]
}

const initialsOf = (name = '') => {
  const parts = String(name).trim().split(/\s+/).filter(Boolean)
  return ((parts[0]?.[0] || '') + (parts[1]?.[0] || '')).toUpperCase() || '?'
}

/* ------------------------------------------------------------------ */
/* Data loading                                                        */
/* ------------------------------------------------------------------ */
const API = 'https://localhost:7049/api'
const getList = (url) => fetch(url).then((r) => (r.ok ? r.json() : [])).catch(() => [])
const asArray = (v) => (Array.isArray(v) ? v : [])

const fetchAssignees = async () => {
  const [a, u] = await Promise.all([getList(`${API}/TestAssignments`), getList(`${API}/Users`)])
  assignments.value = asArray(a)
  users.value = asArray(u)
}

const fetchCycles = async () => { cycles.value = asArray(await getList(`${API}/TestCycles`)) }

const fetchRunData = async () => {
  runs.value = asArray(await getList(`${API}/TestRuns`))
  let steps = asArray(await getList(`${API}/ExecutionSteps`))
  // Fallback: if there is no "get all" endpoint, load steps run by run
  if (!steps.length && runs.value.length) {
    const parts = await Promise.all(runs.value.map((r) => getList(`${API}/ExecutionSteps/by-run/${r.id}`)))
    steps = parts.flatMap(asArray)
  }
  execSteps.value = steps
}

const fetchTestCases = async () => {
  loadingTestCases.value = true
  try {
    const res = await testCaseService.getAll()
    testCases.value = asArray(res.data)
  } catch (error) {
    console.error('Error fetching test cases:', error)
  } finally {
    loadingTestCases.value = false
  }
}

onMounted(() => {
  fetchTestCases()
  fetchAssignees()
  fetchRunData()
  fetchCycles()
})
</script>

<style scoped>
.stat-card {
  transition: all 0.2s ease-in-out;
}
.stat-card:hover {
  transform: translateY(-4px);
  border-color: #3b82f6 !important;
  box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
}
.cursor-pointer { cursor: pointer; }
.border-b { border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
.border-t { border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }

.clickable-row { cursor: pointer; transition: background-color 0.15s; }
.clickable-row:hover,
.clickable-row:focus-visible { background: rgba(var(--v-theme-primary), 0.06); outline: none; }

.cycle-row { padding: 6px 8px; margin: 0 -8px; border-radius: 8px; cursor: pointer; }
.cycle-row:hover { background: rgba(0, 0, 0, 0.05); }
.row-open { background: rgba(var(--v-theme-primary), 0.06); }
.detail-case { cursor: pointer; border-bottom: 1px solid rgba(var(--v-border-color), 0.1); }
.detail-case:last-child { border-bottom: none; }
.detail-case:hover { background: rgba(0, 0, 0, 0.04); }

.defect-link { padding: 2px 6px; border-radius: 6px; }
.defect-link--active { cursor: pointer; text-decoration: underline; }
.defect-link--active:hover { background: rgba(var(--v-theme-error), 0.1); }

.day-band { cursor: pointer; }
.day-band:hover { fill: rgba(99, 102, 241, 0.08); }
.chart-dot { cursor: pointer; transition: r 0.1s; }
.chart-dot:hover { r: 6.5; }

.donut-seg { cursor: pointer; transition: stroke-width 0.15s; }
.donut-seg:hover { stroke-width: 22; }

.legend-item { display: flex; align-items: center; padding: 2px 6px; border-radius: 6px; }
.legend-item--active { cursor: pointer; }
.legend-item--active:hover { background: rgba(0, 0, 0, 0.06); }
.legend-dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; margin-right: 6px; }

.qv-row { display: flex; align-items: center; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid rgba(var(--v-border-color), 0.12); font-size: 0.875rem; }
.qv-label { color: #6b7280; font-weight: 600; }
</style>