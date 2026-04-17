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
    <ul class="tw:list-none tw:m-0 tw:p-0">
      <p-treeselect @update:modelValue="onFileChange" v-model="fileActionEntry.inputValue" :options="treeNode" :placeholder="t.SelectFile"
        class="tw:md:w-80 tw:w-full" />
      <div class="tw:text-red-500 tw:mt-2">{{ errorMessage }}</div>
    </ul>
    <div class="tw:mt-3 tw:flex tw:flex-row tw:justify-end tw:gap-2">
      <button class="tw:inline-flex tw:items-center tw:justify-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:font-normal tw:leading-normal tw:cursor-pointer tw:select-none tw:border tw:rounded-md tw:transition-colors tw:text-[var(--bs-body-color)] tw:bg-[var(--bs-secondary-bg)] tw:border-[var(--bs-border-color)] hover:tw:bg-[var(--bs-tertiary-bg)]" @click="emit('closed')">
        {{ t.Cancel }}
      </button>
      <button id="btn-submit" class="tw:inline-flex tw:items-center tw:justify-center tw:gap-1.5 tw:px-3 tw:py-1.5 tw:text-sm tw:font-normal tw:leading-normal tw:cursor-pointer tw:select-none tw:border tw:rounded-md tw:transition-colors tw:text-white tw:bg-[#0d6efd] tw:border-[#0d6efd] hover:tw:bg-[#0b5ed7] hover:tw:border-[#0a58ca]"
        @click="validate({ inputValue: fileActionEntry.inputValue })">
        <slot name="submit"></slot>
      </button>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { type IConfirmFilePickerViewModel, TreeNode } from '@bloom/media/interfaces';
import dbg from 'debug';
import { getFileExtension } from '@bloom/media/utils';
import { getTranslations } from '@bloom/helpers/localizations';

const t = getTranslations();

const debug = dbg("orchardcore:file-app");

const props = defineProps({
  title: String,
  modalName: {
    type: String,
    required: true
  },
  files: {
    type: {} as PropType<TreeNode[]>,
    required: true
  },
  allowedExtensions: Array<string>,
  showModalProp: {
    type: Boolean,
    default: false
  }
})

const fileActionEntry = ref<IConfirmFilePickerViewModel>({ inputValue: "" });

let treeNode = ref<TreeNode[]>(props.files);
let errorMessage = ref<string>("");
let showModal = ref(props.showModalProp);

/**
 * Reset error message when user changes file selection
 */
const onFileChange = () => {
  errorMessage.value = "";
}

/**
 * Validate user input and emit confirm or set error message
 * @param {IConfirmFilePickerViewModel} elem
 */
const validate = (elem: IConfirmFilePickerViewModel) => {
  if (elem.inputValue == null || elem.inputValue == "") {
    errorMessage.value = t.ValidationFilenameRequired;
  }
  else if (props.allowedExtensions && !props.allowedExtensions.find(x => x.replace(".", "") == getFileExtension(Object.keys(elem.inputValue ?? {})[0]))) {
    errorMessage.value = t.ValidationFileExtensionRequired;
  }
  else {
    debug("validate pass", elem)
    emit('confirm', elem)
  }
}

const emit = defineEmits<{
  (e: 'confirm', viewModel: IConfirmFilePickerViewModel): void
  (e: 'closed', viewModel?: IConfirmFilePickerViewModel): void
}>()
</script>
