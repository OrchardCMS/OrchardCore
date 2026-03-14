<template>
  <div id="folder-tree" ref="scrollContainer">
    <!-- Root folder pinned at top (not scrollable) -->
    <FolderRow
      v-if="rootNode"
      :node="rootNode"
      @select="onSelect"
      @toggle="onToggle"
    />
    <RecycleScroller
      v-if="scrollerHeight > 0"
      ref="scroller"
      :items="childNodes"
      :item-size="40"
      key-field="id"
      :buffer="200"
      :style="{ height: scrollerHeight + 'px' }"
    >
      <template #default="{ item }">
        <FolderRow
          :node="item"
          @select="onSelect"
          @toggle="onToggle"
        />
      </template>
    </RecycleScroller>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue';
import { RecycleScroller } from 'vue-virtual-scroller';
import 'vue-virtual-scroller/dist/vue-virtual-scroller.css';
import FolderRow from './FolderRow.vue';
import { useHierarchicalTreeBuilder, type IFlatTreeNode } from '../services/HierarchicalTreeBuilder';
import { useGlobals } from '../services/Globals';
import { useEventBus } from '../services/UseEventBus';
import { IFileLibraryItemDto, IHFileLibraryItemDto } from '@bloom/media/interfaces';
import { FileDataService } from '@bloom/media/api/file-data-service';

const { visibleFolderNodes } = useHierarchicalTreeBuilder();
const { selectedDirectory, expandedFolders, basePath, loadedFolders, hierarchicalDirectories, expandFolder, toggleFolder, setFolderLoading, setFolderLoaded } = useGlobals();
const { on, emit } = useEventBus();

const ROOT_ROW_HEIGHT = 40;

function findNodeByPath(root: IHFileLibraryItemDto, path: string): IHFileLibraryItemDto | null {
  if (root.directoryPath === path) return root;
  for (const child of root.children) {
    const found = findNodeByPath(child, path);
    if (found) return found;
  }
  return null;
}

const scrollContainer = ref<HTMLElement>();
const scroller = ref<InstanceType<typeof RecycleScroller>>();
const scrollerHeight = ref(0);
let resizeObserver: ResizeObserver | null = null;

onMounted(() => {
  const parent = scrollContainer.value?.parentElement;
  if (parent) {
    resizeObserver = new ResizeObserver((entries) => {
      for (const entry of entries) {
        // Subtract the pinned root row height from available space.
        scrollerHeight.value = Math.max(0, entry.contentRect.height - ROOT_ROW_HEIGHT);
      }
    });
    resizeObserver.observe(parent);
  }
});

onBeforeUnmount(() => {
  resizeObserver?.disconnect();
});

// Root node rendered separately (pinned).
const rootNode = computed((): IFlatTreeNode | null => {
  const nodes = visibleFolderNodes.value;
  return nodes.length > 0 ? nodes[0] : null;
});

// All non-root nodes go into the scroller.
const childNodes = computed((): IFlatTreeNode[] => {
  return visibleFolderNodes.value.slice(1);
});

const onSelect = (node: IFlatTreeNode) => {
  const { children, ...folder } = node.item;
  const path = node.item.directoryPath;

  // Select and expand immediately. Children will arrive via DirChildrenLoaded
  // from the combined GetDirectoryContent request (no separate getFolders call).
  expandFolder(path);
  emit("DirSelected", folder as IFileLibraryItemDto);
};

const onToggle = async (node: IFlatTreeNode) => {
  const path = node.item.directoryPath;
  const isExpanded = expandedFolders.value.has(path);

  if (!isExpanded && !loadedFolders.value.has(path)) {
    // Toggle-only (no selection) — need a separate fetch since DirSelected won't fire.
    await loadChildrenFromApi(node.item);
  }

  toggleFolder(path);
};

/**
 * Applies folder children data to a tree node (shared by event handler and API fallback).
 */
const applyChildren = (folder: IHFileLibraryItemDto, children: IFileLibraryItemDto[]) => {
  folder.children = children.map((child: IFileLibraryItemDto) => ({
    name: child.name,
    directoryPath: child.directoryPath,
    filePath: "",
    isDirectory: true,
    selected: false,
    hasChildren: child.hasChildren ?? false,
    children: [],
  }));
  folder.hasChildren = folder.children.length > 0;
  setFolderLoaded(folder.directoryPath);
};

/**
 * Handle folder children arriving from the combined GetDirectoryContent response.
 * This avoids a separate GetFolders request when clicking a folder.
 */
on("DirChildrenLoaded", (data: { directoryPath: string; folders: IFileLibraryItemDto[] }) => {
  const root = hierarchicalDirectories.value;
  if (!root) return;
  const node = findNodeByPath(root, data.directoryPath);
  if (node && !loadedFolders.value.has(data.directoryPath)) {
    applyChildren(node, data.folders);
  }
});

/**
 * Lazy-load children via a separate API call (used only for toggle without selection).
 */
const loadChildrenFromApi = async (folder: IHFileLibraryItemDto) => {
  const fileDataService = new FileDataService(basePath.value);

  setFolderLoading(folder.directoryPath, true);
  try {
    const result = await fileDataService.getFolders(folder.directoryPath);
    applyChildren(folder, result.items);
  } catch (error) {
    console.error("Failed to load children for", folder.directoryPath, error);
  } finally {
    setFolderLoading(folder.directoryPath, false);
  }
};

// When a new folder is created, expand the parent to show it.
on("DirAddReq", (element: { selectedDirectory: IFileLibraryItemDto; data: IFileLibraryItemDto; }) => {
  const parentPath = element.selectedDirectory.directoryPath;
  expandFolder(parentPath);

  // Invalidate the cache so next toggle re-fetches children.
  if (loadedFolders.value.has(parentPath)) {
    const next = new Set(loadedFolders.value);
    next.delete(parentPath);
    loadedFolders.value = next;
  }
});

// Open ancestor folders when a directory is selected (e.g., via breadcrumb or URL).
watch(() => selectedDirectory.value?.directoryPath, (dirPath) => {
  if (dirPath) {
    const segments = dirPath.split("/");
    let path = "";
    for (let i = 0; i < segments.length; i++) {
      path = i > 0 ? path + "/" + segments[i] : segments[i];
      expandFolder(path);
    }
    // Also expand root.
    expandFolder("");
  }
}, { immediate: true });
</script>

<style scoped>
</style>
