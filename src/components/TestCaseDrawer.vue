<template>
  <v-navigation-drawer
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    location="right"
    temporary
    width="600"
    elevation="10"
  >
    <div class="d-flex flex-column h-100">
      <div class="pa-4 text-white d-flex align-center justify-space-between" style="background-color: #1e293b;">
        <span class="text-h6 font-weight-bold">{{ isEdit ? 'Edit Test Case' : 'New Test Case' }}</span>
        <v-btn icon variant="text" size="small" @click="close">
          <v-icon color="white">mdi-close</v-icon>
        </v-btn>
      </div>

      <div class="pa-6 flex-grow-1 overflow-y-auto">
        <v-tabs v-model="tab" color="#4338ca" class="mb-4">
          <v-tab value="header"><v-icon start>mdi-file-document-outline</v-icon> Header</v-tab>
          <v-tab value="description"><v-icon start>mdi-text-box-outline</v-icon> Details</v-tab>
        </v-tabs>

        <v-window v-model="tab" class="mt-2">
          <v-window-item value="header">
            <v-row density="compact">
              <v-col cols="12">
                <v-text-field label="Test Title *" v-model="form.title" variant="outlined" density="compact"></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-text-field
                  label="Test Case ID"
                  v-model="form.testCaseId"
                  variant="outlined"
                  density="compact"
                  disabled
                  :placeholder="isEdit ? '' : 'Auto-generated on save (TC-xxx)'"
                ></v-text-field>
              </v-col>
              <v-col cols="12">
                <v-autocomplete
                  label="Department"
                  :items="departmentOptions"
                  v-model="form.testDepartment"
                  variant="outlined"
                  density="compact"
                  :disabled="isEdit"
                  @update:model-value="onDeptChange"
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" sm="6">
                <v-select label="Priority" :items="['Low', 'Medium', 'High', 'Critical']" v-model="form.priority" variant="outlined" density="compact"></v-select>
              </v-col>
              <v-col cols="12" sm="6">
                <v-select label="Status" :items="['Draft', 'Passed', 'Failed', 'Pending']" v-model="form.status" variant="outlined" density="compact"></v-select>
              </v-col>
              <v-col cols="12">
                <v-text-field label="Test Number" v-model="form.testNumber" variant="outlined" density="compact"></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field label="Test Date" type="date" v-model="form.testDate" variant="outlined" density="compact"></v-text-field>
              </v-col>
              <v-col cols="12" sm="6">
                <v-text-field label="Execution Date" type="date" v-model="form.executionDate" variant="outlined" density="compact"></v-text-field>
              </v-col>
            </v-row>
          </v-window-item>

          <v-window-item value="description">
            <v-row density="compact">
              <v-col cols="12">
                <v-textarea label="Test Description" v-model="form.testDescription" variant="outlined" density="compact" rows="3"></v-textarea>
              </v-col>
              <v-col cols="12">
                <v-textarea label="Dependencies" v-model="form.testDependencies" variant="outlined" density="compact" rows="2"></v-textarea>
              </v-col>
              <v-col cols="12">
                <v-textarea label="Conditions" v-model="form.testConditions" variant="outlined" density="compact" rows="2"></v-textarea>
              </v-col>
              <v-col cols="12">
                <v-textarea label="Control" v-model="form.testControl" variant="outlined" density="compact" rows="2"></v-textarea>
              </v-col>
            </v-row>
          </v-window-item>
        </v-window>
      </div>
      <div class="pa-4 border-t d-flex justify-end">
        <v-btn variant="text" @click="close" class="mr-2">Cancel</v-btn>
        <v-btn color="#4338ca" class="text-white" @click="save">Save</v-btn>
      </div>
    </div>
  </v-navigation-drawer>
</template>

<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
  modelValue: Boolean,
  initialData: Object,
  isEdit: Boolean
})

const emit = defineEmits(['update:modelValue', 'save'])

const tab = ref('header')
const form = ref({ ...props.initialData })

const departmentOptions = [
  'PEM', 'ADGM', 'PMM', 'SMHR', 'PB8', 'PB7', 'PB4', 'PB10', 
  'MOTC', 'SOTC', 'AOE', 'PQA', 'SEIT', 'MGMT', 'OTR', 'SOIT'
]

watch(() => props.initialData, (newVal) => {
  form.value = { ...newVal }
}, { deep: true })

const onDeptChange = (dept) => {
  
}

const close = () => {
  emit('update:modelValue', false)
}

const save = () => {
  if (!form.value.title) return alert('Sila isi Test Title!')
  emit('save', form.value)
  close()
}
</script>

<style scoped>
.border-t { border-top: 1px solid rgba(var(--v-border-color), var(--v-border-opacity)); }
</style>