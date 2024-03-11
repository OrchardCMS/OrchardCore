<script setup lang="ts">
import { ref, getCurrentInstance, PropType } from 'vue'
import { VueFinalModal } from 'vue-final-modal'
import { IFileStoreEntry } from "../services/MediaApiClient";
import { IFileActionElem, FileAction, IConfirmFileActionViewModel, IConfirmFileActionEntry } from '../interfaces/interfaces';
import dbg from 'debug';

const debug = dbg("aptix:file-app");
const emitter = getCurrentInstance()?.appContext.app.config.globalProperties.emitter;

const props = defineProps({
  title: String,
  modalName: {
    type: String,
    required: true
  },
  files: {
    type: Object as PropType<IFileStoreEntry[]>,
    required: true
  },
  t: {
    type: Object,
    required: true
  },
})

const FileActionElems = [
  {
    id: FileAction.Rename,
    displayName: props.t.Rename,
    allowedFolder: '*'
  },
  {
    id: FileAction.Delete,
    displayName: props.t.Delete,
    allowedFolder: '*'
  },
  {
    id: FileAction.Download,
    displayName: props.t.Download,
    allowedFolder: '*'
  },
] as IFileActionElem[];

const commonActions = FileActionElems.filter(item => item.allowedFolder == "*");
let showModal = ref(false);

let fileActionEntries: IConfirmFileActionEntry[] = [];

if (props.files.length > 1) {
  fileActionEntries = props.files?.map((file) => {
    debug("map files")
    return { file: file, inputValue: file.name }
  }) as IConfirmFileActionEntry[];
}
else {
  debug("map file")
  fileActionEntries.push({ file: props.files[0], inputValue: props.files[0].name });
}

let inputValues = ref(fileActionEntries);

const onPressEnter = () => {
  document.getElementById('btn-submit')?.click();
};

emitter.on("resetModalFileAction", () => {
  inputValues.value.forEach((inputValue) => {
    inputValue.action = undefined;
  });
});

const emit = defineEmits<{
  (e: 'confirm', viewModel: IConfirmFileActionViewModel): void
  (e: 'closed', viewModel?: IConfirmFileActionViewModel): void
}>()
</script>

<template>
  <VueFinalModal v-model="showModal" :modal-id="modalName" class="flex justify-center items-center"
    content-class="flex flex-column max-w-xl mx-4 p-4 bg-white dark:bg-gray-900 border dark:border-gray-700 rounded-lg space-y-2 action-modal">
    <div class="flex justify-content-between">
      <span class="modal__title">
        {{ title }}
      </span>
      <span class="cursor-pointer" @click="emit('closed')">
        <fa-icon icon="fa-solid fa-xmark fa-2xl"></fa-icon>
      </span>
    </div>
    <slot></slot>
    <template v-for="(fileActionEntry, index) in inputValues">
      <div class="mb-3">
        <input @keyup.enter="onPressEnter" :disabled="fileActionEntry.action != FileAction.Rename"
          class="p-inputtext p-component w-100" type="text" name="rename" v-model="fileActionEntry.inputValue" />
      </div>
      <h6 class="font-bold">{{ t.Common }}</h6>
      <ul class="list-none m-0 p-0">

        <template v-for="fileActionElem in commonActions">
          <li class="p-1 w-100 flex align-items-center">
            <input class="p-radiobutton p-component" role="radiobutton" :name="'action[' + index + ']'" type="radio"
              :id="'action[' + index + ']-' + fileActionElem.id" :value="fileActionElem.id" v-model="fileActionEntry.action" />
            <label class="ms-2 cursor-pointer w-100" :for="'action[' + index + ']-' + fileActionElem.id">
              {{ fileActionElem.displayName }}
            </label>
          </li>
        </template>
      </ul>
    </template>
    <div class="mt-3 flex flex-row justify-content-end">
      <button class="btn btn-secondary" @click="emit('closed')">
        {{ t.Cancel }}
      </button>
      <button id="btn-submit" class="ms-2 btn btn-primary"
        @click="emit('confirm', { actionEntries: inputValues })">
        <slot name="submit"></slot>
      </button>
    </div>
  </VueFinalModal>
</template>