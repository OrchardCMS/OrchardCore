<template>
  <div class="mf-thumbs-container">
    <!-- Empty state -->
    <div v-if="visibleItems.length === 0" class="mf-empty-card">
      <span class="mf-hint">{{ t.noImages }}</span>
    </div>

    <!-- Thumbnail grid -->
    <ol
      ref="gridRef"
      class="mf-thumbs-grid tw:flex tw:flex-row tw:items-start tw:flex-wrap tw:gap-2 tw:p-0 tw:m-0"
    >
      <li
        v-for="media in visibleItems"
        :key="media.vuekey"
        :class="[
          'mf-thumb-item',
          { 'mf-thumb-item-active': selectedMedia === media },
        ]"
        :style="{ width: thumbSize + 2 + 'px' }"
        :draggable="allowMultiple"
        @click="emit('selectMedia', media)"
        @dragstart="onDragStart($event, media)"
        @dragover.prevent
        @drop="onDrop($event, media)"
        @dragend="onDragEnd"
      >
        <!-- Transient error -->
        <div v-if="media.errorType === 'transient'">
          <div class="mf-thumb-preview tw:flex tw:flex-col tw:items-center tw:justify-center" :style="{ height: thumbSize + 'px' }">
            <i class="fa-solid fa-triangle-exclamation tw:text-yellow-500 tw:text-2xl tw:block" aria-hidden="true"></i>
            <span class="tw:text-yellow-500 tw:text-xs tw:block tw:mt-1">{{ t.mediaTemporarilyUnavailable }}</span>
          </div>
          <div class="mf-thumb-footer">
            <button
              type="button"
              class="mf-btn-icon mf-btn-delete"
              @click.stop="emit('deleteMedia', media)"
            >
              <i class="fa-solid fa-trash" aria-hidden="true"></i>
            </button>
            <span class="mf-filename tw:text-yellow-500" :title="media.name">{{ media.name }}</span>
          </div>
        </div>

        <!-- Normal item -->
        <div v-else-if="!media.errorType">
          <div class="mf-thumb-preview tw:flex tw:items-center tw:justify-center" :style="{ height: thumbSize + 'px' }">
            <img
              v-if="media.mime && media.mime.startsWith('image')"
              :src="buildMediaUrl(media.url!, thumbSize)"
              :data-mime="media.mime"
              class="tw:max-w-full tw:max-h-full tw:object-contain"
              draggable="false"
            />
            <i
              v-else
              :class="getIconClassForFilename(media.name, 'fa-4x')"
              :data-mime="media.mime"
            ></i>
          </div>
          <div class="mf-thumb-footer">
            <button
              type="button"
              class="mf-btn-icon mf-btn-delete"
              @click.stop="emit('deleteMedia', media)"
            >
              <i class="fa-solid fa-trash" aria-hidden="true"></i>
            </button>
            <a
              :href="media.url"
              target="_blank"
              class="mf-btn-icon mf-btn-download"
              @click.stop
            >
              <i class="fa-solid fa-download" aria-hidden="true"></i>
            </a>
            <span class="mf-filename" :title="media.mediaPath">{{
              media.isNew ? media.name.substring(36) : media.name
            }}</span>
          </div>
        </div>

        <!-- Not found error -->
        <div v-else>
          <div class="mf-thumb-preview tw:flex tw:flex-col tw:items-center tw:justify-center" :style="{ height: thumbSize + 'px' }">
            <i class="fa-solid fa-ban tw:text-red-500 tw:text-2xl tw:block" aria-hidden="true"></i>
            <span class="tw:text-red-500 tw:text-xs tw:block tw:mt-1">{{ t.mediaNotFound }}</span>
          </div>
          <div class="mf-thumb-footer">
            <button
              type="button"
              class="mf-btn-icon mf-btn-delete"
              @click.stop="emit('deleteMedia', media)"
            >
              <i class="fa-solid fa-trash" aria-hidden="true"></i>
            </button>
            <span class="mf-filename tw:text-red-500" :title="media.name">{{ media.name }}</span>
          </div>
        </div>
      </li>
    </ol>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, toRaw } from "vue";
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import { getIconClassForFilename } from "../services/FontAwesomeThumbnails";
import { getTranslations } from "@bloom/helpers/localizations";
import { buildMediaUrl } from "@bloom/media/utils";

const props = defineProps<{
  mediaItems: IMediaFieldItem[];
  selectedMedia: IMediaFieldItem | null;
  thumbSize: number;
  allowMultiple: boolean;
}>();

const emit = defineEmits<{
  selectMedia: [media: IMediaFieldItem];
  deleteMedia: [media: IMediaFieldItem];
  reorder: [items: IMediaFieldItem[]];
}>();

const t = getTranslations();

const visibleItems = computed(() =>
  props.mediaItems.filter((m) => !m.isRemoved)
);

// --- Native drag-and-drop reordering ---
const draggedItem = ref<IMediaFieldItem | null>(null);

function onDragStart(e: DragEvent, media: IMediaFieldItem) {
  if (!props.allowMultiple) return;
  draggedItem.value = media;
  if (e.dataTransfer) {
    e.dataTransfer.effectAllowed = "move";
  }
}

function onDrop(_e: DragEvent, targetMedia: IMediaFieldItem) {
  if (!draggedItem.value || toRaw(draggedItem.value) === toRaw(targetMedia)) return;
  const items = [...props.mediaItems];
  const rawDragged = toRaw(draggedItem.value);
  const rawTarget = toRaw(targetMedia);
  const fromIdx = items.findIndex((i) => toRaw(i) === rawDragged);
  const toIdx = items.findIndex((i) => toRaw(i) === rawTarget);
  if (fromIdx < 0 || toIdx < 0) return;
  items.splice(fromIdx, 1);
  items.splice(toIdx, 0, draggedItem.value);
  emit("reorder", items);
}

function onDragEnd() {
  draggedItem.value = null;
}

</script>
