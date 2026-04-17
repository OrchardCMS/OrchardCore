<!--
    <notification-toast> component — top-right toast notifications for validation/error messages
-->

<template>
  <div class="notification-toast-container">
    <TransitionGroup name="notification-toast">
      <div v-for="n in notifications" :key="n.id" class="notification-toast-item"
        :class="severityClass(n.severity)">
        <div class="notification-toast-content">
          <div class="notification-toast-icon">
            <fa-icon :icon="severityIcon(n.severity)"></fa-icon>
          </div>
          <div class="notification-toast-text">
            <div v-if="n.summary" class="notification-toast-summary">{{ n.summary }}</div>
            <div v-if="n.detail" class="notification-toast-detail">{{ n.detail }}</div>
          </div>
          <button class="notification-toast-copy" @click="copyMessage(n)" :title="n.copied ? t.Copied : t.CopyError">
            <fa-icon :icon="n.copied ? 'fa-solid fa-check' : 'fa-regular fa-copy'"></fa-icon>
          </button>
          <button class="notification-toast-close" @click="dismiss(n.id)">
            <fa-icon icon="fa-solid fa-times"></fa-icon>
          </button>
        </div>
      </div>
    </TransitionGroup>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { SeverityLevel } from '@bloom/services/notifications/interfaces';
import { getTranslations } from '@bloom/helpers/localizations';

const t = getTranslations();

interface ToastNotification {
  id: number;
  summary?: string;
  detail?: string;
  severity?: SeverityLevel;
  copied: boolean;
}

let nextId = 0;
const notifications = ref<ToastNotification[]>([]);
let handler: ((message: any) => void) | null = null; // eslint-disable-line @typescript-eslint/no-explicit-any

const dismiss = (id: number) => {
  notifications.value = notifications.value.filter((n) => n.id !== id);
};

const copyMessage = async (n: ToastNotification) => {
  const text = [n.summary, n.detail].filter(Boolean).join('\n');
  try {
    await navigator.clipboard.writeText(text);
    n.copied = true;
    setTimeout(() => { n.copied = false; }, 2000);
  } catch {
    // Clipboard API not available
  }
};

const addNotification = (message: any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
  const id = nextId++;
  notifications.value.push({
    id,
    summary: message?.summary,
    detail: message?.detail,
    severity: message?.severity ?? SeverityLevel.Info,
    copied: false,
  });

  const autoCloseMs = message?.severity === SeverityLevel.Error ? 8000 : 5000;
  setTimeout(() => dismiss(id), autoCloseMs);
};

const severityClass = (severity?: SeverityLevel) => {
  switch (severity) {
    case SeverityLevel.Success: return 'is-success';
    case SeverityLevel.Warn: return 'is-warn';
    case SeverityLevel.Error: return 'is-error';
    default: return 'is-info';
  }
};

const severityIcon = (severity?: SeverityLevel) => {
  switch (severity) {
    case SeverityLevel.Success: return 'fa-solid fa-circle-check';
    case SeverityLevel.Warn: return 'fa-solid fa-triangle-exclamation';
    case SeverityLevel.Error: return 'fa-solid fa-circle-xmark';
    default: return 'fa-solid fa-circle-info';
  }
};

onMounted(() => {
  if (window.notificationBus) {
    handler = (message: any) => addNotification(message); // eslint-disable-line @typescript-eslint/no-explicit-any
    window.notificationBus.on('notify', handler);
  }
});

onUnmounted(() => {
  if (window.notificationBus && handler) {
    window.notificationBus.off('notify', handler);
  }
});
</script>
