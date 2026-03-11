<!--
  Media Picker Modal — opens a vue-final-modal with the full Media App
  embedded inside for browsing, uploading, and selecting media items.
  The media-app module URL and config come from data-* attributes on the field element.
-->
<template>
  <VueFinalModal
    v-model="visible"
    class="tw:flex tw:items-center tw:justify-center"
    content-class="mf-modal-content mf-modal-picker"
    @opened="onOpened"
    @closed="onClosed"
  >
    <div class="mf-modal-header tw:flex tw:items-center tw:justify-between">
      <h5>{{ T.selectMedia }}</h5>
      <button type="button" class="mf-btn-icon" @click="cancel">
        <i class="fa-solid fa-xmark" aria-hidden="true"></i>
      </button>
    </div>
    <div class="mf-modal-body mf-picker-body">
      <!-- Loading state while media-app module is being imported -->
      <div v-if="loading" class="tw:flex tw:items-center tw:justify-center tw:py-12">
        <i class="fa-solid fa-spinner fa-spin tw:mr-2" aria-hidden="true"></i>
        {{ T.loadingMediaBrowser }}
      </div>
      <!-- Error state -->
      <div v-else-if="error" class="tw:text-center tw:py-12" style="color: #dc3545;">
        <i class="fa-solid fa-triangle-exclamation tw:mr-2" aria-hidden="true"></i>
        {{ error }}
      </div>
      <!-- Container where the media-app will be mounted -->
      <div v-show="!loading && !error" ref="containerRef" class="mf-picker-media-app"></div>
    </div>
    <div class="mf-modal-footer tw:flex tw:items-center tw:justify-between">
      <span class="tw:text-sm tw:text-gray-500">
        {{ selectedCount > 0 ? `${selectedCount} selected` : '' }}
      </span>
      <div class="tw:flex tw:gap-2">
        <button type="button" class="mf-btn" @click="cancel">{{ T.cancel }}</button>
        <button
          type="button"
          class="mf-btn mf-btn-primary"
          :disabled="selectedCount === 0"
          @click="confirm"
        >
          {{ T.ok }}
        </button>
      </div>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { VueFinalModal } from "vue-final-modal";
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import { useLocalizations } from "@bloom/helpers/localizations";

interface IMediaPickerHandle {
  getSelectedFiles(): { filePath: string; name: string; mime?: string; url?: string; size?: number }[];
  clearSelection(): void;
  unmount(): void;
}

const props = defineProps<{
  fieldId: string;
  allowedExtensions: string;
  allowMultiple?: boolean;
  /** URL to the media-app ES module (from data-media-app-url) */
  mediaAppUrl: string;
  /** JSON-serialized translations for the media-app */
  mediaAppTranslations: string;
  /** Base path (e.g., "/") */
  basePath: string;
  /** Upload endpoint URL */
  uploadFilesUrl: string;
}>();

const emit = defineEmits<{
  select: [files: IMediaFieldItem[]];
}>();

const { translations: T } = useLocalizations();

const visible = ref(false);
const loading = ref(false);
const error = ref("");
const containerRef = ref<HTMLElement | null>(null);
const selectedCount = ref(0);

let pickerHandle: IMediaPickerHandle | null = null;

/**
 * Resolve a sibling file URL relative to this script (media-field2.js).
 */
function resolveSiblingUrl(filename: string): string {
  try {
    return new URL(/* @vite-ignore */ filename, import.meta.url).href;
  } catch {
    return `/OrchardCore.Media/Scripts/${filename}`;
  }
}

/**
 * Resolve the media-app module URL.
 */
function resolveMediaAppUrl(): string {
  return props.mediaAppUrl || resolveSiblingUrl("media2.js");
}

/**
 * Ensure media-app CSS is loaded (inject link tag if not already present).
 */
function ensureMediaAppCss() {
  // Check if media2.css is already loaded (e.g. by Razor <style> tag with version hash)
  const existing = [...document.querySelectorAll('link[rel="stylesheet"]')]
    .some(l => l.href.includes("/Styles/media2.css"));
  if (existing) return;

  // Derive CSS URL: Scripts/media2.js → Styles/media2.css
  const jsUrl = resolveMediaAppUrl();
  const cssUrl = jsUrl.replace("/Scripts/media2.js", "/Styles/media2.css");
  const link = document.createElement("link");
  link.id = "media-app-picker-css";
  link.rel = "stylesheet";
  link.href = cssUrl;
  document.head.appendChild(link);
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
    ensureMediaAppCss();
    const moduleUrl = resolveMediaAppUrl();
    const mediaAppModule = await import(/* @vite-ignore */ moduleUrl);

    if (!mediaAppModule.mountMediaAppAsPicker) {
      throw new Error("mountMediaAppAsPicker not found in media-app module");
    }

    pickerHandle = mediaAppModule.mountMediaAppAsPicker(containerRef.value, {
      translations: props.mediaAppTranslations,
      basePath: props.basePath,
      uploadFilesUrl: props.uploadFilesUrl,
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

  // Convert to IMediaFieldItem
  const items: IMediaFieldItem[] = selected.map((f, i) => ({
    name: f.name,
    mime: f.mime || "",
    mediaPath: f.filePath,
    url: f.url,
    size: f.size,
    vuekey: f.filePath + i,
  }));

  emit("select", items);
  visible.value = false;
}

function cancel() {
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
