<template>
  <div class="fileApp" v-on:dragover="handleScrollWhileDrag">
    <div class="ma-alert ma-alert-danger message-warning" v-if="errors.length > 0">
      <ul>
        <li v-for="(e, i) in errors" :key="i">{{ e }}</li>
      </ul>
    </div>
    <div id="customdropzone">
      <h3>{{ t.DropHere }}</h3>
      <p>{{ t.DropTitle }}</p>
    </div>
    <div v-show="isLoading" class="file-loading-overlay">
      <div class="spinner">
        <div class="loader">
          <svg class="circular" viewBox="25 25 50 50">
            <circle class="track" cx="50" cy="50" r="20" fill="none" stroke-width="4" />
            <circle class="path" cx="50" cy="50" r="20" fill="none" stroke-width="4" />
          </svg>
        </div>
      </div>
    </div>
    <div v-show="!isLoading" id="fileContainer" class="tw:items-stretch">
      <div id="navigationApp" class="file-container-navigation tw:m-0 tw:p-0" v-cloak>
        <FolderTree />
      </div>
      <div id="fileContainerMain" v-cloak>
        <div class="file-container-top-bar">
          <nav id="breadcrumb" class="tw:flex tw:justify-end tw:items-center">
            <div class="breadcrumb-path tw:px-3">
              <span v-for="(breadcrumb, i) in breadcrumbs" :key="breadcrumb.directoryPath" v-cloak
                class="breadcrumb-item">
                <a :href="breadcrumbs.length - i == 1 ? 'javascript:void(0)' : '#'"
                  v-on:click="clickBreadCrumb(breadcrumb)">
                  {{ breadcrumb.name }}
                </a>
              </span>
            </div>
            <div class="tw:relative tw:px-3">
              <a href="javascript:void(0)" class="storage-info-btn" :title="t.StorageInfo || 'Storage Info'"
                @click="toggleStoragePopover">
                <fa-icon icon="fa-solid fa-hard-drive"></fa-icon>
              </a>
              <div v-if="showStoragePopover" class="storage-popover">
                <div class="storage-popover-content" v-if="storageLoading">
                  <span>{{ t.Loading || 'Loading...' }}</span>
                </div>
                <div class="storage-popover-content" v-else-if="storageInfo">
                  <div class="storage-popover-row">
                    <span class="storage-popover-label">{{ t.AvailableStorage || 'Available Storage' }}</span>
                    <span>{{ storageInfo.text }}</span>
                  </div>
                </div>
                <div class="storage-popover-content" v-else>
                  <span>{{ t.Unavailable || 'Unavailable' }}</span>
                </div>
              </div>
            </div>
            <div class="tw:relative tw:px-3">
              <a href="javascript:void(0)" class="settings-btn" :title="t.Settings || 'Settings'"
                @click="toggleSettingsPopover">
                <fa-icon icon="fa-solid fa-gear"></fa-icon>
              </a>
              <div v-if="showSettingsPopover" class="settings-popover">
                <div class="settings-popover-content">
                  <div class="settings-popover-row">
                    <label class="settings-popover-label" for="settings-page-size">{{ t.ItemsPerPage || 'Items per page' }}</label>
                    <select id="settings-page-size" class="ma-input settings-select" v-model.number="pageSize">
                      <option :value="10">10</option>
                      <option :value="20">20</option>
                      <option :value="50">50</option>
                      <option :value="100">100</option>
                    </select>
                  </div>
                  <div class="settings-popover-row">
                    <span class="settings-popover-label">{{ t.ThumbnailSize || 'Thumbnail size' }}</span>
                    <div class="ma-btn-group">
                      <button type="button" class="ma-btn ma-btn-sm" :class="{ 'ma-btn-primary': !largeThumbs, 'ma-btn-light': largeThumbs }"
                        @click="largeThumbs = false">
                        {{ t.Normal || 'Normal' }}
                      </button>
                      <button type="button" class="ma-btn ma-btn-sm" :class="{ 'ma-btn-primary': largeThumbs, 'ma-btn-light': !largeThumbs }"
                        @click="largeThumbs = true">
                        {{ t.Large || 'Large' }}
                      </button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </nav>
          <div class="action-bar tw:py-3 tw:px-4 tw:flex tw:flex-wrap tw:gap-y-3">
            <div class="tw:mr-auto">
              <div v-show="canManage" class="tw:flex tw:items-center tw:flex-wrap">
                <div class="ma-btn-group tw:mr-2">
                  <label :title="t.UploadFiles" for="fileupload" class="ma-btn ma-btn-primary fileinput-button upload-button">
                    <input id="fileupload" type="file" name="files" multiple />
                    <fa-icon icon="fa-solid fa-cloud-arrow-up"></fa-icon>
                    {{ t.UploadFiles }}
                  </label>
                </div>
                <a :title="t.DeleteAll" href="javascript:void(0)" class="ma-btn ma-btn-light tw:mr-2"
                  @click="() => deleteSelectedFiles()" :class="{ 'is-disabled': selectedFiles.length < 1 }">
                  <fa-icon icon="fa-solid fa-trash"></fa-icon>
                  <span class="ma-badge tw:ml-1" v-show="selectedFiles.length > 0">{{ selectedFiles.length
                    }}</span>
                </a>
                <a :title="t.DownloadAll" href="javascript:void(0)" class="ma-btn ma-btn-light tw:mr-2"
                  @click="() => downloadSelectedFiles()"
                  :class="{ 'is-disabled': selectedFiles.length < 1 || isDownloading }">
                  <fa-icon icon="fa-solid fa-download"></fa-icon>
                  <span class="ma-badge tw:ml-1" v-show="selectedFiles.length > 0">{{ selectedFiles.length
                    }}</span>
                </a>
                <a :title="gridView ? (t.TableView ?? 'Table view') : (t.GridView ?? 'Grid view')"
                  href="javascript:void(0)" class="ma-btn ma-btn-light tw:mr-2"
                  @click="gridView = !gridView">
                  <fa-icon :icon="gridView ? 'fa-solid fa-list' : 'fa-solid fa-grip'"></fa-icon>
                </a>
              </div>
            </div>
            <div>
              <div class="file-filter">
                <div class="ma-input-group">
                  <fa-icon icon="fa-solid fa-filter icon-inside-input"></fa-icon>
                  <input type="text" id="file-filter-input" v-model="fileFilter" class="ma-input input-filter"
                    :placeholder="t.Filter" :aria-label="t.Filter" />
                  <button id="clear-file-filter-button" class="ma-btn ma-btn-outline" :disabled="fileFilter == ''"
                    v-on:click="fileFilter = ''">
                    <fa-icon icon="fa-solid fa-times"></fa-icon>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
        <div v-if="assetsStore.length > 0" class="file-container-middle tw:p-3">
          <router-view :key="selectedDirectory.directoryPath" :is-selected-all="isSelectedAll"
            :filtered-file-items="itemsInPage" :selected-files="selectedFiles"
            v-show="!isLoadingFiles && filteredFileItems.length > 0 && !gridView">
          </router-view>
          <ol class="file-items-grid" :class="{ 'large-thumbs': largeThumbs }" v-show="!isLoadingFiles && filteredFileItems.length > 0 && gridView">
            <li v-for="file in itemsInPage" :key="file.filePath"
              :class="{ selected: isFileSelected(file) }"
              class="card" draggable="true"
              @dragstart="dragFileStart(file, $event)">
              <div class="thumb-container" @click.stop="toggleFile(file)">
                <img v-if="isImage(file)" :src="buildMediaUrl(file.url!, thumbSize)" :alt="file.name" loading="lazy" />
                <div v-else class="file-icon-placeholder">
                  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path
                      d="M10.28 4.46553H13.4658C13.4889 4.4658 13.5118 4.46145 13.5332 4.45274C13.5545 4.44402 13.574 4.43113 13.5903 4.4148C13.6066 4.39848 13.6195 4.37906 13.6282 4.35768C13.6369 4.3363 13.6413 4.3134 13.641 4.29031C13.6417 4.13738 13.6084 3.9862 13.5437 3.84764C13.479 3.70907 13.3844 3.58656 13.2667 3.48889L10.5946 1.26233C10.3921 1.09433 10.137 1.00273 9.87384 1.00348C9.83983 1.00342 9.80614 1.01007 9.7747 1.02305C9.74327 1.03604 9.71471 1.0551 9.69066 1.07915C9.66661 1.1032 9.64755 1.13176 9.63456 1.1632C9.62158 1.19463 9.61493 1.22832 9.61499 1.26233V3.801C9.61499 3.88831 9.6322 3.97476 9.66562 4.05542C9.69905 4.13607 9.74804 4.20935 9.8098 4.27107C9.87156 4.33278 9.94488 4.38172 10.0256 4.41509C10.1062 4.44845 10.1927 4.46559 10.28 4.46553Z"
                      class="file-icon-path" />
                    <path
                      d="M8.70453 3.8V1H4.12C3.82324 1.00092 3.5389 1.11921 3.32906 1.32906C3.11921 1.5389 3.00092 1.82324 3 2.12V13.88C3.00092 14.1768 3.11921 14.4611 3.32906 14.6709C3.5389 14.8808 3.82324 14.9991 4.12 15H12.52C12.8168 14.9991 13.1011 14.8808 13.3109 14.6709C13.5208 14.4611 13.6391 14.1768 13.64 13.88V5.37497H10.28C9.86241 5.37444 9.46206 5.20836 9.16673 4.91312C8.87141 4.61788 8.70519 4.21759 8.70453 3.8Z"
                      class="file-icon-path" />
                  </svg>
                  <span class="tw:uppercase file-ext tw:text-white">{{ getFileExtension(file.name) }}</span>
                </div>
              </div>
              <div class="card-footer">
                <span class="tw:truncate tw:text-sm tw:grow" :title="file.name">{{ file.name }}</span>
                <FileMenu :file-item="file" @click.stop></FileMenu>
              </div>
            </li>
          </ol>
          <div
            v-show="isLoadingFiles"
            class="empty-folder-state file-panel-loading">
            <div class="spinner">
              <div class="loader">
                <svg class="circular" viewBox="25 25 50 50">
                  <circle class="track" cx="50" cy="50" r="20" fill="none" stroke-width="4" />
                  <circle class="path" cx="50" cy="50" r="20" fill="none" stroke-width="4" />
                </svg>
              </div>
            </div>
          </div>
          <div
            v-show="!isLoadingFiles && fileItems.length > 0 && filteredFileItems.length < 1"
            class="empty-folder-state">
            <svg width="64" height="64" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.5 13.25V2.75C1.5 2.33579 1.83579 2 2.25 2H5.58579C5.78471 2 5.97547 2.07902 6.11612 2.21967L7.38388 3.48744C7.52453 3.62808 7.71529 3.70711 7.91421 3.70711H13.75C14.1642 3.70711 14.5 4.04289 14.5 4.45711V13.25C14.5 13.6642 14.1642 14 13.75 14H2.25C1.83579 14 1.5 13.6642 1.5 13.25Z" stroke="currentColor" stroke-width="0.8" fill="none"/>
            </svg>
            <span class="tw:mt-3">{{ t.FolderFilterEmpty }}</span>
          </div>
          <div
            v-show="!isLoadingFiles && fileItems.length < 1"
            class="empty-folder-state">
            <svg width="64" height="64" viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
              <path d="M1.5 13.25V2.75C1.5 2.33579 1.83579 2 2.25 2H5.58579C5.78471 2 5.97547 2.07902 6.11612 2.21967L7.38388 3.48744C7.52453 3.62808 7.71529 3.70711 7.91421 3.70711H13.75C14.1642 3.70711 14.5 4.04289 14.5 4.45711V13.25C14.5 13.6642 14.1642 14 13.75 14H2.25C1.83579 14 1.5 13.6642 1.5 13.25Z" stroke="currentColor" stroke-width="0.8" fill="none"/>
            </svg>
            <span class="tw:mt-3">{{ t.FolderEmpty }}</span>
          </div>
        </div>
        <div v-show="filteredFileItems.length > 0" class="file-container-footer tw:p-3 tw:pb-0">
          <pager :source-items="filteredFileItems" :page-size="pageSize" :key="selectedDirectory.directoryPath"></pager>
        </div>
      </div>
    </div>
    <notification-toast />
    <upload-toast :tus-enabled="tusEnabled === 'true'" />
    <ModalsContainer />
  </div>
</template>

<style>
@import "./assets/css/file.css";
</style>

<script setup lang="ts">
import { ref, watch, defineProps } from "vue";
import FolderTree from "./components/FolderTree.vue";
import UploadToast from "./components/UploadToast.vue";
import NotificationToast from "./components/NotificationToast.vue";
import pager from "./components/PagerComponent.vue";
import { IFileLibraryItemDto, FileAction, IModalFileEvent } from "@bloom/media/interfaces";
import { ModalsContainer } from 'vue-final-modal';
import { v4 as uuidv4 } from 'uuid';

import { RouterView } from "vue-router";
import { useFileUpload } from "./services/UppyFileUpload";
import { useConfirmModal } from "./services/ConfirmModalService";
import { useGlobals } from "./services/Globals";
import { usePermissions } from "./services/Permissions";
import { useLocalStorage } from "./services/LocalStorage";
import { useFileLibraryManager } from "./services/FileLibraryManager";
import { useEventBusService } from "./services/EventBusService";
import { useSignalR } from "./services/SignalR";
import { useEventBus } from "./services/UseEventBus";
import { useRouterService } from "./services/RouterService";
import { getTranslations, setTranslations } from "@bloom/helpers/localizations";
import { useFileListFiltering } from "./composables/useFileListFiltering";
import { useBreadcrumbs } from "./composables/useBreadcrumbs";
import { useStoragePopover } from "./composables/useStoragePopover";
import { downloadSelectedFiles, getFileExtension, isFileSelected, buildMediaUrl } from "./services/Utils";
import FileMenu from "./components/FileMenu.vue";

const props = defineProps({
  basePath: {
    type: String,
    required: true,
  },
  translations: {
    type: String,
    required: true,
  },
  uploadFilesUrl: {
    type: String,
    required: true,
  },
  maxUploadChunkSize: {
    type: Number,
    required: true,
  },
  maxFileSize: {
    type: Number,
    default: 0,
  },
  allowedExtensions: {
    type: String,
    default: "",
  },
  tusEnabled: {
    type: String,
    default: "false",
  },
  tusEndpointUrl: {
    type: String,
    default: "",
  },
  tusFileInfoUrl: {
    type: String,
    default: "",
  },
  signalrEnabled: {
    type: String,
    default: "false",
  },
  debugEnabled: {
    type: String,
    default: "false",
  },
})

const {
  isSelectedAll,
  errors,
  isLoading,
  fileFilter,
  itemsInPage,
  assetsStore,
  selectedFiles,
  selectedDirectory,
  fileItems,
  isDownloading,
  isLoadingFiles,
  setIsLoading,
  setBasePath,
  setSelectedFiles,
  setSelectedAll,
  setIsDownloading,
  setUploadFilesUrl,
} = useGlobals();

setBasePath(props.basePath);
setIsDownloading(false);

const translations = getTranslations();
if (props.translations) {
  try {
    setTranslations(typeof props.translations === "string" ? JSON.parse(props.translations) : props.translations);
  } catch (e) {
    console.warn("Failed to parse media-app translations:", e);
  }
}
const t = translations;

const { on, emit } = useEventBus();

setIsLoading(true);

const { getFileLibraryStoreAsync } = useFileLibraryManager();
getFileLibraryStoreAsync().then(() => {
  setIsLoading(false);
  setLocalStorage();

  useEventBusService();
  useRouterService();

  on("SelectAll", () => {
    selectAll();
  });
});

const { setLocalStorage, gridView, pageSize, largeThumbs } = useLocalStorage();

const thumbSize = 480;

const showSettingsPopover = ref(false);
const toggleSettingsPopover = () => {
  showSettingsPopover.value = !showSettingsPopover.value;
};
const handleSettingsClickOutside = (e: MouseEvent) => {
  if (showSettingsPopover.value &&
    !(e.target as HTMLElement)?.closest('.settings-btn, .settings-popover')) {
    showSettingsPopover.value = false;
  }
};
import { onMounted, onUnmounted } from "vue";
onMounted(() => document.addEventListener('click', handleSettingsClickOutside));
onUnmounted(() => document.removeEventListener('click', handleSettingsClickOutside));
const { canManage } = usePermissions();
if (props.signalrEnabled === "true") {
  useSignalR();
}

const { storageInfo, showStoragePopover, storageLoading, toggleStoragePopover } = useStoragePopover(props.basePath);

setUploadFilesUrl(props.uploadFilesUrl);

const { showConfirmModal } = useConfirmModal();

useFileUpload({
  maxUploadChunkSize: props.maxUploadChunkSize,
  maxFileSize: props.maxFileSize,
  allowedExtensions: props.allowedExtensions,
  debugEnabled: props.debugEnabled === "true",
  tusEnabled: props.tusEnabled === "true",
  tusEndpointUrl: props.tusEndpointUrl,
  tusFileInfoUrl: props.tusFileInfoUrl,
});

const imageExtensions = new Set(["jpg", "jpeg", "png", "gif", "webp", "svg", "bmp", "ico", "avif"]);

const isImage = (file: IFileLibraryItemDto): boolean => {
  if (file.mime?.startsWith("image/")) return true;
  const ext = getFileExtension(file.name)?.toLowerCase() ?? "";
  return imageExtensions.has(ext);
};

const toggleFile = (file: IFileLibraryItemDto) => {
  emit("FileSelectReq", file);
};

const dragFileStart = (file: IFileLibraryItemDto, e: DragEvent) => {
  emit("FileDragReq", { file, e });
};

const { filteredFileItems } = useFileListFiltering();

const selectAll = () => {
  if (isSelectedAll.value) {
    setSelectedFiles([]);
    setSelectedAll(false);
  } else {
    selectedFiles.value = [];

    for (let i = 0; i < filteredFileItems.value.length; i++) {
      selectedFiles.value.push(filteredFileItems.value[i]);
    }

    setSelectedAll(true);
  }
};

const { breadcrumbs } = useBreadcrumbs();

const clickBreadCrumb = (breadcrumb: IFileLibraryItemDto) => {
  emit("DirSelected", breadcrumb as IFileLibraryItemDto);
}

const deleteSelectedFiles = () => {
  if (selectedFiles.value === null || selectedFiles.value.length === 0) {
    return;
  }

  const modal = { files: selectedFiles.value, modalName: 'delete', uuid: uuidv4(), isEdit: true, modalTitle: t.DeleteFileTitle, action: FileAction.Delete } as IModalFileEvent;
  showConfirmModal(modal);
}

const handleScrollWhileDrag = (e: { clientY: number; }) => {
  if (e.clientY < 150) {
    window.scrollBy(0, -10);
  }

  if (e.clientY > window.innerHeight - 100) {
    window.scrollBy(0, 10);
  }
}
</script>
