<template>
    <li :class="{ selected: isSelected }" v-on:dragleave.prevent="handleDragLeave();"
        v-on:dragover.prevent.stop="handleDragOver();" v-on:drop.prevent.stop="moveMediaToFolder(model, $event)">
        <ModalConfirm :t="t" :action-name="t.MoveMediaTitle" :modal-name="getModalName('media', 'move')"
            :title="t.MoveMediaTitle" @confirm="() => confirm('media', 'move')">
            <label>{{ t.MoveMediaMessage }}</label>
        </ModalConfirm>
        <div :class="{ folderhovered: isHovered, treeroot: level == 1 }">
            <a href="javascript:void(0)" :style="{ 'padding-left': padding + 'px' }" v-on:click="select" draggable="false"
                class="folder-menu-item">
                <span v-on:click.stop="toggle" class="expand">
                    <fa-icon v-if="open" icon="fas fa-chevron-down"></fa-icon>
                    <fa-icon v-if="!open" icon="fas fa-chevron-up"></fa-icon>
                </span>
                <div class="folder-name ms-2">{{ model?.name ?? t.MediaLibrary }}</div>
                <div class="btn-group folder-actions">
                    <a v-cloak href="javascript:void(0)" :title="t.CreateFolderTitle" class="btn btn-primary btn-sm"
                        @click="() => openModal('folder', 'create')" v-if="isSelected || isRoot"><fa-icon
                            icon="fas fa-plus"></fa-icon>
                        <ModalInputConfirm :t="t" :action-name="t.CreateFolderTitle"
                            :modal-name="getModalName('folder', 'create')" :new-name="t.NewFolder"
                            :title="t.CreateFolderTitle" @confirm="(folderName) => confirm(folderName, 'create')">
                            <label>{{ t.CreateFolderMessage }}</label>
                        </ModalInputConfirm>
                    </a>
                    <a v-cloak href="javascript:void(0)" :title="t.DeleteFolderTitle" class="btn btn-primary btn-sm"
                        @click="() => openModal('folder', 'delete')" v-if="isSelected && !isRoot"><fa-icon
                            icon="fas fa-trash"></fa-icon>
                        <ModalConfirm :t="t" :action-name="t.Delete" :modal-name="getModalName('folder', 'delete')"
                            :title="t.DeleteFolderTitle" @confirm="() => confirm('folder', 'delete')">
                            <label>{{ t.DeleteFolderMessage }}</label>
                        </ModalConfirm>
                    </a>
                </div>
            </a>
        </div>
        <ol v-show="open">
            <folder v-for="folder in children" :base-path="basePath" :t="t" :key="folder.path" :model="folder"
                :selected-in-media-app="selectedInMediaApp" :level="(level ? level : 0) + 1">
            </folder>
        </ol>
    </li>
</template>
  
<script lang="ts">
import { defineComponent } from 'vue'
import dbg from 'debug';
import { useVfm } from 'vue-final-modal'
import ModalConfirm from './ModalConfirm.vue'
import ModalInputConfirm from './ModalInputConfirm.vue'
import { MediaApiClient } from "../services/MediaApiClient";
import { SeverityLevel } from "../interfaces/Interfaces"
import { notify } from "../services/Notifier.js";

const debug = dbg("oc:media-app");
let moveAssetsState = <any>{};

export default defineComponent({
    components: {
        ModalConfirm: ModalConfirm,
        ModalInputConfirm: ModalInputConfirm,
    },
    expose: ['select', 'selectFolder'],
    name: "folder",
    props: {
        model: <any>Object,
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
    },
    data() {
        return {
            open: false,
            children: <any>[], // not initialized state (for lazy-loading)
            parent: null,
            isHovered: false,
            padding: 0,
        }
    },
    computed: {
        empty: function () {
            return !this.children || this.children.length == 0;
        },
        isSelected: function () {
            return (this.selectedInMediaApp?.name == this.$props.model?.name) && (this.selectedInMediaApp?.path == this.$props.model?.path);
        },
        isRoot: function () {
            return this.model?.path === '';
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
        let self = this;

        this.emitter.on('deleteFolder', function (folder: never) {
            if (self.children) {
                let index = self.children && self.children.indexOf(folder)
                if (index > -1) {
                    self.children.splice(index, 1)
                    self.emitter.emit('folderDeleted');
                }
            }
        });

        this.emitter.on('addFolder', function (element: { selectedFolder: any; data: any; }) {
            let target = element.selectedFolder;
            let folder = <any>element.data;

            if (self.model == target) {
                if (self.children !== null) {
                    self.children.push(<never>folder);
                    self.children.sort(function (a: any, b: any) {
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
            let parentFolder = this.selectedInMediaApp;

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

            if (this.open) {
                this.loadChildren();
            }
        },
        select: function () {
            this.emitter.emit('folderSelected', this.model);
            this.loadChildren();
        },
        selectFolder: function (folder: any) {
            this.emitter.emit('folderSelected', folder);
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

            const apiClient = new MediaApiClient(this.basePath);
            apiClient
                .getFolders(self.model?.path)
                .then((response) => {
                    self.children = response;
                    self.children.forEach(function (c: any) {
                        c.parent = self.model;
                    });
                })
                .catch(async (error) => {
                    notify({ summary: self.t.ErrorGetFolders, detail: error.response?.detail, severity: SeverityLevel.Error });
                })
        },
        handleDragOver: function () {
            this.isHovered = true;
        },
        handleDragLeave: function () {
            this.isHovered = false;
        },
        moveMediaToFolder: function (folder: { path: any; }, e: DragEvent) {
            debug("Move media to folder", folder, e);
            let self = this;
            self.isHovered = false;

            let mediaNamesData = e.dataTransfer?.getData('mediaNames') ?? "";
            let mediaNames = JSON.parse(mediaNamesData);

            if (mediaNames.length < 1) {
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
  