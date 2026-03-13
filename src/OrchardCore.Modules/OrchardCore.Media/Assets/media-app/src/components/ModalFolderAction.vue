<template>
  <VueFinalModal :focus-trap="false" v-model="showModal" :modal-id="modalName" :esc-to-close="false" :click-to-close="false" class="ma-vfm tw:flex tw:justify-center tw:items-center"
    content-class="tw:flex tw:flex-col tw:max-w-xl tw:mx-4 tw:p-4 tw:rounded-lg tw:space-y-3 action-modal">
    <div class="tw:flex tw:justify-between">
      <span class="modal__title">
        {{ title }}
      </span>
      <span class="tw:cursor-pointer" @click="showModal = false">
        <fa-icon icon="fa-solid fa-xmark fa-2xl"></fa-icon>
      </span>
    </div>
    <slot></slot>
    <div class="tw:mb-3" :class="{ 'tw:hidden': folderAction == null || folderAction != FolderAction.Create }">
      <input @keyup.enter="onPressEnter" class="p-inputtext tw:w-full" placeholder="Enter a folder name"
        type="text" name="create-folder" v-model="inputValue" />
    </div>
    <div>
      <template v-for="(folderActionElem, index) in commonActions" v-bind:key="index">
        <div class="tw:py-1 tw:w-full tw:flex tw:items-center">
          <input class="tw:w-4 tw:h-4" role="radiobutton" name="folder-action" type="radio"
            :id="'action-' + folderActionElem.id" :value="folderActionElem.id" v-model="folderAction" />
          <label class="tw:ml-2 tw:cursor-pointer tw:w-full" :for="'action-' + folderActionElem.id">
            {{ folderActionElem.displayName }}
          </label>
        </div>
      </template>
    </div>
    <div class="tw:mt-3 tw:flex tw:flex-row tw:justify-end">
      <button class="ma-btn ma-btn-light tw:border tw:border-gray-400 cancel" @click="showModal = false">
        {{ t.Cancel }}
      </button>
      <button id="btn-submit" class="tw:ml-2 ma-btn ma-btn-primary"
        @click="emit('confirm', { action: folderAction, folder: folder, inputValue: inputValue })">
        <slot name="submit"></slot>
      </button>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { IConfirmFolderActionViewModel, FolderAction } from '@bloom/media/interfaces';

type IFileActionElem = { id: FolderAction; displayName: string; allowedDirectory: string };
import { useEventBus } from '../services/UseEventBus'
//import dbg from 'debug';
import { useLocalizations } from '@bloom/helpers/localizations';

//const debug = dbg("orchardcore:file-app");

const { translations } = useLocalizations();
const t = translations
const { on } = useEventBus();

const props = defineProps({
  title: String,
  modalName: {
    type: String,
    required: true
  },
  folder: {
    type: Object as PropType<IFileLibraryItemDto>,
    required: true
  },
  showModalProp: {
    type: Boolean,
    default: false
  }
})

let folderActionElems = [
  {
    id: FolderAction.Create,
    displayName: t.CreateSubFolder,
    allowedDirectory: '*'
  }
] as IFileActionElem[];

// If we are on root folder we don't allow delete
if (props.folder.directoryPath && props.folder.directoryPath !== "/") {
  folderActionElems.push(
    {
      id: FolderAction.Delete,
      displayName: t.Delete,
      allowedDirectory: '*'
    })
}

const commonActions = folderActionElems.filter(item => item.allowedDirectory == "*");
let showModal = ref(props.showModalProp);
let inputValue = "";
let folderAction = ref();

/**
 * Triggers the click event on the submit button when the Enter key is pressed
 */
const onPressEnter = () => {
  document.getElementById('btn-submit')?.click();
};

on("ResetModalFolderAction", () => {
  folderAction = ref();
  inputValue = "";
});

const emit = defineEmits<{
  (e: 'confirm', action: IConfirmFolderActionViewModel): void
}>()
</script>
