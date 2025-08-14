<template>
  <li :class="{ selected: isSelected }" v-on:dragleave.prevent="handleDragLeave();"
    v-on:dragover.prevent.stop="handleDragOver();" v-on:drop.prevent.stop="moveFileToFolder(currentFolder, $event)">
    <ModalConfirm :t="t" :action-name="t.MoveFileTitle" :modal-name="getModalName('file', 'move')"
      :title="t.MoveFileTitle" @confirm="() => confirm('file', 'move')">
      <label>{{ t.MoveFileMessage }}</label>
    </ModalConfirm>
    <div :class="{ folderhovered: isHovered, treeroot: level == 1 }">
      <a href="javascript:void(0)" :style="{ 'padding-left': padding + 'px' }" v-on:click="select" draggable="false"
        class="folder-menu-item">
        <span v-on:click.stop="toggle" class="expand">
          <fa-icon v-if="open" icon="fas fa-chevron-down"></fa-icon>
          <fa-icon v-if="!open" icon="fas fa-chevron-up"></fa-icon>
        </span>
        <span>
          <svg class="ms-1" width="16" height="16" viewBox="0 0 16 16" fill="none"
            xmlns="http://www.w3.org/2000/svg">
            <path
              d="M7.06065 3.06033C6.80732 2.80699 6.46732 2.66699 6.11398 2.66699H2.66732C1.93398 2.66699 1.34065 3.26699 1.34065 4.00033L1.33398 12.0003C1.33398 12.7337 1.93398 13.3337 2.66732 13.3337H13.334C14.0673 13.3337 14.6673 12.7337 14.6673 12.0003V5.33366C14.6673 4.60033 14.0673 4.00033 13.334 4.00033H8.00065L7.06065 3.06033Z"
              fill="#2c84d8" />
          </svg>
        </span>
        <div class="folder-name ms-2">{{ currentFolder.name }}</div>
        <div class="folder-actions">
          <a v-cloak href="javascript:void(0)" :title="t.DeleteFolderTitle" class="px-2"
            @click="() => openFolderModal('folder-action', currentFolder)" v-if="isSelected && !isRoot"><fa-icon
              icon="fas fa-ellipsis-v" size="xl"></fa-icon>
            <ModalFolderActionConfirm ref="modalAction" :t="t"
              :modal-name="getFolderModalName('folder-action', currentFolder)" :folder="currentFolder"
              :title="t.ActionFolderTitle"
              @confirm="(viewModel: IConfirmFolderActionViewModel) => confirmFolderModal('folder-action', viewModel)">
              <p class="font-bold">{{ currentFolder.path }}</p>
              <p>{{ t.ActionFolderMessage }}</p>
              <template #submit>{{ t.Ok }}</template>
            </ModalFolderActionConfirm>
          </a>
        </div>
      </a>
    </div>
    <ol v-show="open">
      <folder :base-url="baseUrl" v-for="folder in children" :t="t" :key="folder.path" :current-folder="folder"
        :selected-in-file-app="selectedInFileApp" :level="(level ? level : 0) + 1">
      </folder>
    </ol>
  </li>
</template>

<script lang="ts">
import { defineComponent, PropType, nextTick, ref } from 'vue'
import dbg from 'debug';
import { useVfm } from 'vue-final-modal'
import ModalConfirm from './ModalConfirm.vue'
import ModalFolderActionConfirm from './ModalFolderActionConfirm.vue'
import { MediaApiClient, IFileStoreEntry } from "../services/MediaApiClient";
import { notify, tryGetErrorMessage } from "../services/notifier";
import { SeverityLevel } from "../interfaces/interfaces"
import { IConfirmFolderActionViewModel, FolderAction, FileAction } from '../interfaces/interfaces';

const debug = dbg("aptix:file-app");
let moveAssetsState = <any>{};
const modalAction = ref<InstanceType<typeof ModalFolderActionConfirm>>();

export default defineComponent({
  components: {
    ModalConfirm: ModalConfirm,
    ModalFolderActionConfirm: ModalFolderActionConfirm
  },
  expose: ['select', 'selectFolder'],
  name: "folder",
  props: {
    currentFolder: {
      type: Object as PropType<IFileStoreEntry>,
      required: true
    },
    selectedInFileApp: Object as PropType<IFileStoreEntry>,
    level: Number,
    baseUrl: String,
    t: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      open: false,
      children: <any>[], // not initialized state (for lazy-loading)
      parent: null,
      isHovered: false,
      padding: 0
    }
  },
  computed: {
    empty: function () {
      return !this.children || this.children.length == 0;
    },
    isSelected: function () {
      return (this.selectedInFileApp?.name == this.$props.currentFolder.name) && (this.selectedInFileApp.path == this.$props.currentFolder.path);
    },
    isRoot: function () {
      return this.currentFolder?.path === '';
    }
  },
  mounted() {
    if (this.isAncestorOfSelectedFolder()) {
      this.toggle();
    }

    let level = this.level ? this.level : 0;

    this.padding = level < 3 ? 8 : (level * 8);
  },
  created: function () {
    const me = this;

    this.emitter.on('deleteFolder', function (folder: never) {
      if (me.children) {
        let index = me.children && me.children.indexOf(folder)
        if (index > -1) {
          me.children.splice(index, 1)
          me.emitter.emit('folderDeleted');
        }
      }
    });

    this.emitter.on('addFolder', function (element: { selectedFolder: any; data: any; }) {
      let target = element.selectedFolder;
      let folder = <any>element.data;

      if (me.currentFolder == target) {
        if (me.children !== null) {
          me.children.push(<never>folder);
          me.children.sort(function (a: any, b: any) {
            if (a.name > b.name) { return -1; }
            if (a.name < b.name) { return 1; }
            return 0;
          });
        }

        folder.parent = me.currentFolder;
        me.emitter.emit('folderAdded', folder);
      }
    });
  },
  methods: {
    isAncestorOfSelectedFolder: function () {
      let parentFolder = this.selectedInFileApp;

      while (parentFolder) {
        if (parentFolder.path == this.currentFolder?.path) {
          return true;
        }
        parentFolder = parentFolder.parent; // TODO: refactor as this is a param added programmatically
      }

      return false;
    },
    toggle: function () {
      this.open = !this.open;

      if (this.open) {
        this.loadChildren();
      }
    },
    select: function () {
      this.emitter.emit('folderSelected', this.currentFolder);
      this.loadChildren();
    },
    selectFolder: function (folder: IFileStoreEntry) {
      this.emitter.emit('folderSelected', folder);
      this.loadChildren();
    },
    createFolder: function (folder: IFileStoreEntry) {
      debug(folder)
      this.emitter.emit('createFolderRequested', folder);
    },
    deleteFolder: function (folder: IFileStoreEntry) {
      this.emitter.emit('deleteFolderRequested', folder);
    },
    loadChildren: function () {
      const me = this;

      if (this.open == false) {
        this.open = true;
      }

      const apiClient = new MediaApiClient(me.baseUrl);
      apiClient
        .getFolders(me.currentFolder?.path)
        .then((response) => {
          me.children = response;
          me.children.forEach(function (c: any) {
            c.parent = me.currentFolder; // TODO: refactor as this is a param added programmatically
          });
        })
        .catch(async (error) => {
          notify({ summary: me.t.ErrorGetFolders, detail: await tryGetErrorMessage(error), severity: SeverityLevel.Error });
        })
    },
    handleDragOver: function () {
      this.isHovered = true;
    },
    handleDragLeave: function () {
      this.isHovered = false;
    },
    moveFileToFolder: function (folder: IFileStoreEntry, e: DragEvent) {
      debug("Move file to folder", folder, e);
      const me = this;
      me.isHovered = false;

      let fileNamesData = e.dataTransfer?.getData('fileNames') ?? "";
      let fileNames = JSON.parse(fileNamesData);

      if (fileNames.length < 1) {
        return;
      }

      let sourceFolder = e.dataTransfer?.getData('sourceFolder');
      let targetFolder = folder.path;

      if (sourceFolder === '') {
        sourceFolder = 'root';
      }

      if (targetFolder === '') {
        targetFolder = 'root';
      }

      if (sourceFolder === targetFolder) {
        notify({ summary: me.t.ErrorMovingFile, detail: this.$props.t.SameFolderMessage, severity: SeverityLevel.Error });
        return;
      }

      moveAssetsState = {
        fileNames: fileNames,
        sourceFolder: sourceFolder,
        targetFolder: targetFolder
      };

      const uVfm = useVfm();

      uVfm.open(this.getModalName('file', 'move'));
    },
    getFolderModalName: function (action: string, folder: IFileStoreEntry) {
      debug(folder)
      return action + "-folder-" + folder.name;
    },
    openFolderModal: async function (action: string, folder: IFileStoreEntry) {
      const uVfm = useVfm();
      const modalName = this.getFolderModalName(action, folder)
      debug('OpenFolderModal', modalName)
      uVfm.open(modalName);

      await nextTick();

      this.emitter.emit('resetModalFolderAction');
    },
    confirmFolderModal: function (modalName: string, confirmAction: IConfirmFolderActionViewModel) {
      const uVfm = useVfm();
      debug('confirmFolderModal confirmAction', confirmAction)
      uVfm.close(this.getFolderModalName(modalName, confirmAction.folder));

      if (confirmAction.action == FolderAction.Create && confirmAction.inputValue) {
        this.createFolder({ name: confirmAction.inputValue, path: confirmAction.inputValue, isDirectory: true });
      }
      else if (confirmAction.action == FolderAction.Delete) {
        this.deleteFolder(confirmAction.folder);
      }
    },
    getModalName: function (name: String, action: String) {
      return action + "-folder-" + name;
    },
    openModal: function (file: String, action: String) {
      const uVfm = useVfm();
      uVfm.open(this.getModalName(file, action));
    },
    confirm: function (file: String, action: String) {
      const me = this;
      const uVfm = useVfm();

      if (action == "move") {
        if (moveAssetsState.fileNames.length > 0) {
          me.emitter.emit('fileListMove', moveAssetsState);
        }

        uVfm.close(this.getModalName('file', action));
        moveAssetsState = {};
      }
    },
  }
});
</script>
