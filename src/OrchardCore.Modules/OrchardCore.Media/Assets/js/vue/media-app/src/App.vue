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
                    <folder :base-url="baseUrl" :current-folder="rootFolder" :t="t" ref="baseFolder"
                        :selected-in-file-app="selectedFolder" :level="1">
                    </folder>
                </ol>
            </div>

            <div id="fileContainerMain" v-cloak>
                <div class="file-container-top-bar">
                    <nav id="breadcrumb" class="d-flex justify-content-end align-items-center">
                        <div class="breadcrumb-path px-3">
                            <span class="breadcrumb-item" :class="{ active: isHome }">
                                <a id="t-fileLibrary" :href="isHome ? 'javascript:void(0)' : '#'"
                                    v-on:click="selectRootFolder">
                                    {{ t.FolderRoot }}
                                </a>
                            </span>
                            <span v-for="(folder, i) in parents" :key="folder.filePath" v-cloak class="breadcrumb-item"
                                :class="{
        active: parents.length - i == 1,
        'no-breadcrumb-divider': i == 0,
    }">
                                <a :href="parents.length - i == 1
        ? 'javascript:void(0)'
        : '#'
        " v-on:click="selectedFolder = folder">
                                    {{ folder.name }}
                                </a>
                            </span>
                        </div>
                    </nav>
                    <nav class="nav action-bar p-3 flex">
                        <div class="me-auto">
                            <div class="btn-group btn-group me-2">
                                <label :title="t.UploadFiles" for="fileupload"
                                    class="btn btn-primary fileinput-button upload-button">
                                    <input id="fileupload" type="file" name="files" multiple @change="onFileInputChange" />
                                    <fa-icon icon="fa-solid fa-cloud-arrow-up"></fa-icon>
                                    {{ t.UploadFiles }}
                                </label>
                            </div>
                            <a :title="t.Invert" href="javascript:void(0)" class="btn btn-light me-2"
                                v-on:click="invertSelection">
                                <fa-icon icon="fa-solid fa-right-left"></fa-icon>
                            </a>
                            <a :title="t.Delete" href="javascript:void(0)" class="btn btn-light me-2"
                                @click="() => openModal('deleteAll')" :class="{ disabled: selectedFiles.length < 1 }">
                                <fa-icon icon="fa-solid fa-trash"></fa-icon>
                                <span class="badge rounded-pill ms-1" v-show="selectedFiles.length > 0">{{
        selectedFiles.length }}</span>
                                <ModalConfirm :t="t" :action-name="t.Delete" modal-name="deleteAll"
                                    :title="t.DeleteFileTitle" @confirm="() => confirmModal('deleteAll')">
                                    <p>{{ t.DeleteFileMessage }}</p>
                                </ModalConfirm>
                            </a>
                            <div class="btn-group visibility-buttons">
                                <button class="btn btn-light" :class="{ selected: !gridView }"
                                    @click="gridView = false" :title="t.TableView">
                                    <fa-icon icon="fa-solid fa-list"></fa-icon>
                                </button>
                                <button class="btn btn-light" :class="{ selected: gridView }"
                                    @click="gridView = true" :title="t.GridView">
                                    <fa-icon icon="fa-solid fa-grip"></fa-icon>
                                </button>
                            </div>
                        </div>
                        <div class="nav-item mx-2 mt-3 md:mt-0">
                            <div class="file-filter">
                                <div class="input-group input-group">
                                    <fa-icon icon="fa-solid fa-filter icon-inside-input"></fa-icon>
                                    <input type="text" id="file-filter-input" v-model="fileFilter"
                                        class="form-control input-filter" :placeholder="t.Filter"
                                        :aria-label="t.Filter" />
                                    <button id="clear-file-filter-button" class="btn btn-outline-secondary"
                                        :disabled="fileFilter == ''" v-on:click="fileFilter = ''">
                                        <fa-icon icon="fa-solid fa-times"></fa-icon>
                                    </button>
                                </div>
                            </div>
                        </div>
                    </nav>
                </div>
                <div class="file-container-middle p-3">
                    <file-items :t="t" :is-selected-all="isSelectedAll" :sort-by="sortBy" :sort-asc="sortAsc"
                        :filtered-file-items="itemsInPage" :selected-files="selectedFiles" :thumb-size="thumbSize"
                        v-show="!isLoadingFolder && itemsInPage.length > 0 && !gridView" ref="fileItems" :base-host="baseHost"></file-items>

                    <file-items-grid :t="t" :filtered-file-items="itemsInPage" :selected-files="selectedFiles"
                        :thumb-size="thumbSize" v-show="!isLoadingFolder && itemsInPage.length > 0 && gridView"
                        :base-host="baseHost"></file-items-grid>

                    <!-- Filter returned no results -->
                    <div v-show="!isLoadingFolder && fileItems.length > 0 && filteredFileItems.length < 1" class="empty-state">
                        <fa-icon icon="fa-solid fa-filter" class="empty-state-icon"></fa-icon>
                        <p class="empty-state-text">{{ t.FolderFilterEmpty }}</p>
                    </div>

                    <!-- Loading spinner -->
                    <div v-show="isLoadingFolder" class="loading-spinner-container">
                        <svg class="loading-spinner" viewBox="0 0 50 50" xmlns="http://www.w3.org/2000/svg">
                            <circle cx="25" cy="25" r="20" fill="none" stroke="currentColor" stroke-width="4"
                                stroke-linecap="round" stroke-dasharray="90, 150" stroke-dashoffset="0" />
                        </svg>
                    </div>

                    <!-- Folder is empty -->
                    <div v-show="fileItems.length < 1 && !isLoadingFolder" class="empty-state">
                        <fa-icon icon="fa-regular fa-folder-open" class="empty-state-icon"></fa-icon>
                        <p class="empty-state-text">{{ t.FolderEmpty }}</p>
                        <p class="empty-state-hint">{{ t.DropTitle }}</p>
                    </div>
                </div>
                <div v-show="!isLoadingFolder && filteredFileItems.length > 0" class="file-container-footer p-3 pb-0">
                    <pager :t="t" :source-items="filteredFileItems"> </pager>
                </div>
            </div>
        </div>
        <upload-toast :t="t"></upload-toast>
    </div>
</template>

<style lang="scss">
@import "./assets/scss/file.scss";
</style>

<script lang="ts">
import { defineComponent, ref } from "vue";
import dbg from "debug";
import FolderComponent from "./components/folderComponent.vue";
import UploadToast from "./components/uploadToast.vue";
import FileItemsComponent from "./components/fileItemsComponent.vue";
import FileItemsGridComponent from "./components/fileItemsGridComponent.vue";
import PagerComponent from "./components/pagerComponent.vue";
import DragDropThumbnail from "./assets/drag-thumbnail.png";
import { MediaApiClient, MoveMedia } from "./services/MediaApiClient";
import type { IFileStoreEntry } from "./interfaces/interfaces";
import { notify, tryGetErrorMessage } from "./services/notifier";
import { SeverityLevel } from "./interfaces/interfaces";
import { useVfm } from "vue-final-modal";
import ModalConfirm from "./components/ModalConfirm.vue";
import Uppy, { debugLogger } from "@uppy/core";
import DropTarget from "@uppy/drop-target";
import XHRUpload from "@uppy/xhr-upload";
import axios from "axios";

import "@uppy/core/dist/style.css";
import "@uppy/drop-target/dist/style.css";

const debug = dbg("media:file-app");
const baseFolder = ref(null);
const fileItems = ref(null);

declare global {
    interface Window {
        uppy: Uppy;
    }
}

export default defineComponent({
    components: {
        Folder: FolderComponent,
        UploadToast: UploadToast,
        FileItems: FileItemsComponent,
        FileItemsGrid: FileItemsGridComponent,
        Pager: PagerComponent,
        ModalConfirm: ModalConfirm,
    },
    name: "file-app",
    props: {
        baseHost: {
            type: String,
            required: false,
        },
        basePath: {
            type: String,
            required: true,
        },
        siteId: {
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
    },
    data() {
        return {
            t: {} as Record<string, string>,
            selectedFolder: {} as IFileStoreEntry,
            fileItems: [] as IFileStoreEntry[],
            uploadedFileItems: [] as IFileStoreEntry[],
            selectedFiles: [] as IFileStoreEntry[],
            isSelectedAll: false,
            errors: [] as string[],
            dragDropThumbnail: new Image(),
            smallThumbs: false,
            gridView: false,
            fileFilter: "",
            sortBy: "",
            sortAsc: true,
            itemsInPage: [] as IFileStoreEntry[],
            rootFolder: {} as IFileStoreEntry,
            uppy: null as Uppy | null,
            isLoadingFolder: true,
            loadFolderRequestId: 0,
            loadFolderCancelSource: null as ReturnType<typeof axios.CancelToken.source> | null,
        };
    },
    created: function () {
        const me = this;
        this.t = JSON.parse(this.$props.translations);

        me.dragDropThumbnail.src = DragDropThumbnail;

        this.rootFolder = {
            name: this.t.FileLibrary,
            filePath: "/",
            isDirectory: true,
        };

        this.emitter.on("folderSelected", (folder: IFileStoreEntry) => {
            me.selectedFolder = folder;
            me.isSelectedAll = false;
        });

        this.emitter.on("folderDeleted", () => {
            me.selectRootFolder();
        });

        this.emitter.on("folderAdded", (folder: IFileStoreEntry) => {
            me.selectedFolder = folder;
            folder.selected = true;
        });

        this.emitter.on("fileListMove", (elem: any) => {
            me.fileListMove(elem);
        });

        this.emitter.on("fileListMoved", (errorInfo: never) => {
            me.loadFolder(me.selectedFolder);
        });

        this.emitter.on("fileRenamed", (element: any) => {
            me.loadFolder(me.selectedFolder);
        });

        this.emitter.on("createFolderRequested", (folder: IFileStoreEntry) => {
            me.createFolder(folder);
        });

        this.emitter.on("deleteFolderRequested", (folder: IFileStoreEntry) => {
            me.deleteFolder(folder);
        });

        // common handlers for actions in both grid and table view.
        this.emitter.on("sortChangeRequested", (newSort: any) => {
            me.changeSort(newSort);
        });

        this.emitter.on("fileToggleRequested", (file: IFileStoreEntry) => {
            me.toggleSelectionOfFile(file);
            me.isSelectedAll = false;
        });

        this.emitter.on("renameFileRequested", (file: any) => {
            me.renameFile(file);
        });

        this.emitter.on("deleteFileRequested", (file: IFileStoreEntry) => {
            me.deleteFileItem(file);
        });

        this.emitter.on("fileDragStartRequested", (file: IFileStoreEntry) => {
            me.handleDragStart(file);
        });

        // handler for pager events
        this.emitter.on("pagerEvent", (itemsInPage: IFileStoreEntry[]) => {
            //debug("pagerEvent", itemsInPage)
            me.itemsInPage = itemsInPage;
        });

        this.emitter.on("select-all", () => {
            me.selectAll();
        });

        if (
            !localStorage.getItem("FileLibraryPrefs" + "-" + this.$props.siteId)
        ) {
            me.selectedFolder = this.rootFolder;
            return;
        }

        const fileApplicationPrefs = localStorage.getItem(
            "FileLibraryPrefs" + "-" + this.$props.siteId,
        );

        if (fileApplicationPrefs != null) {
            me.currentPrefs = JSON.parse(fileApplicationPrefs);
        }
    },
    computed: {
        baseUrl: function () {
            return this.$props.baseHost
                ? this.$props.baseHost + this.$props.basePath
                : this.$props.basePath;
        },
        isHome: function () {
            return this.selectedFolder == this.rootFolder;
        },
        parents: function () {
            let p = [];
            let parentFolder = this.selectedFolder;

            while (parentFolder && parentFolder.filePath != "") {
                p.unshift(parentFolder);
                parentFolder = parentFolder.parent; // TODO: refactor as this is a param added programmatically
            }

            return p;
        },
        filteredFileItems: function () {
            const me = this;

            let filtered = me.fileItems.filter(function (item: any) {
                return (
                    item.name
                        .toLowerCase()
                        .indexOf(me.fileFilter.toLowerCase()) > -1
                );
            });

            switch (me.sortBy) {
                case "size":
                    filtered.sort(function (a: any, b: any) {
                        return me.sortAsc ? a.size - b.size : b.size - a.size;
                    });
                    break;
                case "mime":
                    filtered.sort(function (a: any, b: any) {
                        return me.sortAsc
                            ? a.mime
                                .toLowerCase()
                                .localeCompare(b.mime.toLowerCase())
                            : b.mime
                                .toLowerCase()
                                .localeCompare(a.mime.toLowerCase());
                    });
                    break;
                case "lastModifiedUtc":
                    filtered.sort(function (a: any, b: any) {
                        return me.sortAsc
                            ? a.lastModifiedUtc - b.lastModifiedUtc
                            : b.lastModifiedUtc - a.lastModifiedUtc;
                    });
                    break;
                default:
                    filtered.sort(function (a: any, b: any) {
                        return me.sortAsc
                            ? a.name
                                .toLowerCase()
                                .localeCompare(b.name.toLowerCase())
                            : b.name
                                .toLowerCase()
                                .localeCompare(a.name.toLowerCase());
                    });
            }

            return filtered;
        },
        hiddenCount: function () {
            let result = 0;
            result = this.fileItems.length - this.filteredFileItems.length;
            return result;
        },
        thumbSize: function () {
            return this.smallThumbs ? 160 : 240;
        },
        currentPrefs: {
            get: function () {
                return {
                    smallThumbs: this.smallThumbs,
                    selectedFolder: this.selectedFolder,
                    gridView: this.gridView,
                };
            },
            set: function (newPrefs: any) {
                if (!newPrefs) {
                    return;
                }

                this.smallThumbs = newPrefs.smallThumbs;
                this.selectedFolder = newPrefs.selectedFolder;
                this.gridView = newPrefs.gridView;
            },
        },
    },
    watch: {
        currentPrefs: function (newPrefs) {
            localStorage.setItem(
                "FileLibraryPrefs" + "-" + this.$props.siteId,
                JSON.stringify(newPrefs),
            );
        },
        selectedFolder: function (newFolder) {
            this.fileFilter = "";
            this.selectedFolder = newFolder;
            this.loadFolder(newFolder);
        },
    },
    mounted: function () {
        const me = this;

        let uploadUrl = me.baseHost
            ? me.baseHost + me.uploadFilesUrl
            : me.uploadFilesUrl;

        const uppy = new Uppy({ logger: debugLogger })
            .use(XHRUpload, {
                endpoint: uploadUrl,
                fieldName: 'files',
                bundle: true,
            });

        uppy.use(DropTarget, {
            target: document.body,
            onDragLeave: (event) => {
                event.stopPropagation();
            },
        });

        me.uppy = uppy;
        window.uppy = uppy;

        uppy.on("file-added", (file) => {
            debug("file added", file);
            me.emitter.emit("uploadFileAdded", { name: file.name });

            // Update the endpoint to include the current folder path.
            const folderPath = me.selectedFolder?.filePath ?? "/";
            const xhrPlugin = uppy.getPlugin("XHRUpload");
            if (xhrPlugin) {
                const sep = uploadUrl.includes("?") ? "&" : "?";
                xhrPlugin.setOptions({
                    endpoint: uploadUrl + sep + "path=" + encodeURIComponent(folderPath),
                });
            }

            uppy.upload();
        })

        uppy.on("upload-progress", (file, progress) => {
            if (file) {
                me.emitter.emit("uploadProgress", {
                    name: file.name,
                    percentage: Math.round((progress.bytesUploaded / progress.bytesTotal) * 100),
                });
            }
        });

        uppy.on("upload-success", (file, response) => {
            if (file) {
                // In bundle mode the server returns { files: [...] } where each
                // entry may carry an `error` field for rejected uploads.
                const body = response?.body as any;
                const serverFiles = body?.files as any[];
                if (serverFiles) {
                    const match = serverFiles.find(
                        (sf: any) => sf.name?.toLowerCase() === file.name.toLowerCase()
                    );
                    if (match?.error) {
                        me.emitter.emit("uploadError", {
                            name: file.name,
                            errorMessage: match.error,
                        });
                        uppy.removeFile(file.id);
                        return;
                    }
                }

                me.emitter.emit("uploadSuccess", { name: file.name });
                me.loadFolder(me.selectedFolder);
                uppy.removeFile(file.id);
            }
        });

        uppy.on("upload-error", (file, error) => {
            if (file) {
                me.emitter.emit("uploadError", {
                    name: file.name,
                    errorMessage: error?.message ?? me.t.ErrorUploadFile,
                });
                uppy.removeFile(file.id);
            }
        });

        uppy.on("complete", (result) => {
            if (result.successful.length > 0) {
                me.loadFolder(me.selectedFolder);
            }
        });

        if (me.currentPrefs.selectedFolder != null) {
            (<any>me.$refs.baseFolder).selectFolder(
                me.currentPrefs.selectedFolder,
            );
        } else {
            (<any>me.$refs.baseFolder).select();
        }


    },
    methods: {
        onFileInputChange: function (event: Event) {
            const input = event.target as HTMLInputElement;
            if (!input.files) return;
            for (const file of Array.from(input.files)) {
                try {
                    this.uppy.addFile({
                        name: file.name,
                        type: file.type,
                        data: file,
                    });
                } catch (err: any) {
                    debug("Error adding file", err);
                    this.emitter.emit("uploadFileAdded", { name: file.name });
                    this.emitter.emit("uploadError", {
                        name: file.name,
                        errorMessage: err?.message ?? this.t.ErrorUploadFile,
                    });
                }
            }
            input.value = "";
        },
        openModal: function (action: string) {
            const uVfm = useVfm();
            uVfm.open(action);
        },
        confirmModal: function (action: string) {
            const uVfm = useVfm();

            if (action == "deleteAll") {
                this.deleteFileList();
            }

            uVfm.close(action);
        },
        selectRootFolder: function () {
            this.selectedFolder = this.rootFolder;
        },
        fileListMove: function (elem: any) {
            const me = this;
            debug("fileListMove", elem, me.baseUrl);

            if (elem) {
                const apiClient = new MediaApiClient(me.baseUrl);
                apiClient
                    .moveMediaList(
                        new MoveMedia({
                            mediaNames: elem.fileNames,
                            sourceFolder: elem.sourceFolder,
                            targetFolder: elem.targetFolder,
                        }),
                    )
                    .then((res) => {
                        me.emitter.emit("fileListMoved"); // FileApp will listen to this, and then it will reload page so the moved files won't be there anymore
                    })
                    .catch(async (error) => {
                        notify({
                            summary: me.t.ErrorMovingFile,
                            detail: await tryGetErrorMessage(error),
                            severity: SeverityLevel.Error,
                        });
                    });
            }
        },
        loadFolder: function (folder: IFileStoreEntry) {
            // Cancel any in-flight folder load request.
            if (this.loadFolderCancelSource) {
                this.loadFolderCancelSource.cancel();
            }

            this.errors = [];
            this.selectedFiles = [];
            this.fileItems = [];
            this.isLoadingFolder = true;
            const me = this;

            const requestId = ++this.loadFolderRequestId;
            const cancelSource = axios.CancelToken.source();
            this.loadFolderCancelSource = cancelSource;

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .getMediaItems(folder.filePath, undefined, cancelSource.token)
                .then((response) => {
                    if (requestId !== me.loadFolderRequestId) return;
                    me.fileItems = response;
                    me.selectedFiles = [];
                    me.sortBy = "";
                    me.sortAsc = true;
                    me.isLoadingFolder = false;
                })
                .catch(async (error) => {
                    if (requestId !== me.loadFolderRequestId) return;
                    notify({
                        summary: me.t.ErrorLoadingFolder,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
                    me.isLoadingFolder = false;
                    me.selectRootFolder();
                });
        },
        selectAll: function () {
            if (this.isSelectedAll) {
                this.selectedFiles = [];
                this.isSelectedAll = false;
            } else {
                this.selectedFiles = [];
                for (let i = 0; i < this.filteredFileItems.length; i++) {
                    this.selectedFiles.push(this.filteredFileItems[i]);
                }
                this.isSelectedAll = true;
            }
        },
        invertSelection: function () {
            this.isSelectedAll = false;
            let temp = [];

            for (let i = 0; i < this.filteredFileItems.length; i++) {
                if (this.isFileSelected(this.filteredFileItems[i]) == false) {
                    temp.push(this.filteredFileItems[i]);
                }
            }
            this.selectedFiles = temp;

            if (temp.length == this.filteredFileItems.length) {
                this.isSelectedAll = true;
            }
        },
        toggleSelectionOfFile: function (file: any) {
            if (this.isFileSelected(file) == true) {
                this.selectedFiles.splice(this.selectedFiles.indexOf(file), 1);
            } else {
                this.selectedFiles.push(file);
            }
        },
        isFileSelected: function (file: IFileStoreEntry) {
            return this.selectedFiles?.some(
                (element) => element.url?.toLowerCase() === file.url?.toLowerCase(),
            );
        },
        deleteFolder: function (folder: IFileStoreEntry) {
            const me = this;
            // The root folder can't be deleted
            if (folder == this.rootFolder) {
                return;
            }

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .deleteFolder(folder.filePath)
                .then(() => {
                    me.emitter.emit("deleteFolder", folder);
                })
                .catch(async (error) => {
                    notify({
                        summary: me.t.ErrorDeleteFolder,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
                });
        },
        createFolder: function (folder: IFileStoreEntry) {
            const me = this;

            if (folder.name === "") {
                return;
            }

            debug(
                "selected folder before create new one",
                me.selectedFolder.filePath,
            );

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .createFolder(me.selectedFolder.filePath, folder.name)
                .then((response) => {
                    me.emitter.emit("addFolder", {
                        selectedFolder: me.selectedFolder,
                        data: response,
                    });
                })
                .catch(async (error) => {
                    notify({
                        summary: me.t.ErrorCreateFolder,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
                });
        },
        renameFile: function (element: any) {
            //TODO, create TS interface for this
            const me = this;
            let newName = element.newName;
            let file = element.file;

            debug("Rename file", element, file.name, newName, this.basePath);

            const oldPath = file.filePath; // TODO make this better
            const newPath = oldPath.replace(file.name, newName);

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .moveMedia(oldPath, newPath)
                .then(() => {
                    me.emitter.emit("fileRenamed", {
                        newName: newName,
                        newPath: newPath,
                        oldPath: oldPath,
                    });
                })
                .catch(async (error) => {
                    notify({
                        summary: me.t.ErrorRenamingFile,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
                });
        },
        deleteFileList: function () {
            const me = this;
            let files = this.selectedFiles;

            if (files.length < 1) {
                return;
            }

            let imagePaths: string[] = [];

            for (let i = 0; i < files.length; i++) {
                imagePaths.push(files[i].filePath ?? "");
            }

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .deleteMediaList(imagePaths)
                .then(() => {
                    for (let i = 0; i < me.selectedFiles.length; i++) {
                        let index =
                            me.fileItems &&
                            me.fileItems.indexOf(me.selectedFiles[i]);
                        if (index > -1) {
                            me.fileItems.splice(index, 1);
                            me.emitter.emit("fileDeleted", me.selectedFiles[i]);
                        }
                    }
                    me.selectedFiles = [];
                    me.isSelectedAll = false;
                })
                .catch(async (error) => {
                    notify({
                        summary: me.t.ErrorDeleteFile,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
                });
        },
        deleteFileItem: function (file: IFileStoreEntry) {
            const me = this;

            if (!file) {
                debug("Cannot delete null file item", file);
                return;
            }

            debug("delete file item", file);

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .deleteMedia(file.filePath)
                .then(() => {
                    let index = me.fileItems && me.fileItems.indexOf(file);
                    if (index > -1) {
                        me.fileItems.splice(index, 1);
                        me.emitter.emit("fileDeleted", file);
                    }
                })
                .catch(async (error) => {
                    notify({
                        summary: me.t.ErrorDeleteFile,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
                });
        },
        handleDragStart: function (element: any) {
            // first part of move file to folder:
            // prepare the data that will be handled by the folder component on drop event
            let fileNames = [];
            this.selectedFiles.forEach(function (item: any) {
                fileNames.push(item.name);
            });

            // in case the user drags an unselected item, we select it first
            if (this.isFileSelected(element.file) == false) {
                fileNames.push(element.file.name);
                this.selectedFiles.push(element.file);
            }

            element.e.dataTransfer.setData(
                "fileNames",
                JSON.stringify(fileNames),
            );
            element.e.dataTransfer.setData(
                "sourceFolder",
                this.selectedFolder.filePath,
            );
            element.e.dataTransfer.setDragImage(this.dragDropThumbnail, 10, 10);
            element.e.dataTransfer.effectAllowed = "move";
        },
        handleScrollWhileDrag: function (e: any) {
            const me = this;

            if (e.clientY < 150) {
                window.scrollBy(0, -10);
            }

            if (e.clientY > window.innerHeight - 100) {
                window.scrollBy(0, 10);
            }

            me.selectedFiles = [];
            me.isSelectedAll = false;
        },
        changeSort: function (newSort: any) {
            if (this.sortBy == newSort) {
                this.sortAsc = !this.sortAsc;
            } else {
                this.sortAsc = true;
                this.sortBy = newSort;
            }
        },
    },
});
</script>
