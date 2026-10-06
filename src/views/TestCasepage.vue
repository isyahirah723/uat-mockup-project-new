<template>
  <v-container class="pa-4 mx-auto lab-page" style="max-width: 1280px;">
   
    <v-row class="mb-3" align="center">
      <v-col cols="12" sm="6">
        <div class="d-flex align-center" style="gap: 10px;">
          <div class="accent-bar"></div>
          <div>
            <div class="text-h5 page-heading d-flex align-center gap-1" style="font-size: 1.25rem !important;">
              <v-icon color="#0f766e" class="mr-1" size="20">mdi-flask-outline</v-icon>
              Test Case Repository
            </div>
            <div class="page-subheading" style="font-size: 0.75rem;">Every scripted test, tracked from draft to result</div>
            <div v-if="apiLoading" class="text-caption mt-1 d-flex align-center" style="color: #0f766e; font-weight: 600; font-size: 0.7rem;">
              <v-progress-circular indeterminate size="12" width="2" color="#0f766e" class="mr-1"></v-progress-circular>
              Loading data from the server...
            </div>
          </div>
        </div>
      </v-col>

      <v-col v-if="mainTab === 'cases'" cols="12" sm="6" class="d-flex justify-end align-center" style="gap: 8px;">
        <v-btn color="#107c41" size="small" class="rounded-lg text-white font-weight-bold btn-glow" prepend-icon="mdi-microsoft-excel" elevation="1" @click="exportExcel">
          Excel
        </v-btn>
        <v-btn color="#e11d48" size="small" class="rounded-lg text-white font-weight-bold btn-glow" prepend-icon="mdi-file-pdf-box" elevation="1" @click="exportPDF">
          PDF
        </v-btn>
        <v-btn color="#0f766e" size="small" class="text-white rounded-lg text-capitalize font-weight-bold btn-glow" elevation="2" prepend-icon="mdi-plus-circle" @click="openCreateDialog">
          New Test Case
        </v-btn>
      </v-col>
    </v-row>

    
    <v-tabs v-model="mainTab" color="#0f766e" class="mb-3 border-b font-weight-bold">
      <v-tab value="cases"><v-icon start>mdi-flask-outline</v-icon>Test Cases</v-tab>
      <v-tab value="cycles"><v-icon start>mdi-calendar-range</v-icon>Test Cycles</v-tab>
    </v-tabs>

    <!-- TAB: TEST CASES -->
    <div v-show="mainTab === 'cases'">
    <div v-if="cycleFilter !== 'All'" class="mb-2">
      <v-chip closable size="small" color="teal-darken-2" label @click:close="cycleFilter = 'All'">
        Cycle: {{ getCycleName(cycleFilter) }}
      </v-chip>
    </div>
    <v-card variant="outlined" class="rounded-lg pa-0 overflow-hidden main-table-card" elevation="1">
      <v-table hover density="compact" class="test-case-table">
        <thead>
          <tr class="table-header-row">
            <th class="font-weight-bold">ID</th>
            <th class="font-weight-bold">TITLE</th>
            <th class="font-weight-bold">CYCLE</th>
            <th class="font-weight-bold text-center" style="width: 140px;">ACTIONS</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="item in paginatedTestCases" :key="item.id">
          <tr class="test-case-row" title="Click to open runs for this test case" @click="handleRowClick(item)">
            <td class="py-1">
              <div class="d-flex align-center" style="gap: 4px;">
                <v-btn
                  icon
                  size="x-small"
                  variant="tonal"
                  color="teal-darken-2"
                  style="width: 20px; height: 20px;"
                  :title="expandedTestCaseIds.has(item.id) ? 'Hide flow' : 'Show Test Case → Assignment → Run flow'"
                  @click.stop="toggleExpand(item)"
                >
                  <v-icon size="14">{{ expandedTestCaseIds.has(item.id) ? 'mdi-chevron-down' : 'mdi-chevron-right' }}</v-icon>
                </v-btn>
                <span class="id-text font-weight-bold">{{ item.test_case_code || item.id }}</span>
              </div>
            </td>

            <td class="py-1">
              <span class="test-title-text font-weight-bold">{{ item.title }}</span>
              <div class="text-caption text-truncate text-grey-darken-1" style="max-width: 240px; font-size: 0.7rem;">
                {{ item.test_description || 'No description' }}
              </div>
            </td>

            <td class="py-1">
              <v-chip size="x-small" variant="flat" color="#e0f2fe" class="text-light-blue-darken-4 font-weight-bold rounded-lg">
                <v-icon start size="12">mdi-sync</v-icon>
                {{ getCycleName(item.cycle_id) }}
              </v-chip>
            </td>

            <td class="text-center py-1" @click.stop>
              <div class="action-buttons-group">
                <v-btn icon variant="tonal" size="x-small" color="blue-darken-1" title="Edit" style="width: 22px; height: 22px;" @click="openEditDialog(item)">
                  <v-icon size="14">mdi-pencil-outline</v-icon>
                </v-btn>

                <v-btn icon variant="tonal" size="x-small" color="teal-darken-2" title="View Details" style="width: 22px; height: 22px;" @click="openViewDialog(item)">
                  <v-icon size="14">mdi-eye-outline</v-icon>
                </v-btn>

                <v-btn
                  icon
                  variant="tonal"
                  size="x-small"
                  color="purple-darken-1"
                  style="width: 22px; height: 22px;"
                  :disabled="getAssignedUsers(item.id).length === 0"
                  :title="getAssignedUsers(item.id).length === 0 ? 'No testers assigned yet' : 'View tester runs'"
                  @click="openUsersDialog(item)"
                >
                  <v-icon size="14">mdi-account-multiple-outline</v-icon>
                </v-btn>

                <v-btn icon variant="tonal" size="x-small" color="red-darken-1" title="Delete" style="width: 22px; height: 22px;" @click="confirmDelete(item.id)">
                  <v-icon size="14">mdi-delete-outline</v-icon>
                </v-btn>
              </div>
            </td>
          </tr>

         
          <tr v-if="expandedTestCaseIds.has(item.id)">
            <td colspan="4" class="pa-0">
              <div class="expand-panel pa-3">
                <!-- flow bar -->
                <div class="flow-bar mb-3">
                  <div class="flow-bar-step">
                    <div class="flow-bar-icon" style="width: 22px; height: 22px;"><v-icon size="12">mdi-file-document-outline</v-icon></div>
                    <span>Test Case</span>
                  </div>
                  <div class="flow-bar-connector" style="width: 24px; height: 2px;"></div>
                  <div class="flow-bar-step">
                    <div class="flow-bar-icon" style="width: 22px; height: 22px;"><v-icon size="12">mdi-account-multiple-outline</v-icon></div>
                    <span>Assignment</span>
                  </div>
                  <div class="flow-bar-connector" style="width: 24px; height: 2px;"></div>
                  <div class="flow-bar-step">
                    <div class="flow-bar-icon" style="width: 22px; height: 22px;"><v-icon size="12">mdi-play-circle-outline</v-icon></div>
                    <span>Test Run</span>
                  </div>
                </div>

                
                <div class="expand-section mb-2">
                  <div class="expand-section-title" style="font-size: 0.65rem;">Test Case</div>
                  <div class="expand-case-title" style="font-size: 0.85rem;">{{ item.title }}</div>
                </div>

                
                <div class="expand-section mb-2">
                  <div class="expand-section-title" style="font-size: 0.65rem;">Assigned Testers</div>
                  <div v-if="expandedTreeLoading[item.id]" class="text-caption text-grey">Loading…</div>
                  <div v-else-if="(expandedTreeData[item.id] || []).length === 0" class="text-caption text-grey">
                    No tester assigned yet
                  </div>
                  <div v-else class="d-flex flex-wrap" style="gap: 4px;">
                    <v-chip
                      v-for="(t, idx) in expandedTreeData[item.id]"
                      :key="'a-' + idx"
                      size="x-small"
                      variant="tonal"
                      color="teal"
                    >
                      <v-icon start size="12">mdi-account</v-icon>
                      {{ t.user_name }}
                    </v-chip>
                  </div>
                </div>

               
                <div class="expand-section expand-section-last">
                  <div class="expand-section-title" style="font-size: 0.65rem;">Test Run</div>
                  <div class="d-flex align-center justify-space-between" style="gap: 8px;">
                    <span class="text-caption text-grey" style="font-size: 0.75rem;">
                      {{ (expandedTreeData[item.id] || []).length }} tester run(s) for this test case
                    </span>
                    <v-btn
                      size="small"
                      color="#0f766e"
                      class="text-white text-capitalize font-weight-bold rounded-lg py-1"
                      variant="flat"
                      prepend-icon="mdi-play-circle-outline"
                      :disabled="getAssignedUsers(item.id).length === 0"
                      @click.stop="router.push({ name: 'TestCaseRuns', params: { id: item.id } })"
                    >
                      Runs
                    </v-btn>
                  </div>
                </div>

              </div>
            </td>
          </tr>
          </template>

          <tr v-if="filteredTestCases.length === 0 && !apiLoading">
            <td colspan="4" class="text-center text-grey py-4">
              No Test Case records found.
            </td>
          </tr>
        </tbody>
      </v-table>

     
      <div
        v-if="filteredTestCases.length > 0"
        class="d-flex flex-wrap justify-space-between align-center pa-2 px-3 border-t bg-slate-50"
        style="row-gap: 8px;"
      >
        <div class="text-caption text-grey-darken-1 font-weight-medium" style="font-size: 0.75rem;">
          Showing <span class="text-teal-darken-3 font-weight-bold">{{ pageStart }}-{{ pageEnd }}</span> of <span class="text-teal-darken-3 font-weight-bold">{{ filteredTestCases.length }}</span> records
        </div>

        <div class="d-flex align-center" style="gap: 12px;">
          <v-select
            v-model="itemsPerPage"
            :items="itemsPerPageOptions"
            label="Rows per page"
            variant="outlined"
            density="compact"
            hide-details
            style="max-width: 120px;"
            class="custom-field"
          ></v-select>

          <v-pagination
            v-model="currentPage"
            :length="totalPages"
            :total-visible="5"
            density="compact"
            active-color="#0f766e"
          ></v-pagination>
        </div>
      </div>
    </v-card>
    </div>

    <!-- TAB: TEST CYCLES -->
    <TestCyclesPanel
      :key="cyclesPanelKey"
      v-show="mainTab === 'cycles'"
      :test-cases="apiTestCases"
      @changed="onCyclesChanged"
      @view-cases="viewCasesForCycle"
    />

    <v-dialog v-model="dialog" max-width="1000px" persistent scrollable>
      <v-card class="rounded-xl overflow-hidden">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between dialog-header">
          <span class="text-h6 font-weight-bold d-flex align-center">
            <v-icon class="mr-2">{{ isEdit ? 'mdi-pencil-box-outline' : 'mdi-plus-box-outline' }}</v-icon>
            {{ isEdit ? 'Edit Test Case' : 'New Test Case' }}
          </span>
          <v-btn icon variant="text" size="small" @click="dialog = false">
            <v-icon color="white">mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-6" style="max-height: 65vh; overflow-y: auto;">
          <v-tabs v-model="activeTab" color="#0f766e" class="mb-4 border-b font-weight-bold">
            <v-tab value="header"><v-icon start>mdi-card-text-outline</v-icon>Header</v-tab>
            <v-tab value="description"><v-icon start>mdi-text-box-outline</v-icon>Description</v-tab>
            <v-tab value="steps"><v-icon start>mdi-format-list-checks</v-icon>Steps</v-tab>
          </v-tabs>

          <v-window v-model="activeTab">
            <v-window-item value="header">
              <div class="testmo-edit-layout">
                <!-- Main column -->
                <div class="testmo-main-col">
                  <div class="d-flex align-center mb-3" style="gap: 10px;">
                    <div class="testmo-icon-badge"><v-icon size="16" color="white">mdi-file-document-outline</v-icon></div>
                    <span class="text-caption font-weight-bold text-slate-500" style="text-transform: uppercase; letter-spacing: 0.04em;">Name</span>
                  </div>
                  <v-text-field
                    label="Test Title *"
                    v-model="form.title"
                    variant="outlined"
                    density="comfortable"
                    class="custom-field mb-4"
                    hide-details
                  ></v-text-field>

                  <div class="d-flex align-center mb-3" style="gap: 10px;">
                    <div class="testmo-icon-badge"><v-icon size="16" color="white">mdi-identifier</v-icon></div>
                    <span class="text-caption font-weight-bold text-slate-500" style="text-transform: uppercase; letter-spacing: 0.04em;">Test Case ID</span>
                  </div>
                  <v-text-field
                    v-model="form.testCaseId"
                    variant="outlined"
                    density="comfortable"
                    placeholder="Auto-generated by system"
                    readonly
                    hide-details
                    class="custom-field mb-4"
                  ></v-text-field>

                  <div class="d-flex align-center mb-3" style="gap: 10px;">
                    <div class="testmo-icon-badge"><v-icon size="16" color="white">mdi-account-multiple-outline</v-icon></div>
                    <span class="text-caption font-weight-bold text-slate-500" style="text-transform: uppercase; letter-spacing: 0.04em;">Assigned Testers</span>
                  </div>
                  <v-select
                    v-model="form.assignedTesterIds"
                    :items="testerOptions"
                    item-title="full_name"
                    item-value="id"
                    variant="outlined"
                    density="comfortable"
                    multiple
                    chips
                    closable-chips
                    hide-details
                    class="custom-field"
                  ></v-select>
                </div>

                <!-- Properties panel (Testmo-style right column) -->
                <div class="testmo-props-col">
                  <div class="testmo-props-field">
                    <span class="testmo-props-label">Test Cycle</span>
                    <v-combobox
                      v-model="form.cycleInput"
                      :items="cycleNameOptions"
                      @update:model-value="onCycleChange"
                      placeholder="Pilih atau taip nama Test Cycle"
                      variant="outlined"
                      density="compact"
                      clearable
                      hide-details
                      class="custom-field"
                    ></v-combobox>
                    <span
                      v-if="form.cycleInput && !form.cycleId"
                      class="text-caption"
                      style="font-size: 0.65rem; color: #0f766e;"
                    >Cycle baru "{{ form.cycleInput }}" akan dicipta bila Save.</span>
                  </div>

                  <div class="testmo-props-field">
                    <span class="testmo-props-label">State</span>
                    <v-select
                      :items="['Draft', 'Passed', 'Failed', 'Pending']"
                      v-model="form.status"
                      variant="outlined"
                      density="compact"
                      hide-details
                      class="custom-field"
                    ></v-select>
                  </div>

                  <div class="testmo-props-field">
                    <span class="testmo-props-label">Priority</span>
                    <v-select
                      :items="['Low', 'Medium', 'High', 'Critical']"
                      v-model="form.priority"
                      variant="outlined"
                      density="compact"
                      hide-details
                      class="custom-field"
                    ></v-select>
                  </div>

                  <div class="testmo-props-field">
                    <span class="testmo-props-label">Department</span>
                    <v-select
                      :items="departmentOptions"
                      v-model="form.testDepartment"
                      variant="outlined"
                      density="compact"
                      hide-details
                      class="custom-field"
                    ></v-select>
                  </div>

                  <div class="testmo-props-field">
                    <span class="testmo-props-label">Version Tag</span>
                    <v-text-field
                      v-model="form.versionTag"
                      variant="outlined"
                      density="compact"
                      readonly
                      placeholder="Auto from Test Cycle"
                      hide-details
                      class="custom-field"
                    ></v-text-field>
                  </div>

                  <div class="testmo-props-field">
                    <span class="testmo-props-label">Module / Feature</span>
                    <v-select
                      :items="moduleOptions"
                      v-model="form.module"
                      variant="outlined"
                      density="compact"
                      clearable
                      hide-details
                      class="custom-field"
                    ></v-select>
                  </div>
                </div>
              </div>
            </v-window-item>

            <v-window-item value="description">
              <v-textarea label="Test Description" v-model="form.testDescription" variant="outlined" density="compact" rows="3" class="custom-field"></v-textarea>
              <v-textarea label="Test Dependencies" v-model="form.dependencies" variant="outlined" density="compact" rows="2" class="mt-2 custom-field"></v-textarea>
              <v-textarea label="Test Conditions" v-model="form.conditions" variant="outlined" density="compact" rows="2" class="mt-2 custom-field"></v-textarea>
              <v-textarea label="Test Control" v-model="form.control" variant="outlined" density="compact" rows="2" class="mt-2 custom-field"></v-textarea>
            </v-window-item>

            <v-window-item value="steps">
              <div class="d-flex justify-space-between align-center mb-3">
                <div class="text-subtitle-2 font-weight-bold text-teal-darken-3">Test Steps</div>
                <v-btn size="small" color="#0f766e" prepend-icon="mdi-plus" class="text-capitalize text-white font-weight-bold rounded-lg" @click="addStep">Add Step</v-btn>
              </div>
              <div v-for="(step, idx) in form.steps" :key="idx" class="mb-3 pa-3 rounded-xl step-box">
                <div class="d-flex align-center justify-space-between mb-2">
                  <span class="font-weight-bold text-caption text-teal-darken-4">STEP {{ idx + 1 }}</span>
                  <v-btn icon size="x-small" variant="tonal" color="red" @click="removeStep(idx)"><v-icon>mdi-delete</v-icon></v-btn>
                </div>
                <v-row dense>
                  <v-col cols="12" md="8">
                    <v-text-field v-model="step.description" label="Step Description" variant="outlined" density="compact" hide-details class="custom-field"></v-text-field>
                  </v-col>
                  <v-col cols="12" md="4">
                    <v-text-field v-model="step.expected" label="Expected Result" variant="outlined" density="compact" hide-details class="custom-field"></v-text-field>
                  </v-col>
                  <v-col cols="12" sm="6" class="mt-2">
                    <v-select v-model="step.passFail" :items="['Pass', 'Fail', 'N/A']" label="Result" variant="outlined" density="compact" hide-details class="custom-field"></v-select>
                  </v-col>
                  <v-col cols="12" sm="6" class="mt-2">
                    <v-text-field v-model="step.actual" label="Actual Result" variant="outlined" density="compact" hide-details class="custom-field"></v-text-field>
                  </v-col>
                </v-row>
              </div>
              <div v-if="!form.steps || form.steps.length === 0" class="text-center text-grey py-4 text-caption">No step added. Click 'Add Step' above.</div>
            </v-window-item>

          </v-window>
        </v-card-text>

        <v-card-actions class="pa-4 border-t bg-slate-50">
          <v-spacer></v-spacer>
          <v-btn variant="tonal" @click="dialog = false" class="text-capitalize font-weight-bold rounded-lg mr-2">Cancel</v-btn>
          <v-btn color="#0f766e" class="px-6 rounded-lg text-capitalize text-white font-weight-bold btn-glow" :loading="saving" :disabled="saving" @click="saveTestCase">Save Test Case</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

   
    <v-dialog v-model="viewDialog" max-width="800px" scrollable>
      <v-card class="rounded-xl overflow-hidden" v-if="viewItem">
        <v-card-title class="text-white pa-4 d-flex align-center justify-space-between dialog-header">
          <span class="text-h6 font-weight-bold">Test Case Details</span>
          <v-btn icon variant="text" size="small" @click="viewDialog = false"><v-icon color="white">mdi-close</v-icon></v-btn>
        </v-card-title>

        <v-card-text class="pa-6" style="max-height: 65vh; overflow-y: auto;">
          <div class="mb-4">
            <div class="text-caption text-grey font-weight-bold">TITLE</div>
            <div class="text-h6 font-weight-bold text-teal-darken-4">{{ viewItem.title }}</div>
          </div>
          
          <v-row class="mb-2">
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">ID</div>
              <div class="font-weight-bold text-teal-darken-3">{{ viewItem.test_case_code || '#' + viewItem.id }}</div>
            </v-col>
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">DEPARTMENT</div>
              <div>{{ viewItem.test_department || '-' }}</div>
            </v-col>
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">VERSION TAG</div>
              <div>{{ viewItem.version_tag || '-' }}</div>
            </v-col>
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">STATUS</div>
              <v-chip size="x-small" variant="flat" :color="getStatusColor(viewItem.status)" label class="font-weight-bold text-white">
                {{ viewItem.status || 'Draft' }}
              </v-chip>
            </v-col>
          </v-row>

          <v-row class="mb-2">
            <v-col cols="6" sm="3">
              <div class="text-caption text-grey font-weight-bold">PRIORITY</div>
              <v-chip size="x-small" variant="flat" :color="getPriorityColor(viewItem.priority)" label class="font-weight-bold text-white">
                {{ viewItem.priority || 'Medium' }}
              </v-chip>
            </v-col>
            <v-col cols="6" sm="9">
              <div class="text-caption text-grey font-weight-bold mb-1">ASSIGNED TESTERS</div>
              <div v-if="getAssignedUsers(viewItem.id).length" class="d-flex flex-wrap" style="gap: 4px;">
                <v-chip
                  v-for="a in getAssignedUsers(viewItem.id)"
                  :key="a.id"
                  size="small"
                  variant="tonal"
                  color="teal-darken-3"
                  class="font-weight-medium"
                >
                  <v-icon start size="12">mdi-account</v-icon>
                  {{ userNameById[a.user_id] || a.user_name || ('User #' + a.user_id) }}
                </v-chip>
              </div>
              <span v-else class="text-caption text-grey-italic">Unassigned</span>
            </v-col>
          </v-row>

          <v-divider class="my-4"></v-divider>
          
          <div class="mb-4">
            <div class="text-caption text-grey font-weight-bold">DESCRIPTION</div>
            <div class="text-body-2">{{ viewItem.test_description || 'No description' }}</div>
          </div>

          <v-divider class="my-4"></v-divider>
          
          <div class="text-subtitle-2 font-weight-bold mb-2 text-teal-darken-3">TEST STEPS ({{ viewItem.steps ? viewItem.steps.length : 0 }})</div>
          <div v-if="viewItem.steps && viewItem.steps.length > 0">
            <div v-for="(step, idx) in viewItem.steps" :key="idx" class="mb-2 pa-3 rounded-lg step-box">
              <div class="d-flex align-center justify-space-between">
                <div><span class="font-weight-bold mr-2 text-teal-darken-4">#{{ idx+1 }}</span> {{ step.description }}</div>
                <v-chip size="x-small" variant="flat" class="text-white" :color="step.pass_fail === 'Pass' || step.passFail === 'Pass' ? 'green' : step.pass_fail === 'Fail' || step.passFail === 'Fail' ? 'red' : 'grey'">
                  {{ step.pass_fail || step.passFail || 'N/A' }}
                </v-chip>
              </div>
              <div class="text-caption text-grey mt-1">Expected: {{ step.expected || '-' }}</div>
              <div class="text-caption text-grey" v-if="step.actual">Actual: {{ step.actual }}</div>
            </div>
          </div>
          <div v-else class="text-grey text-caption">no steps provided.</div>

          <v-divider class="my-4"></v-divider>
          
          <div class="text-subtitle-2 font-weight-bold mb-2 text-teal-darken-3">FEEDBACK ({{ viewItem.feedbacks ? viewItem.feedbacks.length : 0 }})</div>
          <div v-if="viewItem.feedbacks && viewItem.feedbacks.length > 0">
            <div v-for="(fb, idx) in viewItem.feedbacks" :key="idx" class="mb-2 pa-3 rounded-lg step-box">
              <div class="d-flex align-center justify-space-between">
                <span class="font-weight-bold text-caption">{{ fb.user_name || fb.user || 'Anonymous' }}</span>
                <span class="text-amber-darken-2 font-weight-bold">Rating: {{ fb.rating || 5 }}/5</span>
              </div>
              <div class="text-body-2 mt-1">{{ fb.comment }}</div>
            </div>
          </div>
          <div v-else class="text-grey text-caption">no feedback</div>

          <v-divider class="my-4"></v-divider>

          <div class="text-subtitle-2 font-weight-bold mb-2 text-teal-darken-3">ADMIN APPROVAL</div>
          <div class="d-flex align-center" style="gap: 12px;">
            <v-chip size="small" variant="flat" :color="getApprovalColor(viewItem.approval_status)" label class="font-weight-bold text-white">
              {{ viewItem.approval_status || 'Pending Review' }}
            </v-chip>
            <span class="text-caption text-grey" v-if="viewItem.approved_at">
              decided {{ formatDateTime(viewItem.approved_at) }}
            </span>
          </div>
        </v-card-text>

        <v-card-actions class="pa-4 border-t bg-slate-50">
          <v-btn
            color="red-darken-1"
            variant="tonal"
            class="text-capitalize font-weight-bold rounded-lg"
            :loading="approving"
            @click="approveTestCase(viewItem, false)"
          >
            Not Approve
          </v-btn>
          <v-btn
            color="green-darken-1"
            variant="tonal"
            class="text-capitalize font-weight-bold rounded-lg"
            :loading="approving"
            @click="approveTestCase(viewItem, true)"
          >
            Approve
          </v-btn>
          <v-spacer></v-spacer>
          <v-btn color="#0f766e" variant="text" class="font-weight-bold" @click="viewDialog = false">Close</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

  
    <v-dialog v-model="usersDialog" max-width="640" scrollable>
      <v-card class="rounded-xl overflow-hidden">
        <v-card-title class="d-flex align-center pa-4 text-white dialog-header">
          <div>
            <div class="text-subtitle-1 font-weight-bold">
              {{ selectedTestCaseForRuns?.test_case_code }} — {{ selectedTestCaseForRuns?.title }}
            </div>
            <div class="text-caption" style="opacity: 0.8;">Assigned testers and their run status</div>
          </div>
          <v-spacer></v-spacer>
          <v-btn icon variant="text" size="small" color="white" @click="usersDialog = false">
            <v-icon>mdi-close</v-icon>
          </v-btn>
        </v-card-title>

        <v-card-text class="pa-0" style="max-height: 65vh; overflow-y: auto;">
          <div v-if="usersDialogLoading" class="text-center text-grey py-8">Loading…</div>

          <v-list v-else-if="testCaseUsers.length > 0" lines="two">
            <v-list-item
              v-for="u in testCaseUsers"
              :key="u.user_id"
              :disabled="!u.run_id"
              @click="u.run_id && viewTesterRun(u)"
            >
              <template #prepend>
                <v-avatar color="teal-lighten-4" size="36">
                  <v-icon color="teal-darken-3">mdi-account</v-icon>
                </v-avatar>
              </template>

              <v-list-item-title class="font-weight-medium">{{ u.user_name }}</v-list-item-title>
              <v-list-item-subtitle>
                {{ u.started_at ? formatDateTime(u.started_at) : 'Not started yet' }}
              </v-list-item-subtitle>

              <template #append>
                <v-chip size="small" variant="flat" :color="getRunStatusColor(u.run_status)" label class="font-weight-bold mr-2 text-white">
                  {{ u.run_status }}
                </v-chip>
                <v-icon v-if="u.run_id" size="small" color="grey">mdi-chevron-right</v-icon>
              </template>
            </v-list-item>
          </v-list>

          <div v-else class="text-center text-grey py-8">No testers assigned to this test case.</div>
        </v-card-text>
      </v-card>
    </v-dialog>

    <!-- Dialog Run Detail -->
    <v-dialog v-model="runDetailDialog" max-width="760" scrollable>
      <v-card class="rounded-xl overflow-hidden">
        <v-card-title class="pa-4 text-white dialog-header">
          <div class="d-flex align-center justify-space-between">
            <div>
              <div class="text-subtitle-1 font-weight-bold">{{ selectedUserRun?.user_name }}'s run</div>
              <div class="text-caption" style="opacity: 0.8;">
                {{ selectedTestCaseForRuns?.test_case_code }} · Run #{{ selectedUserRun?.run_id }}
                · started {{ formatDateTime(selectedUserRun?.started_at) }}
              </div>
            </div>
            <v-btn icon variant="text" size="small" color="white" @click="runDetailDialog = false">
              <v-icon>mdi-close</v-icon>
            </v-btn>
          </div>
        </v-card-title>

        <v-card-text class="pa-4">
          <div v-if="runDetailLoading" class="text-center py-8 text-grey">Loading steps…</div>

          <template v-else>
            <v-chip size="small" variant="flat" :color="getRunStatusColor(selectedUserRun?.run_status)" label class="font-weight-bold mb-4 text-white">
              {{ selectedUserRun?.run_status }}
            </v-chip>

            <v-expansion-panels variant="accordion">
              <v-expansion-panel v-for="(step, idx) in runDetailSteps" :key="step.id || idx">
                <v-expansion-panel-title>
                  <div class="d-flex align-center justify-space-between" style="width: 100%;">
                    <span class="font-weight-medium">#{{ step.sequence_order || idx + 1 }} {{ step.step_name }}</span>
                    <v-chip
                      size="small"
                      variant="flat"
                      :color="getExecutionStatusColor(step.execution_status)"
                      label
                      class="font-weight-bold ml-2 text-white"
                    >
                      {{ step.execution_status }}
                    </v-chip>
                  </div>
                </v-expansion-panel-title>
                <v-expansion-panel-text>
                  <div class="text-caption text-grey mb-1">ACTUAL RESULT</div>
                  <div class="mb-3">{{ step.actual_result || '-' }}</div>

                  <div v-if="step.has_defect" class="mb-3">
                    <v-chip size="small" variant="flat" color="red" label class="font-weight-bold mr-2 text-white">Defect logged</v-chip>
                    <span class="text-caption">Severity: {{ step.severity || '-' }}</span>
                    <span v-if="step.ticket_id" class="text-caption"> · Ticket: {{ step.ticket_id }}</span>
                  </div>

                  <div v-if="step.comments" class="text-caption text-grey mb-1">COMMENTS</div>
                  <div v-if="step.comments">{{ step.comments }}</div>
                </v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>

            <div v-if="runDetailSteps.length === 0" class="text-center text-grey py-6">
              No step results logged for this run.
            </div>
          </template>
        </v-card-text>
      </v-card>
    </v-dialog>

  </v-container>
</template>

<script setup>
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import * as XLSX from 'xlsx' 
import { jsPDF } from 'jspdf'
import 'jspdf-autotable'
import { kotraLogoBase64 } from '@/assets/kotraLogo'
import TestCyclesPanel from './TestCyclesPanel.vue'

const router = useRouter()
const route = useRoute()
const apiTestCases = ref([])
const apiLoading = ref(false)
const testCycles = ref([])
const testAssignments = ref([])
const users = ref([])

const fetchUsers = async () => {
  try {
    const res = await fetch('https://localhost:7049/api/Users')
    if (!res.ok) console.error('Failed to load Users: HTTP', res.status)
    users.value = res.ok ? await res.json() : []
  } catch (err) {
    console.error('Error fetching Users:', err)
    users.value = []
  }
}

const userNameById = computed(() => {
  const map = {}
  users.value.forEach((u) => { map[u.id] = u.full_name })
  return map
})

const FIXED_TESTER_NAMES = ['Realdo', 'Vivian']

const fixedTesterUsers = computed(() => {
  return FIXED_TESTER_NAMES
    .map((name) => users.value.find((u) => u.full_name && u.full_name.toLowerCase().includes(name.toLowerCase())))
    .filter(Boolean)
})

const testerOptions = computed(() => users.value)

const fetchFromAPI = async () => {
  apiLoading.value = true
  try {
    const response = await fetch('https://localhost:7049/api/TestCases')
    if (!response.ok) throw new Error('Failed to connect to the API')
    apiTestCases.value = await response.json()
  } catch (error) {
    console.error('Error API:', error)
  } finally {
    apiLoading.value = false
  }
}

const fetchAssignments = async () => {
  try {
    const response = await fetch('https://localhost:7049/api/TestAssignments')
    if (response.ok) testAssignments.value = await response.json()
  } catch (error) {
    console.error('Error fetching TestAssignments:', error)
  }
}

const getAssignedUsers = (testCaseId) => {
  return testAssignments.value.filter((a) => String(a.test_case_id) === String(testCaseId))
}

const assignedNames = (testCaseId) => {
  const assigned = getAssignedUsers(testCaseId)
  if (!assigned.length) return 'Unassigned'
  return assigned
    .map((a) => userNameById.value[a.user_id] || a.user_name || ('User #' + a.user_id))
    .join(', ')
}

const generateTestCaseId = () => {
  const today = new Date()
  const yy = String(today.getFullYear()).slice(-2)
  const mm = String(today.getMonth() + 1).padStart(2, '0')
  const dd = String(today.getDate()).padStart(2, '0')
  const datePrefix = `TC${yy}${mm}${dd}`

  const existingToday = apiTestCases.value.filter(tc => 
    tc.test_case_code && tc.test_case_code.startsWith(datePrefix)
  )

  const nextSeq = String(existingToday.length + 1).padStart(3, '0')
  return `${datePrefix}${nextSeq}`
}

// Same fixed list as the Test Cycles tab (Version Tag dropdown); typing a new name is still allowed
const cycleNameOptions = ['v2.0.0 Pre-Launch UAT', 'Sprint 25 Regression', 'v1.5.0 Production']

const getCycleNameOrEmpty = (cycleId) => {
  if (!cycleId) return ''
  const cycle = testCycles.value.find(c => String(c.id) === String(cycleId))
  return cycle ? cycle.name : ''
}

// Cycle picked from list OR typed manually; Version Tag follows the name
const onCycleChange = (val) => {
  const name = (typeof val === 'string' ? val : '').trim()
  const match = testCycles.value.find(c => (c.name || '').toLowerCase() === name.toLowerCase())
  form.value.cycleId = match ? match.id : null
  form.value.versionTag = match ? match.name : name
}

// Create a Test Cycle from a typed name (same fields TestCyclesPanel sends)
const cyclesPanelKey = ref(0)
const createCycleByName = async (name) => {
  const d = new Date()
  const today = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
  try {
    const res = await fetch('https://localhost:7049/api/TestCycles', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name,
        department: null,
        dt_start: today,
        dt_end: today,
        status: 'Active',
        auto_assign_rule: 'none',
        created_by: currentUserId.value,
        assigned_to: null
      })
    })
    if (!res.ok) {
      alert(`Failed to create Test Cycle: ${await res.text()}`)
      return null
    }
    let created = null
    try { created = await res.json() } catch (_) {}
    await fetchCycles()
    if (!created || !created.id) {
      created = testCycles.value.find(c => (c.name || '').toLowerCase() === name.toLowerCase()) || null
    }
    cyclesPanelKey.value++
    return created
  } catch (err) {
    console.error('Error creating cycle:', err)
    alert('Error: Cannot connect to the API to create the Test Cycle.')
    return null
  }
}

const searchQuery = ref('')
const statusFilter = ref('All')
const priorityFilter = ref('All')
const cycleFilter = ref('All')
const mainTab = ref(route.query.tab === 'cycles' ? 'cycles' : 'cases')
const dialog = ref(false)
const approving = ref(false)
const isEdit = ref(false)
const activeTab = ref('header')
const viewDialog = ref(false)
const viewItem = ref(null)

const itemsPerPage = ref(10)
const currentPage = ref(1)
const itemsPerPageOptions = [10, 25, 50, 100]

const departmentOptions = [
  'PEM', 'ADGM', 'PMM', 'SMHR', 'PB8', 'PB7', 'PB4', 'PB10',
  'MOTC', 'SOTC', 'AOE', 'PQA', 'SEIT', 'MGMT', 'OTR', 'SOIT',
  'SOVS', 'PAPR', 'PMGM', 'PLFG', 'SOMT', 'SMMG', 'PB6', 'PRND',
  'SETH', 'SEPC', 'SETT', 'PRDA', 'AIT', 'SOPA', 'SEGH', 'PLTB',
  'PLTA', 'SEPH', 'SOGT', 'PB3', 'AMCA', 'PAPK', 'PB2', 'PTM',
  'ARPD', 'PQC', 'SEVN', 'SOIP', 'SOVN', 'SEKA', 'PLOG', 'METH',
  'SOKA', 'PLRM', 'AHR', 'AACT'
]

const moduleOptions = [
  'Login / Authentication',
  'User Management',
  'Dashboard',
  'Reporting',
  'Payment / Billing',
  'Inventory',
  'Order Management',
  'Customer Service',
  'API Integration',
  'Mobile App',
  'Admin Panel',
  'Notifications',
  'Search / Filter',
  'File Upload / Download',
  'Other'
]

const form = ref({
  id: null,
  title: '',
  testCaseId: '',
  testDepartment: '',
  versionTag: '',
  module: '',
  cycleId: null,
  cycleInput: '',
  priority: 'Medium',
  status: 'Draft',
  testDescription: '',
  dependencies: '',
  conditions: '',
  control: '',
  assignedTesterIds: [],
  tags: [],
  steps: []
})

const cycleOptions = computed(() => {
  const options = [{ id: 'All', name: 'All' }]
  if (testCycles.value.length > 0) {
    testCycles.value.forEach(cycle => {
      options.push({ id: cycle.id, name: cycle.name })
    })
  }
  return options
})

const filteredTestCases = computed(() => {
  const sourceData = apiTestCases.value
  
  return sourceData.filter(tc => {
    const matchesSearch = searchQuery.value === '' || 
      (tc.title && tc.title.toLowerCase().includes(searchQuery.value.toLowerCase())) ||
      (tc.test_case_code && tc.test_case_code.toLowerCase().includes(searchQuery.value.toLowerCase())) ||
      (assignedNames(tc.id).toLowerCase().includes(searchQuery.value.toLowerCase()))
    
    const matchesStatus = statusFilter.value === 'All' || tc.status === statusFilter.value
    const matchesPriority = priorityFilter.value === 'All' || tc.priority === priorityFilter.value
    const matchesCycle = cycleFilter.value === 'All' || String(tc.cycle_id) === String(cycleFilter.value)

    return matchesSearch && matchesStatus && matchesPriority && matchesCycle
  })
})

const totalPages = computed(() => {
  return Math.max(1, Math.ceil(filteredTestCases.value.length / itemsPerPage.value))
})

const paginatedTestCases = computed(() => {
  const start = (currentPage.value - 1) * itemsPerPage.value
  return filteredTestCases.value.slice(start, start + itemsPerPage.value)
})

const pageStart = computed(() => {
  if (filteredTestCases.value.length === 0) return 0
  return (currentPage.value - 1) * itemsPerPage.value + 1
})

const pageEnd = computed(() => {
  return Math.min(currentPage.value * itemsPerPage.value, filteredTestCases.value.length)
})

watch([searchQuery, statusFilter, priorityFilter, cycleFilter], () => {
  currentPage.value = 1
})

watch(itemsPerPage, () => {
  currentPage.value = 1
})

watch(totalPages, (newTotal) => {
  if (currentPage.value > newTotal) currentPage.value = newTotal
})

const getCycleName = (cycleId) => {
  if (!cycleId) return '-'
  const cycle = testCycles.value.find(c => String(c.id) === String(cycleId))
  return cycle?.name || cycleId
}

const clearFilters = () => {
  searchQuery.value = ''
  statusFilter.value = 'All'
  priorityFilter.value = 'All'
  cycleFilter.value = 'All'
}

const openCreateDialog = () => {
  isEdit.value = false
  const newCode = generateTestCaseId()

  form.value = {
    id: null,
    title: '',
    testCaseId: newCode,
    testDepartment: '',
    versionTag: '',
    module: '',
    cycleId: null,
    cycleInput: '',
    priority: 'Medium',
    status: 'Draft',
    testDescription: '',
    dependencies: '',
    conditions: '',
    control: '',
    assignedTesterIds: fixedTesterUsers.value.map((u) => u.id),
    tags: [],
    steps: []
  }
  activeTab.value = 'header'
  dialog.value = true
}

const openEditDialog = (item) => {
  isEdit.value = true
  form.value = {
    id: item.id,
    title: item.title || '',
    testCaseId: item.test_case_code || '',
    testDepartment: item.test_department || '',
    versionTag: getCycleNameOrEmpty(item.cycle_id) || item.version_tag || '',
    module: item.module || '',
    cycleId: item.cycle_id ? Number(item.cycle_id) : null,
    cycleInput: getCycleNameOrEmpty(item.cycle_id) || '',
    priority: item.priority || 'Medium',
    status: item.status || 'Draft',
    testDescription: item.test_description || '',
    dependencies: item.dependencies || '',
    conditions: item.conditions || '',
    control: item.control_notes || '',
    assignedTesterIds: testAssignments.value
      .filter((a) => String(a.test_case_id) === String(item.id))
      .map((a) => a.user_id),
    tags: [],
    steps: item.steps ? item.steps.map(s => ({
      id: s.id || 0,
      test_case_id: s.test_case_id || item.id || 0,
      step_order: s.step_order || 1,
      description: s.description || '',
      expected: s.expected || '',
      actual: s.actual || '',
      passFail: s.pass_fail || s.passFail || 'N/A'
    })) : []
  }
  activeTab.value = 'header'
  dialog.value = true
}

const addStep = () => {
  if (!form.value.steps) form.value.steps = []
  form.value.steps.push({ id: 0, test_case_id: form.value.id || 0, step_order: form.value.steps.length + 1, description: '', expected: '', passFail: 'N/A', actual: '' })
}

const removeStep = (index) => {
  form.value.steps.splice(index, 1)
}

const saving = ref(false)

const saveTestCase = async () => {
  if (saving.value) return 
  if (!form.value.title || !form.value.title.trim()) {
    alert('Please fill in the Test Title!')
    return
  }
  saving.value = true

  // typed a new cycle name -> create the Test Cycle first, then link it
  let resolvedCycleId = form.value.cycleId ? Number(form.value.cycleId) : null
  const typedCycle = (form.value.cycleInput || '').trim()
  if (typedCycle && !resolvedCycleId) {
    const created = await createCycleByName(typedCycle)
    if (!created || !created.id) {
      saving.value = false
      return
    }
    resolvedCycleId = created.id
    form.value.cycleId = created.id
    form.value.versionTag = created.name || typedCycle
  }

  const payload = {
    id: form.value.id || 0,
    test_case_code: form.value.testCaseId || null, 
    title: form.value.title,
    test_department: form.value.testDepartment || null,
    version_tag: getCycleNameOrEmpty(resolvedCycleId) || form.value.versionTag || null,
    module: form.value.module || null,
    cycle_id: resolvedCycleId,
    priority: form.value.priority || 'Medium',
    status: form.value.status || 'Draft',
    test_description: form.value.testDescription || null,
    dependencies: form.value.dependencies || null,
    conditions: form.value.conditions || null,
    control_notes: form.value.control || null,
    created_by: currentUserId.value,
    steps: (form.value.steps || []).map((step, idx) => ({
      id: step.id || 0,
      test_case_id: form.value.id || 0,
      step_order: idx + 1,
      description: step.description || '',
      expected: step.expected || '',
      actual: step.actual || '',
      pass_fail: step.passFail || 'N/A'
    }))
  }

  try {
    const url = isEdit.value 
      ? `https://localhost:7049/api/TestCases/${form.value.id}` 
      : 'https://localhost:7049/api/TestCases'
    
    const method = isEdit.value ? 'PUT' : 'POST'

    const response = await fetch(url, {
      method: method,
      headers: { 
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(payload)
    })

    if (response.ok) {
      let testCaseId = form.value.id
      try {
        const saved = await response.json()
        if (saved && saved.id) testCaseId = saved.id
      } catch (_) {
        
      }

      let failedRemovals = []
      if (testCaseId) {
        failedRemovals = await syncTesters(testCaseId)
      }

      dialog.value = false
      await fetchFromAPI()
      await fetchAssignments()
      if (testCaseId && expandedTreeData.value[testCaseId]) {
        const copy = { ...expandedTreeData.value }
        delete copy[testCaseId]
        expandedTreeData.value = copy
      }

      if (failedRemovals.length > 0) {
        alert(
          `Test Case & Steps saved, but these testers could NOT be unassigned: ${failedRemovals.join(', ')}.`
        )
      } else {
        alert(`Test Case, Steps & Feedback successfully ${isEdit.value ? 'updated' : 'saved'}!`)
      }
    } else {
      const errorText = await response.text()
      alert(`Failed to save (Status ${response.status}): ${errorText}`)
    }
  } catch (error) {
    console.error('Error saving to API:', error)
    alert('Error: Cannot connect to the API.')
  } finally {
    saving.value = false
  }
}

const syncTesters = async (testCaseId) => {
  const desired = form.value.assignedTesterIds || []
  const existing = testAssignments.value.filter((a) => String(a.test_case_id) === String(testCaseId))
  const failedRemovals = []

  for (const a of existing) {
    if (!desired.includes(a.user_id)) {
      try {
        const res = await fetch(`https://localhost:7049/api/TestAssignments/${a.id}`, { method: 'DELETE' })
        if (!res.ok) {
          const errText = await res.text().catch(() => '')
          console.error(`Failed to remove tester assignment ${a.id}:`, errText)
          failedRemovals.push(userNameById.value[a.user_id] || a.user_name || `User #${a.user_id}`)
        }
      } catch (err) {
        console.error('Error removing tester assignment:', err)
        failedRemovals.push(userNameById.value[a.user_id] || a.user_name || `User #${a.user_id}`)
      }
    }
  }

  for (const userId of desired) {
    if (existing.some((a) => a.user_id === userId)) continue
    const user = users.value.find((u) => u.id === userId)
    try {
      await fetch('https://localhost:7049/api/TestAssignments', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          test_case_id: testCaseId,
          user_id: userId,
          department: form.value.testDepartment || user?.department || null,
          status: 'Pending',
        }),
      })
    } catch (err) {
      console.error('Error adding tester assignment:', err)
    }
  }

  return failedRemovals
}

const confirmDelete = async (id) => {
  if (!confirm('Are you sure you want to delete this Test Case?')) return

  try {
    const response = await fetch(`https://localhost:7049/api/TestCases/${id}`, {
      method: 'DELETE'
    })

    if (response.ok) {
      alert('The Test Case has been successfully deleted!')
      await fetchFromAPI()
    } else {
      alert('Failed to delete Test Case.')
    }
  } catch (error) {
    console.error('Error deleting:', error)
    alert('Error: Cannot connect to the API.')
  }
}

const averageRating = (feedbacks) => {
  if (!feedbacks || feedbacks.length === 0) return '-'
  const sum = feedbacks.reduce((acc, f) => acc + (f.rating || 0), 0)
  return (sum / feedbacks.length).toFixed(1)
}

const openViewDialog = (item) => {
  viewItem.value = JSON.parse(JSON.stringify(item))
  viewDialog.value = true
}

const approveTestCase = async (item, approved) => {
  if (!item) return
  approving.value = true
  try {
    const adminId = Number(localStorage.getItem('uat_user_id')) || null
    const response = await fetch(`https://localhost:7049/api/TestCases/${item.id}/approve`, {
      method: 'PATCH',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ approved, approvedBy: adminId }),
    })

    if (response.ok) {
      const result = await response.json()
      viewItem.value.approval_status = result.approval_status
      viewItem.value.approved_at = result.approved_at
      await fetchFromAPI()
    } else {
      alert('Failed to update approval status.')
    }
  } catch (error) {
    console.error('Error updating approval:', error)
    alert('Error: Cannot connect to the API.')
  } finally {
    approving.value = false
  }
}

const getPriorityColor = (p) => {
  if (p === 'Critical') return '#e11d48'
  if (p === 'High') return '#ea580c'
  if (p === 'Medium') return '#0284c7'
  return '#64748b'
}

const getStatusColor = (s) => {
  if (s === 'Passed') return '#16a34a'
  if (s === 'Failed') return '#dc2626'
  if (s === 'Pending') return '#d97706'
  return '#475569'
}

const getApprovalColor = (s) => {
  if (s === 'Approved') return '#059669'
  if (s === 'Not Approved') return '#dc2626'
  return '#64748b'
}

const formatDateTime = (d) => (d ? new Date(d).toLocaleString() : '-')

const usersDialog = ref(false)
const usersDialogLoading = ref(false)
const selectedTestCaseForRuns = ref(null)
const testCaseUsers = ref([])

const expandedTestCaseIds = ref(new Set())
const expandedTreeData = ref({})
const expandedTreeLoading = ref({})


const startExecutionFor = (item, tester) => {
  router.push({
    name: 'TestExecution',
    params: { id: item.id },
    query: {
      run_as: tester.user_id,
      run_as_name: tester.user_name,
      department: tester.department || '',
    },
  })
}

const onRunCompleted = async (testCaseId) => {
  await Promise.all([fetchAssignments(), fetchFromAPI()])
  if (testCaseId && expandedTreeData.value[testCaseId]) {
    const copy = { ...expandedTreeData.value }
    delete copy[testCaseId]
    expandedTreeData.value = copy
  }
}

const currentUserId = computed(() => Number(localStorage.getItem('uat_user_id')) || 18)

const loadTreeData = async (id) => {
  if (expandedTreeData.value[id]) return expandedTreeData.value[id]

  expandedTreeLoading.value = { ...expandedTreeLoading.value, [id]: true }
  try {
    const assigned = getAssignedUsers(id)

    const runsRes = await fetch(`https://localhost:7049/api/TestRuns/by-testcase/${id}`)
    const runs = runsRes.ok ? await runsRes.json() : []

    const lastRunByUser = {}
    for (const r of runs) {
      if (!(r.executed_by in lastRunByUser)) lastRunByUser[r.executed_by] = r
    }

    const rows = await Promise.all(
      assigned.map(async (a) => {
        const run = lastRunByUser[a.user_id]
        let catatan = '-'

        if (run) {
          const tc = apiTestCases.value.find((c) => c.id === id)
          const fb = tc?.feedbacks?.find((f) => f.run_id === run.id)
          if (fb?.comment) catatan = fb.comment
        }

        return {
          user_id: a.user_id,
          user_name: a.user_name || userNameById.value[a.user_id] || `User #${a.user_id}`,
          department: a.department || users.value.find((u) => u.id === a.user_id)?.department || null,
          run_status: run ? run.run_status : 'Not Started',
          started_at: run ? run.started_at : null,
          catatan,
        }
      })
    )

    expandedTreeData.value = { ...expandedTreeData.value, [id]: rows }
    return rows
  } catch (err) {
    console.error('Failed to build test case flow:', err)
    return []
  } finally {
    expandedTreeLoading.value = { ...expandedTreeLoading.value, [id]: false }
  }
}

const toggleExpand = async (item) => {
  const id = item.id
  const next = new Set(expandedTestCaseIds.value)
  if (next.has(id)) {
    next.delete(id)
    expandedTestCaseIds.value = next
    return
  }
  next.add(id)
  expandedTestCaseIds.value = next
  await loadTreeData(id)
}

const handleRowClick = (item) => {

  router.push({ name: 'TestCaseRuns', params: { id: item.id } })
}

const getRunStatusColor = (status) => {
  switch (status) {
    case 'Completed': return '#16a34a'
    case 'In Progress': return '#0284c7'
    case 'Not Started': return '#64748b'
    default: return '#64748b'
  }
}

const getExecutionStatusColor = (status) => {
  switch (status) {
    case 'Pass': return '#16a34a'
    case 'Fail': return '#dc2626'
    case 'Blocked': return '#ea580c'
    default: return '#64748b'
  }
}

const openUsersDialog = async (testCase) => {
  selectedTestCaseForRuns.value = testCase
  usersDialogLoading.value = true
  usersDialog.value = true
  testCaseUsers.value = []

  try {
    const assigned = getAssignedUsers(testCase.id)

    const runsRes = await fetch(`https://localhost:7049/api/TestRuns/by-testcase/${testCase.id}`)
    const runs = runsRes.ok ? await runsRes.json() : []

    const lastRunByUser = {}
    for (const r of runs) {
      if (!(r.executed_by in lastRunByUser)) lastRunByUser[r.executed_by] = r
    }

    testCaseUsers.value = assigned.map((a) => {
      const run = lastRunByUser[a.user_id]
      return {
        user_id: a.user_id,
        user_name: a.user_name,
        run_id: run ? run.id : null,
        run_status: run ? run.run_status : 'Not Started',
        started_at: run ? run.started_at : null,
        completed_at: run ? run.completed_at : null,
      }
    })
  } catch (err) {
    console.error('Failed to pull run for this test case:', err)
  } finally {
    usersDialogLoading.value = false
  }
}

const runDetailDialog = ref(false)
const runDetailLoading = ref(false)
const selectedUserRun = ref(null)
const runDetailSteps = ref([])

const viewTesterRun = (user) => {
  usersDialog.value = false
  router.push({ name: 'TestCaseRuns', params: { id: selectedTestCaseForRuns.value.id } })
}

const openRunDetail = async (user) => {
  selectedUserRun.value = user
  runDetailDialog.value = true
  runDetailLoading.value = true
  runDetailSteps.value = []

  try {
    const res = await fetch(`https://localhost:7049/api/ExecutionSteps/by-run/${user.run_id}`)
    const steps = res.ok ? await res.json() : []
    runDetailSteps.value = steps.sort((a, b) => (a.sequence_order || 0) - (b.sequence_order || 0))
  } catch (err) {
    console.error('Failed to pull run for this test case:', err)
  } finally {
    runDetailLoading.value = false
  }
}

const exportExcel = () => {
  const rows = filteredTestCases.value.map(tc => ({
    'Test Case ID': tc.test_case_code || ('#' + tc.id),
    'Title': tc.title || '',
    'Cycle': getCycleName(tc.cycle_id),
    'Department': tc.test_department || '',
    'Priority': tc.priority || '',
    'Status': tc.status || '',
    'Assigned To': assignedNames(tc.id),
    'Description': tc.test_description || ''
  }))

  if (rows.length === 0) {
    alert('No Test Cases to export.')
    return
  }

  const worksheet = XLSX.utils.json_to_sheet(rows)
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Test Cases')
  XLSX.writeFile(workbook, `TestCases_${new Date().toISOString().substr(0, 10)}.xlsx`)
}

const exportPDF = () => {
  if (filteredTestCases.value.length === 0) {
    alert('No Test Cases to export.')
    return
  }

  const doc = new jsPDF({ orientation: 'landscape' })
  const pageWidth = doc.internal.pageSize.getWidth()
  const margin = 10
  const usableWidth = pageWidth - margin * 2

  
  const logoX = margin
  const logoY = 8
  const logoW = 16
  const logoH = 19
  doc.addImage(kotraLogoBase64, 'PNG', logoX, logoY, logoW, logoH)

  const textX = logoX + logoW + 4
  doc.setFontSize(11)
  doc.setFont('arial', 'bold')
  doc.setTextColor(103, 14, 95)
  doc.text('KOTRA PHARMA (M) SDN BHD', textX, 13)
  doc.setFont('arial', 'normal')
  doc.setFontSize(8)
  doc.setTextColor(71, 85, 105)
  doc.text('1, 2 & 3, Jalan TTC 12, Cheng Industrial Estate, 75250 Melaka, Malaysia', textX, 18)
  doc.setFontSize(8)
  doc.text('Office of Systems Integration', textX, 22.5)

  doc.setFontSize(13)
  doc.setFont('arial', 'bold')
  doc.setTextColor(0, 0, 0)
  doc.text('Test Cases Report', pageWidth - margin, 13, { align: 'right' })
  doc.setFont('arial', 'normal')
  doc.setFontSize(8)
  doc.setTextColor(71, 85, 105)
  doc.text(`Generated: ${new Date().toLocaleString()}`, pageWidth - margin, 18, { align: 'right' })
  doc.text(`Total Test Cases: ${filteredTestCases.value.length}`, pageWidth - margin, 22.5, { align: 'right' })

  doc.setDrawColor(0)
  doc.setLineWidth(0.4)
  doc.line(margin, logoY + logoH + 3, pageWidth - margin, logoY + logoH + 3)

  
  let y = logoY + logoH + 7
  doc.setFillColor(190, 190, 190)
  doc.rect(margin, y, usableWidth, 6, 'F')
  doc.setDrawColor(0)
  doc.rect(margin, y, usableWidth, 6)
  doc.setFontSize(9)
  doc.setFont('arial', 'bold')
  doc.setTextColor(0, 0, 0)
  doc.text('TEST CASES', margin + usableWidth / 2, y + 4.2, { align: 'center' })
  y += 6

  
  doc.autoTable({
    startY: y,
    margin: { left: margin, right: margin },
    head: [['ID', 'Title', 'Cycle', 'Dept', 'Priority', 'Status', 'Assigned To']],
    body: filteredTestCases.value.map(tc => [
      tc.test_case_code || ('#' + tc.id),
      tc.title || '',
      getCycleName(tc.cycle_id),
      tc.test_department || '-',
      tc.priority || '-',
      tc.status || '-',
      assignedNames(tc.id)
    ]),
    theme: 'grid',
    styles: { fontSize: 8, font: 'arial', lineColor: [0, 0, 0], lineWidth: 0.2 },
    headStyles: { fillColor: [190, 190, 190], textColor: [0, 0, 0], fontStyle: 'bold' }
  })

  doc.save(`TestCases_${new Date().toISOString().substr(0, 10)}.pdf`)
}

const fetchCycles = async () => {
  try {
    const cycleRes = await fetch('https://localhost:7049/api/TestCycles')
    if (!cycleRes.ok) throw new Error('HTTP ' + cycleRes.status)

    const data = await cycleRes.json()
    testCycles.value = (Array.isArray(data) ? data : []).filter((item, index, self) =>
      item && item.name && index === self.findIndex((t) => t.name === item.name)
    )
  }
  catch (err) {
    console.error('Failed to pull Test Cycles from API:', err)
  }
}


watch(() => route.query.tab, (tab) => {
  const next = tab === 'cycles' ? 'cycles' : 'cases'
  if (mainTab.value !== next) mainTab.value = next
})


watch(mainTab, (tab) => {
  if (route.query.tab === tab) return
  router.replace({ query: { ...route.query, tab } })
})


const onCyclesChanged = async () => {
  await fetchCycles()
}


const viewCasesForCycle = (cycleId) => {
  cycleFilter.value = String(cycleId)
  mainTab.value = 'cases'
}

onMounted(async () => {
  await fetchUsers()
  await fetchFromAPI()
  await fetchAssignments()

  await fetchCycles()

  if (route.query.status) statusFilter.value = route.query.status
  if (route.query.priority) priorityFilter.value = route.query.priority
  if (route.query.cycle) cycleFilter.value = String(route.query.cycle)
  if (route.query.tab === 'cycles') mainTab.value = 'cycles'
})
</script>

<style scoped>


.lab-page, 
.lab-page * {
  font-family: Arial, Helvetica, sans-serif !important;
}

.page-heading {
  font-weight: 700;
  letter-spacing: -0.02em;
  color: #0f172a;
}

.page-subheading {
  color: #475569;
  font-size: 0.875rem;
  margin-top: 2px;
}


.accent-bar {
  width: 5px;
  border-radius: 4px;
  background: linear-gradient(180deg, #c44eb4 0%, #ecaae3 100%);
  align-self: stretch;
  box-shadow: 0 0 10px rgba(118, 15, 79, 0.4);
}


.id-text {
  color: #0f172a;
  font-size: 12px;
  letter-spacing: 0.3px;
  white-space: nowrap;
}

.test-title-text {
  font-size: 14px;
  color: #0f172a;
  line-height: 1.3;
}

.dept-tag {
  background-color: #f1f5f9;
  color: #334155;
  padding: 3px 8px;
  border-radius: 6px;
  border: 1px solid #cbd5e1;
}


.table-header-row {
  background: linear-gradient(90deg, #760f6f 0%, #641f61 100%);
}

.test-case-table th {
  font-size: 0.75rem !important;
  color: #ffffff !important;
  text-transform: uppercase;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.test-case-row {
  transition: all 0.2s ease;
  border-bottom: 1px solid #f1f5f9;
}

.test-case-row:hover {
  background-color: #f0fdfa !important;
  transform: translateY(-1px);
}

.shadow-chip {
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.15);
}


.filter-card {
  background: linear-gradient(135deg, #ffffff 0%, #f8fafc 100%);
  border: 1px solid #e2e8f0;
}

.main-table-card {
  border: 1px solid #cbd5e1;
}

.bg-slate-50 {
  background-color: #f8fafc;
}


.dialog-header {
  background: linear-gradient(135deg, #670e5f 0%, #e987d4 100%);
}


.testmo-edit-layout {
  display: grid;
  grid-template-columns: 1fr 260px;
  gap: 28px;
}

@media (max-width: 640px) {
  .testmo-edit-layout {
    grid-template-columns: 1fr;
  }
}

.testmo-main-col {
  min-width: 0;
}

.testmo-icon-badge {
  width: 26px;
  height: 26px;
  border-radius: 8px;
  background: linear-gradient(135deg, #0f766e 0%, #115e59 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.testmo-props-col {
  border-left: 1px solid #e2e8f0;
  padding-left: 20px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.testmo-props-field {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.testmo-props-label {
  font-size: 0.65rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #94a3b8;
  font-weight: 700;
}


.step-box {
  border: 1px solid #ccfbf1;
  background-color: #f0fdfa;
}

.btn-glow {
  transition: all 0.2s ease-in-out;
}

.btn-glow:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(15, 118, 110, 0.3) !important;
}

.action-buttons-group {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 4px;
}

.border-b {
  border-bottom: 1px solid #e2e8f0;
}

.border-t {
  border-top: 1px solid #e2e8f0;
}

.text-grey-italic {
  color: #94a3b8;
  font-style: italic;
}


.expand-panel {
  background: linear-gradient(180deg, #f0fdfa 0%, #f8fafc 100%);
  border-top: 2px dashed #3b1235;
  border-bottom: 1px solid #e2e8f0;
  padding: 20px 24px;
}

.flow-bar {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-bottom: 20px;
}

.flow-bar-step {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.8rem;
  font-weight: 700;
  color: #0f766e;
}

.flow-bar-icon {
  width: 28px;
  height: 28px;
  border-radius: 50%;
  background: linear-gradient(135deg, #760f44 0%, #cc0c9f 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 6px rgba(118, 15, 85, 0.3);
}

.flow-bar-connector {
  width: 40px;
  height: 3px;
  background-color: #d899be;
  border-radius: 2px;
}

.expand-section {
  margin-bottom: 18px;
}

.expand-section-last {
  margin-bottom: 0;
}

.expand-section-title {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: #760f50;
  font-weight: 700;
  margin-bottom: 8px;
}

.expand-case-title {
  font-size: 0.95rem;
  font-weight: 700;
  color: #0f172a;
}


.test-run-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 12px;
}

.test-run-card {
  background-color: #ffffff !important;
  padding: 14px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  border: 1px solid #cbd5e1 !important;
  box-shadow: 0 2px 6px rgba(0,0,0,0.04);
}

.test-run-card-header {
  color: #0f172a;
}

.test-run-card-body {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.test-run-field {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.test-run-label {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #64748b;
  font-weight: 700;
}

.test-run-value {
  font-size: 0.8rem;
  color: #0f172a;
  font-weight: 600;
}

.test-run-notes {
  white-space: pre-wrap;
  word-break: break-word;
}

.test-run-card-action {
  margin-top: auto;
  text-transform: none;
}
</style>