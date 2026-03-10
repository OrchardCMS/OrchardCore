<!--
  List view row for a gallery media item.
-->
<template>
  <li
    v-if="!media.isRemoved"
    class="mf-gallery-list-item"
    :class="{
      'mf-item-warning': media.errorType === 'transient',
      'mf-item-danger': media.errorType === 'not-found',
    }"
    :draggable="allowMultiple"
    @dragstart="$emit('dragstart', $event)"
    @dragover.prevent
    @drop="$emit('drop', $event)"
    @dragend="$emit('dragend')"
  >
    <!-- Preview thumbnail -->
    <div class="mf-gallery-list-preview">
      <img
        v-if="media.mime?.startsWith('image') && !media.errorType"
        :src="buildMediaUrl(media.url!, 32)"
        :data-mime="media.mime"
        class="tw-w-full tw-h-full tw-object-cover"
      />
      <i
        v-else-if="media.errorType === 'transient'"
        class="fa-solid fa-triangle-exclamation tw-text-yellow-500"
        :title="media.name"
      ></i>
      <i
        v-else-if="media.errorType === 'not-found'"
        class="fa-solid fa-triangle-exclamation tw-text-red-500"
        :title="media.name"
      ></i>
      <i
        v-else
        :class="getIconClassForFilename(media.name, 'fa-2x')"
        :data-mime="media.mime"
      ></i>
    </div>

    <!-- Name -->
    <div class="mf-gallery-list-name tw-flex-1 tw-min-w-0">
      <span v-if="media.errorType === 'transient'" class="tw-text-yellow-500 tw-text-sm">
        {{ T.mediaTemporarilyUnavailable }}
      </span>
      <span v-else-if="media.errorType === 'not-found'" class="tw-text-red-500 tw-text-sm">
        {{ T.mediaNotFound }}
      </span>
      <span v-else class="tw-text-sm tw-truncate tw-block" :title="media.name">
        {{ media.name }}
      </span>
    </div>

    <!-- Actions -->
    <div class="mf-gallery-list-actions tw-flex tw-items-center tw-gap-1 tw-flex-shrink-0">
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
  </li>
</template>

<script setup lang="ts">
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import { getIconClassForFilename } from "../services/FontAwesomeThumbnails";
import { useLocalizations } from "@bloom/helpers/localizations";

defineProps<{
  media: IMediaFieldItem;
  allowMultiple: boolean;
  allowMediaText: boolean;
  allowAnchors: boolean;
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

function buildMediaUrl(url: string, size: number): string {
  const sep = url.indexOf("?") === -1 ? "?" : "&";
  return `${url}${sep}width=${size}&height=${size}`;
}
</script>
