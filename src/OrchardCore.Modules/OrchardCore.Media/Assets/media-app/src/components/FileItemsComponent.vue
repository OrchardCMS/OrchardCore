<template>
  <div class="file-items-table tw:m-0">
    <div class="table-head">
      <div class="table-row header-row">
        <div scope="col" class="table-cell checkbox-column">
          <div class="ma-check">
            <input :checked="isSelectedAll" class="ma-check-input tw:cursor-pointer" type="checkbox" value=""
              id="select-all" name="select-all" role="checkbox" :title="isSelectedAll ? t.SelectNone : t.SelectAll"
              v-on:click="selectAll">
          </div>
        </div>
        <div class="table-cell" scope="col" v-on:click="changeSort('name')">
          {{ t.NameHeader }}
          <sort-indicator colname="name" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
        </div>
        <div class="table-cell last-modified" scope="col" v-on:click="changeSort('lastModify')">
          {{ t.LastModifyHeader }}
          <sort-indicator colname="lastModify" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
        </div>
        <div class="table-cell size" scope="col" v-on:click="changeSort('size')">
          <span class="optional-col">
            {{ t.SizeHeader }}
            <sort-indicator colname="size" :selectedcolname="sortBy" :asc="sortAsc"></sort-indicator>
          </span>
        </div>
        <div class="table-cell"></div>
      </div>
    </div>
    <div class="table-body">
      <template v-for="file in filteredFileItems" :key="file.filePath">
        <div class="table-row file-item" draggable="true" v-on:dragstart="dragStart(file, $event)">
          <div class="table-cell checkbox-column">
            <div class="ma-check">
              <input :checked="isFileSelected(file)"
                class="ma-check-input tw:cursor-pointer" type="checkbox" value="" name="select-file" role="checkbox"
                v-on:click.stop="toggleSelectionOfFile(file)">
            </div>
          </div>
          <div class="table-cell">
            <div class="tw:flex tw:items-center">
              <div class="thumbnail-column">
                <div class="img-wrapper">
                  <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
                    <path
                      d="M10.28 4.46553H13.4658C13.4889 4.4658 13.5118 4.46145 13.5332 4.45274C13.5545 4.44402 13.574 4.43113 13.5903 4.4148C13.6066 4.39848 13.6195 4.37906 13.6282 4.35768C13.6369 4.3363 13.6413 4.3134 13.641 4.29031C13.6417 4.13738 13.6084 3.9862 13.5437 3.84764C13.479 3.70907 13.3844 3.58656 13.2667 3.48889L10.5946 1.26233C10.3921 1.09433 10.137 1.00273 9.87384 1.00348C9.83983 1.00342 9.80614 1.01007 9.7747 1.02305C9.74327 1.03604 9.71471 1.0551 9.69066 1.07915C9.66661 1.1032 9.64755 1.13176 9.63456 1.1632C9.62158 1.19463 9.61493 1.22832 9.61499 1.26233V3.801C9.61499 3.88831 9.6322 3.97476 9.66562 4.05542C9.69905 4.13607 9.74804 4.20935 9.8098 4.27107C9.87156 4.33278 9.94488 4.38172 10.0256 4.41509C10.1062 4.44845 10.1927 4.46559 10.28 4.46553Z"
                      fill="#2D2D2D" />
                    <path
                      d="M8.70453 3.8V1H4.12C3.82324 1.00092 3.5389 1.11921 3.32906 1.32906C3.11921 1.5389 3.00092 1.82324 3 2.12V13.88C3.00092 14.1768 3.11921 14.4611 3.32906 14.6709C3.5389 14.8808 3.82324 14.9991 4.12 15H12.52C12.8168 14.9991 13.1011 14.8808 13.3109 14.6709C13.5208 14.4611 13.6391 14.1768 13.64 13.88V5.37497H10.28C9.86241 5.37444 9.46206 5.20836 9.16673 4.91312C8.87141 4.61788 8.70519 4.21759 8.70453 3.8Z"
                      fill="#2D2D2D" />
                  </svg>
                  <span class="tw:uppercase file-ext tw:text-white">{{ getFileExtension(file.name) }}</span>
                </div>
              </div>
              <div class="file-name-cell">
                <span class="break-word"> {{ file.name }} </span>
              </div>
            </div>
          </div>
          <div class="table-cell last-modified">
            <div class="text-col"> {{ printDateTime(file.lastModifiedUtc ?? '') }} </div>
          </div>
          <div class="table-cell size">
            <div class="text-col optional-col"> {{ humanFileSize(file.size ?? 0, true, 2) }}</div>
          </div>
          <div class="table-cell tw:text-right">
            <FileMenu :file-item="file"></FileMenu>
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { PropType } from 'vue'
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import SortIndicator from './SortIndicatorComponent.vue';
import { getFileExtension } from '@bloom/media/utils'
import FileMenu from './FileMenu.vue';
import { IRenameFileLibraryItemDto } from '@bloom/media/interfaces';
import { humanFileSize } from '@bloom/media/utils'
import { useEventBus } from '../services/UseEventBus'
//import dbg from "debug";
import { useGlobals } from '../services/Globals';
import { printDateTime } from '@bloom/media/utils'
import { useLocalizations } from '@bloom/helpers/localizations';
import { isFileSelected } from '../services/Utils';
//const debug = dbg("orchardcore:file-app");

const { sortAsc, sortBy, selectedDirectory } = useGlobals();
const { translations } = useLocalizations();
const t = translations;
const { emit } = useEventBus();

const props = defineProps({
  filteredFileItems: {
    type: Array as PropType<Array<IRenameFileLibraryItemDto>>,
    required: true
  },
  selectedFiles: Array as PropType<Array<IFileLibraryItemDto>>,
  isSelectedAll: {
    type: Boolean,
    required: true
  },
})


/**
 * Emits the "SelectAll" event, which tells the parent component
 * to select all files in the current directory.
 */
const selectAll = () => {
  emit("SelectAll");
}


/**
 * Emits the "FileSortChangeReq" event, which tells the parent component
 * to change the sort order of the files in the current directory.
 * @param newSort the new sort order
 */
const changeSort = (newSort: string) => {
  emit("FileSortChangeReq", newSort);
}

/**
 * Emits the "FileSelectReq" event, which tells the parent component
 * to toggle the selection state of the given file.
 * @param file the file to toggle the selection state of
 */
const toggleSelectionOfFile = (file: IFileLibraryItemDto) => {
  emit("FileSelectReq", file);
}

/**
 * Emits the "FileDragReq" event, which tells the parent component
 * that a user has started dragging a file.
 * @param file the file that is being dragged
 * @param e the dragstart event
 */
const dragStart = (file: IFileLibraryItemDto, e: DragEvent) => {
  emit("FileDragReq", { file: file, e: e });
}
</script>
