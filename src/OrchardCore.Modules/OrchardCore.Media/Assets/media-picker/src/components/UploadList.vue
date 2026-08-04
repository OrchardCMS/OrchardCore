<!--
  Upload toast for attached media field.
  Fixed position bottom-right, shows per-file progress and errors.
-->
<template>
  <div v-show="files.length > 0" class="mf-upload-toast tw:fixed tw:bottom-4 tw:right-4 tw:w-[360px] tw:max-w-[calc(100vw-2rem)] tw:z-[1060] tw:border tw:border-[var(--bs-border-color)] tw:rounded tw:bg-[var(--bs-body-bg)] tw:shadow-md tw:overflow-hidden dark:tw:border-[var(--bs-border-color)]">
    <div class="mf-upload-toast-header tw:flex tw:justify-between tw:items-center tw:px-3 tw:py-2 tw:bg-[#f8f9fa] tw:border-b tw:border-[var(--bs-border-color)] tw:cursor-pointer tw:text-sm tw:font-medium dark:tw:bg-[rgba(255,255,255,0.05)] dark:tw:border-[var(--bs-border-color)]" @click="expanded = !expanded">
      <div class="tw:flex tw:items-center">
        <span>{{ t.uploads }}</span>
        <span v-if="pendingCount > 0" class="tw:ms-1">({{ pendingCount }})</span>
        <span v-if="errorCount > 0" class="tw:text-red-500 tw:ms-1">
          {{ t.errors }}: {{ errorCount }}
        </span>
      </div>
      <div class="mf-upload-toast-actions tw:flex tw:items-center">
        <button
          v-if="errorCount > 0"
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-red-500 hover:tw:text-[#7bc143] tw:me-1"
          @click.stop="$emit('clearErrors')"
          :title="t.clearErrors"
        >
          <i class="fa-solid fa-broom" aria-hidden="true"></i>
        </button>
        <button
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6]"
          @click.stop="expanded = !expanded"
        >
          <i :class="expanded ? 'fa-solid fa-chevron-down' : 'fa-solid fa-chevron-up'" aria-hidden="true"></i>
        </button>
        <button
          v-if="pendingCount === 0"
          type="button"
          class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-[var(--bs-body-color)] hover:tw:text-[#7bc143] dark:tw:text-[#dee2e6] tw:ms-1"
          @click.stop="dismissAll"
        >
          <i class="fa-solid fa-xmark" aria-hidden="true"></i>
        </button>
      </div>
    </div>
    <div v-show="expanded" class="tw:overflow-y-auto tw:max-h-[calc(50vh-40px)]">
      <div
        v-for="f in files"
        :key="f.name"
        class="tw:px-3 tw:py-1.5 tw:text-[0.8125rem] tw:border-b tw:border-[#f0f0f0] last:tw:border-b-0 dark:tw:border-b-[rgba(255,255,255,0.06)]"
        :class="{
          'is-error': f.errorMessage,
          'tw:bg-[rgba(220,53,69,0.06)] dark:tw:bg-[rgba(220,53,69,0.12)]': f.errorMessage,
          'tw:bg-[rgba(25,135,84,0.06)] dark:tw:bg-[rgba(25,135,84,0.12)]': f.success,
        }"
      >
        <div class="tw:flex tw:justify-between tw:items-center">
          <span class="tw:overflow-hidden tw:text-ellipsis tw:whitespace-nowrap tw:flex-1 tw:min-w-0" :title="f.errorMessage || f.name">{{ f.name }}</span>
          <button
            v-if="f.errorMessage"
            type="button"
            class="tw:px-2 tw:py-1.5 tw:border-none tw:bg-transparent tw:cursor-pointer tw:text-red-500 hover:tw:text-[#7bc143]"
            @click="$emit('dismiss', f)"
          >
            <i class="fa-solid fa-xmark" aria-hidden="true"></i>
          </button>
          <i v-else-if="f.success" class="fa-solid fa-check tw:text-green-500" aria-hidden="true"></i>
        </div>
        <div v-if="f.errorMessage" class="mf-upload-toast-error tw:text-xs tw:mt-0.5 tw:text-red-500">
          {{ f.errorMessage }}
        </div>
        <div v-else-if="!f.success" class="tw:mt-1 tw:h-[3px] tw:bg-[#e9ecef] tw:rounded-sm tw:overflow-hidden dark:tw:bg-[#495057]">
          <div class="mf-upload-toast-progress-bar tw:h-full tw:bg-[#7bc143] tw:rounded-sm tw:transition-[width]" :style="{ width: f.percentage + '%' }"></div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import type { IUploadFileEntry } from "../services/FieldUploadService";
import { getTranslations } from "@bloom/helpers/localizations";

const props = defineProps<{
  files: IUploadFileEntry[];
}>();

const emit = defineEmits<{
  clearErrors: [];
  dismiss: [entry: IUploadFileEntry];
  dismissAll: [];
}>();

const t = getTranslations();
const expanded = ref(true);

const pendingCount = computed(
  () => props.files.filter((f) => !f.errorMessage && !f.success).length
);
const errorCount = computed(
  () => props.files.filter((f) => f.errorMessage !== "").length
);

function dismissAll() {
  emit("dismissAll");
}
</script>
