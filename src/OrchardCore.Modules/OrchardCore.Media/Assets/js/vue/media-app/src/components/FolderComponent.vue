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
import { PropType, nextTick, ref, computed, onMounted, watch } from 'vue'
import dbg from 'debug';
import { useVfm } from 'vue-final-modal'
import ModalFolderAction from './ModalFolderAction.vue'
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import Folder from "./FolderComponent.vue";
import { v4 as uuidv4 } from 'uuid';
import {
  IConfirmFolderActionViewModel, FolderAction,
  IHFileLibraryItemDto,
  FileAction,
  IModalFileEvent
} from '@bloom/media/interfaces';
import {
  BASE_DIR
} from "@bloom/media/constants"
// @ts-ignore
import _ from "lodash";
import { useEventBus } from '../services/UseEventBus'
import { useConfirmModal } from '../services/ConfirmModalService';
import { useGlobals } from '../services/Globals';
import { FontAwesomeIcon as FaIcon } from '@fortawesome/vue-fontawesome';
import { useLocalizations } from '@bloom/helpers/localizations';
import { usePermissions } from '../services/Permissions';

const debug = dbg("aptix:file-app");
const modalAction = ref<InstanceType<typeof ModalFolderAction>>();

const { showConfirmModal } = useConfirmModal();
const { selectedDirectory } = useGlobals();
const { canManageFolder } = usePermissions();

const { translations } = useLocalizations();
const t = translations
const { on, emit } = useEventBus();

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
//let childrens = ref([] as IFileLibraryItemDto[]); // not initialized state (for lazy-loading)
let isHovered = ref(false);
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
  let folder = _.clone(props.hierarchicalDirectories);
  _.unset(folder, 'children');
  open.value = true;
  emit("DirSelected", folder as IFileLibraryItemDto);
}

/**
 * Create a folder
 * @param folder the folder to create
 * @function
 */
const createFolder = (folder: IFileLibraryItemDto) => {
  debug("Create Folder", folder)
  let directory = _.clone(folder);
  _.unset(directory, 'children');
  emit("DirCreateReq", directory);
}

/**
 * Delete a folder
 * @param folder the folder to delete
 * @function
 */
const deleteFolder = (folder: IFileLibraryItemDto) => {
  let directory = _.clone(folder);
  _.unset(directory, 'children');

  emit("DirDeleteReq", directory);
}

/**
 * Set isHovered to true when the user drags an item over the folder
 * This is used to change the style of the folder when it is being hovered
 * @function
 */
const handleDragOver = () => {
  isHovered.value = true;
}

/**
 * Set isHovered to false when the user drags an item out of the folder
 * This is used to change the style of the folder when it is no longer being hovered
 * @function
 */
const handleDragLeave = () => {
  isHovered.value = false;
}

/**
 * Move files to the given folder
 * @param {IFileLibraryItemDto} folder - The folder to move the files to
 * @param {DragEvent} e - The drag event to get the files from
 * @returns {void}
 * @description
 * This function is used to move files from one folder to another using drag and drop.
 * It gets the files from the drag event, and the target folder from the given folder.
 * If the source folder is the same as the target folder, it will show an error notification.
 * Otherwise it will show a confirmation modal to move the files.
 */
const moveFileToFolder = (folder: IFileLibraryItemDto, e: DragEvent): void => {
  debug("Move file to folder", folder, e);
  isHovered.value = false;

  let filesData = e.dataTransfer?.getData('files') ?? "";
  let files = JSON.parse(filesData);

  if (files.length < 1) {
    return;
  }

  let sourceFolder = e.dataTransfer?.getData('sourceFolder') ?? "root";
  let targetFolder = folder.directoryPath;

  if (targetFolder === '' || targetFolder == "/") {
    targetFolder = 'root';
  }

  if (sourceFolder === targetFolder) {
    notify(new NotificationMessage({ summary: t.ErrorMovingFile, detail: t.SameFolderMessage, severity: SeverityLevel.Error }));
    return;
  }

  const modal = { files: files, modalName: 'move', uuid: uuidv4(), isEdit: true, modalTitle: t.MoveFileTitle, action: FileAction.Move, targetFolder: targetFolder } as IModalFileEvent;
  showConfirmModal(modal);
}

/**
 * Return a unique modal name for a folder action
 * @param {string} action - The action to perform on the folder
 * @param {IFileLibraryItemDto} folder - The folder to get the modal name for
 * @returns {string} A unique modal name
 * @description
 * This function is used to generate a unique modal name for a folder action.
 * It takes the action and folder as parameters, and returns a string
 * that is unique to the folder and action.
 */
const getFolderModalName = (action: string, folder: IFileLibraryItemDto): string => {
  debug("Get folder modal name", folder)
  return action + "-folder-" + folder.name;
}

/**
 * Open a folder modal for a given action and folder
 * @param {string} action - The action to perform on the folder
 * @param {IFileLibraryItemDto} folder - The folder to open the modal for
 * @returns {Promise<void>}
 * @description
 * This function is used to open a modal for a folder action.
 * It takes the action and folder as parameters, and opens the modal
 * if the user has permission to manage the folder. Otherwise it shows
 * an error notification.
 */
const openFolderModal = async (action: string, folder: IFileLibraryItemDto): Promise<void> => {
  if (canManageFolder(folder.directoryPath)) {
    const uVfm = useVfm();
    const modalName = getFolderModalName(action, folder)
    debug('OpenFolderModal', modalName)
    uVfm.open(modalName);

    await nextTick();

    emit("ResetModalFolderAction", null);
  }
  else {
    notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFolder, severity: SeverityLevel.Warn }));
  }
}

/**
 * Confirm a folder action modal
 * @param {string} modalName - The name of the modal
 * @param {IConfirmFolderActionViewModel} confirmAction - The action to confirm
 * @description
 * This function is used to confirm a folder action modal.
 * If the action is create and the input value is not empty, it creates a new folder.
 * If the action is delete, it deletes the folder.
 * Otherwise it just closes the modal.
 */
const confirmFolderModal = (modalName: string, confirmAction: IConfirmFolderActionViewModel) => {
  const uVfm = useVfm();
  debug('confirmFolderModal confirmAction', confirmAction)
  uVfm.close(getFolderModalName(modalName, confirmAction.folder));

  if (confirmAction.action == FolderAction.Create && confirmAction.inputValue) {
    createFolder({ name: confirmAction.inputValue, directoryPath: confirmAction.inputValue, isDirectory: true, filePath: '' });
  }
  else if (confirmAction.action == FolderAction.Delete) {
    deleteFolder(confirmAction.folder);
  }
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
