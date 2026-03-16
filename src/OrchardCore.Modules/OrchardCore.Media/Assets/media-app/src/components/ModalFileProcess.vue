<template>
  <VueFinalModal :focus-trap="false" :teleport-to="teleportTo" v-model="showModal" :modal-id="modalName"
    :esc-to-close="false" :click-to-close="false" class="ma-vfm tw:flex tw:justify-center tw:items-center"
    content-class="tw:flex tw:flex-col tw:max-w-xl tw:mx-4 tw:p-4 tw:rounded-lg tw:space-y-4 action-modal">
    <div class="tw:flex tw:justify-between">
      <span class="modal__title">
        {{ title }}
      </span>
      <span class="tw:cursor-pointer" @click="emit('closed')">
        <fa-icon icon="fa-solid fa-xmark fa-2xl"></fa-icon>
      </span>
    </div>
    <div class="file-list">
      <template v-for="(fileEntry, index) in inputValues" v-bind:key="index">
        <div class="tw:m-3 tw:ml-0 tw:w-full tw:flex tw:flex-col">
          <div class="file-wrapper tw:mb-5">
            <div class="img-wrapper">
              <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                <path
                  d="M10.28 4.46553H13.4658C13.4889 4.4658 13.5118 4.46145 13.5332 4.45274C13.5545 4.44402 13.574 4.43113 13.5903 4.4148C13.6066 4.39848 13.6195 4.37906 13.6282 4.35768C13.6369 4.3363 13.6413 4.3134 13.641 4.29031C13.6417 4.13738 13.6084 3.9862 13.5437 3.84764C13.479 3.70907 13.3844 3.58656 13.2667 3.48889L10.5946 1.26233C10.3921 1.09433 10.137 1.00273 9.87384 1.00348C9.83983 1.00342 9.80614 1.01007 9.7747 1.02305C9.74327 1.03604 9.71471 1.0551 9.69066 1.07915C9.66661 1.1032 9.64755 1.13176 9.63456 1.1632C9.62158 1.19463 9.61493 1.22832 9.61499 1.26233V3.801C9.61499 3.88831 9.6322 3.97476 9.66562 4.05542C9.69905 4.13607 9.74804 4.20935 9.8098 4.27107C9.87156 4.33278 9.94488 4.38172 10.0256 4.41509C10.1062 4.44845 10.1927 4.46559 10.28 4.46553Z"
                  fill="#2D2D2D" />
                <path
                  d="M8.70453 3.8V1H4.12C3.82324 1.00092 3.5389 1.11921 3.32906 1.32906C3.11921 1.5389 3.00092 1.82324 3 2.12V13.88C3.00092 14.1768 3.11921 14.4611 3.32906 14.6709C3.5389 14.8808 3.82324 14.9991 4.12 15H12.52C12.8168 14.9991 13.1011 14.8808 13.3109 14.6709C13.5208 14.4611 13.6391 14.1768 13.64 13.88V5.37497H10.28C9.86241 5.37444 9.46206 5.20836 9.16673 4.91312C8.87141 4.61788 8.70519 4.21759 8.70453 3.8Z"
                  fill="#2D2D2D" />
              </svg>
              <span class="tw:uppercase file-ext tw:text-white">{{ getFileExtension(fileEntry.file.name) }}</span>
            </div>
            <div class="tw:font-bold tw:text-lg tw:ml-2">{{ fileEntry.file.name }}</div>
          </div>
        </div>
      </template>
    </div>
    <div class="tw:mt-3 tw:flex tw:flex-row tw:justify-end tw:items-center">
      <div class="tw:flex tw:items-center">
        <button class="ma-btn ma-btn-light tw:border tw:border-gray-400 cancel" @click="emit('closed')">
          {{ t.Cancel }}
        </button>
        <button id="btn-submit" class="tw:ml-2 ma-btn ma-btn-primary submit"
          @click="emit('confirm', { actionEntries: inputValues, isEdit: isEdit })">
          <slot name="submit"></slot>
        </button>
      </div>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { IFileLibraryItemDto } from '@bloom/media/interfaces';
import { getFileExtension } from '@bloom/media/utils'
import { getTranslations } from '@bloom/helpers/localizations';

const t = getTranslations();

interface IFileProcessEntry {
  file: IFileLibraryItemDto;
}

interface IConfirmFileProcessViewModel {
  actionEntries: IFileProcessEntry[];
  isEdit: boolean;
}

const props = defineProps({
  title: String,
  modalName: {
    type: String,
    required: true
  },
  files: {
    type: Object as PropType<IFileLibraryItemDto[]>,
    required: true
  },
  isEdit: {
    type: Boolean,
    required: true
  },
  showModalProp: {
    type: Boolean,
    default: false
  },
  teleportTo: {
    type: String,
    default: 'body'
  }
})

const emit = defineEmits<{
  (e: 'confirm', viewModel: IConfirmFileProcessViewModel): void
  (e: 'closed', viewModel?: IConfirmFileProcessViewModel): void
}>()

let showModal = ref(props.showModalProp);

let fileEntries: IFileProcessEntry[] = props.files?.map((file: IFileLibraryItemDto) => {
  return { file: file }
}) ?? [];

let inputValues = ref(fileEntries);
</script>
