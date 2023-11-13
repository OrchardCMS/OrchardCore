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
              <img v-if="media.mime.startsWith('image')" draggable="false" :src="buildMediaUrl(media.url, thumbSize)" />
              <fa-icon v-else icon="fa-regular fa-file" size="3x" :data-mime="media.mime"></fa-icon>
            </div>
          </td>
          <td>
            <div class="media-name-cell">
              <span class="break-word"> {{ media.name }} </span>
              <div class="buttons-container">
                <a href="javascript:void(0)" class="btn btn-link btn-sm me-1 edit-button"
                  @click="() => openModal(media, 'rename')"> {{ t.EditButton }}
                  <ModalInputConfirm :t="t" :action-name="t.RenameMediaTitle" :modal-name="getModalName(media.name, 'rename')"
                    :new-name="media.name" :title="t.RenameMediaTitle"
                    @confirm="(newName) => confirm(media, 'rename', newName)">
                    <div>
                      <label>{{ t.RenameMediaMessage }}</label>
                    </div>
                  </ModalInputConfirm>
                </a>
                <a href="javascript:void(0)" class="btn btn-link btn-sm delete-button"
                  @click="() => openModal(media, 'delete')"> {{ t.DeleteButton }}
                  <ModalConfirm :t="t" :action-name="t.Delete" :modal-name="getModalName(media.name, 'delete')" :title="t.DeleteMediaTitle"
                    @confirm="() => confirm(media, 'delete', '')">
                    <p>{{ t.DeleteMediaMessage }}</p>
                    <p>{{ media.name }}</p>
                  </ModalConfirm>
                </a>
                <a :href="media.url" target="_blank" class="btn btn-link btn-sm view-button"> {{
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
  import ModalInputConfirm from './ModalInputConfirm.vue'
  import SortIndicatorComponent from './SortIndicatorComponent.vue';
  import { IMedia } from '../interfaces/Interfaces';
  
  const debug = dbg("oc:media-app");
  
  export default defineComponent({
    components: {
      ModalConfirm: ModalConfirm,
      ModalInputConfirm: ModalInputConfirm,
      SortIndicator: SortIndicatorComponent,
    },
    name: "media-items-table",
    props: {
      sortBy: {
        type: String,
        required: true
      },
      sortAsc: Boolean,
      filteredMediaItems: Array<IMedia>,
      selectedMedias: Array,
      thumbSize: {
        type: Number,
        required: true
      },
      basePath: {
        type: String,
        required: true
      },
      t: {
        type: Object,
        required: true,
      }
    },
    methods: {
      isMediaSelected: function (media: IMedia) {
        let result = this.selectedMedias?.some(function (element: any) {
          return element.url.toLowerCase() === media.url.toLowerCase();
        });
        return result;
      },
      buildMediaUrl: function (url: string | string[], thumbSize: Number) {
        return url + (url.indexOf('?') == -1 ? '?' : '&') + 'width=' + thumbSize + '&height=' + thumbSize + '&rmode=crop';
      },
      changeSort: function (newSort: any) {
        this.emitter.emit('sortChangeRequested', newSort);
      },
      toggleSelectionOfMedia: function (media: IMedia) {
        this.emitter.emit('mediaToggleRequested', media);
      },
      renameMedia: function (media: IMedia, newName: any) {
        this.emitter.emit('renameMediaRequested', { media, newName });
      },
      deleteMedia: function (media: IMedia) {
        this.emitter.emit('deleteMediaRequested', media);
      },
      dragStart: function (media: IMedia, e: DragEvent) {
        this.emitter.emit('mediaDragStartRequested', { media: media, e: e });
      },
      printDateTime: function (datemillis: string | number | Date) {
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
  