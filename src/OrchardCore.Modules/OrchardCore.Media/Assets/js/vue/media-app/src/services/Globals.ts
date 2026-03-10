import { computed, ref } from "vue";
import { IFileLibraryItemDto, IHFileLibraryItemDto } from "@bloom/media/interfaces";

const assetsStore = ref([] as IFileLibraryItemDto[]);
const selectedDirectory = ref({} as IFileLibraryItemDto);
const fileItems = ref([] as IFileLibraryItemDto[]);
const rootDirectory = ref({} as IFileLibraryItemDto);
const hierarchicalDirectories = ref({} as IHFileLibraryItemDto);
const basePath = ref("");
const selectedFiles = ref([] as IFileLibraryItemDto[]);
const isSelectedAll = ref(false);
const errors = ref(<any>[]); // eslint-disable-line @typescript-eslint/no-explicit-any
const fileFilter = ref("");
const sortBy = ref("");
const sortAsc = ref(true);
const itemsInPage = ref([] as IFileLibraryItemDto[]);
const isLoading = ref(true);
const isDownloading = ref(false);
const uploadBaseUrl = ref("");
const uploadFilesUrl = computed(() => {
  const dirPath = selectedDirectory.value.directoryPath ?? "";
  return `${uploadBaseUrl.value}?path=${encodeURIComponent(dirPath)}`;
});

export const useGlobals = () => {
  const setIsDownloading = (value: boolean) => {
    isDownloading.value = value;
  };

  const setUploadFilesUrl = (value: string) => {
    uploadBaseUrl.value = value;
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

  const setBasePath = (value: string) => {
    basePath.value = value;
  };

  const setAssetsStore = (value: IFileLibraryItemDto[]) => {
    assetsStore.value = value;
  };

  const setSelectedDirectory = (value: IFileLibraryItemDto) => {
    selectedDirectory.value = value;
  };

  const setFileItems = (value: IFileLibraryItemDto[]) => {
    fileItems.value = value;
  };

  const setRootDirectory = (value: IFileLibraryItemDto) => {
    rootDirectory.value = value;
  };

  const setHierarchicalData = (value: IHFileLibraryItemDto) => {
    hierarchicalDirectories.value = value;
  };

  return {
    errors,
    fileFilter,
    sortBy,
    sortAsc,
    itemsInPage,
    isLoading,
    isDownloading,
    isSelectedAll,
    selectedFiles,
    basePath,
    assetsStore,
    selectedDirectory,
    fileItems,
    rootDirectory,
    hierarchicalDirectories,
    uploadFilesUrl,
    setAssetsStore,
    setSelectedDirectory,
    setFileItems,
    setRootDirectory,
    setHierarchicalData,
    setBasePath,
    setSelectedFiles,
    setSelectedAll,
    setErrors,
    setFileFilter,
    setSortBy,
    setSortAsc,
    setItemsInPage,
    setIsLoading,
    setIsDownloading,
    setUploadFilesUrl,
  };
};
