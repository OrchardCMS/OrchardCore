<template>
    <div v-on:dragover="handleScrollWhileDrag">
        <div id="customdropzone">
            <h3>Drop your media here</h3>
            <p>Your files will be uploaded to the current folder when you drop them here</p>
            <ul>
                <li>{{ basePath }}{{ getFoldersUrl }}</li>
                <li>{{ basePath }}{{ deleteFoldersUrl }}</li>
                <li>{{ basePath }}{{ createFoldersUrl }}</li>
                <li>{{ basePath }}{{ getMediaItemsUrl }}</li>
                <li>{{ basePath }}{{ deleteMediaUrl }}</li>
                <li>{{ basePath }}{{ renameMediaUrl }}</li>
                <li>{{ basePath }}{{ deleteMediaListUrl }}</li>
                <li>{{ basePath }}{{ moveMediaListUrl }}</li>
                <li>{{ basePath }}{{ uploadFilesUrl }}</li>
            </ul>
        </div>
        <div class="alert message-warning" v-if="errors.length > 0">
            <ul>
                <li v-for="e in errors">
                    <p>{{ e }}</p>
                </li>
            </ul>
        </div>
        <div id="mediaContainer" class="align-items-stretch">
            <div id="navigationApp" class="media-container-navigation m-0 p-0" v-cloak>
                <ol id="folder-tree">
                    <folder :model="root" ref="rootFolder" :selected-in-media-app="selectedFolder" :level="1">
                    </folder>
                </ol>
            </div>

            <div id="mediaContainerMain" v-cloak>
                <div class="media-container-top-bar">
                    <nav class="nav action-bar pb-3 pt-3 pl-3">
                        <div class="me-auto ms-4">
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="selectAll">
                                Select All
                            </a>
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="unSelectAll"
                                :class="{ disabled: selectedMedias.length < 1 }">
                                Select None
                            </a>
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="invertSelection">
                                Invert
                            </a>
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="deleteMediaList"
                                :class="{ disabled: selectedMedias.length < 1 }">
                                Delete <span class="badge rounded-pill bg-light" v-show="selectedMedias.length > 0">{{
                                    selectedMedias.length }}</span>
                            </a>
                        </div>
                        <div class="btn-group visibility-buttons">
                            <button type="button" id="toggle-grid-table-button" class="btn btn-light btn-sm"
                                :class="{ selected: gridView }" v-on:click="gridView = true">
                                <span title="Grid View"><i class="fa fa-th-large" aria-hidden="true"></i></span>
                            </button>
                            <button type="button" id="toggle-grid-table-button" class="btn btn-light btn-sm"
                                :class="{ selected: !gridView }" v-on:click="gridView = false">
                                <span title="List View"><i class="fa fa-th-list" aria-hidden="true"></i></span>
                            </button>
                        </div>
                        <div class="btn-group visibility-buttons" v-show="gridView">
                            <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm"
                                :class="{ selected: smallThumbs }" v-on:click="smallThumbs = true">
                                <span title="Small Thumbs"><i class="fa fa-compress" aria-hidden="true"></i></span>
                            </button>
                            <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm me-2"
                                :class="{ selected: !smallThumbs }" v-on:click="smallThumbs = false">
                                <span title="Large Thumbs"><i class="fa fa-expand" aria-hidden="true"></i></span>
                            </button>
                        </div>

                        <div class="nav-item ms-2">
                            <div class="media-filter">
                                <div class="input-group input-group-sm">
                                    <span class="fa fa-filter icon-inside-input"></span>
                                    <input type="text" id="media-filter-input" v-model="mediaFilter"
                                        class="form-control input-filter" placeholder="Filter..." aria-label="Filter..." />
                                    <button id="clear-media-filter-button" class="btn btn-outline-secondary" type="button"
                                        :disabled="mediaFilter == ''" v-on:click="mediaFilter = ''"><i class="fa fa-times"
                                            aria-hidden="true"></i></button>
                                </div>
                            </div>
                        </div>
                        <div class="d-inline-flex ms-2 me-3 mb-1 pt-1">
                            <div class="btn-group btn-group-sm">
                                <label for="fileupload" class="btn btn-sm btn-primary fileinput-button upload-button">
                                    <input id="fileupload" type="file" name="files" multiple />
                                    <fa-icon icon="fa-solid fa-plus"></fa-icon>
                                    Upload
                                </label>
                            </div>
                        </div>
                    </nav>

                    <nav id="breadcrumb" class="d-flex justify-content-end align-items-end">
                        <div class="breadcrumb-path p-3">
                            <span class="breadcrumb-item" :class="{ active: isHome }">
                                <a id="t-mediaLibrary" :href="isHome ? 'javascript:void(0)' : '#'" v-on:click="selectRoot">Media Library</a>
                            </span>
                            <span v-for="(folder, i) in parents" v-cloak class="breadcrumb-item"
                                :class="{ active: parents.length - i == 1 }">
                                <a :href="parents.length - i == 1 ? 'javascript:void(0)' : '#'" v-on:click="selectedFolder = folder;">{{
                                    folder.name }}</a>
                            </span>
                        </div>
                    </nav>
                </div>
                <div class="media-container-middle p-3">
                    <upload-list></upload-list>

                    <media-items-table :sort-by="sortBy" :sort-asc="sortAsc" :filtered-media-items="itemsInPage"
                        :selected-medias="selectedMedias" :thumb-size="thumbSize"
                        v-show="itemsInPage.length > 0 && !gridView"></media-items-table>

                    <media-items-grid v-show="gridView" :filtered-media-items="itemsInPage"
                        :selected-medias="selectedMedias" :thumb-size="thumbSize"></media-items-grid>

                    <div class="alert-info p-2" v-show="mediaItems.length > 0 && filteredMediaItems.length < 1">Nothing to
                        show with this filter</div>
                    <div class="alert-info p-2" v-show="mediaItems.length < 1">This folder is empty</div>
                </div>
                <div v-show="filteredMediaItems.length > 0" class="media-container-footer p-3">
                    <pager :source-items="filteredMediaItems"> </pager>
                </div>
            </div>
        </div>
    </div>
</template>
 
<style lang="scss">
@import "./assets/scss/media.scss";
</style>

<script lang="ts">
import { defineComponent, PropType } from 'vue';
import axios from 'axios';
import dbg from 'debug';
import { IMedia, IMediaElement } from './interfaces/interfaces';
import FolderComponent from './components/folderComponent.vue';
import UploadListComponent from './components/uploadListComponent.vue';
import MediaItemsGridComponent from './components/mediaItemsGridComponent.vue';
import MediaItemsTableComponent from './components/mediaItemsTableComponent.vue';
import PagerComponent from './components/pagerComponent.vue';
import DragDropThumbnail from './assets/drag-thumbnail.png';

const debug = dbg("oc:media-app");

export default defineComponent({
    components: {
        Folder: FolderComponent,
        UploadList: UploadListComponent,
        MediaItemsGrid: MediaItemsGridComponent,
        MediaItemsTable: MediaItemsTableComponent,
        Pager: PagerComponent,
    },
    name: "mediaApp",
    props: {
        basePath: {
            type: String,
            required: true
        },
        getFoldersUrl: {
            type: String,
            required: true
        },
        deleteFoldersUrl: {
            type: String,
            required: true
        },
        createFoldersUrl: {
            type: String,
            required: true
        },
        getMediaItemsUrl: {
            type: String,
            required: true
        },
        deleteMediaUrl: {
            type: String,
            required: true
        },
        renameMediaUrl: {
            type: String,
            required: true
        },
        deleteMediaListUrl: {
            type: String,
            required: true
        },
        moveMediaListUrl: {
            type: String,
            required: true
        },
        uploadFilesUrl: {
            type: String,
            required: true
        },
        pathBase: {
            type: String,
            required: true
        },
    },
    data() {
        return {
            selectedFolder: Object,
            mediaItems: [],
            selectedMedias: [],
            errors: [],
            dragDropThumbnail: new Image(),
            smallThumbs: false,
            gridView: false,
            mediaFilter: '',
            sortBy: '',
            sortAsc: true,
            itemsInPage: [],
            root: {
                name: document.querySelector('#t-mediaLibrary')?.textContent,
                path: '',
                folder: '',
                isDirectory: true
            } as IMediaElement
        }
    },
    created: function () {
        let self = this;

        self.dragDropThumbnail.src = DragDropThumbnail;

        this.emitter.on('folderSelected', (folder: {}) => {
            self.selectedFolder = folder;
        })

        this.emitter.on('folderDeleted', (folder: {}) => {
            self.selectRoot();
        })

        this.emitter.on('folderAdded', (folder: { selected?: any; }) => {
            self.selectedFolder = folder;
            folder.selected = true;
        })

        this.emitter.on('mediaListMoved', (errorInfo: never) => {
            self.loadFolder(self.selectedFolder);
            if (errorInfo) {
                self.errors.push(errorInfo);
            }
        })

        this.emitter.on('mediaRenamed', (newName: any, newPath: any, oldPath: any) => {
            let media = <IMedia>self.mediaItems.filter(function (item: IMedia) {
                return item.mediaPath === oldPath; // mediaPath ??? should it not be .url ?
            })[0];

            media.mediaPath = newPath;
            media.name = newName;
        })

        this.emitter.on('createFolderRequested', () => {
            self.createFolder();
        })

        this.emitter.on('deleteFolderRequested', () => {
            self.deleteFolder();
        })

        // common handlers for actions in both grid and table view.
        this.emitter.on('sortChangeRequested', (newSort: any) => {
            self.changeSort(newSort);
        })

        this.emitter.on('mediaToggleRequested', (media: any) => {
            self.toggleSelectionOfMedia(media);
        })

        this.emitter.on('renameMediaRequested', (media: any) => {
            self.renameMedia(media);
        })

        this.emitter.on('deleteMediaRequested', (media: any) => {
            self.deleteMediaItem(media);
        })

        this.emitter.on('mediaDragStartRequested', (media: any, e: any) => {
            self.handleDragStart(media, e);
        })

        // handler for pager events
        this.emitter.on('pagerEvent', (itemsInPage: never[]) => {
            self.itemsInPage = itemsInPage;
            self.selectedMedias = [];
        })

        if (!localStorage.getItem('mediaApplicationPrefs')) {
            self.selectedFolder = this.root;
            return;
        }

        let mediaApplicationPrefs = localStorage.getItem('mediaApplicationPrefs');

        if (mediaApplicationPrefs != null) {
            self.currentPrefs = JSON.parse(mediaApplicationPrefs);
        }
    },
    computed: {
        isHome: function () {
            return this.selectedFolder == this.root;
        },
        parents: function () {
            let p = [];
            let parentFolder = this.selectedFolder;
            while (parentFolder && parentFolder.path != '') {
                p.unshift(parentFolder);
                parentFolder = parentFolder.parent;
            }
            return p;
        },
        /*         root: function () {
                    return this.$data.root;
                }, */
        filteredMediaItems: function () {
            let self = this;

            self.selectedMedias = [];

            let filtered = self.mediaItems.filter(function (item: IMedia) {
                return item.name.toLowerCase().indexOf(self.mediaFilter.toLowerCase()) > - 1;
            });

            switch (self.sortBy) {
                case 'size':
                    filtered.sort(function (a, b) {
                        return self.sortAsc ? a.size - b.size : b.size - a.size;
                    });
                    break;
                case 'mime':
                    filtered.sort(function (a, b) {
                        return self.sortAsc ? a.mime.toLowerCase().localeCompare(b.mime.toLowerCase()) : b.mime.toLowerCase().localeCompare(a.mime.toLowerCase());
                    });
                    break;
                case 'lastModify':
                    filtered.sort(function (a, b) {
                        return self.sortAsc ? a.lastModify - b.lastModify : b.lastModify - a.lastModify;
                    });
                    break;
                default:
                    filtered.sort(function (a, b) {
                        return self.sortAsc ? a.name.toLowerCase().localeCompare(b.name.toLowerCase()) : b.name.toLowerCase().localeCompare(a.name.toLowerCase());
                    });
            }

            return filtered;
        },
        hiddenCount: function () {
            let result = 0;
            result = this.mediaItems.length - this.filteredMediaItems.length;
            return result;
        },
        thumbSize: function () {
            return this.smallThumbs ? 100 : 240;
        },
        currentPrefs: {
            get: function () {
                return {
                    smallThumbs: this.smallThumbs,
                    selectedFolder: this.selectedFolder,
                    gridView: this.gridView
                };
            },
            set: function (newPrefs) {
                if (!newPrefs) {
                    return;
                }

                this.smallThumbs = newPrefs.smallThumbs;
                this.selectedFolder = newPrefs.selectedFolder;
                this.gridView = newPrefs.gridView;
            }
        }
    },
    watch: {
        currentPrefs: function (newPrefs) {
            localStorage.setItem('mediaApplicationPrefs', JSON.stringify(newPrefs));
        },
        selectedFolder: function (newFolder) {
            this.mediaFilter = '';
            this.selectedFolder = newFolder;
            this.loadFolder(newFolder);
        }

    },
    mounted: function () {
        this.$refs.rootFolder.toggle();
    },
    methods: {
        uploadUrl: function () {

            if (!this.selectedFolder) {
                return null;
            }

            let urlValue = (<HTMLInputElement>document.getElementById('uploadFiles')).value;

            return urlValue + (urlValue.indexOf('?') == -1 ? '?' : '&') + "path=" + encodeURIComponent(this.selectedFolder.path);
        },
        selectRoot: function () {
            this.selectedFolder = this.root;
        },
        loadFolder: function (folder: IMediaElement) {
            this.errors = [];
            this.selectedMedias = [];
            let self = this;
            let mediaUrl = this.$props.basePath + this.$props.getMediaItemsUrl;
            debug("loadFolder (folder.path):", folder);

            if (mediaUrl != null) {
                axios.get(mediaUrl + (mediaUrl.indexOf('?') == -1 ? '?' : '&') + "path=" + encodeURIComponent(folder.path))
                    .then((response: { data: any; }) => {
                        response.data.forEach(function (item: { open: boolean; }) {
                            item.open = false;
                        });
                        self.mediaItems = response.data;
                        self.selectedMedias = [];
                        self.sortBy = '';
                        self.sortAsc = true;
                    })
                    .catch(() => {
                        debug('loadFolder: error loading folder:', folder);
                        self.selectRoot();
                    });
            }
        },
        selectAll: function () {
            this.selectedMedias = [];
            for (let i = 0; i < this.filteredMediaItems.length; i++) {
                this.selectedMedias.push(this.filteredMediaItems[i]);
            }
        },
        unSelectAll: function () {
            this.selectedMedias = [];
        },
        invertSelection: function () {
            let temp: never[] = [];
            for (let i = 0; i < this.filteredMediaItems.length; i++) {
                if (this.isMediaSelected(this.filteredMediaItems[i]) == false) {
                    temp.push(this.filteredMediaItems[i]);
                }
            }
            this.selectedMedias = temp;
        },
        toggleSelectionOfMedia: function (media: any) {
            if (this.isMediaSelected(media) == true) {
                this.selectedMedias.splice(this.selectedMedias.indexOf(media), 1);
            } else {
                this.selectedMedias.push(media);
            }
        },
        isMediaSelected: function (media: { url: string; }) {
            let result = this.selectedMedias?.some(function (element, index, array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        deleteFolder: function () {
            let folder = this.selectedFolder;
            let self = this;
            // The root folder can't be deleted
            if (folder == this.root.model) {
                return;
            }

            /*             confirmDialog({
                            ...$("#deleteFolder").data(), callback: function (resp) {
                                if (resp) {
                                    $.ajax({
                                        url: $('#deleteFolderUrl').val() + "?path=" + encodeURIComponent(folder.path),
                                        method: 'POST',
                                        data: {
                                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                                        },
                                        success: function (data) {
                                            bus.$emit('deleteFolder', folder);
                                        },
                                        error: function (error) {
                                            console.error(error.responseText);
                                        }
                                    });
                                }
                            }
                        }); */
        },
        createFolder: function () {
            (<HTMLElement>document.getElementById('createFolderModal-errors')).textContent = "";
            //modal.show();
            let createFolderModalInput = (<HTMLInputElement>document.querySelector('#createFolderModal .modal-body input'));
            createFolderModalInput.value = "";
            createFolderModalInput.focus();
        },
        renameMedia: function (media) {
            $('#renameMediaModal-errors').empty();
            let modal = bootstrap.Modal.getOrCreateInstance($('#renameMediaModal'));
            modal.show();
            $('#old-item-name').val(media.name);
            $('#renameMediaModal .modal-body input').val(media.name).focus();
        },
        selectAndDeleteMedia: function (media: any) {
            //this.deleteMedia();
        },
        deleteMediaList: function () {
            let mediaList = this.selectedMedias;
            let self = this;

            if (mediaList.length < 1) {
                return;
            }

            /*             confirmDialog({
                            ...$("#deleteMedia").data(), callback: function (resp) {
                                if (resp) {
                                    let paths = [];
                                    for (let i = 0; i < mediaList.length; i++) {
                                        paths.push(mediaList[i].mediaPath);
                                    }
            
                                    $.ajax({
                                        url: $('#deleteMediaListUrl').val(),
                                        method: 'POST',
                                        data: {
                                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val(),
                                            paths: paths
                                        },
                                        success: function (data) {
                                            for (let i = 0; i < self.selectedMedias.length; i++) {
                                                let index = self.mediaItems && self.mediaItems.indexOf(self.selectedMedias[i]);
                                                if (index > -1) {
                                                    self.mediaItems.splice(index, 1);
                                                    bus.$emit('mediaDeleted', self.selectedMedias[i]);
                                                }
                                            }
                                            self.selectedMedias = [];
                                        },
                                        error: function (error) {
                                            console.error(error.responseText);
                                        }
                                    });
                                }
                            }
                        }); */
        },
        deleteMediaItem: function (media) {
            let self = this;
            if (!media) {
                return;
            }

            /*             confirmDialog({
                            ...$("#deleteMedia").data(), callback: function (resp) {
                                if (resp) {
                                    $.ajax({
                                        url: $('#deleteMediaUrl').val() + "?path=" + encodeURIComponent(media.mediaPath),
                                        method: 'POST',
                                        data: {
                                            __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                                        },
                                        success: function (data) {
                                            let index = self.mediaItems && self.mediaItems.indexOf(media)
                                            if (index > -1) {
                                                self.mediaItems.splice(index, 1);
                                                bus.$emit('mediaDeleted', media);
                                            }
                                            //self.selectedMedia = null;
                                        },
                                        error: function (error) {
                                            console.error(error.responseText);
                                        }
                                    });
                                }
                            }
                        }); */
        },
        handleDragStart: function (media: IMedia, e: { dataTransfer: { setData: (arg0: string, arg1: string) => void; setDragImage: (arg0: HTMLImageElement, arg1: number, arg2: number) => void; effectAllowed: string; }; }) {
            // first part of move media to folder:
            // prepare the data that will be handled by the folder component on drop event
            let mediaNames = [];
            this.selectedMedias.forEach(function (item) {
                mediaNames.push(item.name);
            });

            // in case the user drags an unselected item, we select it first
            if (this.isMediaSelected(media) == false) {
                mediaNames.push(media.name);
                this.selectedMedias.push(media);
            }

            e.dataTransfer.setData('mediaNames', JSON.stringify(mediaNames));
            e.dataTransfer.setData('sourceFolder', this.selectedFolder.path);
            e.dataTransfer.setDragImage(this.dragDropThumbnail, 10, 10);
            e.dataTransfer.effectAllowed = 'move';
        },
        handleScrollWhileDrag: function (e: { clientY: number; }) {
            if (e.clientY < 150) {
                window.scrollBy(0, -10);
            }

            if (e.clientY > window.innerHeight - 100) {
                window.scrollBy(0, 10);
            }
        },
        changeSort: function (newSort: string) {
            if (this.sortBy == newSort) {
                this.sortAsc = !this.sortAsc;
            } else {
                this.sortAsc = true;
                this.sortBy = newSort;
            }
        }
    }
});
</script>
