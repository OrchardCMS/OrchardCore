<template>
  <div class="media-field">
    <!-- Hidden input: serialized paths for form submission -->
    <input :name="inputName" type="hidden" :value="serializedPaths" />

    <!-- Toolbar (mf-toolbar kept as JS hook for click-outside deselect) -->
    <div class="mf-toolbar tw:flex tw:items-center tw:gap-2 tw:mb-2">
      <!-- Upload button (file input trigger) -->
      <label
        :for="uploadInputId"
        class="mf-btn-primary tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[#7bc143] tw:rounded tw:bg-[#7bc143] tw:text-white tw:cursor-pointer hover:tw:bg-[#6aab36] hover:tw:border-[#6aab36]"
        :class="{ 'is-disabled tw:opacity-65 tw:pointer-events-none': !canAddMedia }"
      >
        <i class="fa-solid fa-upload" aria-hidden="true"></i>
        {{ t.addMedia }}
      </label>
      <input
        :id="uploadInputId"
        type="file"
        class="tw:hidden"
        :multiple="multiple"
        :accept="config.allowedExtensions || undefined"
        :disabled="!canAddMedia"
        @change="onFileInputChange"
      />

      <button
        v-if="selectedMedia"
        type="button"
        class="mf-btn-danger tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[#dc3545] tw:rounded tw:bg-[#dc3545] tw:text-white tw:cursor-pointer hover:tw:bg-[#bb2d3b] hover:tw:border-[#bb2d3b]"
        @click="removeSelected"
      >
        <i class="fa-solid fa-trash" aria-hidden="true"></i>
        {{ t.removeMedia }}
      </button>

      <button
        v-if="selectedMedia && allowMediaText"
        type="button"
        class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:cursor-pointer hover:tw:bg-[#e9ecef] dark:tw:bg-[#2b3035] dark:tw:text-[#dee2e6] dark:tw:border-[#495057] dark:hover:tw:bg-[#343a40]"
        @click="showMediaTextModal"
      >
        <i class="fa-solid fa-comment" aria-hidden="true"></i>
        {{ t.mediaText }}
      </button>

      <button
        v-if="selectedMedia && allowAnchors && isSelectedImage"
        type="button"
        class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:cursor-pointer hover:tw:bg-[#e9ecef] dark:tw:bg-[#2b3035] dark:tw:text-[#dee2e6] dark:tw:border-[#495057] dark:hover:tw:bg-[#343a40]"
        @click="showAnchorModal"
      >
        <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
        {{ t.anchor }}
      </button>

      <!-- Thumb size toggle -->
      <button
        type="button"
        class="mf-btn-icon tw:inline-flex tw:items-center tw:gap-1.5 tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6] tw:ml-auto"
        :title="smallThumbs ? t.largeThumbsTitle : t.smallThumbsTitle"
        @click="smallThumbs = !smallThumbs"
      >
        <i :class="smallThumbs ? 'fa-solid fa-up-right-and-down-left-from-center' : 'fa-solid fa-down-left-and-up-right-to-center'" aria-hidden="true"></i>
      </button>
    </div>

    <!-- Drop zone wrapper -->
    <div
      class="mf-drop-zone tw:relative tw:border-2 tw:border-dashed tw:border-transparent tw:rounded tw:transition-colors"
      :class="isDraggingOver ? 'mf-drop-zone-active tw:border-[#7bc143] tw:bg-[rgba(123,193,67,0.05)] dark:tw:bg-[rgba(123,193,67,0.1)]' : ''"
      @dragover.prevent="isDraggingOver = true"
      @dragleave="isDraggingOver = false"
      @drop.prevent="onDrop"
    >
      <!-- Thumbnails -->
      <ThumbsContainer
        :media-items="mediaItems"
        :selected-media="selectedMedia"
        :thumb-size="thumbSize"
        :allow-multiple="multiple"
        @select-media="selectMedia"
        @delete-media="selectAndDeleteMedia"
        @reorder="onReorder"
      />

      <!-- Drop hint overlay -->
      <div v-if="isDraggingOver && canAddMedia" class="mf-drop-overlay tw:absolute tw:inset-0 tw:flex tw:flex-col tw:items-center tw:justify-center tw:bg-[rgba(255,255,255,0.85)] tw:text-[#7bc143] tw:font-medium tw:rounded tw:z-10 tw:pointer-events-none dark:tw:bg-[rgba(33,37,41,0.9)]">
        <i class="fa-solid fa-cloud-arrow-up tw:text-4xl tw:mb-2" aria-hidden="true"></i>
        <span>{{ t.dropFiles }}</span>
      </div>
    </div>

    <!-- Upload toast (fixed bottom-right) -->
    <UploadList
      :files="uploadService.files.value"
      @clear-errors="uploadService.clearErrors()"
      @dismiss="uploadService.dismiss($event)"
      @dismiss-all="uploadService.dismissAll()"
    />

    <!-- Media Text Modal -->
    <VueFinalModal
      v-model="mediaTextModalVisible"
      class="tw:flex tw:items-center tw:justify-center"
      content-class="tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:rounded-lg tw:p-0 tw:min-w-[300px] tw:max-w-[600px] tw:w-[90vw] tw:shadow-md dark:tw:bg-[#212529] dark:tw:text-[#dee2e6] dark:tw:border dark:tw:border-[#495057]"
    >
      <div class="tw:p-4 tw:border-b tw:border-[var(--bs-border-color)] dark:tw:border-[#495057]">
        <h5 class="tw:m-0 tw:text-lg tw:font-medium">{{ t.editMediaText }}</h5>
      </div>
      <div class="tw:p-4" v-if="selectedMedia">
        <label class="tw:block tw:text-sm tw:font-medium tw:mb-1">{{ t.mediaText }}</label>
        <textarea
          v-model="editingMediaText"
          class="tw:block tw:w-full tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] focus:tw:border-[#7bc143] focus:tw:outline-none dark:tw:bg-[#2b3035] dark:tw:text-[#dee2e6] dark:tw:border-[#495057]"
          rows="3"
        ></textarea>
      </div>
      <div class="tw:flex tw:justify-end tw:gap-2 tw:px-4 tw:py-3 tw:border-t tw:border-[var(--bs-border-color)] dark:tw:border-[#495057]">
        <button type="button" class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:cursor-pointer hover:tw:bg-[#e9ecef] dark:tw:bg-[#2b3035] dark:tw:text-[#dee2e6] dark:tw:border-[#495057] dark:hover:tw:bg-[#343a40]" @click="cancelMediaText">{{ t.cancel }}</button>
        <button type="button" class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[#7bc143] tw:rounded tw:bg-[#7bc143] tw:text-white tw:cursor-pointer hover:tw:bg-[#6aab36] hover:tw:border-[#6aab36]" @click="saveMediaText">{{ t.ok }}</button>
      </div>
    </VueFinalModal>

    <!-- Anchor Modal -->
    <VueFinalModal
      v-model="anchorModalVisible"
      class="tw:flex tw:items-center tw:justify-center"
      content-class="tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:rounded-lg tw:p-0 tw:min-w-[300px] tw:max-w-[800px] tw:w-[90vw] tw:shadow-md dark:tw:bg-[#212529] dark:tw:text-[#dee2e6] dark:tw:border dark:tw:border-[#495057]"
    >
      <div class="tw:p-4 tw:border-b tw:border-[var(--bs-border-color)] dark:tw:border-[#495057]">
        <h5 class="tw:m-0 tw:text-lg tw:font-medium">{{ t.editAnchor }}</h5>
      </div>
      <div class="tw:p-4 tw:relative" v-if="selectedMedia">
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
      <div class="tw:flex tw:justify-end tw:gap-2 tw:px-4 tw:py-3 tw:border-t tw:border-[var(--bs-border-color)] dark:tw:border-[#495057]">
        <button type="button" class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:cursor-pointer hover:tw:bg-[#e9ecef] dark:tw:bg-[#2b3035] dark:tw:text-[#dee2e6] dark:tw:border-[#495057] dark:hover:tw:bg-[#343a40]" @click="resetAnchor">{{ t.resetAnchor }}</button>
        <button type="button" class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:text-[var(--bs-body-color)] tw:cursor-pointer hover:tw:bg-[#e9ecef] dark:tw:bg-[#2b3035] dark:tw:text-[#dee2e6] dark:tw:border-[#495057] dark:hover:tw:bg-[#343a40]" @click="cancelAnchor">{{ t.cancel }}</button>
        <button type="button" class="tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[#7bc143] tw:rounded tw:bg-[#7bc143] tw:text-white tw:cursor-pointer hover:tw:bg-[#6aab36] hover:tw:border-[#6aab36]" @click="saveAnchor">{{ t.ok }}</button>
      </div>
    </VueFinalModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount, nextTick } from "vue";
import { VueFinalModal } from "vue-final-modal";
import ThumbsContainer from "./ThumbsContainer.vue";
import UploadList from "./UploadList.vue";
import type {
  IMediaFieldItem,
  IMediaFieldPath,
} from "../interfaces/MediaFieldTypes";
import { getTranslations } from "@bloom/helpers/localizations";
import { useFieldUpload, type IFieldUploadConfig } from "../services/FieldUploadService";
import { isValidMediaPath, normalizeMediaPath, sanitizeFieldPaths } from "../services/MediaPath";
import { loadInitialMediaItems } from "../services/InitialMediaLoader";

export interface IAttachedFieldConfig {
  paths: IMediaFieldPath[];
  multiple: boolean;
  allowMediaText: boolean;
  allowAnchors: boolean;
  allowedExtensions: string;
  mediaItemUrl: string;
  mediaItemsUrl?: string;
  uploadAction: string;
  tempUploadFolder: string;
  maxUploadChunkSize?: number;
  maxFileSize?: number;
  debugEnabled?: boolean;
  tusEnabled?: boolean;
  tusEndpointUrl?: string;
  tusFileInfoUrl?: string;
}

const props = defineProps<{
  config: IAttachedFieldConfig;
  inputName: string;
}>();

const t = getTranslations();

// Upload service
const uploadConfig: IFieldUploadConfig = {
  uploadAction: props.config.uploadAction,
  tempUploadFolder: props.config.tempUploadFolder,
  maxUploadChunkSize: props.config.maxUploadChunkSize,
  maxFileSize: props.config.maxFileSize,
  debugEnabled: props.config.debugEnabled,
  tusEnabled: props.config.tusEnabled,
  tusEndpointUrl: props.config.tusEndpointUrl,
  tusFileInfoUrl: props.config.tusFileInfoUrl,
};
const uploadService = useFieldUpload(uploadConfig);

// --- State ---
const mediaItems = ref<IMediaFieldItem[]>([]);
const selectedMedia = ref<IMediaFieldItem | null>(null);
const initialized = ref(false);
const smallThumbs = ref(false);
const isDraggingOver = ref(false);
const uploadInputId = `upload-${Math.random().toString(36).substring(2, 9)}`;

// Modals
const mediaTextModalVisible = ref(false);
const editingMediaText = ref("");
const anchorModalVisible = ref(false);
const editingAnchor = ref({ x: 0.5, y: 0.5 });
const anchorImageRef = ref<HTMLImageElement | null>(null);

// --- Computed ---
const multiple = computed(() => props.config.multiple);
const allowMediaText = computed(() => props.config.allowMediaText);
const allowAnchors = computed(() => props.config.allowAnchors);

const thumbSize = computed(() => (smallThumbs.value ? 120 : 240));

const canAddMedia = computed(() => {
  const nonRemoved = mediaItems.value.filter((m) => !m.isRemoved);
  return nonRemoved.length === 0 || multiple.value;
});

const isSelectedImage = computed(() => {
  if (!selectedMedia.value?.mime) return false;
  return selectedMedia.value.mime.startsWith("image");
});

const serializedPaths = computed(() => {
  if (!initialized.value) {
    return JSON.stringify(props.config.paths);
  }
  const paths: (IMediaFieldPath & { isRemoved?: boolean; isNew?: boolean; attachedFileName?: string })[] =
    mediaItems.value
      .filter((m) => isValidMediaPath(m.mediaPath))
      .map((m) => ({
        path: normalizeMediaPath(m.mediaPath)!,
        isRemoved: m.isRemoved,
        isNew: m.isNew,
        mediaText: m.mediaText,
        anchor: m.anchor,
        attachedFileName: m.attachedFileName,
      }));
  return JSON.stringify(paths);
});

// --- Load initial media items ---
onMounted(() => {
  try {
    const prefs = JSON.parse(localStorage.getItem("mediaFieldPrefs") || "null");
    if (prefs) smallThumbs.value = prefs.smallThumbs;
  } catch {
    // ignore
  }

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

// --- Click outside to deselect ---
function onDocumentClick(e: MouseEvent) {
  const el = e.target as HTMLElement;
  if (el.closest(".mf-thumb-item") || el.closest(".vfm")) {
    return;
  }

  selectedMedia.value = null;
}

// --- File upload handling ---
async function handleFiles(fileList: File[]) {
  if (!canAddMedia.value) return;

  const { uploaded, errors } = await uploadService.uploadFiles(fileList);

  if (errors.length > 0) {
    console.warn("[media-field] Upload errors:", errors);
  }

  if (uploaded.length > 0) {
    const newItems: IMediaFieldItem[] = uploaded
      .map((f, i) => {
        const mediaPath = normalizeMediaPath(f.mediaPath);
        if (!mediaPath) {
          return null;
        }

        return {
          name: f.name,
          mime: f.mime || "",
          mediaPath,
          url: f.url || "",
          size: f.size || 0,
          isNew: true,
          attachedFileName: f.attachedFileName,
          vuekey: f.name + Date.now() + i,
        } as IMediaFieldItem;
      })
      .filter((item): item is IMediaFieldItem => item !== null);

    if (newItems.length === 0) {
      return;
    }

    if (newItems.length > 1 && !multiple.value) {
      mediaItems.value = [newItems[0]];
    } else if (multiple.value) {
      mediaItems.value = mediaItems.value.concat(newItems);
    } else {
      mediaItems.value = [newItems[0]];
    }

    initialized.value = true;

    if (!multiple.value && mediaItems.value.length === 1) {
      await nextTick();
      selectedMedia.value = mediaItems.value[0];
    }
  }
}

function onFileInputChange(event: Event) {
  const input = event.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const fileList = Array.from(input.files);
  handleFiles(fileList);
  input.value = ""; // reset so same file can be re-selected
}

function onDrop(event: DragEvent) {
  isDraggingOver.value = false;
  if (!event.dataTransfer?.files || event.dataTransfer.files.length === 0) return;
  const fileList = Array.from(event.dataTransfer.files);
  handleFiles(fileList);
}

// --- Actions ---
function selectMedia(media: IMediaFieldItem) {
  selectedMedia.value = selectedMedia.value === media ? null : media;
}

function removeSelected() {
  if (selectedMedia.value) {
    const idx = mediaItems.value.indexOf(selectedMedia.value);
    if (idx > -1) mediaItems.value.splice(idx, 1);
  } else if (mediaItems.value.length === 1) {
    mediaItems.value.splice(0, 1);
  }
  selectedMedia.value = null;
}

function selectAndDeleteMedia(media: IMediaFieldItem) {
  selectedMedia.value = media;
  nextTick(() => removeSelected());
}

function onReorder(items: IMediaFieldItem[]) {
  mediaItems.value = items.filter((item) => isValidMediaPath(item.mediaPath));
}

// --- Media Text Modal ---
function showMediaTextModal() {
  if (!selectedMedia.value) return;
  editingMediaText.value = selectedMedia.value.mediaText || "";
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
function showAnchorModal() {
  if (!selectedMedia.value) return;
  editingAnchor.value = selectedMedia.value.anchor
    ? { ...selectedMedia.value.anchor }
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

// --- Persist prefs ---
watch(smallThumbs, (val) => {
  localStorage.setItem("mediaFieldPrefs", JSON.stringify({ smallThumbs: val }));
});

// --- Trigger content preview on media changes ---
watch(
  mediaItems,
  () => {
    setTimeout(() => {
      if (typeof document !== "undefined") {
        document.dispatchEvent(new CustomEvent("contentpreview:render"));
      }
    }, 100);
  },
  { deep: true }
);

onMounted(() => {
  document.addEventListener("click", onDocumentClick);
});

onBeforeUnmount(() => {
  document.removeEventListener("click", onDocumentClick);
});

defineExpose({ mediaItems, selectedMedia, isDraggingOver });
</script>
