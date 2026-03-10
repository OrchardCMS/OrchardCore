<!--
  MediaBrowserPicker — lightweight file browser for picking media items.
  Uses the shared @bloom/media API to browse folders and select files.
-->
<template>
  <div class="mf-browser">
    <!-- Toolbar -->
    <div class="mf-browser-toolbar">
      <!-- Breadcrumb -->
      <div class="mf-browser-breadcrumb">
        <button
          type="button"
          class="mf-btn-icon"
          :class="{ 'mf-active': currentPath === '' }"
          @click="navigateTo('')"
        >
          <i class="fa-solid fa-house" aria-hidden="true"></i>
        </button>
        <template v-for="(segment, i) in breadcrumbs" :key="segment.path">
          <span class="mf-breadcrumb-sep">/</span>
          <button
            type="button"
            class="mf-btn-icon mf-breadcrumb-item"
            :class="{ 'mf-active': segment.path === currentPath }"
            @click="navigateTo(segment.path)"
          >
            {{ segment.name }}
          </button>
        </template>
      </div>

      <!-- View toggle -->
      <div class="mf-browser-view-toggle">
        <button type="button" class="mf-btn-icon" :class="{ 'mf-active': !gridView }" @click="gridView = false" title="List">
          <i class="fa-solid fa-list" aria-hidden="true"></i>
        </button>
        <button type="button" class="mf-btn-icon" :class="{ 'mf-active': gridView }" @click="gridView = true" title="Grid">
          <i class="fa-solid fa-grip" aria-hidden="true"></i>
        </button>
      </div>
    </div>

    <!-- Loading -->
    <div v-if="loading" class="mf-browser-loading">
      <i class="fa-solid fa-spinner fa-spin" aria-hidden="true"></i>
    </div>

    <!-- Content -->
    <div v-else class="mf-browser-content">
      <!-- Folders -->
      <template v-if="folders.length > 0">
        <div
          v-for="folder in folders"
          :key="folder.directoryPath"
          class="mf-browser-item mf-browser-folder"
          @dblclick="navigateTo(folder.directoryPath)"
          @click="navigateTo(folder.directoryPath)"
        >
          <i class="fa-solid fa-folder" aria-hidden="true"></i>
          <span class="mf-browser-item-name">{{ folder.name }}</span>
        </div>
      </template>

      <!-- Files -->
      <template v-if="gridView">
        <div class="mf-browser-grid">
          <div
            v-for="file in filteredFiles"
            :key="file.filePath"
            class="mf-browser-card"
            :class="{ 'mf-browser-card-selected': isSelected(file) }"
            @click="toggleSelect(file)"
          >
            <div class="mf-browser-card-preview">
              <img
                v-if="isImage(file)"
                :src="file.url + '?width=150&height=150&rmode=crop'"
                :alt="file.name"
                loading="lazy"
              />
              <i v-else :class="getIcon(file.name)" class="mf-browser-file-icon" aria-hidden="true"></i>
            </div>
            <div class="mf-browser-card-name" :title="file.name">{{ file.name }}</div>
          </div>
        </div>
      </template>
      <template v-else>
        <div class="mf-browser-list">
          <div
            v-for="file in filteredFiles"
            :key="file.filePath"
            class="mf-browser-list-item"
            :class="{ 'mf-browser-list-item-selected': isSelected(file) }"
            @click="toggleSelect(file)"
          >
            <div class="mf-browser-list-preview">
              <img
                v-if="isImage(file)"
                :src="file.url + '?width=40&height=40&rmode=crop'"
                :alt="file.name"
                loading="lazy"
              />
              <i v-else :class="getIcon(file.name)" aria-hidden="true"></i>
            </div>
            <span class="mf-browser-item-name">{{ file.name }}</span>
            <span class="mf-browser-item-size">{{ formatSize(file.size) }}</span>
          </div>
        </div>
      </template>

      <!-- Error state -->
      <div v-if="errorMessage" class="mf-browser-empty" style="color: #dc3545;">
        <i class="fa-solid fa-triangle-exclamation" aria-hidden="true"></i>
        {{ errorMessage }}
      </div>

      <!-- Empty state -->
      <div v-else-if="!loading && folders.length === 0 && filteredFiles.length === 0" class="mf-browser-empty">
        No files found
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from "vue";
import { FileDataService } from "@bloom/media/api/file-data-service";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { getIconClassForFilename, isImageExtension } from "../services/FontAwesomeThumbnails";
import { humanFileSize } from "@bloom/media/utils";

const props = defineProps<{
  allowedExtensions?: string;
  allowMultiple?: boolean;
}>();

const emit = defineEmits<{
  selectionChanged: [files: IFileLibraryItemDto[]];
}>();

const dataService = new FileDataService();

const loading = ref(false);
const errorMessage = ref("");
const currentPath = ref("");
const folders = ref<IFileLibraryItemDto[]>([]);
const files = ref<IFileLibraryItemDto[]>([]);
const selectedFiles = ref<IFileLibraryItemDto[]>([]);
const gridView = ref(true);

// --- Computed ---
const breadcrumbs = computed(() => {
  if (!currentPath.value) return [];
  const parts = currentPath.value.split("/").filter(Boolean);
  return parts.map((name, i) => ({
    name,
    path: parts.slice(0, i + 1).join("/"),
  }));
});

const allowedExtSet = computed(() => {
  if (!props.allowedExtensions) return null;
  const exts = props.allowedExtensions
    .split(",")
    .map((e) => e.trim().toLowerCase().replace(/^\./, ""))
    .filter(Boolean);
  return exts.length > 0 ? new Set(exts) : null;
});

const filteredFiles = computed(() => {
  if (!allowedExtSet.value) return files.value;
  return files.value.filter((f) => {
    const ext = f.name.split(".").pop()?.toLowerCase() || "";
    return allowedExtSet.value!.has(ext);
  });
});

// --- Methods ---
async function loadFolder(path: string) {
  loading.value = true;
  errorMessage.value = "";
  try {
    const [folderList, fileList] = await Promise.all([
      dataService.getFolders(path),
      dataService.getMediaItems(path),
    ]);
    folders.value = folderList;
    files.value = fileList.filter((f) => !f.isDirectory);
  } catch (e: unknown) {
    const msg = e instanceof Error ? e.message : String(e);
    console.error("Failed to load folder:", e);
    errorMessage.value = `Failed to load: ${msg}`;
    folders.value = [];
    files.value = [];
  } finally {
    loading.value = false;
  }
}

function navigateTo(path: string) {
  currentPath.value = path;
}

function isImage(file: IFileLibraryItemDto): boolean {
  if (file.mime?.startsWith("image")) return true;
  const ext = file.name.split(".").pop() || "";
  return isImageExtension(ext);
}

function getIcon(filename: string): string {
  return getIconClassForFilename(filename, "fa-2x");
}

function formatSize(size?: number): string {
  if (size == null) return "";
  return humanFileSize(size);
}

function isSelected(file: IFileLibraryItemDto): boolean {
  return selectedFiles.value.some((f) => f.filePath === file.filePath);
}

function toggleSelect(file: IFileLibraryItemDto) {
  const idx = selectedFiles.value.findIndex((f) => f.filePath === file.filePath);
  if (idx >= 0) {
    selectedFiles.value.splice(idx, 1);
  } else {
    if (props.allowMultiple) {
      selectedFiles.value.push(file);
    } else {
      selectedFiles.value = [file];
    }
  }
  emit("selectionChanged", selectedFiles.value);
}

function getSelectedFiles(): IFileLibraryItemDto[] {
  return selectedFiles.value;
}

function clearSelection() {
  selectedFiles.value = [];
}

// --- Watch folder navigation ---
watch(currentPath, (path) => {
  loadFolder(path);
  // Clear selection on folder change
  selectedFiles.value = [];
  emit("selectionChanged", []);
});

onMounted(() => {
  loadFolder("");
});

defineExpose({ getSelectedFiles, clearSelection });
</script>
