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
                <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button edit-button"
                    v-on:click.stop="renameMedia(media)"><i class="fa fa-edit" aria-hidden="true"></i></a>
                <a href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button"
                    v-on:click.stop="deleteMedia(media)"><i class="fa fa-trash" aria-hidden="true"></i></a>
                <a :href="media.url" target="_blank"
                    class="btn btn-light btn-sm float-end inline-media-button view-button"><fa-icon icon="fa-solid fa-download"></fa-icon></a>
                <span class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
            </div>
        </li>
    </ol>
</template>

<script lang="ts">
import { defineComponent, PropType } from 'vue';
import { IMedia } from '../interfaces/interfaces';

export default defineComponent({
    name: "media-items-grid",
    props: {
        filteredMediaItems: Array as PropType<IMedia[]>,
        selectedMedias: Array as PropType<IMedia[]>,
        thumbSize: {
            type: Number,
            required: true,
        }
    },
    data() {
        return {
            T: {}
        }
    },
    created: function () {
        let self = this;
        // retrieving localized strings from view
        self.T.editButton = (<HTMLInputElement>document.getElementById('t-edit-button'))?.value;
        self.T.deleteButton = (<HTMLInputElement>document.getElementById('t-delete-button'))?.value;
    },
    methods: {
        isMediaSelected: function (media: IMedia) {
            let result = this.selectedMedias?.some(function (element: any, _index, _array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url: string | string[], thumbSize: number) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        toggleSelectionOfMedia: function (media: IMedia) {
            this.emitter.emit('mediaToggleRequested', media);
        },
        renameMedia: function (media: IMedia) {
            this.emitter.emit('renameMediaRequested', media);
        },
        deleteMedia: function (media: IMedia) {
            this.emitter.emit('deleteMediaRequested', media);
        },
        dragStart: function (media: IMedia, e: any) {
            this.emitter.emit('mediaDragStartRequested', media, e);
        }
    }
});
</script>
