<template>
  <div class="fileApp" v-on:dragover="handleScrollWhileDrag">
    <div class="alert alert-danger message-warning" v-if="errors.length > 0">
      <ul>
        <li v-for="(e, i) in errors" :key="i">{{ e }}</li>
      </ul>
    </div>
    <div id="customdropzone">
      <h3>{{ t.DropHere }}</h3>
      <p>{{ t.DropTitle }}</p>
    </div>
    <div id="fileContainer" class="align-items-stretch">
      <div id="navigationApp" class="file-container-navigation m-0 p-0" v-cloak>
        <ol id="folder-tree">
          <folder v-if="!isLoading" :hierarchical-directories="hierarchicalDirectories" :level="1"></folder>
        </ol>
      </div>
      <div id="fileContainerMain" v-cloak>
        <div class="file-container-top-bar">
          <nav id="breadcrumb" class="d-flex justify-content-end align-items-center">
            <div class="breadcrumb-path px-3">
              <span v-for="(breadcrumb, i) in breadcrumbs" :key="breadcrumb.directoryPath" v-cloak
                class="breadcrumb-item">
                <a :href="breadcrumbs.length - i == 1 ? 'javascript:void(0)' : '#'"
                  v-on:click="clickBreadCrumb(breadcrumb)">
                  {{ breadcrumb.name }}
                </a>
              </span>
            </div>
          </nav>
          <nav class="nav action-bar p-3 flex">
            <div class="me-auto">
              <div v-show="canManage">
                <div class="btn-group btn-group me-2">
                  <label :title="t.UploadFiles" for="fileupload" class="btn btn-primary fileinput-button upload-button">
                    <input id="fileupload" type="file" name="files" multiple />
                    <fa-icon icon="fa-solid fa-cloud-arrow-up"></fa-icon>
                    {{ t.UploadFiles }}
                  </label>
                </div>
                <a :title="t.DeleteAll" href="javascript:void(0)" class="btn btn-light me-2"
                  @click="() => deleteSelectedFiles()" :class="{ disabled: selectedFiles.length < 1 }">
                  <fa-icon icon="fa-solid fa-trash"></fa-icon>
                  <span class="badge rounded-pill ml-1" v-show="selectedFiles.length > 0">{{ selectedFiles.length
                    }}</span>
                </a>
                <a :title="t.DownloadAll" href="javascript:void(0)" class="btn btn-light me-2"
                  @click="() => downloadSelectedFiles()"
                  :class="{ disabled: selectedFiles.length < 1 || isDownloading }">
                  <fa-icon icon="fa-solid fa-download"></fa-icon>
                  <span class="badge rounded-pill ml-1" v-show="selectedFiles.length > 0">{{ selectedFiles.length
                    }}</span>
                </a>
              </div>
            </div>
            <div class="nav-item mx-2 mt-3 md:mt-0">
              <div class="file-filter">
                <div class="input-group input-group">
                  <fa-icon icon="fa-solid fa-filter icon-inside-input"></fa-icon>
                  <input type="text" id="file-filter-input" v-model="fileFilter" class="form-control input-filter"
                    :placeholder="t.Filter" :aria-label="t.Filter" />
                  <button id="clear-file-filter-button" class="btn btn-outline-secondary" :disabled="fileFilter == ''"
                    v-on:click="fileFilter = ''">
                    <fa-icon icon="fa-solid fa-times"></fa-icon>
                  </button>
                </div>
              </div>
            </div>
          </nav>
        </div>
        <div v-if="assetsStore.length > 0" class="file-container-middle p-3">
          <router-view :key="selectedDirectory.directoryPath" :is-selected-all="isSelectedAll"
            :filtered-file-items="itemsInPage" :selected-files="selectedFiles"
            v-show="filteredFileItems.length > 0 && !gridView">
          </router-view>
          <div
            v-show="assetsStore.filter(x => x.isDirectory == false && x.directoryPath == selectedDirectory.directoryPath).length > 0 && filteredFileItems.length < 1"
            class="p-message p-component p-message-info">
            <div class="p-message-wrapper">
              <div class="p-message-text">{{ t.FolderFilterEmpty }}</div>
            </div>
          </div>
          <div
            v-show="assetsStore.filter(x => x.isDirectory == false && x.directoryPath == selectedDirectory.directoryPath).length < 1"
            class="p-message p-component p-message-info">
            <div class="p-message-wrapper">
              <div class="p-message-text">{{ t.FolderEmpty }}</div>
            </div>
          </div>
        </div>
        <div v-show="filteredFileItems.length > 0" class="file-container-footer p-3 pb-0">
          <pager :source-items="filteredFileItems" :key="selectedDirectory.directoryPath"></pager>
        </div>
      </div>
    </div>
    <notification-toast />
    <upload-toast />
    <ModalsContainer />
  </div>
</template>

<style lang="scss">
@import "./assets/scss/file.scss";
</style>

<script setup lang="ts">
import { watch, computed, defineProps } from "vue";
import folder from "./components/FolderComponent.vue";
import UploadToast from "./components/UploadToast.vue";
import NotificationToast from "./components/NotificationToast.vue";
import pager from "./components/PagerComponent.vue";
import { IFileLibraryItemDto } from "./interfaces/interfaces";
import { FileAction, IModalFileEvent } from "./interfaces/interfaces";
import { ModalsContainer } from 'vue-final-modal';
import { v4 as uuidv4 } from 'uuid';

import { RouterView } from "vue-router";
import { useFileUpload } from "./services/UppyFileUpload";
import { useConfirmModal } from "./services/ConfirmModalService";
import { useGlobals } from "./services/Globals";
import { usePermissions } from "./services/Permissions";
import { useHierarchicalTreeBuilder } from "./services/HierarchicalTreeBuilder";
import { useLocalStorage } from "./services/LocalStorage";
import { useFileLibraryManager } from "./services/FileLibraryManager";
import { useEventBusService } from "./services/EventBusService";
import { useSignalR } from "./services/SignalR";
import { useEventBus } from "./services/UseEventBus";
import { useRouterService } from "./services/RouterService";
import { useLocalizations } from "./services/Localizations";
import { downloadSelectedFiles } from "./services/Utils";

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
})

const {
  sortAsc,
  sortBy,
  isSelectedAll,
  errors,
  isLoading,
  fileFilter,
  itemsInPage,
  assetsStore,
  selectedFiles,
  selectedDirectory,
  rootDirectory,
  hierarchicalDirectories,
  fileItems,
  isDownloading,
  setFileItems,
  setIsLoading,
  setBasePath,
  setSelectedFiles,
  setSelectedAll,
  setIsDownloading,
  setUploadFilesUrl,
} = useGlobals();

setBasePath(props.basePath);
setIsDownloading(false);

const { translations, setTranslations } = useLocalizations();
setTranslations(typeof props.translations === "string" ? JSON.parse(props.translations) : props.translations);
const t = translations.value;

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

const { setHierarchicalDirectories } = useHierarchicalTreeBuilder();
const { setLocalStorage, gridView } = useLocalStorage();
const { canManage } = usePermissions();
useSignalR();

setUploadFilesUrl(props.uploadFilesUrl);

const { showConfirmModal } = useConfirmModal();

useFileUpload({
  maxUploadChunkSize: props.maxUploadChunkSize,
});

const filteredFileItems = computed(() => {
  let filtered = fileItems.value.filter(function (item: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
    return item.name.toLowerCase().indexOf(fileFilter.value.toLowerCase()) > -1;
  });

  switch (sortBy.value) {
    case "size":
      filtered.sort(function (a: any, b: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        return sortAsc.value ? (a.size ?? 0) - (b.size ?? 0) : (b.size ?? 0) - (a.size ?? 0);
      });
      break;
    case "mime":
      filtered.sort(function (a: any, b: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        return sortAsc.value ? (a.mime ?? "").toLowerCase().localeCompare((b.mime ?? "").toLowerCase()) : (b.mime ?? "").toLowerCase().localeCompare((a.mime ?? "").toLowerCase());
      });
      break;
    case "lastModify":
      filtered.sort(function (a: any, b: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        return sortAsc.value ? new Date(a.lastModifiedUtc ?? 0).getTime() - new Date(b.lastModifiedUtc ?? 0).getTime() : new Date(b.lastModifiedUtc ?? 0).getTime() - new Date(a.lastModifiedUtc ?? 0).getTime();
      });
      break;
    default:
      filtered.sort(function (a: any, b: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        return sortAsc.value ? a.name.toLowerCase().localeCompare(b.name.toLowerCase()) : b.name.toLowerCase().localeCompare(a.name.toLowerCase());
      });
  }

  return filtered;
});

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

const breadcrumbs = computed((): IFileLibraryItemDto[] => {
  let result: IFileLibraryItemDto[] = [];

  if (assetsStore.value.length > 0 && selectedDirectory.value?.directoryPath) {
    result.push(rootDirectory.value);

    const directories = selectedDirectory.value.directoryPath.split("/");
    let directoryName = ""

    directories.forEach((directory, index) => {
      directoryName = index > 0 ? directoryName + "/" + directory : directory;
      const directoryFound = assetsStore.value.find(x => x.isDirectory && x.directoryPath == directoryName);

      if (directoryFound) {
        result.push(directoryFound)
      }
    });
  }

  return result;
});

watch(
  () => assetsStore.value,
  (newAssetsStore: IFileLibraryItemDto[]) => {
    const foundFileItems = Object.values(newAssetsStore).filter(x => (x.directoryPath == selectedDirectory.value.directoryPath) && x.isDirectory == false);
    setFileItems(foundFileItems);
    setHierarchicalDirectories(newAssetsStore);
  },
  { deep: true }
)

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
