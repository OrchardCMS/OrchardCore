<!--
    <upload-toast> component — bottom-right toast notifications for file uploads
-->

<template>
  <div class="upload-toast-container" v-show="files.length > 0">
    <div class="upload-toast-header" @click="expanded = !expanded">
      <div class="upload-toast-title">
        <span>{{ t.Uploads }}</span>
        <span v-if="pendingCount > 0" class="tw:ms-1">({{ pendingCount }})</span>
        <span v-if="errorCount > 0" class="tw:text-red-500 tw:ms-1">
          {{ t.Errors }}: {{ errorCount }}
        </span>
      </div>
      <div class="upload-toast-actions">
        <button v-if="errorCount > 0" class="ma-btn ma-btn-link ma-btn-sm tw:text-red-500 tw:p-0 tw:me-2"
          @click.stop="clearErrors">{{ t.ClearErrors }}</button>
        <button class="ma-btn ma-btn-link ma-btn-sm tw:p-0" @click.stop="expanded = !expanded">
          <fa-icon :icon="expanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-up'"></fa-icon>
        </button>
        <button v-if="pendingCount === 0" class="ma-btn ma-btn-link ma-btn-sm tw:p-0 tw:ms-2" @click.stop="dismissAll">
          <fa-icon icon="fa-solid fa-times"></fa-icon>
        </button>
      </div>
    </div>
    <div class="upload-toast-body" v-show="expanded">
      <div v-for="f in files" :key="f.name" class="upload-toast-item"
        :class="{ 'is-error': f.errorMessage, 'is-success': f.success }">
        <div class="tw:flex tw:justify-between tw:items-center">
          <span class="upload-toast-filename" :title="f.errorMessage || f.name">{{ f.name }}</span>
          <span v-if="!f.errorMessage && !f.success && props.tusEnabled" class="tw:flex tw:items-center tw:gap-1">
            <button class="ma-btn ma-btn-link ma-btn-sm tw:p-0" @click="togglePause(f)"
              :title="f.paused ? t.ResumeUpload : t.PauseUpload">
              <fa-icon :icon="f.paused ? 'fa-solid fa-play' : 'fa-solid fa-pause'"></fa-icon>
            </button>
          </span>
          <button v-else-if="f.errorMessage" class="ma-btn ma-btn-link ma-btn-sm tw:p-0 tw:text-red-500"
            @click="dismiss(f)">
            <fa-icon icon="fa-solid fa-times"></fa-icon>
          </button>
          <span v-else-if="f.success" class="tw:flex tw:items-center tw:gap-1">
            <span v-if="f.resumed" class="tw:text-xs tw:text-blue-500" :title="t.UploadResumed">
              <fa-icon icon="fa-solid fa-rotate" class="tw:text-blue-500"></fa-icon>
            </span>
            <fa-icon icon="fa-solid fa-check" class="tw:text-green-500"></fa-icon>
          </span>
        </div>
        <div v-if="f.errorMessage" class="upload-toast-error tw:text-red-500"
          :class="{ 'is-expanded': f.errorExpanded }"
          @click="f.errorExpanded = !f.errorExpanded">
          <fa-icon :icon="f.errorExpanded ? 'fa-solid fa-chevron-up' : 'fa-solid fa-chevron-down'"
            class="upload-toast-error-toggle"></fa-icon>
          {{ f.errorMessage }}
        </div>
        <div v-else-if="!f.success" class="tw:flex tw:items-center tw:gap-2">
          <div class="upload-toast-progress tw:flex-1">
            <div class="upload-toast-progress-bar" :class="{ 'is-paused': f.paused }" :style="{ width: f.percentage + '%' }"></div>
          </div>
          <span v-if="f.speed && !f.paused" class="upload-toast-speed">{{ f.speed }}</span>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useEventBus } from '../services/UseEventBus'
import { getTranslations } from '@bloom/helpers/localizations'

const props = defineProps<{
  tusEnabled: boolean;
}>();

const { on, emit } = useEventBus();
const t = getTranslations();

interface UploadFile {
  name: string;
  percentage: number;
  errorMessage: string;
  success: boolean;
  resumed: boolean;
  paused: boolean;
  errorExpanded: boolean;
  speed: string;
  lastBytes: number;
  lastTime: number;
}

const files = ref<UploadFile[]>([]);
const expanded = ref(true);

const pendingCount = computed(() => files.value.filter((f) => !f.errorMessage && !f.success).length);
const errorCount = computed(() => files.value.filter((f) => f.errorMessage !== '').length);

const formatSpeed = (bytesPerSec: number): string => {
  if (bytesPerSec >= 1024 * 1024) return (bytesPerSec / (1024 * 1024)).toFixed(1) + ' MB/s';
  if (bytesPerSec >= 1024) return (bytesPerSec / 1024).toFixed(0) + ' KB/s';
  return bytesPerSec.toFixed(0) + ' B/s';
};

on('UploadFileAdded', (data) => {
  const existing = files.value.find((f) => f.name === data.name);
  if (!existing) {
    files.value.push({ name: data.name, percentage: 0, errorMessage: '', success: false, resumed: false, paused: false, errorExpanded: false, speed: '', lastBytes: 0, lastTime: Date.now() });
    expanded.value = true;
  }
});

on('UploadProgress', (data) => {
  const file = files.value.find((f) => f.name === data.name);
  if (file) {
    file.percentage = data.percentage;
    const now = Date.now();
    const elapsed = (now - file.lastTime) / 1000;
    if (elapsed >= 0.5) {
      const bytesDelta = data.bytesUploaded - file.lastBytes;
      const bytesPerSec = bytesDelta / elapsed;
      file.speed = bytesPerSec > 0 ? formatSpeed(bytesPerSec) : '';
      file.lastBytes = data.bytesUploaded;
      file.lastTime = now;
    }
  }
});

on('UploadSuccess', (data) => {
  const file = files.value.find((f) => f.name === data.name);
  if (file) {
    file.success = true;
    file.percentage = 100;
    file.resumed = !!data.resumed;
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

on('UploadPaused', (data) => {
  const file = files.value.find((f) => f.name === data.name);
  if (file) {
    file.paused = data.paused;
    if (data.paused) {
      file.speed = '';
    } else {
      file.lastTime = Date.now();
      file.lastBytes = 0;
    }
  }
});

const togglePause = (file: UploadFile) => {
  emit('UploadPauseToggle', { name: file.name });
};

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
