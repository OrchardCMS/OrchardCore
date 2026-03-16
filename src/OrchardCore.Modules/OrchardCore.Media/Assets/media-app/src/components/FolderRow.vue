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
        <div class="folder-name tw:ms-2 tw:overflow-hidden">
          <div class="tw:pr-2 tw:truncate">{{ node.item.name }}</div>
        </div>
        <div class="folder-actions tw:shrink-0">
          <a href="javascript:void(0)" :title="t.ActionFolderTitle" class="action-button"
            @click.stop="() => openFolderModal('folder-action', node.item)"><fa-icon
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
import { computed, ref } from 'vue';
import ModalFolderAction from './ModalFolderAction.vue';
import { IFileLibraryItemDto, IConfirmFolderActionViewModel, IHFileLibraryItemDto } from '@bloom/media/interfaces';
import { useEventBus } from '../services/UseEventBus';
import { useGlobals } from '../services/Globals';
import { FontAwesomeIcon as FaIcon } from '@fortawesome/vue-fontawesome';
import { getTranslations } from '@bloom/helpers/localizations';
import { useFolderModal } from '../composables/useFolderModal';
import { useFolderDragDrop } from '../composables/useFolderDragDrop';
import type { IFlatTreeNode } from '../services/HierarchicalTreeBuilder';
import { BASE_DIR } from "@bloom/media/constants";

const props = defineProps<{
  node: IFlatTreeNode;
}>();

const emitComponent = defineEmits<{
  select: [node: IFlatTreeNode];
  toggle: [node: IFlatTreeNode];
}>();

const { selectedDirectory, expandedFolders, loadingFolders } = useGlobals();
const t = getTranslations();
const { emit } = useEventBus();
const { getFolderModalName, openFolderModal, confirmFolderModal: confirmModal } = useFolderModal();
const { isHovered, handleDragOver, handleDragLeave, moveFileToFolder } = useFolderDragDrop();

const modalAction = ref<InstanceType<typeof ModalFolderAction>>();
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

const confirmFolderModal = (modalName: string, confirmAction: IConfirmFolderActionViewModel) => {
  confirmModal(
    modalName,
    confirmAction,
    (folder) => emit("DirCreateReq", folder),
    (folder) => {
      const { children, ...directory } = folder as IHFileLibraryItemDto;
      emit("DirDeleteReq", directory);
    },
  );
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

/* Show folder actions only on hover */
.folder-actions {
  opacity: 0;
  transition: opacity 0.15s ease;
}

.folder:hover .folder-actions {
  opacity: 1;
}
</style>
