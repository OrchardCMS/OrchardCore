<template>
  <VueFinalModal
    v-model="visible"
    class="tw:flex tw:items-center tw:justify-center"
    content-class="card shadow-sm w-100"
    content-style="max-width: 1200px; height: min(92vh, 900px); overflow: hidden;"
    @opened="onOpened"
    @closed="onClosed"
  >
    <div class="card-header d-flex justify-content-between align-items-center">
      <h5 class="mb-0">{{ labels.selectMedia }}</h5>
      <button type="button" class="btn-close" :aria-label="labels.cancel" @click="cancel"></button>
    </div>

    <div class="card-body p-0" style="overflow: auto;">
      <div ref="pickerContainer"></div>
    </div>

    <div class="card-footer d-flex justify-content-between align-items-center gap-2">
      <span class="text-muted small">{{ selectedCount > 0 ? `${selectedCount} selected` : '' }}</span>
      <div class="d-flex gap-2">
        <button type="button" class="btn btn-outline-secondary" @click="cancel">
          {{ labels.cancel }}
        </button>
        <button
          type="button"
          class="btn btn-primary"
          :disabled="selectedCount === 0"
          @click="confirm"
        >
          {{ labels.ok }}
        </button>
      </div>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { computed, ref } from "vue";
import { VueFinalModal } from "vue-final-modal";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";

interface IMediaPickerConfig {
  translations: string;
  basePath: string;
  uploadFilesUrl: string;
  allowedExtensions?: string;
  allowMultiple?: boolean;
  maxUploadChunkSize?: number;
}

interface IMediaPickerHandle {
  getSelectedFiles(): IFileLibraryItemDto[];
  clearSelection(): void;
  unmount(): void;
}

const props = defineProps<{
  config: IMediaPickerConfig;
  mountPicker: (
    container: HTMLElement,
    config: IMediaPickerConfig & { onSelectionChange?: (count: number) => void }
  ) => IMediaPickerHandle;
  onResolve: (files: IFileLibraryItemDto[]) => void;
}>();

const visible = ref(true);
const pickerContainer = ref<HTMLElement | null>(null);
const selectedCount = ref(0);
let pickerHandle: IMediaPickerHandle | null = null;
let resolved = false;

const labels = computed(() => {
  const defaults = { selectMedia: "Select Media", ok: "OK", cancel: "Cancel" };

  try {
    const parsed = JSON.parse(props.config.translations || "{}");
    return {
      selectMedia: parsed.selectMedia || defaults.selectMedia,
      ok: parsed.ok || defaults.ok,
      cancel: parsed.cancel || defaults.cancel,
    };
  } catch {
    return defaults;
  }
});

function resolveOnce(files: IFileLibraryItemDto[]) {
  if (resolved) {
    return;
  }

  resolved = true;
  props.onResolve(files);
}

function onOpened() {
  if (!pickerContainer.value) {
    resolveOnce([]);
    return;
  }

  pickerHandle = props.mountPicker(pickerContainer.value, {
    translations: props.config.translations,
    basePath: props.config.basePath,
    uploadFilesUrl: props.config.uploadFilesUrl,
    allowedExtensions: props.config.allowedExtensions,
    allowMultiple: props.config.allowMultiple,
    maxUploadChunkSize: props.config.maxUploadChunkSize,
    onSelectionChange: (count: number) => {
      selectedCount.value = count;
    },
  });
}

function cancel() {
  visible.value = false;
  resolveOnce([]);
}

function confirm() {
  const selectedFiles = pickerHandle?.getSelectedFiles() ?? [];
  visible.value = false;
  resolveOnce(selectedFiles);
}

function onClosed() {
  pickerHandle?.unmount();
  pickerHandle = null;
  selectedCount.value = 0;
  resolveOnce([]);
}
</script>
