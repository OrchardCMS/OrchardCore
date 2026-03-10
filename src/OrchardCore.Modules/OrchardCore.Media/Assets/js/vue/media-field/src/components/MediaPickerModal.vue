<!--
  Media Picker Modal — opens a vue-final-modal with an embedded file browser
  for selecting media items from the library.
-->
<template>
  <VueFinalModal
    v-model="visible"
    class="tw-flex tw-items-center tw-justify-center"
    content-class="mf-modal-content mf-modal-picker"
    @closed="onClosed"
  >
    <div class="mf-modal-header tw-flex tw-items-center tw-justify-between">
      <h5>{{ T.selectMedia }}</h5>
      <button type="button" class="mf-btn-icon" @click="cancel">
        <i class="fa-solid fa-xmark" aria-hidden="true"></i>
      </button>
    </div>
    <div class="mf-modal-body mf-picker-body">
      <MediaBrowserPicker
        ref="browserRef"
        :allowed-extensions="allowedExtensions"
        :allow-multiple="allowMultiple"
        @selection-changed="onSelectionChanged"
      />
    </div>
    <div class="mf-modal-footer tw-flex tw-items-center tw-justify-between">
      <span class="tw-text-sm tw-text-gray-500">
        {{ selectedCount > 0 ? `${selectedCount} selected` : '' }}
      </span>
      <div class="tw-flex tw-gap-2">
        <button type="button" class="mf-btn" @click="cancel">{{ T.cancel }}</button>
        <button
          type="button"
          class="mf-btn mf-btn-primary"
          :disabled="selectedCount === 0"
          @click="confirm"
        >
          {{ T.ok }}
        </button>
      </div>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref } from "vue";
import { VueFinalModal } from "vue-final-modal";
import MediaBrowserPicker from "./MediaBrowserPicker.vue";
import type { IMediaFieldItem } from "../interfaces/MediaFieldTypes";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useLocalizations } from "@bloom/helpers/localizations";

const props = defineProps<{
  fieldId: string;
  allowedExtensions: string;
  allowMultiple?: boolean;
}>();

const emit = defineEmits<{
  select: [files: IMediaFieldItem[]];
}>();

const { translations: T } = useLocalizations();

const visible = ref(false);
const browserRef = ref<InstanceType<typeof MediaBrowserPicker> | null>(null);
const selectedCount = ref(0);
let currentSelection: IFileLibraryItemDto[] = [];

function open() {
  visible.value = true;
  currentSelection = [];
  selectedCount.value = 0;
}

function onSelectionChanged(files: IFileLibraryItemDto[]) {
  currentSelection = files;
  selectedCount.value = files.length;
}

function confirm() {
  if (currentSelection.length === 0) return;

  // Convert IFileLibraryItemDto to IMediaFieldItem
  const items: IMediaFieldItem[] = currentSelection.map((f, i) => ({
    name: f.name,
    mime: f.mime || "",
    mediaPath: f.filePath,
    url: f.url,
    size: f.size,
    vuekey: f.filePath + i,
  }));

  emit("select", items);
  visible.value = false;
}

function cancel() {
  visible.value = false;
}

function onClosed() {
  browserRef.value?.clearSelection();
  currentSelection = [];
  selectedCount.value = 0;
}

defineExpose({ open });
</script>
