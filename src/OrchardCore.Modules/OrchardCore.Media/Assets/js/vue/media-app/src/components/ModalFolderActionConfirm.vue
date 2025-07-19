<script setup lang="ts">
import { ref, getCurrentInstance, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { IFileStoreEntry } from "../services/MediaApiClient";
import { IFileActionElem, IConfirmFolderActionViewModel, FolderAction } from '../interfaces/interfaces';
import dbg from 'debug';

const debug = dbg("aptix:file-app");
const emitter = getCurrentInstance()?.appContext.app.config.globalProperties.emitter;

const props = defineProps({
  title: String,
  modalName: {
    type: String,
    required: true
  },
  folder: {
    type: Object as PropType<IFileStoreEntry>,
    required: true
  },
  t: {
    type: Object,
    required: true
  },
})

const FolderActionElems = [
  {
    id: FolderAction.Create,
    displayName: props.t.CreateSubFolder,
    allowedFolder: '*'
  },
  {
    id: FolderAction.Delete,
    displayName: props.t.Delete,
    allowedFolder: '*'
  }
] as IFileActionElem[];

const commonActions = FolderActionElems.filter(item => item.allowedFolder == "*");
let showModal = ref(false);
let inputValue = "";
let folderAction = ref();

const onPressEnter = () => {
  document.getElementById('btn-submit')?.click();
};

emitter.on("resetModalFolderAction", () => {
  folderAction = ref();
  inputValue = "";
});

const emit = defineEmits<{
  (e: 'confirm', action: IConfirmFolderActionViewModel): void
}>()
</script>

<template>
  <VueFinalModal v-model="showModal" :modal-id="modalName" class="flex justify-center items-center"
    content-class="flex flex-column max-w-xl mx-4 p-4 bg-white dark:bg-gray-900 border dark:border-gray-700 rounded-lg space-y-2 action-modal">
    <div class="flex justify-content-between">
      <span class="modal__title">
        {{ title }}
      </span>
      <span class="cursor-pointer" @click="showModal = false">
        <fa-icon icon="fa-solid fa-xmark fa-2xl"></fa-icon>
      </span>
    </div>
    <slot></slot>
    <div class="mb-3" :class="{ hidden: folderAction == null || folderAction != FolderAction.Create }">
      <input @keyup.enter="onPressEnter" class="p-inputtext p-component w-100" placeholder="Enter a folder name"
        type="text" name="create-folder" v-model="inputValue" />
    </div>
    <ul class="list-none m-0 p-0">
      <template v-for="folderActionElem in commonActions">
        <li class="p-1 w-100 flex align-items-center">
          <input class="p-radiobutton p-component" role="radiobutton" name="folder-action" type="radio"
            :id="'action-' + folderActionElem.id" :value="folderActionElem.id" v-model="folderAction" />
          <label class="ms-2 cursor-pointer w-100" :for="'action-' + folderActionElem.id">
            {{ folderActionElem.displayName }}
          </label>
        </li>
      </template>
    </ul>
    <div class="mt-3 flex flex-row justify-content-end">
      <button class="btn btn-secondary" @click="showModal = false">
        {{ t.Cancel }}
      </button>
      <button id="btn-submit" class="ms-2 btn btn-primary"
        @click="emit('confirm', { action: folderAction, folder: folder, inputValue: inputValue })">
        <slot name="submit"></slot>
      </button>
    </div>
  </VueFinalModal>
</template>