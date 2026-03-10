<template>
  <VueFinalModal :focus-trap="false" v-model="showModal" :modal-id="modalName" :esc-to-close="false" :click-to-close="false" class="flex justify-center items-center"
    content-class="flex flex-column max-w-xl mx-4 p-4 rounded-lg space-y-2 action-modal">
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
      <template v-for="(folderActionElem, index) in commonActions" v-bind:key="index">
        <li class="p-1 w-100 flex align-items-center">
          <input class="p-radiobutton p-component" role="radiobutton" name="folder-action" type="radio"
            :id="'action-' + folderActionElem.id" :value="folderActionElem.id" v-model="folderAction" />
          <label class="ml-2 cursor-pointer w-100" :for="'action-' + folderActionElem.id">
            {{ folderActionElem.displayName }}
          </label>
        </li>
      </template>
    </ul>
    <div class="mt-3 flex flex-row justify-content-end">
      <button class="btn btn-light border border-secondary cancel" @click="showModal = false">
        {{ t.Cancel }}
      </button>
      <button id="btn-submit" class="ml-2 btn btn-primary"
        @click="emit('confirm', { action: folderAction, folder: folder, inputValue: inputValue })">
        <slot name="submit"></slot>
      </button>
    </div>
  </VueFinalModal>
</template>

<script setup lang="ts">
import { ref, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { IFileLibraryItemDto } from "../interfaces/interfaces";
import { IConfirmFolderActionViewModel, FolderAction } from '../interfaces/interfaces';

type IFileActionElem = { id: FolderAction; displayName: string; allowedDirectory: string };
import { useEventBus } from '../services/UseEventBus'
//import dbg from 'debug';
import { useLocalizations } from '../services/Localizations';

//const debug = dbg("aptix:file-app");

const { translations } = useLocalizations();
const t = translations.value
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
if (props.folder.directoryPath != "/") {
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
