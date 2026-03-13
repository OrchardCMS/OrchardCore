<!--
  Card/grid view item for a gallery media item.
-->
<template>
  <li
    v-if="!media.isRemoved"
    class="mf-gallery-card-item"
    :draggable="allowMultiple"
    @dragstart="$emit('dragstart', $event)"
    @dragover.prevent
    @drop="$emit('drop', $event)"
    @dragend="$emit('dragend')"
  >
    <div
      class="mf-gallery-card"
      :class="{
        'mf-item-warning': media.errorType === 'transient',
        'mf-item-danger': media.errorType === 'not-found',
      }"
    >
      <!-- Preview area -->
      <div class="mf-gallery-card-preview">
        <div
          v-if="media.mime?.startsWith('image') && !media.errorType"
          class="mf-gallery-card-image"
        >
          <img
            :src="buildMediaUrl(media.url!, thumbSize)"
            :data-mime="media.mime"
            class="tw:w-full tw:h-full tw:object-cover"
          />
        </div>
        <div
          v-else-if="media.errorType === 'transient'"
          class="mf-gallery-card-icon tw:text-yellow-500"
        >
          <i class="fa-solid fa-triangle-exclamation fa-2x"></i>
          <span class="tw:text-xs tw:mt-1">{{ T.mediaTemporarilyUnavailable }}</span>
        </div>
        <div
          v-else-if="media.errorType === 'not-found'"
          class="mf-gallery-card-icon tw:text-red-500"
        >
          <i class="fa-solid fa-triangle-exclamation fa-2x"></i>
          <span class="tw:text-xs tw:mt-1">{{ T.mediaNotFound }}</span>
        </div>
        <div v-else class="mf-gallery-card-icon">
          <i :class="getIconClassForFilename(media.name, 'fa-3x')" :data-mime="media.mime"></i>
          <span class="tw:text-xs tw:mt-1 tw:truncate tw:max-w-full tw:px-1" :title="media.name">
            {{ media.name }}
          </span>
        </div>
      </div>

      <!-- Actions bar -->
      <div class="mf-gallery-card-actions">
        <button
          v-if="allowMediaText && !media.errorType"
          type="button"
          class="mf-btn-icon"
          title="Edit media text"
          @click.stop="$emit('editMediaText', media)"
        >
          <i :class="media.mediaText ? 'fa-solid fa-comment' : 'fa-regular fa-comment'" aria-hidden="true"></i>
        </button>
        <button
          v-if="allowAnchors && media.mime?.startsWith('image') && !media.errorType"
          type="button"
          class="mf-btn-icon"
          title="Set anchor"
          @click.stop="$emit('editAnchor', media)"
        >
          <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
        </button>
        <a
          v-if="!media.errorType"
          :href="media.url"
          target="_blank"
          class="mf-btn-icon"
          title="Download"
          @click.stop
        >
          <i class="fa-solid fa-download" aria-hidden="true"></i>
        </a>
        <button
          type="button"
          class="mf-btn-icon"
          title="Remove"
          @click.stop="$emit('deleteMedia', media)"
        >
          <i class="fa-solid fa-trash" aria-hidden="true"></i>
        </button>
      </div>
    </div>
  </li>
</template>

<script setup lang="ts">
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import { getIconClassForFilename } from "../services/FontAwesomeThumbnails";
import { useLocalizations } from "@bloom/helpers/localizations";

const props = defineProps<{
  media: IMediaFieldItem;
  allowMultiple: boolean;
  allowMediaText: boolean;
  allowAnchors: boolean;
  size: "sm" | "lg";
}>();

defineEmits<{
  editMediaText: [media: IMediaFieldItem];
  editAnchor: [media: IMediaFieldItem];
  deleteMedia: [media: IMediaFieldItem];
  dragstart: [e: DragEvent];
  drop: [e: DragEvent];
  dragend: [];
}>();

const { translations: T } = useLocalizations();

const thumbSize = props.size === "sm" ? 120 : 240;

function buildMediaUrl(url: string, size: number): string {
  const sep = url.indexOf("?") === -1 ? "?" : "&";
  return `${url}${sep}width=${size}&height=${size}`;
}
</script>
