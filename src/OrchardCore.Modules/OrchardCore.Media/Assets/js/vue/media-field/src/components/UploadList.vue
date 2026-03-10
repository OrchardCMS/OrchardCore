<!--
  Upload toast for attached media field.
  Fixed position bottom-right, shows per-file progress and errors.
-->
<template>
  <div v-show="files.length > 0" class="mf-upload-toast">
    <div class="mf-upload-toast-header" @click="expanded = !expanded">
      <div class="mf-upload-toast-title">
        <span>{{ T.uploads }}</span>
        <span v-if="pendingCount > 0" class="tw-ms-1">({{ pendingCount }})</span>
        <span v-if="errorCount > 0" class="tw-text-red-500 tw-ms-1">
          {{ T.errors }}: {{ errorCount }}
        </span>
      </div>
      <div class="mf-upload-toast-actions">
        <button
          v-if="errorCount > 0"
          type="button"
          class="mf-btn-icon tw-text-red-500 tw-me-1"
          @click.stop="$emit('clearErrors')"
          :title="T.clearErrors"
        >
          <i class="fa-solid fa-broom" aria-hidden="true"></i>
        </button>
        <button
          type="button"
          class="mf-btn-icon"
          @click.stop="expanded = !expanded"
        >
          <i :class="expanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-up'" aria-hidden="true"></i>
        </button>
        <button
          v-if="pendingCount === 0"
          type="button"
          class="mf-btn-icon tw-ms-1"
          @click.stop="dismissAll"
        >
          <i class="fa-solid fa-xmark" aria-hidden="true"></i>
        </button>
      </div>
    </div>
    <div v-show="expanded" class="mf-upload-toast-body">
      <div
        v-for="f in files"
        :key="f.name"
        class="mf-upload-toast-item"
        :class="{ 'is-error': f.errorMessage, 'is-success': f.success }"
      >
        <div class="tw-flex tw-justify-between tw-items-center">
          <span class="mf-upload-toast-filename" :title="f.errorMessage || f.name">{{ f.name }}</span>
          <button
            v-if="f.errorMessage"
            type="button"
            class="mf-btn-icon tw-text-red-500"
            @click="$emit('dismiss', f)"
          >
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
          <i v-else-if="f.success" class="fa-solid fa-check tw-text-green-500" aria-hidden="true"></i>
        </div>
        <div v-if="f.errorMessage" class="mf-upload-toast-error tw-text-red-500">
          {{ f.errorMessage }}
        </div>
        <div v-else-if="!f.success" class="mf-upload-toast-progress">
          <div class="mf-upload-toast-progress-bar" :style="{ width: f.percentage + '%' }"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import type { IUploadFileEntry } from "../services/FieldUploadService";
import { useLocalizations } from "@bloom/helpers/localizations";

const props = defineProps<{
  files: IUploadFileEntry[];
}>();

const emit = defineEmits<{
  clearErrors: [];
  dismiss: [entry: IUploadFileEntry];
  dismissAll: [];
}>();

const { translations: T } = useLocalizations();
const expanded = ref(true);

const pendingCount = computed(
  () => props.files.filter((f) => !f.errorMessage && !f.success).length
);
const errorCount = computed(
  () => props.files.filter((f) => f.errorMessage !== "").length
);

function dismissAll() {
  emit("dismissAll");
}
</script>
