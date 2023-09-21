<template>
    <li :class="{ selected: isSelected }" v-on:dragleave.prevent="handleDragLeave($event);"
        v-on:dragover.prevent.stop="handleDragOver($event);" v-on:drop.prevent.stop="moveMediaToFolder(model, $event)">
        <ModalConfirm :modal-name="getModalName('media', 'move')" :title="t.MoveMediaTitle"
            @confirm="(elem) => confirm(elem, 'move')">
            <p>{{ t.MoveMediaMessage }}</p>
        </ModalConfirm>
        <div :class="{ folderhovered: isHovered, treeroot: level == 1 }">
            <a href="javascript:void(0)" :style="{ 'padding-left': padding + 'px' }" v-on:click="select" draggable="false"
                class="folder-menu-item">
                <span v-on:click.stop="toggle" class="expand">
                    <fa-icon v-if="!empty && open" icon="fas fa-chevron-down"></fa-icon>
                    <fa-icon v-if="!empty && !open" icon="fas fa-chevron-up"></fa-icon>
                </span>
                <div class="folder-name ms-2">{{ model?.name }}</div>
                <div class="btn-group folder-actions">
                    <a v-cloak href="javascript:void(0)" class="btn btn-sm" @click="() => openModal('folder', 'create')"
                        v-if="isSelected || isRoot"><fa-icon icon="fas fa-plus"></fa-icon>
                        <ModalInputConfirm action-name="Create folder" :modal-name="getModalName('folder', 'create')"
                            :new-name="t.NewFolder" :title="t.CreateFolderTitle"
                            @confirm="(folderName) => confirm(folderName, 'create')">
                            <p>{{ t.CreateFolderMessage }}</p>
                        </ModalInputConfirm>
                    </a>
                    <a v-cloak href="javascript:void(0)" class="btn btn-sm" @click="() => openModal('folder', 'delete')"
                        v-if="isSelected && !isRoot"><fa-icon icon="fas fa-trash"></fa-icon>
                        <ModalConfirm :modal-name="getModalName('folder', 'delete')" :title="t.DeleteFolderTitle"
                            @confirm="() => confirm('folder', 'delete')">
                            <p>{{ t.DeleteFolderMessage }}</p>
                        </ModalConfirm>
                    </a>
                </div>
            </a>
        </div>
        <ol v-show="open">
            <folder :move-media-list-url="moveMediaListUrl" v-for="folder in children" :base-path="basePath" :t="t" :key="folder.path" :model="folder"
                :selected-in-media-app="selectedInMediaApp" :level="(level ? level : 0) + 1">
            </folder>
        </ol>
    </li>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import axios from 'axios';
import dbg from 'debug';
import { useVfm } from 'vue-final-modal'
import ModalConfirm from './ModalConfirm.vue'
import ModalInputConfirm from './ModalInputConfirm.vue'
import { IMedia } from '../interfaces/interfaces';

const debug = dbg("oc:media-app");
let moveAssetsState = {};

export default defineComponent({
    components: {
        ModalConfirm: ModalConfirm,
        ModalInputConfirm: ModalInputConfirm,
    },
    name: "folder",
    props: {
        model: Object,
        selectedInMediaApp: Object,
        level: Number,
        basePath: {
            type: String,
            required: true
        },
        t: {
            type: Object,
            required: true,
        },
        moveMediaListUrl: {
            type: String,
            required: true
        },
    },
    data() {
        return {
            open: false,
            children: [], // not initialized state (for lazy-loading)
            parent: null,
            isHovered: false,
            padding: 0,
            getFoldersUrl: document.getElementById('mediaApp')?.dataset.getFoldersUrl,
        }
    },
    computed: {
        empty: function () {
            return !this.children || this.children.length == 0;
        },
        isSelected: function () {
            return (this.selectedInMediaApp?.name == this.model?.name) && (this.selectedInMediaApp?.path == this.model?.path);
        },
        isRoot: function () {
            return this.model?.path === '';
        }
    },
    mounted() {
        if ((this.isRoot == false) && (this.isAncestorOfSelectedFolder())) {
            this.toggle();
        }

        let level = this.level ? this.level : 0;

        this.padding = level < 3 ? 16 : 16 + (level * 8);
    },
    created: function () {
        let self = this;

        this.emitter.on('deleteFolder', function (folder) {
            if (self.children) {
                let index = self.children && self.children.indexOf(folder)
                if (index > -1) {
                    self.children.splice(index, 1)
                    self.emitter.emit('folderDeleted');
                }
            }
        });

        this.emitter.on('addFolder', function (element) {
            let target = element.selectedFolder;
            let folder = element.data;

            if (self.model == target) {
                if (self.children !== null) {
                    self.children.push(folder);
                    self.children.sort(function (a, b) {
                        if (a.name > b.name) { return -1; }
                        if (a.name < b.name) { return 1; }
                        return 0;
                    });
                }

                folder.parent = self.model;
                self.emitter.emit('folderAdded', folder);
            }
        });
    },
    methods: {
        isAncestorOfSelectedFolder: function () {
            let parentFolder = mediaApp.selectedFolder;

            while (parentFolder) {
                if (parentFolder.path == this.model.path) {
                    return true;
                }
                parentFolder = parentFolder.parent;
            }

            return false;
        },
        toggle: function () {
            this.open = !this.open;

            if (this.open && !this.children) {
                this.loadChildren();
            }
        },
        select: function () {
            this.emitter.emit('folderSelected', this.model);
            this.loadChildren();
        },
        createFolder: function (media: String) {
            this.emitter.emit('createFolderRequested', media);
        },
        deleteFolder: function () {
            this.emitter.emit('deleteFolderRequested');
        },
        loadChildren: function () {
            let self = this;

            if (this.open == false) {
                this.open = true;
            }

            axios.get(this.basePath + this.getFoldersUrl + "?path=" + encodeURIComponent(self.model?.path))
                .then((response) => {
                    self.children = response.data;
                    self.children.forEach(function (c) {
                        c.parent = self.model;
                    });
                })
                .catch((error) => {
                    //emtpy = false;
                    console.error(error.responseText);
                });
        },
        handleDragOver: function (e) {
            this.isHovered = true;
        },
        handleDragLeave: function (e) {
            this.isHovered = false;
        },
        moveMediaToFolder: function (folder, e) {
            debug("Move media to folder", folder, e);
            let self = this;
            self.isHovered = false;

            let mediaNames = JSON.parse(e.dataTransfer.getData('mediaNames'));

            if (mediaNames.length < 1) {
                return;
            }

            let sourceFolder = e.dataTransfer.getData('sourceFolder');
            let targetFolder = folder.path;

            if (sourceFolder === '') {
                sourceFolder = 'root';
            }

            if (targetFolder === '') {
                targetFolder = 'root';
            }

            if (sourceFolder === targetFolder) {
                alert(this.$props.t.SameFolderMessage);
                return;
            }

            moveAssetsState = {
                mediaNames: mediaNames,
                sourceFolder: sourceFolder,
                targetFolder: targetFolder
            };

            const uVfm = useVfm();

            uVfm.open(this.getModalName('media', 'move'));
        },
        getModalName: function (name: String, action: String) {
            return action + "-media-item-table-" + name;
        },
        openModal: function (media: String, action: String) {
            const uVfm = useVfm();

            uVfm.open(this.getModalName(media, action));
        },
        confirm: function (media: String, action: String) {
            let self = this;
            const uVfm = useVfm();

            if (action == "delete") {
                this.deleteFolder();

                uVfm.close(this.getModalName(media, action));
            }
            else if (action == "create") {
                //debug("Confirm folder create:", newName);
                this.createFolder(media);

                uVfm.close(this.getModalName('folder', action));
            }
            else if (action == "move") {
                if (moveAssetsState.mediaNames.length > 0) {
                    self.emitter.emit('mediaListMove', moveAssetsState);
                }

                uVfm.close(this.getModalName('media', action));
                moveAssetsState = {};
            }
        },
    }
});
</script>


