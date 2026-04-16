import { computed, ref, shallowRef } from "vue";
import { IFileLibraryItemDto, IHFileLibraryItemDto } from "@bloom/media/interfaces";

const assetsStore = shallowRef([] as IFileLibraryItemDto[]);
const selectedDirectory = ref({} as IFileLibraryItemDto);
const fileItems = shallowRef([] as IFileLibraryItemDto[]);
const rootDirectory = ref({} as IFileLibraryItemDto);
const hierarchicalDirectories = ref({} as IHFileLibraryItemDto);
const basePath = ref("");
const selectedFiles = ref([] as IFileLibraryItemDto[]);
const allowMultipleSelection = ref(true);
const isSelectedAll = ref(false);
const errors = ref(<any>[]); // eslint-disable-line @typescript-eslint/no-explicit-any
const fileFilter = ref("");
const sortBy = ref("");
const sortAsc = ref(true);
const itemsInPage = ref([] as IFileLibraryItemDto[]);
const isLoading = ref(true);
const isDownloading = ref(false);
const isLoadingFiles = ref(false);
const expandedFolders = ref(new Set<string>([""])); // root is always expanded
const loadingFolders = ref(new Set<string>());
const loadedFolders = ref(new Set<string>());
const directoryIndex = computed(() => {
  const map = new Map<string, IFileLibraryItemDto>();
  // Include assetsStore entries (flat directory list from server tree).
  for (const item of assetsStore.value) {
    if (item.isDirectory) {
      map.set(item.directoryPath, item);
    }
  }
  // Walk the hierarchical tree to index all loaded folders (including lazy-loaded subfolders).
  const root = hierarchicalDirectories.value;
  if (root && root.name) {
    const walk = (node: IHFileLibraryItemDto) => {
      if (!map.has(node.directoryPath)) {
        map.set(node.directoryPath, node as IFileLibraryItemDto);
      }
      if (node.children) {
        for (const child of node.children) {
          walk(child);
        }
      }
    };
    walk(root);
  }
  return map;
});

const uploadBaseUrl = ref("");
const allowedExtensions = ref("");
const uploadFilesUrl = computed(() => {
  const dirPath = selectedDirectory.value.directoryPath ?? "";
  return `${uploadBaseUrl.value}?path=${encodeURIComponent(dirPath)}`;
});

export const useGlobals = () => {
  const setIsDownloading = (value: boolean) => {
    isDownloading.value = value;
  };

  const setIsLoadingFiles = (value: boolean) => {
    isLoadingFiles.value = value;
  };

  const setUploadFilesUrl = (value: string) => {
    uploadBaseUrl.value = value;
  };

  const setAllowedExtensions = (value: string) => {
    allowedExtensions.value = value;
  };

  const setIsLoading = (value: boolean) => {
    isLoading.value = value;
  };

  const setItemsInPage = (value: IFileLibraryItemDto[]) => {
    itemsInPage.value = value;
  };

  const setSortAsc = (value: boolean) => {
    sortAsc.value = value;
  };

  const setSortBy = (value: string) => {
    sortBy.value = value;
  };

  const setFileFilter = (value: string) => {
    fileFilter.value = value;
  };

  const setErrors = (value: any[]) => { // eslint-disable-line @typescript-eslint/no-explicit-any
    errors.value = value;
  };

  const setSelectedAll = (value: boolean) => {
    isSelectedAll.value = value;
  };

  const setSelectedFiles = (value: IFileLibraryItemDto[]) => {
    selectedFiles.value = value;
  };

  const setAllowMultipleSelection = (value: boolean) => {
    allowMultipleSelection.value = value;
  };

  const setBasePath = (value: string) => {
    basePath.value = value;
  };

  const setAssetsStore = (value: IFileLibraryItemDto[]) => {
    assetsStore.value = [...value];
  };

  const setSelectedDirectory = (value: IFileLibraryItemDto) => {
    selectedDirectory.value = value;
  };

  const setFileItems = (value: IFileLibraryItemDto[]) => {
    fileItems.value = [...value];
  };

  const setRootDirectory = (value: IFileLibraryItemDto) => {
    rootDirectory.value = value;
  };

  const setHierarchicalData = (value: IHFileLibraryItemDto) => {
    hierarchicalDirectories.value = value;
  };

  const toggleFolder = (path: string) => {
    const next = new Set(expandedFolders.value);
    if (next.has(path)) {
      next.delete(path);
    } else {
      next.add(path);
    }
    expandedFolders.value = next;
  };

  const expandFolder = (path: string) => {
    if (!expandedFolders.value.has(path)) {
      const next = new Set(expandedFolders.value);
      next.add(path);
      expandedFolders.value = next;
    }
  };

  const setFolderLoading = (path: string, loading: boolean) => {
    const next = new Set(loadingFolders.value);
    if (loading) {
      next.add(path);
    } else {
      next.delete(path);
    }
    loadingFolders.value = next;
  };

  const setFolderLoaded = (path: string) => {
    const next = new Set(loadedFolders.value);
    next.add(path);
    loadedFolders.value = next;
  };

  const markAllFoldersLoaded = (root: IHFileLibraryItemDto) => {
    const paths = new Set<string>();
    const walk = (node: IHFileLibraryItemDto) => {
      paths.add(node.directoryPath);
      if (node.children) {
        for (const child of node.children) walk(child);
      }
    };
    walk(root);
    loadedFolders.value = paths;
  };

  const resetLazyState = () => {
    expandedFolders.value = new Set([""]);
    loadingFolders.value = new Set();
    loadedFolders.value = new Set();
  };

  return {
    errors,
    fileFilter,
    sortBy,
    sortAsc,
    itemsInPage,
    isLoading,
    allowMultipleSelection,
    isLoadingFiles,
    isDownloading,
    isSelectedAll,
    selectedFiles,
    basePath,
    assetsStore,
    directoryIndex,
    selectedDirectory,
    fileItems,
    rootDirectory,
    hierarchicalDirectories,
    uploadFilesUrl,
    allowedExtensions,
    setAssetsStore,
    setSelectedDirectory,
    setFileItems,
    setRootDirectory,
    setHierarchicalData,
    setBasePath,
    setAllowMultipleSelection,
    setSelectedFiles,
    setSelectedAll,
    setErrors,
    setFileFilter,
    setSortBy,
    setSortAsc,
    setItemsInPage,
    setIsLoading,
    setIsLoadingFiles,
    setIsDownloading,
    setUploadFilesUrl,
    setAllowedExtensions,
    expandedFolders,
    loadingFolders,
    loadedFolders,
    toggleFolder,
    expandFolder,
    setFolderLoading,
    setFolderLoaded,
    markAllFoldersLoaded,
    resetLazyState,
  };
};
