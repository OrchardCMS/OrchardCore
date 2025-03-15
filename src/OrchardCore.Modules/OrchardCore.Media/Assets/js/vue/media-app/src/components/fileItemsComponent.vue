<!-- 
    <file-items> component 
-->

<template>
  <div class="file-items-table m-0">
    <div class="table-head">
      <div class="table-row header-row">
        <div scope="col" class="table-cell thumbnail-column">
          <div class="form-check">
            <input :checked="isSelectedAll" class="form-check-input cursor-pointer" type="checkbox" value=""
              id="select-all" name="select-all" role="checkbox" :title="isSelectedAll ? t.SelectNone : t.SelectAll"
              v-on:click="selectAll">
          </div>
        </div>
        <div class="table-cell" scope="col" v-on:click="changeSort('name')">
          {{ t.NameHeader }}
          <sort-indicator colname="name" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
        </div>
        <div class="table-cell" scope="col" v-on:click="changeSort('lastModify')">
          {{ t.LastModifyHeader }}
          <sort-indicator colname="lastModify" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
        </div>
        <div class="table-cell" scope="col" v-on:click="changeSort('size')">
          <span class="optional-col">
            {{ t.SizeHeader }}
            <sort-indicator colname="size" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
          </span>
        </div>
        <div class="table-cell text-center">{{ t.Status }}</div>
        <div class="table-cell"></div>
      </div>
    </div>
    <div class="table-body">
      <div class="table-row file-item" v-for="file in filteredFileItems" draggable="true"
        v-on:dragstart="dragStart(file, $event)" :key="file.name">
        <div class="table-cell text-center">
          <div class="form-check">
            <input :checked="isFileSelected(file)" class="form-check-input cursor-pointer" type="checkbox" value=""
              name="select-file" role="checkbox" v-on:click.stop="toggleSelectionOfFile(file)">
          </div>
        </div>
        <div class="table-cell">
          <div class="flex align-items-center">
            <div class="thumbnail-column">
              <div class="img-wrapper">
                <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                  <path
                    d="M10.28 4.46553H13.4658C13.4889 4.4658 13.5118 4.46145 13.5332 4.45274C13.5545 4.44402 13.574 4.43113 13.5903 4.4148C13.6066 4.39848 13.6195 4.37906 13.6282 4.35768C13.6369 4.3363 13.6413 4.3134 13.641 4.29031C13.6417 4.13738 13.6084 3.9862 13.5437 3.84764C13.479 3.70907 13.3844 3.58656 13.2667 3.48889L10.5946 1.26233C10.3921 1.09433 10.137 1.00273 9.87384 1.00348C9.83983 1.00342 9.80614 1.01007 9.7747 1.02305C9.74327 1.03604 9.71471 1.0551 9.69066 1.07915C9.66661 1.1032 9.64755 1.13176 9.63456 1.1632C9.62158 1.19463 9.61493 1.22832 9.61499 1.26233V3.801C9.61499 3.88831 9.6322 3.97476 9.66562 4.05542C9.69905 4.13607 9.74804 4.20935 9.8098 4.27107C9.87156 4.33278 9.94488 4.38172 10.0256 4.41509C10.1062 4.44845 10.1927 4.46559 10.28 4.46553Z"
                    fill="#2D2D2D" />
                  <path
                    d="M8.70453 3.8V1H4.12C3.82324 1.00092 3.5389 1.11921 3.32906 1.32906C3.11921 1.5389 3.00092 1.82324 3 2.12V13.88C3.00092 14.1768 3.11921 14.4611 3.32906 14.6709C3.5389 14.8808 3.82324 14.9991 4.12 15H12.52C12.8168 14.9991 13.1011 14.8808 13.3109 14.6709C13.5208 14.4611 13.6391 14.1768 13.64 13.88V5.37497H10.28C9.86241 5.37444 9.46206 5.20836 9.16673 4.91312C8.87141 4.61788 8.70519 4.21759 8.70453 3.8Z"
                    fill="#2D2D2D" />
                </svg>
                <span class="uppercase file-ext text-white">{{ getFileExtension(file.name) }}</span>
              </div>
            </div>
            <div class="file-name-cell">
              <span class="break-word"> {{ file.name }} </span>
            </div>
          </div>
        </div>
        <div class="table-cell">
          <div class="text-col"> {{ printDateTime(file.lastModify ?? '') }} </div>
        </div>
        <div class="table-cell">
          <div class="text-col optional-col"> {{ printSize(file) }}</div>
        </div>
        <div class="table-cell text-center"><span class="dot"></span></div>
        <div class="table-cell text-right text-end">
          <a href="javascript:void(0)" class="btn btn-link btn-sm action-button px-4 py-3"
            @click="() => openFileModal('file-action', [file])">
            <fa-icon icon="fas fa-ellipsis-v" size="xl"></fa-icon>
            <ModalFileActionConfirm :t="t" :modal-name="getFileModalName('file-action', [file])" :files="[file]"
              :title="t.ActionFileTitle" @closed="closeFileModal('file-action', file)"
              @confirm="(viewModel: IConfirmFileActionViewModel) => confirmFileModal('file-action', viewModel)">
              <p>{{ t.ActionFileMessage }}</p>
              <template #submit>{{ t.Ok }}</template>
            </ModalFileActionConfirm>
          </a>
        </div>
      </div>
    </div>
    <ModalsContainer />
  </div>
</template>

<script lang="ts">
import { defineComponent, PropType, nextTick } from 'vue'
import dbg from 'debug';
import { useVfm, useModal, ModalsContainer, VueFinalModal } from 'vue-final-modal'
import { IFileStoreEntry } from "../services/MediaApiClient";
import ModalFileActionConfirm from './ModalFileActionConfirm.vue'
import SortIndicatorComponent from './sortIndicatorComponent.vue';
import { FileAction, IConfirmFileActionViewModel, IConfirmFileActionEntry, IModalFileEvent } from '../interfaces/interfaces';
import { humanFileSize } from '../services/Utils'
import { SeverityLevel } from '../interfaces/interfaces';
import { notify } from "../services/notifier";

const debug = dbg("aptix:file-app");

export default defineComponent({
  components: {
    ModalFileActionConfirm: ModalFileActionConfirm,
    SortIndicator: SortIndicatorComponent,
    ModalsContainer: ModalsContainer
  },
  name: "file-items",
  props: {
    baseHost: {
      type: String,
      required: false
    },
    sortBy: {
      type: String,
      required: true
    },
    sortAsc: Boolean,
    filteredFileItems: Array as PropType<Array<IFileStoreEntry>>,
    selectedFiles: Array,
    thumbSize: {
      type: Number,
      required: true
    },
    isSelectedAll: {
      type: Boolean,
      required: true
    },
    t: {
      type: Object,
      required: true,
    }
  },
  created: function () {
    const me = this;

    this.emitter.on("openFilesModal", (IModalFileEvent: IModalFileEvent) => {
      const { open, close, destroy, options, patchOptions } = useModal({
        // Open the modal or not when the modal was created, the default value is `false`.
        defaultModelValue: false,
        /**
         * If set `keepAlive` to `true`: 
         * 1. The `displayDirective` will be set to `show` by default. 
         * 2. The modal component will not be removed after the modal closed until you manually execute `destroy()`. 
         */
        keepAlive: false,
        // `component` is optional and the default value is `<VueFinalModal>`.
        component: ModalFileActionConfirm,
        slots: {
          default: me.$props.t.BulkActionFilesMessage,
          submit: me.$props.t.Ok,
        },
        attrs: {
          // Bind props to the modal component (ModalFileActionConfirm in this case).
          title: me.$props.t.BulkActionFilesTitle,
          modalName: IModalFileEvent.uuid, // Allows multiple modals to be opened at once for multi-file uploads.
          files: IModalFileEvent.files,
          t: me.$props.t,
          // Bind events to the modal component (ModalFileActionConfirm in this case).
          // Any custom events can be listened for when prefixed with "on", e.g. "onEventName".
          onConfirm(actions: IConfirmFileActionViewModel) {
            debug('onConfirm', actions)
            me.confirmFileModal(IModalFileEvent.uuid, actions); // Here the modal name passed is not used
            destroy();
          },
          onClosed() {
            debug('onClosed event');
            destroy(); // destroy modal instance
          },
        }
      })

      debug('Open File Modal Event', IModalFileEvent)
      open().then(() => { /* Do something after modal opened */ })
    });
  },
  methods: {
    selectAll: function () {
      this.emitter.emit('select-all');
    },
    printSize: function (file: IFileStoreEntry) {
      return humanFileSize(file.size ?? 0, true, 2);
    },
    isFileSelected: function (file: IFileStoreEntry) {
      let result = this.selectedFiles?.some(function (element: any) {
        if (file.url) {
          return element.url.toLowerCase() === file.url.toLowerCase();
        }
      });
      return result;
    },
    buildFileUrl: function (url: string | string[], thumbSize: Number) {
      let path = url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize + '&rmode=crop';

      return this.$props.baseHost ? this.$props.baseHost + path : path;
    },
    getFileExtension: function (fileName: string) {
      return fileName.split('.').pop();
    },
    changeSort: function (newSort: any) {
      this.emitter.emit('sortChangeRequested', newSort);
    },
    toggleSelectionOfFile: function (file: IFileStoreEntry) {
      this.emitter.emit('fileToggleRequested', file);
    },
    renameFile: function (file: IFileStoreEntry, newName: any) {
      this.emitter.emit('renameFileRequested', { file, newName });
    },
    deleteFile: function (file: IFileStoreEntry) {
      this.emitter.emit('deleteFileRequested', file);
    },
    dragStart: function (file: IFileStoreEntry, e: DragEvent) {
      this.emitter.emit('fileDragStartRequested', { file: file, e: e });
    },
    printDateTime: function (datemillis: string | number | Date) {
      if (datemillis != "") {
        let d = new Date(datemillis);

        return d.toLocaleString();
      }
    },
    getFileModalName: function (action: string, files: IFileStoreEntry[]) {
      if (action == "files-action") {
        return action + "-files";
      }
      else {
        return action + "-file-" + files[0].name;
      }
    },
    openFileModal: async function (action: string, files: IFileStoreEntry[]) {
      const uVfm = useVfm();

      const modalName = this.getFileModalName(action, files)
      debug('OpenFileModal', modalName)
      uVfm.open(modalName);

      await nextTick();

      this.emitter.emit('resetModalFileAction');
    },
    closeFileModal: function (modalName: string, file: IFileStoreEntry) {
      const uVfm = useVfm();
      uVfm.close(this.getFileModalName(modalName, [file]));
    },
    confirmFileModal: function (modalName: string, confirmActions: IConfirmFileActionViewModel) {
      const me = this;
      const uVfm = useVfm();

      if (confirmActions.actionEntries.length == 1) {
        uVfm.close(this.getFileModalName(modalName, [confirmActions.actionEntries[0].file]));
      }

      confirmActions.actionEntries.forEach((confirmAction: IConfirmFileActionEntry) => {
        if (confirmAction.action == FileAction.Delete) {
          this.deleteFile(confirmAction.file);
        }
        else if (confirmAction.action == FileAction.Rename) {
          debug("Confirm file rename:", confirmAction.inputValue);
          if (confirmAction.file.name != confirmAction.inputValue) {
            this.renameFile(confirmAction.file, confirmAction.inputValue);
          }
          else {
            notify({ summary: me.$props.t.ErrorValidateConnection, detail: me.$props.t.RenamingFileWarning, severity: SeverityLevel.Warn });
          }
        }
        else if (confirmAction.action == FileAction.Download) {
          if (confirmAction.file.url && confirmAction.file.name) {
            this.downloadFile(confirmAction.file);
          }
        }
      });
    },
    downloadFile: function (file: IFileStoreEntry) {
      if (file.url) {
        fetch(file.url, { method: "get", mode: "no-cors", referrerPolicy: "no-referrer" })
          .then((res) => res.blob())
          .then((res) => {
            const aElement = document.createElement("a");
            aElement.setAttribute("download", file.name);
            const href = URL.createObjectURL(res);
            aElement.href = href;
            aElement.setAttribute("target", "_blank");
            aElement.click();
            URL.revokeObjectURL(href);
          });
      }
    }
  }
});
</script>
