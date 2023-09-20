<!-- 
    <media-items-grid> component 
-->
<template>
    <ol class="row media-items-grid">
        <li v-for="media in filteredMediaItems" :key="media.name" class="media-item media-container-main-list-item card p-0"
            :style="{ width: thumbSize + 2 + 'px' }" :class="{ selected: isMediaSelected(media) }"
            v-on:click.stop="toggleSelectionOfMedia(media)" draggable="true" v-on:dragstart="dragStart(media, $event)">
            <div class="thumb-container" :style="{ height: thumbSize + 'px' }">
                <img v-if="media.mime.startsWith('image')" :src="buildMediaUrl(media.url, thumbSize)"
                    :data-mime="media.mime" :style="{ maxHeight: thumbSize + 'px', maxWidth: thumbSize + 'px' }" />
                <i v-else class="fa fa-file-o fa-lg" :data-mime="media.mime"></i>
            </div>
            <div class="media-container-main-item-title card-body">
                <a alt="{{ t.EditButton }}" href="javascript:void(0)"
                    class="btn btn-light btn-sm float-end inline-media-button edit-button"
                    @click="() => openModal(media, 'rename')"><fa-icon icon="fa-solid fa-edit"></fa-icon>
                    <ModalRenameConfirm :modal-name="getModalName(media.name, 'rename')" :file-name="media.name"
                        :title="t.RenameMediaTitle" @confirm="(fileName) => confirm(media, 'rename', fileName)">
                        <div>
                            <label>{{ t.RenameMediaMessage }}</label>
                        </div>
                    </ModalRenameConfirm>
                </a>
                <a alt="{{ t.DeleteButton }}" href="javascript:void(0)"
                    class="btn btn-light btn-sm float-end inline-media-button delete-button"
                    @click="() => openModal(media, 'delete')"><fa-icon icon="fa-solid fa-trash"></fa-icon>
                    <ModalConfirm :modal-name="getModalName(media.name, 'delete')" :title="t.DeleteMediaTitle"
                        @confirm="() => confirm(media, 'delete', '')">
                        <p>{{ t.DeleteMediaMessage }}</p>
                        <p>{{ media.name }}</p>
                    </ModalConfirm>
                </a>
                <a :href="basePath + media.url" target="_blank"
                    class="btn btn-light btn-sm float-end inline-media-button view-button"><fa-icon
                        icon="fa-solid fa-download"></fa-icon></a>
                <span class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
            </div>
        </li>
    </ol>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import dbg from 'debug';
import { useVfm, ModalId } from 'vue-final-modal'
import ModalConfirm from './ModalConfirm.vue'
import ModalRenameConfirm from './ModalRenameConfirm.vue'
import { IMedia } from '../interfaces/interfaces';

const debug = dbg("oc:media-app");

export default defineComponent({
    components: {
        ModalConfirm: ModalConfirm,
        ModalRenameConfirm: ModalRenameConfirm,
    },
    name: "media-items-grid",
    props: {
        filteredMediaItems: Array<IMedia>,
        selectedMedias: Array,
        basePath: String,
        thumbSize: {
            type: Number,
            required: true,
        },
        t: {
            type: Object,
            required: true,
        }
    },
    setup(props, context) {

    },
    methods: {
        isMediaSelected: function (media: IMedia) {
            var result = this.selectedMedias?.some(function (element, index, array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url, thumbSize) {
            return "https://localhost:5001" + url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        toggleSelectionOfMedia: function (media: IMedia) {
            this.emitter.emit('mediaToggleRequested', media);
        },
        renameMedia: function (media, newName) {
            this.emitter.emit('renameMediaRequested', { media, newName });
        },
        deleteMedia: function (media: IMedia) {
            this.emitter.emit('deleteMediaRequested', media);
        },
        dragStart: function (media: IMedia, e: any) {
            this.emitter.emit('mediaDragStartRequested', { media, e });
        },
        getModalName: function (name: string, action: string) {
            return action + "-media-item-table-" + name;
        },
        openModal: function (media: IMedia, action: string) {
            const uVfm = useVfm();

            uVfm.open(this.getModalName(media.name, action));
        },
        confirm: function (media: IMedia, action: string, newName: string) {
            const uVfm = useVfm();

            if (action == "delete") {
                this.deleteMedia(media);
            }
            else if (action == "rename") {
                debug("Confirm media rename:", newName);
                this.renameMedia(media, newName);
            }

            uVfm.close(this.getModalName(media.name, action));
        },
    }
});
</script>
