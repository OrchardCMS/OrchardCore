<template>
  <div class="media-field mf-basic">
    <!-- Hidden input: serialized paths for form submission -->
    <input :name="inputName" type="hidden" :value="serializedPaths" />

    <!-- Toolbar -->
    <div class="mf-toolbar tw:flex tw:items-center tw:gap-2 tw:mb-2">
      <button
        type="button"
        class="mf-btn mf-btn-primary"
        :class="{ 'is-disabled': !canAddMedia }"
        :disabled="!canAddMedia"
        @click="showPicker"
      >
        <i class="fa-solid fa-plus" aria-hidden="true"></i>
        {{ t.addMedia }}
      </button>

      <button
        v-if="selectedMedia"
        type="button"
        class="mf-btn mf-btn-danger"
        @click="removeSelected"
      >
        <i class="fa-solid fa-trash" aria-hidden="true"></i>
        {{ t.removeMedia }}
      </button>

      <button
        v-if="selectedMedia && config.allowMediaText"
        type="button"
        class="mf-btn"
        @click="showMediaTextModal"
      >
        <i class="fa-solid fa-comment" aria-hidden="true"></i>
        {{ t.mediaText }}
      </button>

      <button
        v-if="selectedMedia && config.allowAnchors && isSelectedImage"
        type="button"
        class="mf-btn"
        @click="showAnchorModal"
      >
        <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
        {{ t.anchor }}
      </button>

      <!-- Thumb size toggle -->
      <button
        type="button"
        class="mf-btn mf-btn-icon tw:ml-auto"
        :title="smallThumbs ? t.largeThumbsTitle : t.smallThumbsTitle"
        @click="smallThumbs = !smallThumbs"
      >
        <i :class="smallThumbs ? 'fa-solid fa-up-right-and-down-left-from-center' : 'fa-solid fa-down-left-and-up-right-to-center'" aria-hidden="true"></i>
      </button>
    </div>

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

    <!-- Media Picker Modal -->
    <MediaPickerModal
      ref="pickerModalRef"
      :field-id="inputName"
      :allowed-extensions="config.allowedExtensions"
      :allow-multiple="multiple"
      :media-app-translations="config.mediaAppTranslations || ''"
      :base-path="config.basePath || ''"
      :upload-files-url="config.uploadFilesUrl || ''"
      @select="onPickerSelect"
    />

    <!-- Media Text Modal (simple inline dialog via vue-final-modal) -->
    <VueFinalModal
      v-model="mediaTextModalVisible"
      class="tw:flex tw:items-center tw:justify-center"
      content-class="mf-modal-content"
    >
      <div class="mf-modal-header">
        <h5>{{ t.editMediaText }}</h5>
      </div>
      <div class="mf-modal-body" v-if="selectedMedia">
        <label class="tw:block tw:text-sm tw:font-medium tw:mb-1">{{ t.mediaText }}</label>
        <textarea
          v-model="editingMediaText"
          class="mf-input tw:w-full"
          rows="3"
        ></textarea>
      </div>
      <div class="mf-modal-footer tw:flex tw:justify-end tw:gap-2">
        <button type="button" class="mf-btn" @click="cancelMediaText">{{ t.cancel }}</button>
        <button type="button" class="mf-btn mf-btn-primary" @click="saveMediaText">{{ t.ok }}</button>
      </div>
    </VueFinalModal>

    <!-- Anchor Modal -->
    <VueFinalModal
      v-model="anchorModalVisible"
      class="tw:flex tw:items-center tw:justify-center"
      content-class="mf-modal-content mf-modal-anchor"
    >
      <div class="mf-modal-header">
        <h5>{{ t.editAnchor }}</h5>
      </div>
      <div class="mf-modal-body tw:relative" v-if="selectedMedia" ref="anchorModalBody">
        <div class="mf-anchor-image-container tw:relative tw:inline-block tw:cursor-crosshair" @click="setAnchor">
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
      <div class="mf-modal-footer tw:flex tw:justify-end tw:gap-2">
        <button type="button" class="mf-btn" @click="resetAnchor">{{ t.resetAnchor }}</button>
        <button type="button" class="mf-btn" @click="cancelAnchor">{{ t.cancel }}</button>
        <button type="button" class="mf-btn mf-btn-primary" @click="saveAnchor">{{ t.ok }}</button>
      </div>
    </VueFinalModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount, nextTick } from "vue";
import { VueFinalModal } from "vue-final-modal";
import ThumbsContainer from "./ThumbsContainer.vue";
import MediaPickerModal from "./MediaPickerModal.vue";
import type {
  IMediaFieldItem,
  IMediaFieldConfig,
  IMediaFieldPath,
} from "../interfaces/MediaFieldTypes";
import { getTranslations } from "@bloom/helpers/localizations";
import { isValidMediaPath, normalizeMediaPath, sanitizeFieldPaths } from "../services/MediaPath";

const props = defineProps<{
  config: IMediaFieldConfig;
  inputName: string;
}>();

const t = getTranslations();

// --- State ---
const mediaItems = ref<IMediaFieldItem[]>([]);
const selectedMedia = ref<IMediaFieldItem | null>(null);
const initialized = ref(false);
const smallThumbs = ref(false);

// Picker modal
const pickerModalRef = ref<InstanceType<typeof MediaPickerModal> | null>(null);

// Modals
const mediaTextModalVisible = ref(false);
const editingMediaText = ref("");
const anchorModalVisible = ref(false);
const editingAnchor = ref({ x: 0.5, y: 0.5 });
const anchorImageRef = ref<HTMLImageElement | null>(null);
const anchorModalBody = ref<HTMLElement | null>(null);

// --- Computed ---
const multiple = computed(() => props.config.multiple);

const thumbSize = computed(() => (smallThumbs.value ? 120 : 240));

const canAddMedia = computed(
  () => mediaItems.value.length === 0 || multiple.value
);

const isSelectedImage = computed(() => {
  if (!selectedMedia.value?.mime) return false;
  return selectedMedia.value.mime.startsWith("image");
});

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

// --- Load initial media items ---
onMounted(() => {
  // Restore prefs
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

  const items: IMediaFieldItem[] = validPaths.map((p, i) => ({
    name: " " + p.path, // placeholder name
    mime: "",
    mediaPath: p.path,
    vuekey: p.path + i,
  }));

  const loaded = await Promise.all(
    validPaths.map(async (p, i) => {
      try {
        const url = `${props.config.mediaItemUrl}?path=${encodeURIComponent(p.path)}`;
        const resp = await fetch(url);
        if (!resp.ok) {
          return {
            name: p.path,
            mime: "",
            mediaPath: p.path,
            errorType: resp.status === 404 ? "not-found" : "transient",
            mediaText: p.mediaText,
            anchor: p.anchor,
            vuekey: p.path + i,
          } as IMediaFieldItem;
        }
        const data = await resp.json();
        return {
          ...data,
          mediaPath: normalizeMediaPath(data.mediaPath) ?? p.path,
          mediaText: p.mediaText,
          anchor: p.anchor,
          vuekey: data.name + i,
        } as IMediaFieldItem;
      } catch {
        return {
          name: p.path,
          mime: "",
          mediaPath: p.path,
          errorType: "transient",
          mediaText: p.mediaText,
          anchor: p.anchor,
          vuekey: p.path + i,
        } as IMediaFieldItem;
      }
    })
  );

  mediaItems.value = loaded;
  initialized.value = true;

  // Auto-select first item for single-select fields
  if (!multiple.value && mediaItems.value.length === 1) {
    await nextTick();
    selectedMedia.value = mediaItems.value[0];
  }
}

// --- Click outside to deselect ---
function onDocumentClick(e: MouseEvent) {
  const el = (e.target as HTMLElement);
  // If the click is inside a thumb item or the toolbar, do nothing
  if (el.closest('.mf-thumb-item') || el.closest('.mf-toolbar') || el.closest('.vfm')) return;
  selectedMedia.value = null;
}

onMounted(() => {
  document.addEventListener('click', onDocumentClick);
});

onBeforeUnmount(() => {
  document.removeEventListener('click', onDocumentClick);
});

// --- Actions ---
function selectMedia(media: IMediaFieldItem) {
  selectedMedia.value = media;
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

/**
 * Called when files are selected from the media picker.
 * This is the public API for external integration (e.g., media picker modal).
 */
function addMediaFiles(files: IMediaFieldItem[]) {
  const validFiles = files.filter((item) => isValidMediaPath(item.mediaPath));
  if (validFiles.length === 0) {
    return;
  }

  if (validFiles.length > 1 && !multiple.value) {
    // Only allow one item for single-select fields
    mediaItems.value = [validFiles[0]];
  } else if (multiple.value) {
    mediaItems.value = mediaItems.value.concat(validFiles);
  } else {
    mediaItems.value = [validFiles[0]];
  }
  initialized.value = true;

  if (!multiple.value && mediaItems.value.length === 1) {
    nextTick(() => {
      selectedMedia.value = mediaItems.value[0];
    });
  }
}

function showPicker() {
  if (!canAddMedia.value) return;
  pickerModalRef.value?.open();
}

function onPickerSelect(files: IMediaFieldItem[]) {
  addMediaFiles(files);
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
      document.dispatchEvent(new CustomEvent("contentpreview:render"));
    }, 100);
  },
  { deep: true }
);

// Expose addMediaFiles for external use (parent/mount function)
defineExpose({ addMediaFiles, mediaItems, selectedMedia, smallThumbs });
</script>
