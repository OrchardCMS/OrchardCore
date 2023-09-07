<!-- 
    <media-items-table> component 
-->
<template>
    <table class="table media-items-table m-0">
        <thead>
            <tr class="header-row">
                <th scope="col" class="thumbnail-column">{{ T.imageHeader }}</th>
                <th scope="col" v-on:click="changeSort('name')">
                    {{ T.nameHeader }}
                    <sort-indicator colname="name" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                </th>
                <th scope="col" v-on:click="changeSort('lastModify')">
                    {{ T.lastModifyHeader }}
                    <sort-indicator colname="lastModify" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                </th>
                <th scope="col" v-on:click="changeSort('size')">
                    <span class="optional-col">
                        {{ T.sizeHeader }}
                        <sort-indicator colname="size" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
                    </span>
                </th>
                <th scope="col" v-on:click="changeSort('mime')">
                    <span class="optional-col">
                        {{ T.typeHeader }}
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
                                v-on:click.stop="renameMedia(media)"> {{ T.editButton }} </a>
                            <a href="javascript:;" class="btn btn-link btn-sm delete-button"
                                v-on:click.stop="deleteMedia(media)"> {{ T.deleteButton }} </a>
                            <a :href="media.url" target="_blank" class="btn btn-link btn-sm view-button"> {{ T.viewButton }}
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

<script lang="ts">
import { defineComponent, PropType } from 'vue';
import SortIndicatorComponent from './sortIndicatorComponent.vue';
import { IMedia } from '../interfaces/interfaces';

export default defineComponent({
    components: {
        SortIndicator: SortIndicatorComponent,
    },
    name: "media-items-table",
    props: {
        sortBy: String,
        sortAsc: Boolean,
        filteredMediaItems: Array as PropType<IMedia[]>,
        selectedMedias: Array as PropType<IMedia[]>,
        thumbSize: Number
    },
    data() {
        return {
            T: {}
        }
    },
    created: function () {
        let self = this;
        self.T.imageHeader = (<HTMLInputElement>document.getElementById('t-image-header'))?.value;
        self.T.nameHeader = (<HTMLInputElement>document.getElementById('t-name-header'))?.value;
        self.T.lastModifyHeader = (<HTMLInputElement>document.getElementById('t-lastModify-header'))?.value;
        self.T.sizeHeader = (<HTMLInputElement>document.getElementById('t-size-header'))?.value;
        self.T.typeHeader = (<HTMLInputElement>document.getElementById('t-type-header'))?.value;
        self.T.editButton = (<HTMLInputElement>document.getElementById('t-edit-button'))?.value;
        self.T.deleteButton = (<HTMLInputElement>document.getElementById('t-delete-button'))?.value;
        self.T.viewButton = (<HTMLInputElement>document.getElementById('t-view-button'))?.value;
    },
    methods: {
        isMediaSelected: function (media: IMedia) {
            let result = this.selectedMedias?.some(function (element) {
                return element.url.toLowerCase() === media.url.toLowerCase();
            });
            return result;
        },
        buildMediaUrl: function (url: string | string[], thumbSize: string) {
            return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize;
        },
        changeSort: function (newSort: any) {
            this.emitter.emit('sortChangeRequested', newSort);
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
        },
        printDateTime: function (datemillis: string | number | Date) {
            let d = new Date(datemillis);
            return d.toLocaleString();
        }
    }
});
</script>
