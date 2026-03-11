<!--
  Gallery container with list/grid view toggle, size toggle, and drag-drop reordering.
-->
<template>
  <div class="mf-gallery">
    <!-- Toolbar (only for multiple mode) -->
    <div v-if="allowMultiple" class="mf-toolbar tw:flex tw:items-center tw:gap-2 tw:mb-2 tw:flex-wrap">
      <button
        type="button"
        class="mf-btn mf-btn-primary"
        @click="$emit('showPicker')"
      >
        <i class="fa-solid fa-plus" aria-hidden="true"></i>
        {{ T.addMedia }}
      </button>

      <!-- View toggles -->
      <div class="mf-view-toggles tw:flex tw:items-center tw:gap-1 tw:ml-auto">
        <button
          type="button"
          class="mf-btn-icon"
          :class="{ 'mf-active': !gridView }"
          title="List view"
          @click="gridView = false"
        >
          <i class="fa-solid fa-th-list" aria-hidden="true"></i>
        </button>
        <button
          type="button"
          class="mf-btn-icon"
          :class="{ 'mf-active': gridView }"
          title="Grid view"
          @click="gridView = true"
        >
          <i class="fa-solid fa-th-large" aria-hidden="true"></i>
        </button>
        <button
          v-show="gridView"
          type="button"
          class="mf-btn-icon"
          :class="{ 'mf-active': size === 'sm' }"
          title="Small thumbnails"
          @click="size = 'sm'"
        >
          <i class="fa-solid fa-compress" aria-hidden="true"></i>
        </button>
        <button
          v-show="gridView"
          type="button"
          class="mf-btn-icon"
          :class="{ 'mf-active': size === 'lg' }"
          title="Large thumbnails"
          @click="size = 'lg'"
        >
          <i class="fa-solid fa-expand" aria-hidden="true"></i>
        </button>
      </div>
    </div>

    <!-- List view -->
    <ol v-if="!gridView" class="mf-gallery-list">
      <GalleryListItem
        v-for="media in uniqueItems"
        :key="media.vuekey ?? media.name"
        :media="media"
        :allow-multiple="allowMultiple"
        :allow-media-text="allowMediaText"
        :allow-anchors="allowAnchors"
        @edit-media-text="$emit('editMediaText', $event)"
        @edit-anchor="$emit('editAnchor', $event)"
        @delete-media="$emit('deleteMedia', $event)"
        @dragstart="onDragStart($event, media)"
        @drop="onDrop($event, media)"
        @dragend="onDragEnd"
      />
      <li
        v-if="uniqueItems.length === 0"
        class="mf-gallery-list-empty"
        @click="$emit('showPicker')"
      >
        <i class="fa-solid fa-plus tw:mr-1" aria-hidden="true"></i>
        {{ T.addMedia }}
      </li>
    </ol>

    <!-- Grid/card view -->
    <ol v-if="gridView" class="mf-gallery-cards" :class="'mf-size-' + size">
      <GalleryCardItem
        v-for="media in uniqueItems"
        :key="media.vuekey ?? media.name"
        :media="media"
        :allow-multiple="allowMultiple"
        :allow-media-text="allowMediaText"
        :allow-anchors="allowAnchors"
        :size="size"
        @edit-media-text="$emit('editMediaText', $event)"
        @edit-anchor="$emit('editAnchor', $event)"
        @delete-media="$emit('deleteMedia', $event)"
        @dragstart="onDragStart($event, media)"
        @drop="onDrop($event, media)"
        @dragend="onDragEnd"
      />
      <!-- Add card -->
      <li
        v-if="allowMultiple || uniqueItems.length === 0"
        class="mf-gallery-card-item mf-gallery-add-card"
        @click="$emit('showPicker')"
      >
        <div class="mf-gallery-card mf-gallery-card-add">
          <div class="mf-gallery-card-preview">
            <div class="mf-gallery-card-icon">
              <i class="fa-solid fa-plus fa-2x tw:opacity-50" aria-hidden="true"></i>
              <span class="tw:text-xs tw:mt-1 tw:opacity-60">{{ T.addMedia }}</span>
            </div>
          </div>
        </div>
      </li>
    </ol>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import GalleryListItem from "./GalleryListItem.vue";
import GalleryCardItem from "./GalleryCardItem.vue";
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import { useLocalizations } from "@bloom/helpers/localizations";

const STORAGE_KEY_PREFIX = "mediaFieldGallery_";

const props = defineProps<{
  mediaItems: IMediaFieldItem[];
  allowMultiple: boolean;
  allowMediaText: boolean;
  allowAnchors: boolean;
  idPrefix: string;
}>();

const emit = defineEmits<{
  reorder: [items: IMediaFieldItem[]];
  editMediaText: [media: IMediaFieldItem];
  editAnchor: [media: IMediaFieldItem];
  deleteMedia: [media: IMediaFieldItem];
  showPicker: [];
}>();

const { translations: T } = useLocalizations();

const gridView = ref(true);
const size = ref<"sm" | "lg">("lg");

// De-duplicate by mediaPath
const uniqueItems = computed(() => {
  const seen = new Set<string>();
  return props.mediaItems.filter((item) => {
    if (item.isRemoved) return false;
    if (seen.has(item.mediaPath)) return false;
    seen.add(item.mediaPath);
    return true;
  });
});

// --- LocalStorage prefs ---
const storageKey = computed(() => STORAGE_KEY_PREFIX + props.idPrefix);

onMounted(() => {
  try {
    const saved = localStorage.getItem(storageKey.value);
    if (saved) {
      const state = JSON.parse(saved);
      size.value = state.size || "lg";
      gridView.value = !props.allowMultiple ? true : (state.gridView ?? true);
    }
  } catch {
    // ignore
  }
});

watch([size, gridView], () => {
  localStorage.setItem(
    storageKey.value,
    JSON.stringify({ size: size.value, gridView: gridView.value })
  );
});

// --- Native drag-drop reordering ---
const draggedItem = ref<IMediaFieldItem | null>(null);

function onDragStart(e: DragEvent, media: IMediaFieldItem) {
  if (!props.allowMultiple) return;
  draggedItem.value = media;
  if (e.dataTransfer) {
    e.dataTransfer.effectAllowed = "move";
  }
}

function onDrop(_e: DragEvent, targetMedia: IMediaFieldItem) {
  if (!draggedItem.value || draggedItem.value === targetMedia) return;
  const items = [...props.mediaItems];
  const fromIdx = items.indexOf(draggedItem.value);
  const toIdx = items.indexOf(targetMedia);
  if (fromIdx < 0 || toIdx < 0) return;
  items.splice(fromIdx, 1);
  items.splice(toIdx, 0, draggedItem.value);
  emit("reorder", items);
}

function onDragEnd() {
  draggedItem.value = null;
}

// Enforce single item for non-multiple mode
watch(
  () => props.mediaItems,
  (items) => {
    if (!props.allowMultiple && items.length > 1) {
      emit("reorder", [items[items.length - 1]]);
    }
  },
  { deep: true }
);
</script>
