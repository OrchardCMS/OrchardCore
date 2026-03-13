<template>
  <div>
    <a href="javascript:void(0)" class="ma-btn ma-btn-link ma-btn-sm action-button tw:px-4 tw:py-3" @click="toggle">
      <fa-icon icon="fas fa-ellipsis-v" size="xl"></fa-icon>
    </a>
    <p-menu ref="menu" id="overlay_menu" class="file-app" :model="items" :popup="true" />
  </div>
  <ModalsContainer v-if="showModal" />
</template>

<script setup lang="ts">
import { computed, PropType, ref } from "vue";
import { FileAction, IModalFileEvent, IRenameFileLibraryItemDto, IFileLibraryItemDto } from "@bloom/media/interfaces";
import { v4 as uuidv4 } from 'uuid';
import { useConfirmModal } from "../services/ConfirmModalService";
import { useFileActionModal } from "../services/FileActionModalService";
import { useGlobals } from "../services/Globals";
import { useEventBus } from "../services/UseEventBus";
import { useLocalizations } from "@bloom/helpers/localizations"
import { downloadFile } from "../services/Utils";
import { ModalsContainer } from 'vue-final-modal'
import dbg from "debug";

const debug = dbg("orchardcore:file-app");

const props = defineProps({
  fileItem: {
    type: Object as PropType<IRenameFileLibraryItemDto>,
    required: true
  },
  showModalProp: {
    type: Boolean,
    default: false
  }
})

const { emit } = useEventBus();
const { translations } = useLocalizations();
const t = translations
const { showConfirmModal } = useConfirmModal();
const { showFileActionModal } = useFileActionModal();

const showModal = ref(props.showModalProp);

const menu = ref();
const { on } = useEventBus();

on("CloseFileMenus", (sender: unknown) => {
  if (sender !== menu) {
    menu.value?.hide();
  }
});

const items = computed(() => {
  return [
    {
      items: [
        {
          label: t.Rename,
          icon: 'fa-solid fa-edit',
          command: () => {
            const modal = { showModal: showModal.value, files: [props.fileItem], modalName: 'rename', uuid: uuidv4(), isEdit: true, modalTitle: t.RenameSingleFileTitle, action: FileAction.Rename } as IModalFileEvent;
            showFileActionModal(modal);
          }
        },
        {
          label: t.Move,
          icon: 'fa-solid fa-folder',
          command: () => {
            const modal = { showModal: showModal.value, files: [props.fileItem], modalName: 'move', uuid: uuidv4(), isEdit: true, modalTitle: t.MoveSingleFileTitle, action: FileAction.Move } as IModalFileEvent;
            showFileActionModal(modal);
          }
        },
        {
          label: t.Copy,
          icon: 'fa-solid fa-copy',
          command: () => {
            const modal = { showModal: showModal.value, files: [props.fileItem], modalName: 'copy', uuid: uuidv4(), isEdit: true, modalTitle: t.CopySingleFileTitle, action: FileAction.Copy } as IModalFileEvent;
            showFileActionModal(modal);
          }
        },
        {
          label: t.Download,
          icon: 'fa-solid fa-download',
          command: () => {
            downloadFile(props.fileItem);
          }
        },
        {
          label: t.Delete,
          icon: 'fa-solid fa-trash',
          command: () => {
            const modal = { showModal: showModal.value, files: [props.fileItem], modalName: 'delete', uuid: uuidv4(), isEdit: true, modalTitle: t.DeleteFileTitle, action: FileAction.Delete } as IModalFileEvent;
            showConfirmModal(modal);
          }
        }
      ]
    }
  ];
});

/**
 * Toggle the menu.
 * @param {object} event The event object of the component.
 */
const toggle = (event: any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
  emit("CloseFileMenus", menu);
  menu.value?.toggle(event);
};
</script>
