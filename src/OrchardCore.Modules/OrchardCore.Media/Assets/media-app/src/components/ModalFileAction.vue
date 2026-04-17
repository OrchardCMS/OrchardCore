<template>
  <VueFinalModal :focus-trap="false" v-model="showModal" :modal-id="modalName" :esc-to-close="false" :click-to-close="false"
    class="ma-vfm tw:flex tw:justify-center tw:items-center"
    content-class="tw:flex tw:flex-col tw:max-w-xl tw:mx-4 tw:p-4 tw:rounded-lg tw:space-y-4 action-modal">
    <div class="tw:flex tw:justify-between">
      <span class="modal__title">
        {{ title }}
      </span>
      <span class="tw:cursor-pointer" @click="emit('closed')">
        <fa-icon icon="fa-solid fa-xmark fa-2xl"></fa-icon>
      </span>
    </div>
    <slot></slot>
    <div class="file-wrapper">
      <div class="img-wrapper">
        <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path
            d="M10.28 4.46553H13.4658C13.4889 4.4658 13.5118 4.46145 13.5332 4.45274C13.5545 4.44402 13.574 4.43113 13.5903 4.4148C13.6066 4.39848 13.6195 4.37906 13.6282 4.35768C13.6369 4.3363 13.6413 4.3134 13.641 4.29031C13.6417 4.13738 13.6084 3.9862 13.5437 3.84764C13.479 3.70907 13.3844 3.58656 13.2667 3.48889L10.5946 1.26233C10.3921 1.09433 10.137 1.00273 9.87384 1.00348C9.83983 1.00342 9.80614 1.01007 9.7747 1.02305C9.74327 1.03604 9.71471 1.0551 9.69066 1.07915C9.66661 1.1032 9.64755 1.13176 9.63456 1.1632C9.62158 1.19463 9.61493 1.22832 9.61499 1.26233V3.801C9.61499 3.88831 9.6322 3.97476 9.66562 4.05542C9.69905 4.13607 9.74804 4.20935 9.8098 4.27107C9.87156 4.33278 9.94488 4.38172 10.0256 4.41509C10.1062 4.44845 10.1927 4.46559 10.28 4.46553Z"
            fill="#2D2D2D" />
          <path
            d="M8.70453 3.8V1H4.12C3.82324 1.00092 3.5389 1.11921 3.32906 1.32906C3.11921 1.5389 3.00092 1.82324 3 2.12V13.88C3.00092 14.1768 3.11921 14.4611 3.32906 14.6709C3.5389 14.8808 3.82324 14.9991 4.12 15H12.52C12.8168 14.9991 13.1011 14.8808 13.3109 14.6709C13.5208 14.4611 13.6391 14.1768 13.64 13.88V5.37497H10.28C9.86241 5.37444 9.46206 5.20836 9.16673 4.91312C8.87141 4.61788 8.70519 4.21759 8.70453 3.8Z"
            fill="#2D2D2D" />
        </svg>
        <span class="tw:uppercase file-ext tw:text-white">{{ getFileExtension(fileItem.filePath) }}</span>
      </div>
      <div class="tw:font-bold tw:text-lg tw:ml-2 tw:min-w-0 tw:break-all">{{ fileItem.name }}</div>
    </div>
    <div class="tw:w-full">
      <template v-if="action == FileAction.Rename">
        <label class="tw:block tw:font-bold tw:text-lg tw:mb-2">{{ t.Filename }}</label>
        <input @keyup.enter="onPressEnter" class="p-inputtext tw:w-full" type="text" name="rename"
          v-model="fileActionEntry.inputValue" />
      </template>
      <template v-if="action == FileAction.Copy">
        <p-treeselect v-model="fileActionEntry.inputValue" :options="treeNode" :placeholder="t.SelectFolder"
          class="tw:w-full" />
      </template>
      <template v-if="action == FileAction.Move">
        <p-treeselect v-model="fileActionEntry.inputValue" :options="treeNode" :placeholder="t.SelectFolder"
          class="tw:w-full" />
      </template>
      <div class="tw:text-red-500 tw:mt-2">{{ errorMessage }}</div>
    </div>
    <div class="tw:mt-3 tw:flex tw:flex-row tw:justify-end tw:gap-2">
      <button class="tw:inline-flex tw:items-center tw:justify-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:font-normal tw:leading-normal tw:cursor-pointer tw:select-none tw:border tw:rounded-md tw:transition-colors tw:text-[var(--bs-body-color)] tw:bg-[var(--bs-secondary-bg)] tw:border-[var(--bs-border-color)] hover:tw:bg-[var(--bs-tertiary-bg)]" @click="emit('closed')">
        {{ t.Cancel }}
      </button>
      <button id="btn-submit" class="tw:inline-flex tw:items-center tw:justify-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:font-normal tw:leading-normal tw:cursor-pointer tw:select-none tw:border tw:rounded-md tw:transition-colors tw:text-white tw:bg-[#0d6efd] tw:border-[#0d6efd] hover:tw:bg-[#0b5ed7] hover:tw:border-[#0a58ca]"
        @click="validate({ action: action, file: fileItem, inputValue: fileActionEntry.inputValue })">
        <slot name="submit"></slot>
      </button>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { FileAction, IConfirmFileActionViewModel, IConfirmFileEntry, IRenameFileLibraryItemDto, TreeNode } from '@bloom/media/interfaces';
import { getFileExtension } from '@bloom/media/utils'
//import dbg from 'debug';
import { useHierarchicalTreeBuilder } from '../services/HierarchicalTreeBuilder';
import { getTranslations } from '@bloom/helpers/localizations';
import { isValidFileExtension } from '@bloom/media/file-extensions';

//const debug = dbg("orchardcore:file-app");
const t = getTranslations();
const { getDirectoryTreeNode } = useHierarchicalTreeBuilder();

const props = defineProps({
  title: String,
  modalName: {
    type: String,
    required: true
  },
  action: {
    type: Number as PropType<FileAction>,
    required: true
  },
  fileItem: {
    type: Object as PropType<IRenameFileLibraryItemDto>,
    required: true
  }
})

const fileActionEntry = ref<IConfirmFileEntry>({ file: props.fileItem, inputValue: props.fileItem.name });

// If we copy or move a file the inital folder selected should be empty
if (props.action == FileAction.Move || props.action == FileAction.Copy) {
  fileActionEntry.value.inputValue = "";
}

let showModal = ref(false);

let treeNode = ref<TreeNode[]>(getDirectoryTreeNode());
let errorMessage = ref<string>("");

const onPressEnter = () => {
  document.getElementById('btn-submit')?.click();
};

/**
 * Validate user input and emit confirm event if input is valid
 * @param elem - Data containing file item and user input
 */
const validate = (elem: IConfirmFileActionViewModel) => {
  if (elem.action == FileAction.Copy || elem.action == FileAction.Move) {
    if (elem.inputValue == null || elem.inputValue === "" || (typeof elem.inputValue === "object" && Object.keys(elem.inputValue).length === 0)) {
      errorMessage.value = t.ValidationFolderRequired;
    } else {
      emit('confirm', elem);
    }
  } else if (elem.action == FileAction.Rename) {
    if (elem.inputValue == null || elem.inputValue === "") {
      errorMessage.value = t.ValidationFilenameRequired;
    } else if (!isValidFileExtension(elem.file.directoryPath, elem.inputValue as string)) {
      errorMessage.value = t.ValidationFileExtensionRequired;
    } else {
      emit('confirm', elem);
    }
  }
}

const emit = defineEmits<{
  (e: 'confirm', viewModel: IConfirmFileActionViewModel): void
  (e: 'closed', viewModel?: IConfirmFileActionViewModel): void
}>()
</script>
