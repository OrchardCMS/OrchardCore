<template>
  <div v-on:dragover="handleScrollWhileDrag">
    <div id="customdropzone">
        <h3>Drop your media here</h3>
        <p>Your files will be uploaded to the current folder when you drop them here</p>
        <ul>
          <li>{{ getFoldersUrl }}</li>
          <li>{{ deleteFoldersUrl }}</li>
          <li>{{ createFoldersUrl }}</li>
          <li>{{ getMediaItemsUrl }}</li>
          <li>{{ deleteMediaUrl }}</li>
          <li>{{ renameMediaUrl }}</li>
          <li>{{ deleteMediaListUrl }}</li>
          <li>{{ moveMediaListUrl }}</li>
          <li>{{ uploadFilesUrl }}</li>
        </ul>
    </div>
    <div class="alert message-warning" v-if="errors.length > 0">
      <ul>
        <li v-for="e in errors">
          <p>{{e}}</p>
        </li>
      </ul>
    </div>
    <div id="mediaContainer" class="align-items-stretch">
      <div id="navigationApp" class="media-container-navigation m-0 p-0" v-cloak>
        <ol id="folder-tree">
          <folder :model="root" ref="rootFolder" :selected-in-media-app="selectedFolder" :level="1">
          </folder>
        </ol>
      </div>

<!--         <div id="mediaContainerMain" v-cloak>
            <div class="media-container-top-bar">
                <nav class="nav action-bar pb-3 pt-3 pl-3">
                    <div class="me-auto ms-4">
                        <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="selectAll">
                          Select All
                        </a>
                        <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="unSelectAll" :class="{disabled: selectedMedias.length < 1 }">
                          Select None
                        </a>
                        <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="invertSelection">
                          Invert
                        </a>
                        <a href="javascript:;" class="btn btn-light btn-sm me-2" v-on:click="deleteMediaList" :class="{disabled: selectedMedias.length < 1 }">
                          Delete <span class="badge rounded-pill bg-light" v-show="selectedMedias.length > 0">{{ selectedMedias.length}}</span>
                        </a>
                    </div>
                    <div class="btn-group visibility-buttons">
                        <button type="button" id="toggle-grid-table-button" class="btn btn-light btn-sm" :class="{selected: gridView}" v-on:click="gridView = true">
                            <span title="Grid View"><i class="fa fa-th-large" aria-hidden="true"></i></span>
                        </button>
                        <button type="button" id="toggle-grid-table-button" class="btn btn-light btn-sm" :class="{selected: !gridView}" v-on:click="gridView = false">
                            <span title="List View"><i class="fa fa-th-list" aria-hidden="true"></i></span>
                        </button>
                    </div>
                    <div class="btn-group visibility-buttons" v-show="gridView">
                        <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm" :class="{selected: smallThumbs}" v-on:click="smallThumbs = true">
                            <span title="Small Thumbs"><i class="fa fa-compress" aria-hidden="true"></i></span>
                        </button>
                        <button type="button" id="toggle-thumbsize-button" class="btn btn-light btn-sm me-2" :class="{selected: !smallThumbs}" v-on:click="smallThumbs = false">
                            <span title="Large Thumbs"><i class="fa fa-expand" aria-hidden="true"></i></span>
                        </button>
                    </div>

                    <div class="nav-item ms-2">
                        <div class="media-filter">
                            <div class="input-group input-group-sm">
                                <span class="fa fa-filter icon-inside-input"></span>
                                <input type="text" id="media-filter-input" v-model="mediaFilter" class="form-control input-filter" placeholder="Filter..." aria-label="Filter...">
                                <button id="clear-media-filter-button" class="btn btn-outline-secondary" type="button" :disabled="mediaFilter == '' " v-on:click="mediaFilter = '' "><i class="fa fa-times" aria-hidden="true"></i></button>
                            </div>
                        </div>
                    </div>
                    <div class="d-inline-flex ms-2 me-3 mb-1 pt-1">
                        <div class="btn-group btn-group-sm">
                            <label for="fileupload" class="btn btn-sm btn-primary fileinput-button upload-button">
                                <input id="fileupload" type="file" name="files" multiple="multiple">
                                <i class="fa fa-plus" aria-hidden="true"></i>
                                Upload
                            </label>
                        </div>
                    </div>
                </nav>

                <nav id="breadcrumb" class="d-flex justify-content-end align-items-end">
                    <div class="breadcrumb-path p-3">
                        <span class="breadcrumb-item" :class="{ active: isHome }"><a id="t-mediaLibrary" :href="isHome ? null : '#'" v-on:click="selectRoot">Media Library</a></span>
                        <span v-for="(folder, i) in parents" v-cloak
                              class="breadcrumb-item"
                              :class="{active: parents.length - i == 1}">
                            <a :href="parents.length - i == 1 ? null : '#'" v-on:click="selectedFolder = folder;">{{ folder.name }}</a>
                        </span>
                    </div>
                </nav>
            </div>
            <div class="media-container-middle p-3">
                <upload-list></upload-list>

                <media-items-table :sort-by="sortBy" :sort-asc="sortAsc"
                                   :filtered-media-items="itemsInPage"
                                   :selected-medias="selectedMedias"
                                   :thumb-size="thumbSize"
                                   v-show="itemsInPage.length > 0 && !gridView"></media-items-table>

                <media-items-grid v-show="gridView"
                                  :filtered-media-items="itemsInPage"
                                  :selected-medias="selectedMedias"
                                  :thumb-size="thumbSize"></media-items-grid>

                <div class="alert-info p-2" v-show="mediaItems.length > 0 && filteredMediaItems.length < 1">Nothing to show with this filter</div>
                <div class="alert-info p-2" v-show="mediaItems.length < 1">This folder is empty</div>
            </div>
            <div v-show="filteredMediaItems.length > 0" class="media-container-footer p-3">
                <pager :source-items="filteredMediaItems"> </pager>
            </div>
        </div> -->
    </div>
  </div>
</template>
 
<style lang="scss">
@import "./assets/scss/media.scss";
</style>

<script lang="ts">
import { defineComponent, defineAsyncComponent } from 'vue';
import FolderComponent from './components/folderComponent.vue';

export default defineComponent({
  components: {
    Folder: FolderComponent
  },
  name: "App",
  props: {
    getFoldersUrl: String,
    deleteFoldersUrl: String,
    createFoldersUrl: String,
    getMediaItemsUrl: String,
    deleteMediaUrl: String,
    renameMediaUrl: String,
    deleteMediaListUrl: String,
    moveMediaListUrl: String,
    uploadFilesUrl: String,
  },
  data() {
    return {
      selectedFolder: {},
      mediaItems: [],
      selectedMedias: [],
      errors: [],
      dragDropThumbnail: new Image(),
      smallThumbs: false,
      gridView: false,
      mediaFilter: '',
      sortBy: '',
      sortAsc: true,
      itemsInPage: [],
      root: {
        name: document.querySelector('#t-mediaLibrary')?.textContent,
        path: '',
        folder: '',
        isDirectory: true
      }
    }
  },
  methods: {
    handleScrollWhileDrag(e:any): void {
        if (e.clientY < 150) {                            
            window.scrollBy(0, -10);
        }

        if (e.clientY > window.innerHeight - 100) {                            
            window.scrollBy(0, 10);
        }
    },
  }
});
</script>
