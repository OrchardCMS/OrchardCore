<template>
    <div class="mediaApp" v-on:dragover="handleScrollWhileDrag">
        <div id="customdropzone">
            <h3>{{ t.DropHere }}</h3>
            <p>{{ t.DropTitle }}</p>
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
                    <folder :model="root" :t="t" :base-path="basePath" ref="rootFolder" :selected-in-media-app="selectedFolder"
                        :level="1">
                    </folder>
                </ol>
            </div>

            <div id="mediaContainerMain" v-cloak>
                <div class="media-container-top-bar">
                    <nav class="nav action-bar pb-3 pt-3 pl-3">
                        <div class="me-auto ms-4">
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="selectAll">
                                {{ t.SelectAll }}
                            </a>
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="unSelectAll"
                                :class="{ disabled: selectedMedias.length < 1 }">
                                {{ t.SelectNone }}
                            </a>
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="invertSelection">
                                {{ t.Invert }}
                            </a>
                            <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="deleteMediaList"
                                :class="{ disabled: selectedMedias.length < 1 }">
                                {{ t.Delete }} <span class="badge rounded-pill bg-light"
                                    v-show="selectedMedias.length > 0">{{
                                        selectedMedias.length }}</span>
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
                            <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm me-2"
                                :class="{ selected: !smallThumbs }" v-on:click="smallThumbs = false">
                                <span title="Large Thumbs"><fa-icon icon="fa-solid fa-expand"></fa-icon></span>
                            </button>
                        </div>

                        <div class="nav-item ms-2">
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
                        <div class="d-inline-flex ms-2 me-3 mb-1 pt-1">
                            <div class="btn-group btn-group-sm">
                                <label for="fileupload" class="btn btn-sm btn-primary fileinput-button upload-button">
                                    <input id="fileupload" type="file" name="files" multiple />
                                    <fa-icon icon="fa-solid fa-plus"></fa-icon>
                                    {{ t.Upload }}
                                </label>
                            </div>
                        </div>
                    </nav>

                    <nav id="breadcrumb" class="d-flex justify-content-end align-items-end">
                        <div class="breadcrumb-path p-3">
                            <span class="breadcrumb-item" :class="{ active: isHome }">
                                <a id="t-mediaLibrary" :href="isHome ? 'javascript:void(0)' : '#'"
                                    v-on:click="selectRoot">{{ t.MediaLibrary }}</a>
                            </span>
                            <span v-for="(folder, i) in parents" v-cloak class="breadcrumb-item"
                                :class="{ active: parents.length - i == 1 }">
                                <a :href="parents.length - i == 1 ? 'javascript:void(0)' : '#'"
                                    v-on:click="selectedFolder = folder;">{{
                                        folder.name }}</a>
                            </span>
                        </div>
                    </nav>
                </div>
                <div class="media-container-middle p-3">
                    <upload-list :t="t"></upload-list>

                    <media-items-table :t="t" :base-path="basePath" :sort-by="sortBy" :sort-asc="sortAsc"
                        :filtered-media-items="itemsInPage" :selected-medias="selectedMedias" :thumb-size="thumbSize"
                        v-show="itemsInPage.length > 0 && !gridView"></media-items-table>

                    <media-items-grid :t="t" :base-path="basePath" v-show="gridView" :filtered-media-items="itemsInPage"
                        :selected-medias="selectedMedias" :thumb-size="thumbSize"></media-items-grid>

                    <div class="alert-info p-2" v-show="mediaItems.length > 0 && filteredMediaItems.length < 1">{{
                        t.FolderFilterEmpty }}</div>
                    <div class="alert-info p-2" v-show="mediaItems.length < 1">{{ t.FolderEmpty }}</div>
                </div>
                <div v-show="filteredMediaItems.length > 0" class="media-container-footer p-3">
                    <pager :t="t" :source-items="filteredMediaItems"> </pager>
                </div>
            </div>
        </div>
    </div>
</template>
 
<style lang="scss">
@import "./assets/scss/media.scss";
</style>

<script>
import axios from 'axios';
import dbg from 'debug';
import FolderComponent from './components/folderComponent.vue';
import UploadListComponent from './components/uploadListComponent.vue';
import MediaItemsGridComponent from './components/mediaItemsGridComponent.vue';
import MediaItemsTableComponent from './components/mediaItemsTableComponent.vue';
import PagerComponent from './components/pagerComponent.vue';
import DragDropThumbnail from './assets/drag-thumbnail.png';
import { ModalsContainer, useModal } from 'vue-final-modal'
import ModalConfirm from './components/ModalConfirm.vue'

import "bootstrap/dist/css/bootstrap.min.css" // TODO remove

const debug = dbg("oc:media-app");

export default {
    components: {
        Folder: FolderComponent,
        UploadList: UploadListComponent,
        MediaItemsGrid: MediaItemsGridComponent,
        MediaItemsTable: MediaItemsTableComponent,
        Pager: PagerComponent,
        ModalsContainer: ModalsContainer,
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
            required: true,
        },
        translations: {
            type: String,
            required: true
        }
    },
    data() {
        return {
            t: Object,
            selectedFolder: {},
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
            }
        }
    },
    created: function () {
        let self = this;

        self.dragDropThumbnail.src = DragDropThumbnail;

        this.t = JSON.parse(this.$props.translations);

        this.emitter.on('folderSelected', (folder) => {
            self.selectedFolder = folder;
        })

        this.emitter.on('folderDeleted', () => {
            self.selectRoot();
        })

        this.emitter.on('folderAdded', (folder) => {
            self.selectedFolder = folder;
            folder.selected = true;
        })

        this.emitter.on('mediaListMoved', (errorInfo) => {
            self.loadFolder(self.selectedFolder);
            if (errorInfo) {
                self.errors.push(errorInfo);
            }
        })

        this.emitter.on('mediaRenamed', (newName, newPath, oldPath) => {
            let media = self.mediaItems.filter(function (item) {
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
        this.emitter.on('sortChangeRequested', (newSort) => {
            self.changeSort(newSort);
        })

        this.emitter.on('mediaToggleRequested', (media) => {
            self.toggleSelectionOfMedia(media);
        })

        this.emitter.on('renameMediaRequested', (media) => {
            self.renameMedia(media);
        })

        this.emitter.on('deleteMediaRequested', (media) => {
            self.deleteMediaItem(media);
        })

        this.emitter.on('mediaDragStartRequested', (media, e) => {
            self.handleDragStart(media, e);
        })

        // handler for pager events
        this.emitter.on('pagerEvent', (itemsInPage) => {
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

            let filtered = self.mediaItems.filter(function (item) {
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
        let me = this;
        this.$refs.rootFolder.toggle();

        var chunkedFileUploadId = crypto.randomUUID();

        $('#fileupload')
            .fileupload({
                dropZone: $('#mediaApp'),
                limitConcurrentUploads: 20,
                dataType: 'json',
                url: me.uploadUrl(),
                maxChunkSize: Number($('#maxUploadChunkSize').val() || 0),
                formData: function () {
                    var antiForgeryToken = $("input[name=__RequestVerificationToken]").val();

                    return [
                        { name: 'path', value: me.selectedFolder.path },
                        { name: '__RequestVerificationToken', value: antiForgeryToken },
                        { name: '__chunkedFileUploadId', value: chunkedFileUploadId },
                    ]
                },
                done: function (e, data) {
                    $.each(data.result.files, function (index, file) {
                        if (!file.error) {
                            me.mediaItems.push(file)
                        }
                    });
                }
            })
            .on('fileuploadchunkbeforesend', (e, options) => {
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



        $(document).bind('dragover', function (e) {
            var dt = e.originalEvent.dataTransfer;
            if (dt.types && (dt.types.indexOf ? dt.types.indexOf('Files') != -1 : dt.types.contains('Files'))) {
                var dropZone = $('#customdropzone'),
                    timeout = window.dropZoneTimeout;
                if (timeout) {
                    clearTimeout(timeout);
                } else {
                    dropZone.addClass('in');
                }
                var hoveredDropZone = $(e.target).closest(dropZone);
                window.dropZoneTimeout = setTimeout(function () {
                    window.dropZoneTimeout = null;
                    dropZone.removeClass('in');
                }, 100);
            }
        });

    },
    methods: {
        uploadUrl: function () {

            if (!this.selectedFolder) {
                return null;
            }

            let urlValue = this.basePath + this.$props.uploadFilesUrl;

            return urlValue + (urlValue.indexOf('?') == -1 ? '?' : '&') + "path=" + encodeURIComponent(this.selectedFolder.path);
        },
        selectRoot: function () {
            this.selectedFolder = this.root;
        },
        loadFolder: function (folder) {
            this.errors = [];
            this.selectedMedias = [];
            let self = this;
            let mediaUrl = this.$props.basePath + this.$props.getMediaItemsUrl;
            debug("loadFolder (folder.path):", folder);

            if (mediaUrl != null) {
                axios.get(mediaUrl + (mediaUrl.indexOf('?') == -1 ? '?' : '&') + "path=" + encodeURIComponent(folder.path))
                    .then((response) => {
                        response.data.forEach(function (item) {
                            item.open = false;
                        });
                        self.mediaItems = response.data;
                        self.selectedMedias = [];
                        self.sortBy = '';
                        self.sortAsc = true;
                    })
                    .catch((e) => {
                        debug('loadFolder: error loading folder:', folder, e);
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
            let temp = [];
            for (let i = 0; i < this.filteredMediaItems.length; i++) {
                if (this.isMediaSelected(this.filteredMediaItems[i]) == false) {
                    temp.push(this.filteredMediaItems[i]);
                }
            }
            this.selectedMedias = temp;
        },
        toggleSelectionOfMedia: function (media) {
            if (this.isMediaSelected(media) == true) {
                this.selectedMedias.splice(this.selectedMedias.indexOf(media), 1);
            } else {
                this.selectedMedias.push(media);
            }
        },
        isMediaSelected: function (media) {
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

            this.confirmDialog({
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
            });
        },
        createFolder: function () {
            //document.getElementById('createFolderModal-errors').empty();
            //modal.show();
            let createFolderModalInput = document.querySelector('#createFolderModal .modal-body input');
            createFolderModalInput.value = "";
            createFolderModalInput.focus();
        },
        renameMedia: function (element) {
            $('#renameMediaModal-errors').empty(); // TODO use a slot

            let self = this;
            let newName = element.newName;
            let media = element.media;

            debug("Rename media", newName, newName);

            let oldPath = media.url.replace('/media/', ''); // TODO make this better
            let newPath = oldPath.replace(media.name, newName);

            axios({
                method: 'post',
                headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                url: self.$props.basePath + self.$props.renameMediaUrl + "?oldPath=" + encodeURIComponent(oldPath) + "&newPath=" + encodeURIComponent(newPath),
            })
                .then((response) => {
                    this.emitter.emit('mediaRenamed', { newName, newPath, oldPath });
                })
                .catch((error) => {
                    $('#renameMediaModal-errors').empty();
                    $('<div class="alert alert-danger" role="alert"></div>').text(error.message).appendTo($('#renameMediaModal-errors'));
                });

            $('#old-item-name').val(media.name); // TODO remove probably
            $('#renameMediaModal .modal-body input').val(media.name).focus(); // TODO remove probably
        },
        selectAndDeleteMedia: function (media) {
            //this.deleteMedia();
        },
        deleteMediaList: function () {
            let mediaList = this.selectedMedias;
            let self = this;

            if (mediaList.length < 1) {
                return;
            }

            this.confirmDialog({
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
            });
        },
        deleteMediaItem: function (media) {
            let self = this;
            if (!media) {
                debug("Cannot delete null media item", media);
                return;
            }

            debug("delete media item", media);

            axios({
                method: 'post',
                headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
                url: self.$props.basePath + self.$props.deleteMediaUrl + "?path=" + encodeURIComponent(media.mediaPath)
            })
                .then((response) => {
                    let index = self.mediaItems && self.mediaItems.indexOf(media)
                    if (index > -1) {
                        self.mediaItems.splice(index, 1);
                        self.emitter.emit('mediaDeleted', media)
                    }
                })
                .catch((e) => {
                    debug('deleteMediaItem: error deleting media item:', media, e);
                    console.error(e);
                });
        },
        handleDragStart: function (media, e) {
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
        handleScrollWhileDrag: function (e) {
            if (e.clientY < 150) {
                window.scrollBy(0, -10);
            }

            if (e.clientY > window.innerHeight - 100) {
                window.scrollBy(0, 10);
            }
        },
        changeSort: function (newSort) {
            if (this.sortBy == newSort) {
                this.sortAsc = !this.sortAsc;
            } else {
                this.sortAsc = true;
                this.sortBy = newSort;
            }
        },
        confirmDialog: function ({ callback, ...options }) {
            const defaultOptions = $('#confirmRemoveModalMetadata').data();
            const { title, message, okText, cancelText, okClass, cancelClass } = $.extend({}, defaultOptions, options);

            $('<div id="confirmRemoveModal" class="modal" tabindex="-1" role="dialog">\
                <div class="modal-dialog modal-dialog-centered" role="document">\
                    <div class="modal-content">\
                        <div class="modal-header">\
                            <h5 class="modal-title">' + title + '</h5>\
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>\
                        </div>\
                        <div class="modal-body">\
                            <p>' + message + '</p>\
                        </div>\
                        <div class="modal-footer">\
                            <button id="modalOkButton" type="button" class="btn ' + okClass + '">' + okText + '</button>\
                            <button id="modalCancelButton" type="button" class="btn ' + cancelClass + '" data-bs-dismiss="modal">' + cancelText + '</button>\
                        </div>\
                    </div>\
                </div>\
            </div>').appendTo('body');

            var confirmModal = new bootstrap.Modal($('#confirmRemoveModal'), {
                backdrop: 'static',
                keyboard: false
            })

            confirmModal.show();

            document.getElementById('confirmRemoveModal').addEventListener('hidden.bs.modal', function () {
                document.getElementById('confirmRemoveModal').remove();
                confirmModal.dispose();
            });

            $('#modalOkButton').click(function () {
                callback(true);
                confirmModal.hide();
            });

            $('#modalCancelButton').click(function () {
                callback(false);
                confirmModal.hide();
            });
        }
    }
};
</script>
