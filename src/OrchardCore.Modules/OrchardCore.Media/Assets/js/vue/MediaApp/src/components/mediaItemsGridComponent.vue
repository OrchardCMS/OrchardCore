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
                <a alt="{{ t.EditButton }}" href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button edit-button"
                    v-on:click.stop="renameMedia(media)"><fa-icon icon="fa-solid fa-edit"></fa-icon></a>
                <a alt="{{ t.DeleteButton }}" href="javascript:;" class="btn btn-light btn-sm float-end inline-media-button delete-button"
                    v-on:click.stop="deleteMedia(media)"><fa-icon icon="fa-solid fa-trash"></fa-icon></a>
                <a :href="basePath + media.url" target="_blank"
                    class="btn btn-light btn-sm float-end inline-media-button view-button"><fa-icon icon="fa-solid fa-download"></fa-icon></a>
                <span class="media-filename card-text small" :title="media.name">{{ media.name }}</span>
            </div>
        </li>
    </ol>
</template>

<script>

export default {
    name: "media-items-grid",
    props: {
        filteredMediaItems: Array,
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
    data() {
        return {
            T: {}
        }
    },
/*     created: function () {
        let self = this;
        // retrieving localized strings from view
        self.T.editButton = (<HTMLInputElement>document.getElementById('t-edit-button'))?.value;
        self.T.deleteButton = (<HTMLInputElement>document.getElementById('t-delete-button'))?.value;
    }, */
    methods: {
        isMediaSelected: function (media) {
            var result = this.selectedMedias?.some(function (element, index, array) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url, thumbSize) {
            return "https://localhost:5001" + url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        toggleSelectionOfMedia: function (media) {
            this.emitter.emit('mediaToggleRequested', media);
        },
        renameMedia: function (media) {
            this.emitter.emit('renameMediaRequested', media);
        },
        deleteMedia: function (media) {
            this.emitter.emit('deleteMediaRequested', media);
        },
        dragStart: function (media, e) {
            this.emitter.emit('mediaDragStartRequested', media, e);
        }
    }
};
</script>
