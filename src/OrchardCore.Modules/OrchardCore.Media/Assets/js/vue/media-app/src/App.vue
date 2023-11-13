<template>
    <div class="mediaApp" v-on:dragover="handleScrollWhileDrag">
        <div class="alert alert-danger message-warning" v-if="errors.length > 0">
            <ul>
                <li v-for="(e, i) in errors" :key="i">{{ e }}</li>
            </ul>
        </div>
        <div id="customdropzone">
            <h3>{{ t.DropHere }}</h3>
            <p>{{ t.DropTitle }}</p>
        </div>
        <div id="mediaContainer" class="align-items-stretch">
            <div id="navigationApp" class="media-container-navigation m-0 p-0" v-cloak>
                <ol id="folder-tree">
                    <folder :model="root" :t="t" :base-path="basePath" ref="rootFolder"
                        :selected-in-media-app="selectedFolder" :level="1">
                    </folder>
                </ol>
            </div>

            <div id="mediaContainerMain" v-cloak>
                <div class="media-container-top-bar">
                    <nav id="breadcrumb" class="d-flex justify-content-end align-items-end">
                        <div class="breadcrumb-path p-3">
                            <span class="breadcrumb-item" :class="{ active: isHome }">
                                <a id="t-mediaLibrary" :href="isHome ? 'javascript:void(0)' : '#'"
                                    v-on:click="selectRoot">{{
                                        t.FolderRoot }}</a>
                            </span>
                            <span v-for="(folder, i) in parents" :key="folder.path" v-cloak class="breadcrumb-item"
                                :class="{ active: parents.length - i == 1, 'no-breadcrumb-divider': i == 0 }">
                                <a :href="parents.length - i == 1 ? 'javascript:void(0)' : '#'"
                                    v-on:click="selectedFolder = folder;">{{
                                        folder.name }}</a>
                            </span>
                        </div>
                    </nav>
                    <nav class="nav action-bar p-3 flex">
                        <div class="me-auto">
                            <a :title="isSelectedAll ? t.SelectNone : t.SelectAll" href="javascript:void(0)"
                                class="btn btn-light btn-sm me-2" v-on:click="selectAll">
                                <fa-icon v-if="isSelectedAll" icon="fa-regular fa-square-check"></fa-icon>
                                <fa-icon v-if="!isSelectedAll" icon="fa-regular fa-square"></fa-icon>
                            </a>
                            <a :title="t.Invert" href="javascript:void(0)" class="btn btn-light btn-sm me-2"
                                v-on:click="invertSelection">
                                <fa-icon icon="fa-solid fa-right-left"></fa-icon>
                            </a>
                            <a :title="t.Delete" href="javascript:void(0)" class="btn btn-light btn-sm me-2"
                                @click="() => openModal('nav', 'delete')" :class="{ disabled: selectedMedias.length < 1 }">
                                <fa-icon icon="fa-solid fa-trash"></fa-icon>
                                <span class="badge rounded-pill ms-1" v-show="selectedMedias.length > 0">{{
                                    selectedMedias.length }}</span>
                                <ModalConfirm :t="t" :action-name="t.Delete" :modal-name="getModalName('nav', 'delete')"
                                    :title="t.DeleteMediaTitle" @confirm="() => confirm('nav', 'delete')">
                                    <p>{{ t.DeleteMediaMessage }}</p>
                                </ModalConfirm>
                            </a>
                        </div>
                        <div class="btn-group visibility-buttons">
                            <button type="button" id="toggle-grid-table-button" class="btn btn-light btn-sm"
                                :class="{ selected: gridView }" v-on:click="gridView = true">
                                <span title="Grid View"><fa-icon icon="fa-solid fa-th-large"></fa-icon></span>
                            </button>
                            <button type="button" id="toggle-grid-table-button" class="btn btn-light btn-sm"
                                :class="{ selected: !gridView }" v-on:click="gridView = false">
                                <span title="List View"><fa-icon icon="fa-solid fa-th-list"></fa-icon></span>
                            </button>
                        </div>
                        <div class="btn-group visibility-buttons" v-show="gridView">
                            <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm"
                                :class="{ selected: smallThumbs }" v-on:click="smallThumbs = true">
                                <span title="Small Thumbs"><fa-icon icon="fa-solid fa-compress"></fa-icon></span>
                            </button>
                            <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm"
                                :class="{ selected: !smallThumbs }" v-on:click="smallThumbs = false">
                                <span title="Large Thumbs"><fa-icon icon="fa-solid fa-expand"></fa-icon></span>
                            </button>
                        </div>

                        <div class="nav-item ms-3 me-2">
                            <div class="media-filter">
                                <div class="input-group input-group-sm">
                                    <fa-icon icon="fa-solid fa-filter icon-inside-input"></fa-icon>
                                    <input type="text" id="media-filter-input" v-model="mediaFilter"
                                        class="form-control input-filter" :placeholder="t.Filter" :aria-label="t.Filter" />
                                    <button id="clear-media-filter-button" class="btn btn-outline-secondary" type="button"
                                        :disabled="mediaFilter == ''" v-on:click="mediaFilter = ''"><fa-icon
                                            icon="fa-solid fa-times"></fa-icon></button>
                                </div>
                            </div>
                        </div>
                        <div class="d-inline-flex mb-1 pt-1">
                            <div class="btn-group btn-group-sm">
                                <label :title="t.UploadFiles" for="fileupload"
                                    class="btn btn-sm btn-primary fileinput-button upload-button">
                                    <input id="fileupload" type="file" name="files" multiple />
                                    <fa-icon icon="fa-solid fa-plus"></fa-icon>
                                    {{ t.Upload }}
                                </label>
                            </div>
                        </div>
                    </nav>
                </div>
                <div class="media-container-middle p-3">
                    <upload-list upload-input-id="fileupload" :t="t"></upload-list>

                    <media-items-table :t="t" :base-path="basePath" :sort-by="sortBy" :sort-asc="sortAsc"
                        :filtered-media-items="itemsInPage" :selected-medias="selectedMedias" :thumb-size="thumbSize"
                        v-show="itemsInPage.length > 0 && !gridView"></media-items-table>

                    <media-items-grid :t="t" :base-path="basePath" v-show="gridView" :filtered-media-items="itemsInPage"
                        :selected-medias="selectedMedias" :thumb-size="thumbSize"></media-items-grid>

                    <div class="alert alert-info p-2" v-show="mediaItems.length > 0 && filteredMediaItems.length < 1">{{
                        t.FolderFilterEmpty }}</div>
                    <div class="alert alert-info p-2" v-show="mediaItems.length < 1">{{ t.FolderEmpty }}</div>
                </div>
                <div v-show="filteredMediaItems.length > 0" class="media-container-footer p-3 pb-0">
                    <pager :t="t" :source-items="filteredMediaItems"> </pager>
                </div>
            </div>
        </div>
    </div>
</template>
   
<style lang="scss">
@import "./assets/scss/media.scss";
</style>
  
<script lang="ts">
import { defineComponent, ref } from 'vue'
import dbg from 'debug';
import FolderComponent from './components/FolderComponent.vue';
import UploadListComponent from './components/UploadListComponent.vue';
import MediaItemsGridComponent from './components/MediaItemsGridComponent.vue';
import MediaItemsTableComponent from './components/MediaItemsTableComponent.vue';
import PagerComponent from './components/PagerComponent.vue';
import DragDropThumbnail from './assets/drag-thumbnail.png';
import { ModalsContainer, useVfm } from 'vue-final-modal'
import ModalConfirm from './components/ModalConfirm.vue'
import { MediaApiClient, MoveMedia } from "./services/MediaApiClient";
import { notify, registerNotificationBus } from "./services/Notifier";

const debug = dbg("oc:media-app");
const rootFolder = ref(null);

export default defineComponent({
    components: {
        Folder: FolderComponent,
        UploadList: UploadListComponent,
        MediaItemsGrid: MediaItemsGridComponent,
        MediaItemsTable: MediaItemsTableComponent,
        Pager: PagerComponent,
        ModalsContainer: ModalsContainer,
        ModalConfirm: ModalConfirm
    },
    name: "media-app",
    props: {
        basePath: {
            type: String,
            required: true
        },
        siteId: {
            type: String,
            required: true
        },
        translations: {
            type: String,
            required: true
        },
        uploadFilesUrl: {
            type: String,
            required: true
        },
        maxUploadChunkSize: {
            type: Number,
            required: true
        }
    },
    data() {
        return {
            t: <any>Object,
            selectedFolder: <any>{},
            mediaItems: <any>[],
            selectedMedias: <any>[],
            isSelectedAll: false,
            errors: <any>[],
            dragDropThumbnail: new Image(),
            smallThumbs: false,
            gridView: false,
            mediaFilter: '',
            sortBy: '',
            sortAsc: true,
            itemsInPage: <any>[],
            root: {
                name: document.querySelector('#t-mediaLibrary')?.textContent,
                path: '',
                folder: '',
                isDirectory: true
            }
        }
    },
    created: function () {
        let self = this;

        self.dragDropThumbnail.src = DragDropThumbnail;

        this.t = JSON.parse(this.$props.translations);

        this.emitter.on('folderSelected', (folder: any) => {
            self.selectedFolder = folder;
            self.isSelectedAll = false;
        })

        this.emitter.on('folderDeleted', () => {
            self.selectRoot();
        })

        this.emitter.on('folderAdded', (folder: any) => {
            self.selectedFolder = folder;
            folder.selected = true;
        })

        this.emitter.on('mediaListMove', (elem: any) => {
            self.mediaListMove(elem);
        })

        this.emitter.on('mediaListMoved', (errorInfo: never) => {
            self.loadFolder(self.selectedFolder);
        })

        this.emitter.on('mediaRenamed', (element: any) => {
            self.loadFolder(self.selectedFolder);
        })

        this.emitter.on('createFolderRequested', (folderName: any) => {
            self.createFolder(folderName);
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
            self.isSelectedAll = false;
        })

        this.emitter.on('renameMediaRequested', (media: any) => {
            self.renameMedia(media);
        })

        this.emitter.on('deleteMediaRequested', (media: any) => {
            self.deleteMediaItem(media);
        })

        this.emitter.on('mediaDragStartRequested', (media: any) => {
            self.handleDragStart(media);
        })

        // handler for pager events
        this.emitter.on('pagerEvent', (itemsInPage: any) => {
            self.itemsInPage = itemsInPage;
            self.selectedMedias = [];
        })

        if (!localStorage.getItem('MediaLibraryPrefs' + "-" + this.$props.siteId)) {
            self.selectedFolder = this.root;
            return;
        }

        let mediaApplicationPrefs = localStorage.getItem('MediaLibraryPrefs' + "-" + this.$props.siteId);

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

            let filtered = self.mediaItems.filter(function (item: any) {
                return item.name.toLowerCase().indexOf(self.mediaFilter.toLowerCase()) > - 1;
            });

            switch (self.sortBy) {
                case 'size':
                    filtered.sort(function (a: any, b: any) {
                        return self.sortAsc ? a.size - b.size : b.size - a.size;
                    });
                    break;
                case 'mime':
                    filtered.sort(function (a: any, b: any) {
                        return self.sortAsc ? a.mime.toLowerCase().localeCompare(b.mime.toLowerCase()) : b.mime.toLowerCase().localeCompare(a.mime.toLowerCase());
                    });
                    break;
                case 'lastModify':
                    filtered.sort(function (a: any, b: any) {
                        return self.sortAsc ? a.lastModify - b.lastModify : b.lastModify - a.lastModify;
                    });
                    break;
                default:
                    filtered.sort(function (a: any, b: any) {
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
            return this.smallThumbs ? 160 : 240;
        },
        currentPrefs: {
            get: function () {
                return {
                    smallThumbs: this.smallThumbs,
                    selectedFolder: this.selectedFolder,
                    gridView: this.gridView
                };
            },
            set: function (newPrefs: any) {
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
            localStorage.setItem('MediaLibraryPrefs' + "-" + this.$props.siteId, JSON.stringify(newPrefs));
        },
        selectedFolder: function (newFolder) {
            this.mediaFilter = '';
            this.selectedFolder = newFolder;
            this.loadFolder(newFolder);
        }

    },
    mounted: function () {
        let me = this;
        registerNotificationBus();

        if (me.currentPrefs.selectedFolder != null) {
            (<any>me.$refs.rootFolder).selectFolder(me.currentPrefs.selectedFolder);
        }
        else {
            (<any>me.$refs.rootFolder).select();
        }

        let chunkedFileUploadId = crypto.randomUUID();

        let fileInput = $('#fileupload');

        fileInput
            .fileupload({
                dropZone: $('#mediaApp'),
                limitConcurrentUploads: 20,
                dataType: 'json',
                url: me.uploadFilesUrl,
                maxChunkSize: me.maxUploadChunkSize,
                formData: function () {
                    var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                    return [
                        { name: 'path', value: me.selectedFolder.path },
                        { name: '__RequestVerificationToken', value: antiForgeryToken },
                        { name: '__chunkedFileUploadId', value: chunkedFileUploadId },
                    ]
                },
                done: function (e: any, data: any) {
                    $.each(data.result.files, function (index, file: any) {
                        if (!file.error) {
                            me.mediaItems.push(<never>file)
                        }
                    });
                }
            })
            .on('fileuploadchunkbeforesend', (e: any, options: any) => {
                let file = options.files[0];
                // Here we replace the blob with a File object to ensure the file name and others are preserved for the backend.
                options.blob = new File(
                    [options.blob],
                    file.name,
                    {
                        type: file.type,
                        lastModified: file.lastModified,
                    });
            });

        $(document).on('dragover', function (e: any) {
            let dt = e.originalEvent.dataTransfer;

            if (dt.types && (dt.types.indexOf ? dt.types.indexOf('Files') != -1 : dt.types.contains('Files'))) {
                let dropZone = $('#customdropzone'),
                    timeout = (<any>window).dropZoneTimeout;

                if (timeout) {
                    clearTimeout(timeout);
                } else {
                    dropZone.addClass('in');
                }

                (<any>window).dropZoneTimeout = setTimeout(function () {
                    (<any>window).dropZoneTimeout = null;
                    dropZone.removeClass('in');
                }, 100);
            }
        });
    },
    methods: {
        getModalName: function (name: String, action: String) {
            return action + "-media-" + name;
        },
        openModal: function (media: String, action: String) {
            const uVfm = useVfm();

            uVfm.open(this.getModalName(media, action));
        },
        confirm: function (media: String, action: String) {
            const uVfm = useVfm();

            if (action == "delete") {
                this.deleteMediaList();
            }

            uVfm.close(this.getModalName(media, action));
        },
        selectRoot: function () {
            this.selectedFolder = this.root;
        },
        mediaListMove: function (elem: any) {
            let self = this;

            if (elem) {

                const apiClient = new MediaApiClient(this.basePath);
                apiClient
                    .moveMediaList(new MoveMedia({
                        mediaNames: elem.mediaNames,
                        sourceFolder: elem.sourceFolder,
                        targetFolder: elem.targetFolder,
                    }))
                    .then((res) => {
                        self.emitter.emit('mediaListMoved'); // MediaApp will listen to this, and then it will reload page so the moved medias won't be there anymore
                    })
                    .catch(async (error) => {
                        error.response.text().then((text: any) => {
                            const error = JSON.parse(text);
                            self.emitter.emit('mediaListMoved', error.detail);
                            notify({ summary: self.t.ErrorMovingFile, detail: error.detail, severity: SeverityLevel.Error });
                        })
                    })
            }
        },
        loadFolder: function (folder: any) {
            this.errors = [];
            this.selectedMedias = [];
            let self = this;

            if (this.basePath != null) {
                const apiClient = new MediaApiClient(this.basePath);
                apiClient
                    .getMediaItems(folder.path, null)
                    .then((response: any) => {
                        response.forEach(function (item: any) {
                            item.open = false;
                        });
                        self.mediaItems = response;
                        self.selectedMedias = [];
                        self.sortBy = '';
                        self.sortAsc = true;
                    })
                    .catch(async (error) => {
                        debug('loadFolder: error loading folder:', folder, error);
                        self.selectRoot();
                    })
            }
        },
        selectAll: function () {
            if (this.isSelectedAll) {
                this.selectedMedias = [];
                this.isSelectedAll = false;
            }
            else {
                this.selectedMedias = [];
                for (let i = 0; i < this.filteredMediaItems.length; i++) {
                    this.selectedMedias.push(this.filteredMediaItems[i]);
                }
                this.isSelectedAll = true;
            }
        },
        invertSelection: function () {
            this.isSelectedAll = false;
            let temp = [];
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
        isMediaSelected: function (media: any) {
            let result = this.selectedMedias?.some(function (element: any, index: any, array: any) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        deleteFolder: function () {
            let folder = this.selectedFolder;
            let self = this;
            // The root folder can't be deleted
            if (folder == this.root) {
                return;
            }

            const config = {
                headers: {
                    "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val()
                }
            }

            const apiClient = new MediaApiClient(this.basePath);
            apiClient
                .deleteFolder(folder.path)
                .then((response: any) => {
                    self.emitter.emit('deleteFolder', folder);
                })
                .catch(async (error) => {
                    error.response.text().then((text: any) => {
                        const error = JSON.parse(text);
                        debug('deleteFolder error: ', folder, error.detail);
                        notify({ summary: self.t.ErrorDeleteFolder, detail: error.detail, severity: SeverityLevel.Error });
                    })
                })
        },
        createFolder: function (folderName: any) {
            let self = this;
            $('createFolderModal-errors')?.empty();

            if (folderName === "") {
                return;
            }

            const apiClient = new MediaApiClient(this.basePath);
            apiClient
                .createFolder(self.selectedFolder.path, folderName)
                .then((response: any) => {
                    self.emitter.emit('addFolder', { selectedFolder: self.selectedFolder, data: response });
                })
                .catch(async (error) => {
                    notify({ summary: self.t.ErrorCreateFolder, detail: error.response.detail, severity: SeverityLevel.Error });
                })
        },
        renameMedia: function (element: any) {
            let self = this;
            let newName = element.newName;
            let media = element.media;

            debug("Rename media", media.name, newName, this.basePath);

            const oldPath = media.mediaPath; // TODO make this better
            const newPath = oldPath.replace(media.name, newName);

            const apiClient = new MediaApiClient(this.basePath);
            apiClient
                .moveMedia(oldPath, newPath)
                .then((response: any) => {
                    self.emitter.emit('mediaRenamed', { newName: newName, newPath: newPath, oldPath: oldPath });
                })
                .catch(async (error) => {
                    error.response.text().then((text: any) => {
                        const error = JSON.parse(text);
                        notify({ summary: self.t.ErrorRenamingFile, detail: error.detail, severity: SeverityLevel.Error });
                    })
                })
        },
        deleteMediaList: function () {
            let mediaList = this.selectedMedias;
            let self = this;

            if (mediaList.length < 1) {
                return;
            }

            let imagePaths = [];

            for (let i = 0; i < mediaList.length; i++) {
                imagePaths.push(mediaList[i].mediaPath);
            }

            const apiClient = new MediaApiClient(this.basePath);
            apiClient
                .deleteMediaList(imagePaths)
                .then((response: any) => {
                    for (let i = 0; i < self.selectedMedias.length; i++) {
                        let index = self.mediaItems && self.mediaItems.indexOf(self.selectedMedias[i]);
                        if (index > -1) {
                            self.mediaItems.splice(index, 1);
                            self.emitter.emit('mediaDeleted', self.selectedMedias[i]);
                        }
                    }
                    self.selectedMedias = [];
                    self.isSelectedAll = false;
                })
                .catch(async (error) => {
                    error.response.text().then((text: any) => {
                        const error = JSON.parse(text);
                        notify({ summary: self.t.ErrorDeleteFiles, detail: error.detail, severity: SeverityLevel.Error });
                    })
                })
        },
        deleteMediaItem: function (media: any) {
            let self = this;
            if (!media) {
                debug("Cannot delete null media item", media);
                return;
            }

            debug("delete media item", media);

            const apiClient = new MediaApiClient(this.basePath);
            apiClient
                .deleteMedia(media.mediaPath)
                .then((response: any) => {
                    let index = self.mediaItems && self.mediaItems.indexOf(media)
                    if (index > -1) {
                        self.mediaItems.splice(index, 1);
                        self.emitter.emit('mediaDeleted', media)
                    }
                })
                .catch(async (error) => {
                    error.response.text().then((text: any) => {
                        const error = JSON.parse(text);
                        debug('deleteMediaItem: error deleting media item:', media, error);
                        notify({ summary: self.t.ErrorDeleteFile, detail: error.detail, severity: SeverityLevel.Error });
                    })
                })
        },
        handleDragStart: function (element: any) {
            // first part of move media to folder:
            // prepare the data that will be handled by the folder component on drop event
            let mediaNames = [];
            this.selectedMedias.forEach(function (item: any) {
                mediaNames.push(item.name);
            });

            // in case the user drags an unselected item, we select it first
            if (this.isMediaSelected(element.media) == false) {
                mediaNames.push(element.media.name);
                this.selectedMedias.push(element.media);
            }

            element.e.dataTransfer.setData('mediaNames', JSON.stringify(mediaNames));
            element.e.dataTransfer.setData('sourceFolder', this.selectedFolder.path);
            element.e.dataTransfer.setDragImage(this.dragDropThumbnail, 10, 10);
            element.e.dataTransfer.effectAllowed = 'move';
        },
        handleScrollWhileDrag: function (e: any) {
            if (e.clientY < 150) {
                window.scrollBy(0, -10);
            }

            if (e.clientY > window.innerHeight - 100) {
                window.scrollBy(0, 10);
            }
        },
        changeSort: function (newSort: any) {
            if (this.sortBy == newSort) {
                this.sortAsc = !this.sortAsc;
            } else {
                this.sortAsc = true;
                this.sortBy = newSort;
            }
        },
    }
});
</script>
  