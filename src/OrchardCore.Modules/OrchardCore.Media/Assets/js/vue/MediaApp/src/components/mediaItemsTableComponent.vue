<!-- 
    <media-items-table> component 
-->
<template>
    <table class="table media-items-table m-0">
        <thead>
            <tr class="header-row">
                <th scope="col" class="thumbnail-column">{{ t.ImageHeader }}</th>
                <th scope="col" v-on:click="changeSort('name')">
                    {{ t.NameHeader }}
                    <sort-indicator colname="name" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                </th>
                <th scope="col" v-on:click="changeSort('lastModify')">
                    {{ t.LastModifyHeader }}
                    <sort-indicator colname="lastModify" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                </th>
                <th scope="col" v-on:click="changeSort('size')">
                    <span class="optional-col">
                        {{ t.SizeHeader }}
                        <sort-indicator colname="size" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                    </span>
                </th>
                <th scope="col" v-on:click="changeSort('mime')">
                    <span class="optional-col">
                        {{ t.TypeHeader }}
                        <sort-indicator colname="mime" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                    </span>
                </th>
            </tr>
        </thead>
        <tbody>
            <tr v-for="media in filteredMediaItems" class="media-item" :class="{ selected: isMediaSelected(media) }"
                v-on:click.stop="toggleSelectionOfMedia(media)" draggable="true" v-on:dragstart="dragStart(media, $event)"
                :key="media.name">
                <td class="thumbnail-column">
                    <div class="img-wrapper">
                        <img v-if="media.mime.startsWith('image')" draggable="false"
                            :src="buildMediaUrl(media.url, thumbSize)" />
                        <i v-else class="fa fa-file-o fa-lg" :data-mime="media.mime"></i>
                    </div>
                </td>
                <td>
                    <div class="media-name-cell">
                        <span class="break-word"> {{ media.name }} </span>
                        <div class="buttons-container">
                            <a href="javascript:;" class="btn btn-link btn-sm me-1 edit-button"
                                v-on:click.stop="renameMedia(media)"> {{ t.EditButton }} </a>
                            <a href="javascript:;" class="btn btn-link btn-sm delete-button"
                                v-on:click.stop="deleteMedia(media)"> {{ t.DeleteButton }} </a>
                            <a :href="basePath + media.url" target="_blank" class="btn btn-link btn-sm view-button"> {{ t.ViewButton }}
                            </a>
                        </div>
                    </div>
                </td>
                <td>
                    <div class="text-col"> {{ printDateTime(media.lastModify) }} </div>
                </td>
                <td>
                    <div class="text-col optional-col"> {{ isNaN(media.size) ? 0 : Math.round(media.size / 1024) }} KB</div>
                </td>
                <td>
                    <div class="text-col optional-col">{{ media.mime }}</div>
                </td>
            </tr>
        </tbody>
    </table>
</template>

<script>
import SortIndicatorComponent from './sortIndicatorComponent.vue';

export default {
    components: {
        SortIndicator: SortIndicatorComponent,
    },
    name: "media-items-table",
    props: {
        sortBy: String,
        sortAsc: Boolean,
        filteredMediaItems: Array,
        selectedMedias: Array,
        thumbSize: Number,
        basePath: String,
        t: {
            type: Object,
            required: true,
        }
    },
/*     data() {
        return {
            T: {}
        }
    }, */
/*     created: function () {
        let self = this;
        self.t.imageHeader = (<HTMLInputElement>document.getElementById('t-image-header'))?.value;
        self.t.nameHeader = (<HTMLInputElement>document.getElementById('t-name-header'))?.value;
        self.t.lastModifyHeader = (<HTMLInputElement>document.getElementById('t-lastModify-header'))?.value;
        self.t.sizeHeader = (<HTMLInputElement>document.getElementById('t-size-header'))?.value;
        self.t.typeHeader = (<HTMLInputElement>document.getElementById('t-type-header'))?.value;
        self.t.editButton = (<HTMLInputElement>document.getElementById('t-edit-button'))?.value;
        self.t.deleteButton = (<HTMLInputElement>document.getElementById('t-delete-button'))?.value;
        self.t.viewButton = (<HTMLInputElement>document.getElementById('t-view-button'))?.value;
    }, */
    methods: {
        isMediaSelected: function (media) {
            let result = this.selectedMedias?.some(function (element) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url, thumbSize) {
            return "https://localhost:5001" + url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        changeSort: function (newSort) {
            this.emitter.emit('sortChangeRequested', newSort);
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
        },
        printDateTime: function (datemillis) {
            let d = new Date(datemillis);
            return d.toLocaleString();
        }
    }
};
</script>
