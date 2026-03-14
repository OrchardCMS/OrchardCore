<template>
  <div class="media-field mf-gallery-field">
    <!-- Hidden input: serialized paths for form submission -->
    <input :name="inputName" type="hidden" :value="serializedPaths" />

    <!-- Gallery container -->
    <GalleryContainer
      :media-items="mediaItems"
      :allow-multiple="multiple"
      :allow-media-text="config.allowMediaText"
      :allow-anchors="config.allowAnchors"
      :id-prefix="inputName"
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
      @select="onPickerSelect"
    />

    <!-- Media Text Modal -->
    <VueFinalModal
      v-model="mediaTextModalVisible"
      class="tw:flex tw:items-center tw:justify-center"
      content-class="mf-modal-content"
    >
      <div class="mf-modal-header">
        <h5>{{ T.editMediaText }}</h5>
      </div>
      <div class="mf-modal-body" v-if="selectedMedia">
        <label class="tw:block tw:text-sm tw:font-medium tw:mb-1">{{ T.mediaText }}</label>
        <textarea
          v-model="editingMediaText"
          class="mf-input tw:w-full"
          rows="3"
        ></textarea>
      </div>
      <div class="mf-modal-footer tw:flex tw:justify-end tw:gap-2">
        <button type="button" class="mf-btn" @click="cancelMediaText">{{ T.cancel }}</button>
        <button type="button" class="mf-btn mf-btn-primary" @click="saveMediaText">{{ T.ok }}</button>
      </div>
    </VueFinalModal>

    <!-- Anchor Modal -->
    <VueFinalModal
      v-model="anchorModalVisible"
      class="tw:flex tw:items-center tw:justify-center"
      content-class="mf-modal-content mf-modal-anchor"
    >
      <div class="mf-modal-header">
        <h5>{{ T.editAnchor }}</h5>
      </div>
      <div class="mf-modal-body tw:relative" v-if="selectedMedia">
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
        <button type="button" class="mf-btn" @click="resetAnchor">{{ T.resetAnchor }}</button>
        <button type="button" class="mf-btn" @click="cancelAnchor">{{ T.cancel }}</button>
        <button type="button" class="mf-btn mf-btn-primary" @click="saveAnchor">{{ T.ok }}</button>
      </div>
    </VueFinalModal>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { VueFinalModal } from "vue-final-modal";
import GalleryContainer from "./GalleryContainer.vue";
import MediaPickerModal from "./MediaPickerModal.vue";
import type {
  IMediaFieldItem,
  IMediaFieldConfig,
  IMediaFieldPath,
} from "../interfaces/MediaFieldTypes";
import { useLocalizations } from "../composables/useLocalizations";

const props = defineProps<{
  config: IMediaFieldConfig;
  inputName: string;
}>();

const { translations: T } = useLocalizations();

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
  const paths: IMediaFieldPath[] = mediaItems.value.map((m) => ({
    path: m.mediaPath,
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
  if (!paths || paths.length === 0) {
    initialized.value = true;
    return;
  }

  const loaded = await Promise.all(
    paths.map(async (p, i) => {
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
}

// --- Actions ---
function onReorder(items: IMediaFieldItem[]) {
  mediaItems.value = items;
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
  if (files.length > 1 && !multiple.value) {
    mediaItems.value = [files[0]];
  } else if (multiple.value) {
    mediaItems.value = mediaItems.value.concat(files);
  } else {
    mediaItems.value = [files[0]];
  }
  initialized.value = true;
}

function showPicker() {
  pickerModalRef.value?.open();
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
watch(
  mediaItems,
  () => {
    setTimeout(() => {
      document.dispatchEvent(new CustomEvent("contentpreview:render"));
    }, 100);
  },
  { deep: true }
);

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
