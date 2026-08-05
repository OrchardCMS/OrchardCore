<!--
  Gallery container with list/grid view toggle, size toggle, and drag-drop reordering.
-->
<template>
  <div>
    <!-- Toolbar (only for multiple mode) -->
    <div v-if="allowMultiple" class="tw:flex tw:items-center tw:gap-2 tw:mb-2 tw:flex-wrap">
      <button
        type="button"
        class="mf-btn-primary tw:inline-flex tw:items-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:leading-normal tw:border tw:border-[#7bc143] tw:rounded tw:bg-[#7bc143] tw:text-white tw:cursor-pointer hover:tw:bg-[#6aab36] hover:tw:border-[#6aab36]"
        @click="$emit('showPicker')"
      >
        <i class="fa-solid fa-plus" aria-hidden="true"></i>
        {{ t.addMedia }}
      </button>

      <!-- View toggles -->
      <div class="tw:flex tw:items-center tw:gap-1 tw:ml-auto">
        <button
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6]"
          :class="{ 'tw:text-[#7bc143]!': !gridView }"
          title="List view"
          @click="gridView = false"
        >
          <i class="fa-solid fa-th-list" aria-hidden="true"></i>
        </button>
        <button
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6]"
          :class="{ 'tw:text-[#7bc143]!': gridView }"
          title="Grid view"
          @click="gridView = true"
        >
          <i class="fa-solid fa-th-large" aria-hidden="true"></i>
        </button>
        <button
          v-show="gridView"
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6]"
          :class="{ 'tw:text-[#7bc143]!': size === 'sm' }"
          title="Small thumbnails"
          @click="size = 'sm'"
        >
          <i class="fa-solid fa-compress" aria-hidden="true"></i>
        </button>
        <button
          v-show="gridView"
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6]"
          :class="{ 'tw:text-[#7bc143]!': size === 'lg' }"
          title="Large thumbnails"
          @click="size = 'lg'"
        >
          <i class="fa-solid fa-expand" aria-hidden="true"></i>
        </button>
      </div>
    </div>

    <!-- List view -->
    <ol v-if="!gridView" class="mf-gallery-list tw:list-none tw:p-0 tw:m-0 tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:overflow-hidden dark:tw:border-[#495057]">
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
        class="mf-gallery-list-empty tw:p-4 tw:text-center tw:cursor-pointer tw:text-[var(--bs-secondary-color)] hover:tw:bg-[var(--bs-tertiary-bg)] hover:tw:text-[#7bc143]"
        @click="$emit('showPicker')"
      >
        <i class="fa-solid fa-plus tw:mr-1" aria-hidden="true"></i>
        {{ t.addMedia }}
      </li>
    </ol>

    <!-- Grid/card view -->
    <ol v-if="gridView" :class="['mf-gallery-cards', size === 'sm' ? 'mf-size-sm' : 'mf-size-lg', 'tw:list-none tw:p-0 tw:m-0 tw:flex tw:flex-wrap tw:gap-2']">
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
        :class="['mf-gallery-add-card', size === 'sm' ? 'tw:w-[120px]' : 'tw:w-[200px]']"
        @click="$emit('showPicker')"
      >
        <div class="tw:border tw:border-dashed tw:border-[var(--bs-border-color)] tw:rounded tw:overflow-hidden tw:flex tw:flex-col tw:h-full tw:cursor-pointer hover:tw:border-[#7bc143] hover:tw:bg-[rgba(123,193,67,0.05)]">
          <div class="tw:aspect-square tw:flex tw:items-center tw:justify-center tw:overflow-hidden tw:bg-[var(--bs-tertiary-bg)]">
            <div class="tw:flex tw:flex-col tw:items-center tw:justify-center tw:w-full tw:h-full tw:p-2 tw:text-[var(--bs-secondary-color)]">
              <i class="fa-solid fa-plus fa-2x" aria-hidden="true"></i>
              <span class="tw:text-xs tw:mt-1">{{ t.addMedia }}</span>
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
import { getTranslations } from "@bloom/helpers/localizations";

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

const t = getTranslations();

const gridView = ref(true);
const size = ref<"sm" | "lg">("lg");

// De-duplicate by mediaPath
const uniqueItems = computed(() => props.mediaItems.filter((item) => !item.isRemoved));

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
  if (!draggedItem.value || draggedItem.value.mediaPath === targetMedia.mediaPath) return;
  const items = [...props.mediaItems];
  const fromIdx = items.findIndex((i) => i.mediaPath === draggedItem.value!.mediaPath);
  const toIdx = items.findIndex((i) => i.mediaPath === targetMedia.mediaPath);
  if (fromIdx < 0 || toIdx < 0) return;
  const [moved] = items.splice(fromIdx, 1);
  items.splice(toIdx, 0, moved);
  emit("reorder", items);
}

function onDragEnd() {
  draggedItem.value = null;
}

defineExpose({ gridView, size, onDragStart, onDrop, onDragEnd });

// Enforce single item for non-multiple mode
watch(
  () => props.mediaItems,
  (items) => {
    if (!props.allowMultiple && items.length > 1) {
      emit("reorder", [items[items.length - 1]]);
    }
  },
  { deep: true, immediate: true }
);
</script>
