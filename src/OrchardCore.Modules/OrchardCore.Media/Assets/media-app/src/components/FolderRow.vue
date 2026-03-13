<template>
  <li class="folder" :class="{ selected: isSelected }"
    @dragleave.prevent="handleDragLeave"
    @dragover.prevent.stop="handleDragOver"
    @drop.prevent.stop="moveFileToFolder(node.item, $event)">
    <div :class="{ folderhovered: isHovered, treeroot: isRoot }">
      <a href="javascript:void(0)" :style="{ 'padding-left': padding + 'px' }" @click="select" draggable="false"
        class="folder-menu-item">
        <span v-if="hasExpandableChildren" @click.stop="toggle" class="expand">
          <span v-if="isLoading" class="folder-spinner"></span>
          <fa-icon v-else :icon="isExpanded ? 'fas fa-chevron-down' : 'fas fa-chevron-right'"></fa-icon>
        </span>
        <span class="expand empty" v-else-if="!isRoot">&nbsp;</span>
        <span>
          <svg class="tw:ml-1" width="16" height="16" viewBox="0 0 16 16" fill="none"
            xmlns="http://www.w3.org/2000/svg">
            <path
              d="M7.06065 3.06033C6.80732 2.80699 6.46732 2.66699 6.11398 2.66699H2.66732C1.93398 2.66699 1.34065 3.26699 1.34065 4.00033L1.33398 12.0003C1.33398 12.7337 1.93398 13.3337 2.66732 13.3337H13.334C14.0673 13.3337 14.6673 12.7337 14.6673 12.0003V5.33366C14.6673 4.60033 14.0673 4.00033 13.334 4.00033H8.00065L7.06065 3.06033Z"
              fill="#2c84d8" />
          </svg>
        </span>
        <div class="folder-name tw:ms-2">
          <div class="tw:pr-2">{{ node.item.name }}</div>
        </div>
        <div class="folder-actions">
          <a v-cloak href="javascript:void(0)" :title="t.ActionFolderTitle" class="action-button"
            @click="() => openFolderModal('folder-action', node.item)" v-if="isSelected"><fa-icon
              icon="fas fa-ellipsis-v" size="xl"></fa-icon>
            <ModalFolderAction ref="modalAction" :show-modal-prop="showModal"
              :modal-name="getFolderModalName('folder-action', node.item)"
              :folder="node.item" :title="t.ActionFolderTitle"
              @confirm="(viewModel: IConfirmFolderActionViewModel) => confirmFolderModal('folder-action', viewModel)">
              <p class="tw:font-bold tw:m-0">{{ node.item.directoryPath }}</p>
              <p class="tw:m-0">{{ t.ActionFolderMessage }}</p>
              <template #submit>{{ t.Ok }}</template>
            </ModalFolderAction>
          </a>
        </div>
      </a>
    </div>
  </li>
</template>

<script setup lang="ts">
import { computed, ref, nextTick } from 'vue';
import { useVfm } from 'vue-final-modal';
import ModalFolderAction from './ModalFolderAction.vue';
import { IFileLibraryItemDto, IConfirmFolderActionViewModel, FolderAction, FileAction, IModalFileEvent, IHFileLibraryItemDto } from '@bloom/media/interfaces';
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { v4 as uuidv4 } from 'uuid';
import { useEventBus } from '../services/UseEventBus';
import { useConfirmModal } from '../services/ConfirmModalService';
import { useGlobals } from '../services/Globals';
import { FontAwesomeIcon as FaIcon } from '@fortawesome/vue-fontawesome';
import { useLocalizations } from '@bloom/helpers/localizations';
import { usePermissions } from '../services/Permissions';
import type { IFlatTreeNode } from '../services/HierarchicalTreeBuilder';
import { BASE_DIR } from "@bloom/media/constants";

const props = defineProps<{
  node: IFlatTreeNode;
}>();

const emitComponent = defineEmits<{
  select: [node: IFlatTreeNode];
  toggle: [node: IFlatTreeNode];
}>();

const { showConfirmModal } = useConfirmModal();
const { selectedDirectory, expandedFolders, loadingFolders } = useGlobals();
const { canManageFolder } = usePermissions();
const { translations } = useLocalizations();
const t = translations;
const { emit } = useEventBus();

const modalAction = ref<InstanceType<typeof ModalFolderAction>>();
const isHovered = ref(false);
const showModal = ref(false);

const isSelected = computed(() => {
  return selectedDirectory.value.name === props.node.item.name
    && selectedDirectory.value.directoryPath === props.node.item.directoryPath;
});

const isRoot = computed(() => {
  return props.node.item.directoryPath === "" || props.node.item.directoryPath === BASE_DIR;
});

const isExpanded = computed(() => {
  return expandedFolders.value.has(props.node.item.directoryPath);
});

const isLoading = computed(() => {
  return loadingFolders.value.has(props.node.item.directoryPath);
});

const hasExpandableChildren = computed(() => {
  if (isRoot.value) return false;
  const item = props.node.item;
  return (item.children && item.children.length > 0) || item.hasChildren;
});

const padding = computed(() => {
  const depth = props.node.depth;
  if (depth < 1) return 2;
  return (depth - 1) * 10 + 2;
});

const select = () => {
  emitComponent("select", props.node);
};

const toggle = () => {
  emitComponent("toggle", props.node);
};

const handleDragOver = () => {
  isHovered.value = true;
};

const handleDragLeave = () => {
  isHovered.value = false;
};

const moveFileToFolder = (folder: IFileLibraryItemDto, e: DragEvent): void => {
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
};

const getFolderModalName = (action: string, folder: IFileLibraryItemDto): string => {
  return action + "-folder-" + folder.name;
};

const openFolderModal = async (action: string, folder: IFileLibraryItemDto): Promise<void> => {
  if (canManageFolder(folder.directoryPath)) {
    const uVfm = useVfm();
    const modalName = getFolderModalName(action, folder);
    uVfm.open(modalName);

    await nextTick();

    emit("ResetModalFolderAction", null);
  }
  /* v8 ignore next 3 -- canManageFolder always returns true; server enforces auth */
  else {
    notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFolder, severity: SeverityLevel.Warn }));
  }
};

const confirmFolderModal = (modalName: string, confirmAction: IConfirmFolderActionViewModel) => {
  const uVfm = useVfm();
  uVfm.close(getFolderModalName(modalName, confirmAction.folder));

  if (confirmAction.action == FolderAction.Create && confirmAction.inputValue) {
    const { children, ...directory } = props.node.item;
    emit("DirCreateReq", { name: confirmAction.inputValue, directoryPath: confirmAction.inputValue, isDirectory: true, filePath: '' } as IFileLibraryItemDto);
  } else if (confirmAction.action == FolderAction.Delete) {
    const { children, ...directory } = confirmAction.folder as IHFileLibraryItemDto;
    emit("DirDeleteReq", directory);
  }
};
</script>

<style scoped>
/* rtl:ignore */
.folder-spinner {
  display: inline-block;
  width: 12px;
  height: 12px;
  border: 2px solid #ccc;
  border-top-color: #2c84d8;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
