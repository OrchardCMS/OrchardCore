<template>
  <div class="media-field">
    <!-- Hidden input: serialized paths for form submission -->
    <input :name="inputName" type="hidden" :value="serializedPaths" />

    <!-- Gallery container -->
    <GalleryContainer
      :media-items="mediaItems"
      :allow-multiple="multiple"
      :allow-media-text="config.allowMediaText"
      :allow-anchors="config.allowAnchors"
      :id-prefix="inputName"
      :disable-thumbnails="config.disableThumbnails ?? false"
      @reorder="onReorder"
      @edit-media-text="onEditMediaText"
      @edit-anchor="onEditAnchor"
      @delete-media="selectAndDeleteMedia"
      @show-picker="showPicker"
    />

    <!-- Media Picker Modal -->
    <MediaPickerModal
      ref="pickerModalRef"
      :field-id="inputName"
      :allowed-extensions="config.allowedExtensions"
      :allow-multiple="multiple"
      :media-app-translations="config.mediaAppTranslations || ''"
      :base-path="config.basePath || ''"
      :upload-files-url="config.uploadFilesUrl || ''"
      :disable-thumbnails="config.disableThumbnails ?? false"
      @select="onPickerSelect"
    />

    <!-- Media Text Modal -->
    <VueFinalModal
      v-model="mediaTextModalVisible"
      class="mf-vfm tw:flex tw:items-center tw:justify-center"
      content-class="mf-modal tw:min-w-[300px] tw:max-w-[600px] tw:w-[90vw]"
    >
      <div class="mf-modal-header">
        <h5 class="mf-modal-title">{{ t.editMediaText }}</h5>
      </div>
      <div class="mf-modal-body" v-if="selectedMedia">
        <label class="tw:block tw:text-sm tw:font-medium tw:mb-1">{{ t.mediaText }}</label>
        <textarea
          v-model="editingMediaText"
          class="mf-form-textarea"
          rows="3"
        ></textarea>
      </div>
      <div class="mf-modal-footer">
        <button type="button" class="mf-btn-secondary" @click="cancelMediaText">{{ t.cancel }}</button>
        <button type="button" class="mf-btn-primary" @click="saveMediaText">{{ t.ok }}</button>
      </div>
    </VueFinalModal>

    <!-- Anchor Modal -->
    <VueFinalModal
      v-model="anchorModalVisible"
      class="mf-vfm tw:flex tw:items-center tw:justify-center"
      content-class="mf-modal tw:min-w-[300px] tw:max-w-[800px] tw:w-[90vw]"
    >
      <div class="mf-modal-header">
        <h5 class="mf-modal-title">{{ t.editAnchor }}</h5>
      </div>
      <div class="mf-modal-body tw:relative" v-if="selectedMedia">
        <div class="tw:relative tw:inline-block tw:cursor-crosshair" @click="setAnchor">
          <img
            ref="anchorImageRef"
            :src="selectedMedia.url"
            class="tw:max-w-full"
          />
          <div
            class="mf-anchor-marker"
            :style="{
              left: (editingAnchor.x * 100) + '%',
              top: (editingAnchor.y * 100) + '%',
            }"
          >
            <i class="fa-solid fa-crosshairs icon-media-anchor"></i>
          </div>
        </div>
      </div>
      <div class="mf-modal-footer">
        <button type="button" class="mf-btn-secondary" @click="resetAnchor">{{ t.resetAnchor }}</button>
        <button type="button" class="mf-btn-secondary" @click="cancelAnchor">{{ t.cancel }}</button>
        <button type="button" class="mf-btn-primary" @click="saveAnchor">{{ t.ok }}</button>
      </div>
    </VueFinalModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount } from "vue";
import { VueFinalModal } from "vue-final-modal";
import GalleryContainer from "./GalleryContainer.vue";
import MediaPickerModal from "./MediaPickerModal.vue";
import type {
  IMediaFieldItem,
  IMediaFieldConfig,
  IMediaFieldPath,
} from "../interfaces/MediaFieldTypes";
import { getTranslations } from "@bloom/helpers/localizations";
import { isValidMediaPath, normalizeMediaPath, sanitizeFieldPaths } from "../services/MediaPath";
import { loadInitialMediaItems } from "../services/InitialMediaLoader";

const props = defineProps<{
  config: IMediaFieldConfig;
  inputName: string;
}>();

const t = getTranslations();

// --- State ---
const mediaItems = ref<IMediaFieldItem[]>([]);
const selectedMedia = ref<IMediaFieldItem | null>(null);
const initialized = ref(false);

// Picker modal
const pickerModalRef = ref<InstanceType<typeof MediaPickerModal> | null>(null);

// Modals
const mediaTextModalVisible = ref(false);
const editingMediaText = ref("");
const anchorModalVisible = ref(false);
const editingAnchor = ref({ x: 0.5, y: 0.5 });
const anchorImageRef = ref<HTMLImageElement | null>(null);

// --- Computed ---
const multiple = computed(() => props.config.multiple);

const serializedPaths = computed(() => {
  if (!initialized.value) {
    return JSON.stringify(props.config.paths);
  }
  const paths: IMediaFieldPath[] = mediaItems.value
    .filter((m) => isValidMediaPath(m.mediaPath))
    .map((m) => ({
      path: normalizeMediaPath(m.mediaPath)!,
      mediaText: m.mediaText,
      anchor: m.anchor,
    }));
  return JSON.stringify(paths);
});

// --- Load initial paths ---
onMounted(() => {
  loadInitialPaths(props.config.paths);
});

async function loadInitialPaths(paths: IMediaFieldPath[]) {
  const validPaths = sanitizeFieldPaths(paths);
  if (validPaths.length === 0) {
    initialized.value = true;
    return;
  }

  mediaItems.value = await loadInitialMediaItems(validPaths, props.config);
  initialized.value = true;
}

// --- Actions ---
function onReorder(items: IMediaFieldItem[]) {
  mediaItems.value = items.filter((item) => isValidMediaPath(item.mediaPath));
}

function selectAndDeleteMedia(media: IMediaFieldItem) {
  const idx = mediaItems.value.indexOf(media);
  if (idx > -1) {
    mediaItems.value.splice(idx, 1);
  }
  if (selectedMedia.value === media) {
    selectedMedia.value = null;
  }
}

function addMediaFiles(files: IMediaFieldItem[]) {
  const existingPaths = new Set(mediaItems.value.map((m) => m.mediaPath));
  const validFiles = files.filter(
    (item) => isValidMediaPath(item.mediaPath) && !existingPaths.has(item.mediaPath)
  );
  if (validFiles.length === 0) {
    return;
  }

  if (validFiles.length > 1 && !multiple.value) {
    mediaItems.value = [validFiles[0]];
  } else if (multiple.value) {
    mediaItems.value = mediaItems.value.concat(validFiles);
  } else {
    mediaItems.value = [validFiles[0]];
  }
  initialized.value = true;
}

function showPicker() {
  pickerModalRef.value?.open?.();
}

function onPickerSelect(files: IMediaFieldItem[]) {
  addMediaFiles(files);
}

// --- Media Text Modal ---
function onEditMediaText(media: IMediaFieldItem) {
  selectedMedia.value = media;
  editingMediaText.value = media.mediaText || "";
  mediaTextModalVisible.value = true;
}

function saveMediaText() {
  if (selectedMedia.value) {
    selectedMedia.value.mediaText = editingMediaText.value;
  }
  mediaTextModalVisible.value = false;
}

function cancelMediaText() {
  mediaTextModalVisible.value = false;
}

// --- Anchor Modal ---
function onEditAnchor(media: IMediaFieldItem) {
  selectedMedia.value = media;
  editingAnchor.value = media.anchor
    ? { ...media.anchor }
    : { x: 0.5, y: 0.5 };
  anchorModalVisible.value = true;
}

function setAnchor(event: MouseEvent) {
  const img = anchorImageRef.value;
  if (!img) return;
  editingAnchor.value = {
    x: event.offsetX / img.clientWidth,
    y: event.offsetY / img.clientHeight,
  };
}

function resetAnchor() {
  editingAnchor.value = { x: 0.5, y: 0.5 };
}

function saveAnchor() {
  if (selectedMedia.value) {
    selectedMedia.value.anchor = { ...editingAnchor.value };
  }
  anchorModalVisible.value = false;
}

function cancelAnchor() {
  anchorModalVisible.value = false;
}

// --- Trigger content preview on media changes ---
// Debounced so rapid changes dispatch once, and cleared on unmount so the timer never fires
// after the component is gone (which would otherwise throw once the DOM is torn down).
let previewTimer: ReturnType<typeof setTimeout> | undefined;
watch(
  mediaItems,
  () => {
    clearTimeout(previewTimer);
    previewTimer = setTimeout(() => {
      document.dispatchEvent(new CustomEvent("contentpreview:render"));
    }, 100);
  },
  { deep: true }
);

onBeforeUnmount(() => clearTimeout(previewTimer));

defineExpose({
  addMediaFiles,
  mediaItems,
  selectedMedia,
  onEditMediaText,
  saveMediaText,
  editingMediaText,
  onEditAnchor,
  saveAnchor,
  resetAnchor,
  editingAnchor,
  mediaTextModalVisible,
  anchorModalVisible,
});
</script>
