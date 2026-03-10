<!--
    <upload-toast> component — bottom-right toast notifications for file uploads
-->

<template>
  <div class="upload-toast-container" v-show="files.length > 0">
    <div class="upload-toast-header" @click="expanded = !expanded">
      <div class="upload-toast-title">
        <span>{{ t.Uploads }}</span>
        <span v-if="pendingCount > 0" class="tw-ms-1">({{ pendingCount }})</span>
        <span v-if="errorCount > 0" class="tw-text-red-500 tw-ms-1">
          {{ t.Errors }}: {{ errorCount }}
        </span>
      </div>
      <div class="upload-toast-actions">
        <button v-if="errorCount > 0" class="ma-btn ma-btn-link ma-btn-sm tw-text-red-500 tw-p-0 tw-me-2"
          @click.stop="clearErrors">{{ t.ClearErrors }}</button>
        <button class="ma-btn ma-btn-link ma-btn-sm tw-p-0" @click.stop="expanded = !expanded">
          <fa-icon :icon="expanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-up'"></fa-icon>
        </button>
        <button v-if="pendingCount === 0" class="ma-btn ma-btn-link ma-btn-sm tw-p-0 tw-ms-2" @click.stop="dismissAll">
          <fa-icon icon="fa-solid fa-times"></fa-icon>
        </button>
      </div>
    </div>
    <div class="upload-toast-body" v-show="expanded">
      <div v-for="f in files" :key="f.name" class="upload-toast-item"
        :class="{ 'is-error': f.errorMessage, 'is-success': f.success }">
        <div class="tw-flex tw-justify-between tw-items-center">
          <span class="upload-toast-filename" :title="f.errorMessage || f.name">{{ f.name }}</span>
          <button v-if="f.errorMessage" class="ma-btn ma-btn-link ma-btn-sm tw-p-0 tw-text-red-500"
            @click="dismiss(f)">
            <fa-icon icon="fa-solid fa-times"></fa-icon>
          </button>
          <fa-icon v-else-if="f.success" icon="fa-solid fa-check" class="tw-text-green-500"></fa-icon>
        </div>
        <div v-if="f.errorMessage" class="upload-toast-error tw-text-red-500">
          {{ f.errorMessage }}
        </div>
        <div v-else-if="!f.success" class="upload-toast-progress">
          <div class="upload-toast-progress-bar" :style="{ width: f.percentage + '%' }"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useEventBus } from '../services/UseEventBus'
import { useLocalizations } from '../services/Localizations'

const { on } = useEventBus();
const { translations } = useLocalizations();
const t = translations.value;

interface UploadFile {
  name: string;
  percentage: number;
  errorMessage: string;
  success: boolean;
}

const files = ref<UploadFile[]>([]);
const expanded = ref(true);

const pendingCount = computed(() => files.value.filter((f) => !f.errorMessage && !f.success).length);
const errorCount = computed(() => files.value.filter((f) => f.errorMessage !== '').length);

on('UploadFileAdded', (data) => {
  const existing = files.value.find((f) => f.name === data.name);
  if (!existing) {
    files.value.push({ name: data.name, percentage: 0, errorMessage: '', success: false });
    expanded.value = true;
  }
});

on('UploadProgress', (data) => {
  const file = files.value.find((f) => f.name === data.name);
  if (file) {
    file.percentage = data.percentage;
  }
});

on('UploadSuccess', (data) => {
  const file = files.value.find((f) => f.name === data.name);
  if (file) {
    file.success = true;
    file.percentage = 100;
  }

  setTimeout(() => {
    files.value = files.value.filter((f) => !(f.name === data.name && f.success));
  }, 3000);
});

on('UploadError', (data) => {
  const file = files.value.find((f) => f.name === data.name);
  if (file) {
    file.errorMessage = data.errorMessage;
  }
});

const clearErrors = () => {
  files.value = files.value.filter((f) => f.errorMessage === '');
};

const dismiss = (file: UploadFile) => {
  files.value = files.value.filter((f) => f.name !== file.name);
};

const dismissAll = () => {
  files.value = [];
};
</script>
