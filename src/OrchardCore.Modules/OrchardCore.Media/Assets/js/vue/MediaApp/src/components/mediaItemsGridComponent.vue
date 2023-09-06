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
                    class="btn btn-light btn-sm float-end inline-media-button view-button""><i class=" fa fa-download"
                    aria-hidden="true"></i></a>
                <span class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
            </div>
        </li>
    </ol>
</template>

<script lang="ts">
import { defineComponent, PropType } from 'vue';

export interface IMedia {
  mime: string;
  name: string;
  url: string;
}

export default defineComponent({
    name: "media-items-grid",
    props: {
        filteredMediaItems: {
            type: Array as PropType<IMedia[]>
        },
        selectedMedias: Array,
        thumbSize: Number
    },
    data() {
        return {
            T: {}
        }
    },
    created: function () {
        var self = this;
        // retrieving localized strings from view
        self.T.editButton = $('#t-edit-button').val();
        self.T.deleteButton = $('#t-delete-button').val();
    },
    methods: {
        isMediaSelected: function (media: IMedia) {
            var result = this.selectedMedias?.some(function (element, _index, _array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url: string | string[], thumbSize: string) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        toggleSelectionOfMedia: function (media: any) {
            bus.$emit('mediaToggleRequested', media);
        },
        renameMedia: function (media: IMedia) {
            bus.$emit('renameMediaRequested', media);
        },
        deleteMedia: function (media: IMedia) {
            bus.$emit('deleteMediaRequested', media);
        },
        dragStart: function (media: IMedia, e: any) {
            bus.$emit('mediaDragStartRequested', media, e);
        }
    }
});
</script>
