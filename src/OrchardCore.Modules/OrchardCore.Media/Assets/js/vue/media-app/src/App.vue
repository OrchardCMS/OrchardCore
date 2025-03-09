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
                            <span v-for="(folder, i) in parents" :key="folder.path" v-cloak class="breadcrumb-item"
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
                                    <input id="fileupload" type="file" name="files" multiple />
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
                    <upload-list upload-input-id="fileupload" :t="t"></upload-list>

                    <file-items :t="t" :is-selected-all="isSelectedAll" :sort-by="sortBy" :sort-asc="sortAsc"
                        :filtered-file-items="itemsInPage" :selected-files="selectedFiles" :thumb-size="thumbSize"
                        v-show="itemsInPage.length > 0 && !gridView" ref="fileItems" :base-host="baseHost"></file-items>

                    <div v-show="fileItems.length > 0 && filteredFileItems.length < 1
        " class="p-message p-component p-message-info" role="alert" aria-live="assertive" aria-atomic="true"
                        data-pc-name="message" data-pc-section="root">
                        <div class="p-message-wrapper" data-pc-section="wrapper">
                            <svg width="14" height="14" viewBox="0 0 14 14" fill="none"
                                xmlns="http://www.w3.org/2000/svg" class="p-icon p-message-icon" aria-hidden="true"
                                data-pc-section="icon">
                                <path fill-rule="evenodd" clip-rule="evenodd"
                                    d="M3.11101 12.8203C4.26215 13.5895 5.61553 14 7 14C8.85652 14 10.637 13.2625 11.9497 11.9497C13.2625 10.637 14 8.85652 14 7C14 5.61553 13.5895 4.26215 12.8203 3.11101C12.0511 1.95987 10.9579 1.06266 9.67879 0.532846C8.3997 0.00303296 6.99224 -0.13559 5.63437 0.134506C4.2765 0.404603 3.02922 1.07129 2.05026 2.05026C1.07129 3.02922 0.404603 4.2765 0.134506 5.63437C-0.13559 6.99224 0.00303296 8.3997 0.532846 9.67879C1.06266 10.9579 1.95987 12.0511 3.11101 12.8203ZM3.75918 2.14976C4.71846 1.50879 5.84628 1.16667 7 1.16667C8.5471 1.16667 10.0308 1.78125 11.1248 2.87521C12.2188 3.96918 12.8333 5.45291 12.8333 7C12.8333 8.15373 12.4912 9.28154 11.8502 10.2408C11.2093 11.2001 10.2982 11.9478 9.23232 12.3893C8.16642 12.8308 6.99353 12.9463 5.86198 12.7212C4.73042 12.4962 3.69102 11.9406 2.87521 11.1248C2.05941 10.309 1.50384 9.26958 1.27876 8.13803C1.05367 7.00647 1.16919 5.83358 1.61071 4.76768C2.05222 3.70178 2.79989 2.79074 3.75918 2.14976ZM7.00002 4.8611C6.84594 4.85908 6.69873 4.79698 6.58977 4.68801C6.48081 4.57905 6.4187 4.43185 6.41669 4.27776V3.88888C6.41669 3.73417 6.47815 3.58579 6.58754 3.4764C6.69694 3.367 6.84531 3.30554 7.00002 3.30554C7.15473 3.30554 7.3031 3.367 7.4125 3.4764C7.52189 3.58579 7.58335 3.73417 7.58335 3.88888V4.27776C7.58134 4.43185 7.51923 4.57905 7.41027 4.68801C7.30131 4.79698 7.1541 4.85908 7.00002 4.8611ZM7.00002 10.6945C6.84594 10.6925 6.69873 10.6304 6.58977 10.5214C6.48081 10.4124 6.4187 10.2652 6.41669 10.1111V6.22225C6.41669 6.06754 6.47815 5.91917 6.58754 5.80977C6.69694 5.70037 6.84531 5.63892 7.00002 5.63892C7.15473 5.63892 7.3031 5.70037 7.4125 5.80977C7.52189 5.91917 7.58335 6.06754 7.58335 6.22225V10.1111C7.58134 10.2652 7.51923 10.4124 7.41027 10.5214C7.30131 10.6304 7.1541 10.6925 7.00002 10.6945Z"
                                    fill="currentColor"></path>
                            </svg>
                            <div class="p-message-text p-message-text" data-pc-section="text">
                                {{ t.FolderFilterEmpty }}
                            </div>
                        </div>
                    </div>

                    <div v-show="fileItems.length < 1" class="p-message p-component p-message-info" role="alert"
                        aria-live="assertive" aria-atomic="true" data-pc-name="message" data-pc-section="root">
                        <div class="p-message-wrapper" data-pc-section="wrapper">
                            <svg width="14" height="14" viewBox="0 0 14 14" fill="none"
                                xmlns="http://www.w3.org/2000/svg" class="p-icon p-message-icon" aria-hidden="true"
                                data-pc-section="icon">
                                <path fill-rule="evenodd" clip-rule="evenodd"
                                    d="M3.11101 12.8203C4.26215 13.5895 5.61553 14 7 14C8.85652 14 10.637 13.2625 11.9497 11.9497C13.2625 10.637 14 8.85652 14 7C14 5.61553 13.5895 4.26215 12.8203 3.11101C12.0511 1.95987 10.9579 1.06266 9.67879 0.532846C8.3997 0.00303296 6.99224 -0.13559 5.63437 0.134506C4.2765 0.404603 3.02922 1.07129 2.05026 2.05026C1.07129 3.02922 0.404603 4.2765 0.134506 5.63437C-0.13559 6.99224 0.00303296 8.3997 0.532846 9.67879C1.06266 10.9579 1.95987 12.0511 3.11101 12.8203ZM3.75918 2.14976C4.71846 1.50879 5.84628 1.16667 7 1.16667C8.5471 1.16667 10.0308 1.78125 11.1248 2.87521C12.2188 3.96918 12.8333 5.45291 12.8333 7C12.8333 8.15373 12.4912 9.28154 11.8502 10.2408C11.2093 11.2001 10.2982 11.9478 9.23232 12.3893C8.16642 12.8308 6.99353 12.9463 5.86198 12.7212C4.73042 12.4962 3.69102 11.9406 2.87521 11.1248C2.05941 10.309 1.50384 9.26958 1.27876 8.13803C1.05367 7.00647 1.16919 5.83358 1.61071 4.76768C2.05222 3.70178 2.79989 2.79074 3.75918 2.14976ZM7.00002 4.8611C6.84594 4.85908 6.69873 4.79698 6.58977 4.68801C6.48081 4.57905 6.4187 4.43185 6.41669 4.27776V3.88888C6.41669 3.73417 6.47815 3.58579 6.58754 3.4764C6.69694 3.367 6.84531 3.30554 7.00002 3.30554C7.15473 3.30554 7.3031 3.367 7.4125 3.4764C7.52189 3.58579 7.58335 3.73417 7.58335 3.88888V4.27776C7.58134 4.43185 7.51923 4.57905 7.41027 4.68801C7.30131 4.79698 7.1541 4.85908 7.00002 4.8611ZM7.00002 10.6945C6.84594 10.6925 6.69873 10.6304 6.58977 10.5214C6.48081 10.4124 6.4187 10.2652 6.41669 10.1111V6.22225C6.41669 6.06754 6.47815 5.91917 6.58754 5.80977C6.69694 5.70037 6.84531 5.63892 7.00002 5.63892C7.15473 5.63892 7.3031 5.70037 7.4125 5.80977C7.52189 5.91917 7.58335 6.06754 7.58335 6.22225V10.1111C7.58134 10.2652 7.51923 10.4124 7.41027 10.5214C7.30131 10.6304 7.1541 10.6925 7.00002 10.6945Z"
                                    fill="currentColor"></path>
                            </svg>
                            <div class="p-message-text p-message-text" data-pc-section="text">
                                {{ t.FolderEmpty }}
                            </div>
                        </div>
                    </div>
                </div>
                <div v-show="filteredFileItems.length > 0" class="file-container-footer p-3 pb-0">
                    <pager :t="t" :source-items="filteredFileItems"> </pager>
                </div>
            </div>
        </div>
    </div>
</template>

<style lang="scss">
@import "./assets/scss/file.scss";
</style>

<script lang="ts">
import { defineComponent, ref, nextTick } from "vue";
import dbg from "debug";
import FolderComponent from "./components/folderComponent.vue";
import UploadListComponent from "./components/uploadListComponent.vue";
import FileItemsComponent from "./components/fileItemsComponent.vue";
import PagerComponent from "./components/pagerComponent.vue";
import DragDropThumbnail from "./assets/drag-thumbnail.png";
import {
    MediaApiClient,
    IFileStoreEntry,
    MoveMedia,
} from "./services/MediaApiClient";
import { notify, tryGetErrorMessage } from "./services/notifier";
import { SeverityLevel } from "./interfaces/interfaces";
import { useVfm } from "vue-final-modal";
import ModalConfirm from "./components/ModalConfirm.vue";
import { v4 as uuidv4 } from "uuid";
import Uppy, { debugLogger } from "@uppy/core";
import DropTarget from "@uppy/drop-target";
import XHRUpload from "@uppy/xhr-upload";

import "@uppy/core/dist/style.css";
import "@uppy/drop-target/dist/style.css";

const debug = dbg("aptix:file-app");
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
        UploadList: UploadListComponent,
        FileItems: FileItemsComponent,
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
            t: <any>Object,
            selectedFolder: {} as IFileStoreEntry,
            fileItems: <any>[],
            uploadedFileItems: [] as IFileStoreEntry[],
            selectedFiles: [] as IFileStoreEntry[],
            isSelectedAll: false,
            errors: <any>[],
            dragDropThumbnail: new Image(),
            smallThumbs: false,
            gridView: false,
            fileFilter: "",
            sortBy: "",
            sortAsc: true,
            itemsInPage: [] as IFileStoreEntry[],
            rootFolder: {} as IFileStoreEntry,
        };
    },
    created: function () {
        const me = this;
        this.t = JSON.parse(this.$props.translations);

        me.dragDropThumbnail.src = DragDropThumbnail;

        this.rootFolder = {
            name: this.t.FileLibrary,
            path: "/",
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

            while (parentFolder && parentFolder.path != "") {
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
                case "lastModify":
                    filtered.sort(function (a: any, b: any) {
                        return me.sortAsc
                            ? a.lastModify - b.lastModify
                            : b.lastModify - a.lastModify;
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

        window.uppy = uppy;

        uppy.on("file-added", (data) => {
            debug("file added", data);
            uppy.upload();
        })

        uppy.on("upload-success", () => {
            console.log("ddd");
        })

        uppy.on("complete", (result) => {
            if (result.failed.length === 0) {
                console.log("Upload successful!");
            } else {
                console.warn("Upload failed!");
            }
            console.log("successful files:", result.successful);
            console.log("failed files:", result.failed);
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
            this.errors = [];
            this.selectedFiles = [];
            const me = this;

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .getMediaItems(folder.path, null)
                .then((response: any) => {
                    response.forEach(function (item: any) {
                        item.open = false;
                    });
                    me.fileItems = response;
                    me.selectedFiles = [];
                    me.sortBy = "";
                    me.sortAsc = true;
                })
                .catch(async (error) => {
                    notify({
                        summary: me.t.ErrorLoadingFolder,
                        detail: await tryGetErrorMessage(error),
                        severity: SeverityLevel.Error,
                    });
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
            let result = this.selectedFiles?.some(function (
                element: any,
                index: any,
                array: any,
            ) {
                return element.url.toLowerCase() === file.url?.toLowerCase();
            });
            return result;
        },
        deleteFolder: function (folder: IFileStoreEntry) {
            const me = this;
            // The root folder can't be deleted
            if (folder == this.rootFolder) {
                return;
            }

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .deleteFolder(folder.path)
                .then((response: any) => {
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
            $("createFolderModal-errors")?.empty();

            if (folder.name === "") {
                return;
            }

            debug(
                "selected folder before create new one",
                me.selectedFolder.path,
            );

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .createFolder(me.selectedFolder.path, folder.name)
                .then((response: any) => {
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
                .then((response: any) => {
                    me.emitter.emit("fileRenamed", {
                        newName: newName,
                        newPath: newPath,
                        oldPath: oldPath,
                    });
                })
                .catch(async (error) => {
                    error.response.text().then((text: any) => {
                        const error = JSON.parse(text);
                        notify({
                            summary: me.t.ErrorRenamingFile,
                            detail: error.detail,
                            severity: SeverityLevel.Error,
                        });
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
                imagePaths.push(files[i].mediaPath ?? ""); // Can't be a required field on IFileStoreEntry for a folder
            }

            const apiClient = new MediaApiClient(me.baseUrl);
            apiClient
                .deleteMediaList(imagePaths)
                .then((response: any) => {
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
                .deleteMedia(file.mediaPath)
                .then((response: any) => {
                    let index = me.fileItems && me.fileItems.indexOf(file);
                    if (index > -1) {
                        me.fileItems.splice(index, 1);
                        me.emitter.emit("fileDeleted", file);
                    }
                })
                .catch(async (error) => {
                    tryGetErrorMessage(error).then((message: any) => {
                        notify({
                            summary: me.t.ErrorDeleteFile,
                            detail: message,
                            severity: SeverityLevel.Error,
                        });
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
                this.selectedFolder.path,
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
