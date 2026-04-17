<!--
  Card/grid view item for a gallery media item.
-->
<template>
  <li
    v-if="!media.isRemoved"
    :class="['mf-gallery-card-item', size === 'sm' ? 'tw:w-[120px]' : 'tw:w-[200px]', allowMultiple ? 'tw:cursor-grab active:tw:cursor-grabbing' : '']"
    :draggable="allowMultiple"
    @dragstart="$emit('dragstart', $event)"
    @dragover.prevent
    @drop="$emit('drop', $event)"
    @dragend="$emit('dragend')"
  >
    <div
      class="mf-gallery-card tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:overflow-hidden tw:flex tw:flex-col tw:h-full tw:bg-[var(--bs-body-bg)]"
      :class="{
        'mf-item-warning': media.errorType === 'transient',
        'mf-item-danger': media.errorType === 'not-found',
        'tw:border-[#ffc107]': media.errorType === 'transient',
        'tw:border-[#dc3545]': media.errorType === 'not-found',
      }"
    >
      <!-- Preview area -->
      <div class="tw:aspect-square tw:flex tw:items-center tw:justify-center tw:overflow-hidden tw:bg-[var(--bs-tertiary-bg)]">
        <div
          v-if="media.mime?.startsWith('image') && !media.errorType"
          class="tw:w-full tw:h-full"
        >
          <img
            :src="buildMediaUrl(media.url!, thumbSize)"
            :data-mime="media.mime"
            class="tw:w-full tw:h-full tw:object-cover"
            draggable="false"
          />
        </div>
        <div
          v-else-if="media.errorType === 'transient'"
          class="tw:flex tw:flex-col tw:items-center tw:justify-center tw:w-full tw:h-full tw:p-2 tw:text-yellow-500"
        >
          <i class="fa-solid fa-triangle-exclamation fa-2x"></i>
          <span class="tw:text-xs tw:mt-1">{{ t.mediaTemporarilyUnavailable }}</span>
        </div>
        <div
          v-else-if="media.errorType === 'not-found'"
          class="tw:flex tw:flex-col tw:items-center tw:justify-center tw:w-full tw:h-full tw:p-2 tw:text-red-500"
        >
          <i class="fa-solid fa-triangle-exclamation fa-2x"></i>
          <span class="tw:text-xs tw:mt-1">{{ t.mediaNotFound }}</span>
        </div>
        <div v-else class="mf-gallery-card-icon tw:flex tw:flex-col tw:items-center tw:justify-center tw:w-full tw:h-full tw:p-2">
          <i :class="getIconClassForFilename(media.name, 'fa-3x')" :data-mime="media.mime"></i>
          <span class="tw:text-xs tw:mt-1 tw:truncate tw:max-w-full tw:px-1" :title="media.name">
            {{ media.name }}
          </span>
        </div>
      </div>

      <!-- Actions bar -->
      <div class="tw:flex tw:items-center tw:justify-center tw:gap-1 tw:p-1 tw:border-t tw:border-[var(--bs-border-color)] tw:flex-shrink-0">
        <button
          v-if="allowMediaText && !media.errorType"
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143]"
          title="Edit media text"
          @click.stop="$emit('editMediaText', media)"
        >
          <i :class="media.mediaText ? 'fa-solid fa-comment' : 'fa-regular fa-comment'" aria-hidden="true"></i>
        </button>
        <button
          v-if="allowAnchors && media.mime?.startsWith('image') && !media.errorType"
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143]"
          title="Set anchor"
          @click.stop="$emit('editAnchor', media)"
        >
          <i class="fa-solid fa-crosshairs" aria-hidden="true"></i>
        </button>
        <a
          v-if="!media.errorType"
          :href="media.url"
          target="_blank"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143]"
          title="Download"
          @click.stop
        >
          <i class="fa-solid fa-download" aria-hidden="true"></i>
        </a>
        <button
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143]"
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
import { getTranslations } from "@bloom/helpers/localizations";
import { buildMediaUrl } from "@bloom/media/utils";

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

const t = getTranslations();

const thumbSize = props.size === "sm" ? 120 : 240;

</script>
