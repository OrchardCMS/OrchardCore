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
                                @click="() => openModal(media, 'rename')"> {{ t.EditButton }}
                                <ModalRenameConfirm :modal-name="getModalName(media.name, 'rename')" :file-name="media.name"
                                    :title="t.RenameMediaTitle" @confirm="(fileName) => confirm(media, 'rename', fileName)">
                                    <div>
                                        <label>{{ t.RenameMediaMessage }}</label>
                                    </div>
                                </ModalRenameConfirm>
                            </a>
                            <a href="javascript:;" class="btn btn-link btn-sm delete-button"
                                @click="() => openModal(media, 'delete')"> {{ t.DeleteButton }}
                                <ModalConfirm :modal-name="getModalName(media.name, 'delete')" :title="t.DeleteMediaTitle"
                                    @confirm="() => confirm(media, 'delete', '')">
                                    <p>{{ t.DeleteMediaMessage }}</p>
                                    <p>{{ media.name }}</p>
                                </ModalConfirm>
                            </a>
                            <a :href="basePath + media.url" target="_blank" class="btn btn-link btn-sm view-button"> {{
                                t.ViewButton }}
                            </a>
                        </div>
                    </div>
                </td>
                <td>
                    <div class="text-col"> {{ printDateTime(media.lastModify) }} </div>
                </td>
                <td>
                    <div class="text-col optional-col"> {{ isNaN(media.size) ? 0 : Math.round(media.size / 1024) }} KB
                    </div>
                </td>
                <td>
                    <div class="text-col optional-col">{{ media.mime }}</div>
                </td>
            </tr>
        </tbody>
    </table>
</template>

<script lang="ts">
import { defineComponent } from 'vue'
import dbg from 'debug';
import { useVfm } from 'vue-final-modal'
import ModalConfirm from './ModalConfirm.vue'
import ModalRenameConfirm from './ModalRenameConfirm.vue'
import SortIndicatorComponent from './sortIndicatorComponent.vue';
import { IMedia } from '../interfaces/interfaces';

const debug = dbg("oc:media-app");

export default defineComponent({
    components: {
        ModalConfirm: ModalConfirm,
        ModalRenameConfirm: ModalRenameConfirm,
        SortIndicator: SortIndicatorComponent,
    },
    name: "media-items-table",
    props: {
        sortBy: String,
        sortAsc: Boolean,
        filteredMediaItems: Array<IMedia>,
        selectedMedias: Array,
        thumbSize: Number,
        basePath: String,
        t: {
            type: Object,
            required: true,
        }
    },
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
        renameMedia: function (media, newName) {
            this.emitter.emit('renameMediaRequested', { media, newName });
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
