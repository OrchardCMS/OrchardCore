<!--
  Media Picker Modal — opens a vue-final-modal with the full Media App
  embedded inside for browsing, uploading, and selecting media items.
-->
<template>
  <VueFinalModal
    v-model="visible"
    class="tw:flex tw:items-center tw:justify-center"
    content-class="tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:rounded-lg tw:p-0 tw:min-w-[300px] tw:max-w-[90vw] tw:w-[1000px] tw:h-[85vh] tw:max-h-[85vh] tw:flex tw:flex-col tw:shadow-md"
    @opened="onOpened"
    @closed="onClosed"
  >
    <div class="tw:flex tw:items-center tw:justify-between tw:px-4 tw:py-2">
      <span class="tw:text-lg tw:font-medium">{{ t.selectMedia }}</span>
      <button type="button" class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143]" @click="cancel">
        <i class="fa-solid fa-xmark" aria-hidden="true"></i>
      </button>
    </div>
    <div class="tw:flex-1 tw:min-h-0 tw:overflow-hidden tw:flex tw:flex-col">
      <!-- Loading state while media-app module is being imported -->
      <div v-if="loading" class="tw:flex tw:items-center tw:justify-center tw:py-12">
        <i class="fa-solid fa-spinner fa-spin tw:mr-2" aria-hidden="true"></i>
        {{ t.loadingMediaBrowser }}
      </div>
      <!-- Error state -->
      <div v-else-if="error" class="tw:text-center tw:py-12 tw:text-[#dc3545]">
        <i class="fa-solid fa-triangle-exclamation tw:mr-2" aria-hidden="true"></i>
        {{ error }}
      </div>
      <!-- Container where the media-app will be mounted -->
      <div v-show="!loading && !error" ref="containerRef" class="mf-picker-media-app tw:flex-1 tw:min-h-0 tw:flex tw:flex-col"></div>
    </div>
    <div class="tw:flex tw:items-center tw:justify-between tw:px-4 tw:py-3">
      <span class="tw:text-sm tw:text-gray-500">
        {{ selectedCount > 0 ? `${selectedCount} selected` : '' }}
      </span>
      <div class="tw:flex tw:gap-2">
        <button type="button" class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-secondary-bg)] tw:text-[var(--bs-body-color)] tw:cursor-pointer hover:tw:bg-[var(--bs-tertiary-bg)]" @click="cancel">{{ t.cancel }}</button>
        <button
          type="button"
          class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[#7bc143] tw:rounded tw:bg-[#7bc143] tw:text-white tw:cursor-pointer hover:tw:bg-[#6aab36] hover:tw:border-[#6aab36] disabled:tw:opacity-65 disabled:tw:pointer-events-none"
          :disabled="selectedCount === 0"
          @click="confirm"
        >
          {{ t.ok }}
        </button>
      </div>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { VueFinalModal } from "vue-final-modal";
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import { getTranslations } from "@bloom/helpers/localizations";
import type { IMediaPickerHandle } from "@media-app";
import { resolvePickerFilePath } from "../services/MediaPath";

const props = defineProps<{
  fieldId?: string;
  allowedExtensions: string;
  allowMultiple?: boolean;
  /** JSON-serialized translations for the media-app */
  mediaAppTranslations: string;
  /** Base path (e.g., "/") */
  basePath: string;
  /** Upload endpoint URL */
  uploadFilesUrl: string;
  /** When true, opens the modal immediately on mount (programmatic use). */
  autoOpen?: boolean;
  /** Called with selected items when confirmed (programmatic use). */
  onResolve?: (items: IMediaFieldItem[]) => void;
}>();

const emit = defineEmits<{
  select: [files: IMediaFieldItem[]];
}>();

onMounted(() => {
  if (props.autoOpen) {
    open();
  }
});

const t = getTranslations();

const visible = ref(false);
const loading = ref(false);
const error = ref("");
const containerRef = ref<HTMLElement | null>(null);
const selectedCount = ref(0);

function parseBoolean(value: unknown, defaultValue: boolean): boolean {
  if (typeof value === "boolean") {
    return value;
  }

  if (typeof value === "string") {
    const normalized = value.trim().toLowerCase();
    if (normalized === "true") {
      return true;
    }
    if (normalized === "false") {
      return false;
    }
  }

  return defaultValue;
}

const allowMultipleSelection = computed(() => parseBoolean(props.allowMultiple as unknown, true));

let pickerHandle: IMediaPickerHandle | null = null;

function parseAllowedExtensions(value: string): Set<string> {
  return new Set(
    value
      .split(",")
      .map((ext) => ext.trim().toLowerCase())
      .filter((ext) => ext.length > 0)
      .map((ext) => (ext.startsWith(".") ? ext : `.${ext}`))
  );
}

function getExtension(pathOrName: string): string {
  const idx = pathOrName.lastIndexOf(".");
  if (idx < 0) {
    return "";
  }

  return pathOrName.substring(idx).toLowerCase();
}

function open() {
  visible.value = true;
  loading.value = true;
  error.value = "";
  selectedCount.value = 0;
}

async function onOpened() {
  if (!containerRef.value) return;

  try {
    const { mountMediaAppAsPicker } = await import("@media-app");

    pickerHandle = mountMediaAppAsPicker(containerRef.value, {
      translations: props.mediaAppTranslations,
      basePath: props.basePath,
      uploadFilesUrl: props.uploadFilesUrl,
      allowedExtensions: props.allowedExtensions,
      allowMultiple: allowMultipleSelection.value,
      onSelectionChange: (count: number) => {
        selectedCount.value = count;
      },
    });
  } catch (e: unknown) {
    const msg = e instanceof Error ? e.message : String(e);
    console.error("Failed to load media app:", e);
    error.value = `Failed to load media browser: ${msg}`;
  } finally {
    loading.value = false;
  }
}

function confirm() {
  if (!pickerHandle) return;

  const selected = pickerHandle.getSelectedFiles();
  if (selected.length === 0) return;

  const allowedExtensions = parseAllowedExtensions(props.allowedExtensions);

  // Keep only selections with a valid media path.
  const items: IMediaFieldItem[] = selected
    .map((f, i) => {
      const mediaPath = resolvePickerFilePath(f);
      if (!mediaPath) {
        return null;
      }

      if (allowedExtensions.size > 0) {
        const ext = getExtension(mediaPath || f.name || "");
        if (!ext || !allowedExtensions.has(ext)) {
          return null;
        }
      }

      return {
        name: f.name,
        mime: f.mime || "",
        mediaPath,
        url: f.url,
        size: f.size,
        vuekey: mediaPath + i,
      } as IMediaFieldItem;
    })
    .filter((item): item is IMediaFieldItem => item !== null);

  if (items.length === 0) {
    return;
  }

  const limitedItems = allowMultipleSelection.value ? items : [items[0]];

  emit("select", limitedItems);
  props.onResolve?.(limitedItems);
  visible.value = false;
}

function cancel() {
  props.onResolve?.([]);
  visible.value = false;
}

function cleanup() {
  if (pickerHandle) {
    pickerHandle.unmount();
    pickerHandle = null;
  }
  selectedCount.value = 0;
}

function onClosed() {
  cleanup();
}

defineExpose({ open });
</script>
