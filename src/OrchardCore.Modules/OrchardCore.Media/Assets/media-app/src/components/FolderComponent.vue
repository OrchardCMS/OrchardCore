<template>
  <li class="folder" :class="{ selected: isSelected }" v-on:dragleave.prevent="handleDragLeave();"
    v-on:dragover.prevent.stop="handleDragOver();"
    v-on:drop.prevent.stop="moveFileToFolder(hierarchicalDirectories, $event)">
    <div :class="{ folderhovered: isHovered, treeroot: level == 1 }">
      <a href="javascript:void(0)" :style="{ 'padding-left': padding + 'px' }" v-on:click="select" draggable="false"
        class="folder-menu-item">
        <span v-on:click.stop="toggle" class="expand" v-if="hierarchicalDirectories?.children?.length > 0">
          <fa-icon v-if="open" icon="fas fa-chevron-down"></fa-icon>
          <fa-icon v-if="!open" icon="fas fa-chevron-up"></fa-icon>
        </span>
        <span class="expand empty" v-else>&nbsp;</span>
        <span>
          <svg class="tw:ml-1" width="16" height="16" viewBox="0 0 16 16" fill="none"
            xmlns="http://www.w3.org/2000/svg">
            <path
              d="M7.06065 3.06033C6.80732 2.80699 6.46732 2.66699 6.11398 2.66699H2.66732C1.93398 2.66699 1.34065 3.26699 1.34065 4.00033L1.33398 12.0003C1.33398 12.7337 1.93398 13.3337 2.66732 13.3337H13.334C14.0673 13.3337 14.6673 12.7337 14.6673 12.0003V5.33366C14.6673 4.60033 14.0673 4.00033 13.334 4.00033H8.00065L7.06065 3.06033Z"
              fill="#2c84d8" />
          </svg>
        </span>
        <div class="folder-name tw:ms-2">
          <div class="tw:pr-2">{{ hierarchicalDirectories.name }}</div>
        </div>
        <div class="folder-actions">
          <a v-cloak href="javascript:void(0)" :title="t.ActionFolderTitle" class="action-button"
            @click="() => openFolderModal('folder-action', hierarchicalDirectories)" v-if="isSelected"><fa-icon
              icon="fas fa-ellipsis-v" size="xl"></fa-icon>
            <ModalFolderAction ref="modalAction" :show-modal-prop="showModal"
              :modal-name="getFolderModalName('folder-action', hierarchicalDirectories)"
              :folder="hierarchicalDirectories" :title="t.ActionFolderTitle"
              @confirm="(viewModel: IConfirmFolderActionViewModel) => confirmFolderModal('folder-action', viewModel)">
              <p class="tw:font-bold tw:m-0">{{ hierarchicalDirectories.directoryPath }}</p>
              <p class="tw:m-0">{{ t.ActionFolderMessage }}</p>
              <template #submit>{{ t.Ok }}</template>
            </ModalFolderAction>
          </a>
        </div>
      </a>
    </div>
    <ol v-show="open">
      <folder v-for="child in hierarchicalDirectories.children" :key="child.directoryPath" :hierarchical-directories="child"
        :level="(level ? level : 0) + 1">
      </folder>
    </ol>
  </li>
</template>

<script setup lang="ts">
import { PropType, ref, computed, onMounted, watch } from 'vue'
import dbg from 'debug';
import ModalFolderAction from './ModalFolderAction.vue'
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import Folder from "./FolderComponent.vue";
import {
  IConfirmFolderActionViewModel,
  IHFileLibraryItemDto,
} from '@bloom/media/interfaces';
import {
  BASE_DIR
} from "@bloom/media/constants"
import { useEventBus } from '../services/UseEventBus'
import { useGlobals } from '../services/Globals';
import { FontAwesomeIcon as FaIcon } from '@fortawesome/vue-fontawesome';
import { useLocalizations } from '../composables/useLocalizations';
import { useFolderModal } from '../composables/useFolderModal';
import { useFolderDragDrop } from '../composables/useFolderDragDrop';

const debug = dbg("orchardcore:file-app");
const modalAction = ref<InstanceType<typeof ModalFolderAction>>();

const { selectedDirectory } = useGlobals();

const { translations: t } = useLocalizations();
const { on, emit } = useEventBus();
const { getFolderModalName, openFolderModal, confirmFolderModal: confirmModal } = useFolderModal();
const { isHovered, handleDragOver, handleDragLeave, moveFileToFolder } = useFolderDragDrop();

const props = defineProps({
  hierarchicalDirectories: { // This cannot use globals
    type: Object as PropType<IHFileLibraryItemDto>,
    required: true
  },
  level: {
    type: Number,
    required: true
  },
  showModalProp: {
    type: Boolean,
    default: false
  }
})

let open = ref(false);
let padding = ref(0);
let showModal = ref(props.showModalProp);

const isSelected = computed(() => {
  return (selectedDirectory.value.name == props.hierarchicalDirectories.name) && (selectedDirectory.value.directoryPath == props.hierarchicalDirectories.directoryPath);
});

const isRoot = computed(() => {
  return props.hierarchicalDirectories.directoryPath === BASE_DIR;
});

on("DirDelete", (folder: IFileLibraryItemDto) => {
  emit("DirDeleted", folder);
});

on("DirAddReq", (element: { selectedDirectory: IFileLibraryItemDto; data: IFileLibraryItemDto; }) => {
  let target = element.selectedDirectory;
  let folder = element.data;

  if (props.hierarchicalDirectories == target) {
    emit("DirAdded", folder);
  }

  openCreatedFolder(target)
});

/**
 * If the created folder is the same as the current hierarchical directory, open it
 * @param element newly created folder
 */
const openCreatedFolder = (element: IFileLibraryItemDto) => {
  if (props.hierarchicalDirectories.directoryPath == element.directoryPath) {
    openFolder();
  }
}

/**
 * If the selected directory is a child of the current hierarchical directory, open all the needed folders
 * to show the selected directory
 */
const openSelectedFolder = () => {
  if (selectedDirectory.value) {
    const folders = selectedDirectory.value.directoryPath.split("/");
    let folderName = "";

    folders.forEach((folder, index) => {
      folderName = index > 0 ? folderName + "/" + folder : folder;
      if (props.hierarchicalDirectories.directoryPath == folderName) {
        openFolder();
      }
    });
  }
}

/**
 * Open the current folder
 * @function
 */
const openFolder = () => {
  open.value = true;
}

/**
 * Toggle the open state of the folder
 */
const toggle = () => {
  open.value = !open.value;
}

/**
 * Select the current folder
 * 
 * This function will emit an event "DirSelected" with the selected folder as parameter.
 * The folder is cloned and the 'children' property is unset to prevent the children from being transferred.
 * Then the open state is set to true to open the folder.
 */
const select = () => {
  const { children, ...folder } = props.hierarchicalDirectories;
  open.value = true;
  emit("DirSelected", folder as IFileLibraryItemDto);
}

const confirmFolderModal = (modalName: string, confirmAction: IConfirmFolderActionViewModel) => {
  confirmModal(
    modalName,
    confirmAction,
    (folder) => {
      debug("Create Folder", folder);
      const { children, ...directory } = folder as IHFileLibraryItemDto;
      emit("DirCreateReq", directory);
    },
    (folder) => {
      const { children, ...directory } = folder as IHFileLibraryItemDto;
      emit("DirDeleteReq", directory);
    },
  );
}

// Open root folder when data becomes available (handles both v-if and v-show mounting).
// With v-show, onMounted fires before data loads so isRoot may be false at that point.
watch(isRoot, (val) => {
  if (val && !open.value) {
    open.value = true;
  }
}, { immediate: true });

// Open ancestor folders when a directory is already selected on mount.
watch(() => selectedDirectory.value?.directoryPath, (dirPath) => {
  if (dirPath) {
    openSelectedFolder();
  }
}, { immediate: true });

onMounted(() => {
  let level = props.level ? props.level : 0;
  padding.value = level < 2 ? 10 : (level * 10);
});
</script>
