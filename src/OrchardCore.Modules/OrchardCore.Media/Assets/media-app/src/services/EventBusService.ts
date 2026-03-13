import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import {
  IFileCopyDto,
  IFileListMoveDto,
  IHFileLibraryItemDto,
  IRenameFileLibraryItemDto,
} from "@bloom/media/interfaces";
import { useFileLibraryManager } from "./FileLibraryManager";
import { useEventBus } from "./UseEventBus";
import { useGlobals } from "./Globals";
import { isFileSelected } from "./Utils";
import router from "../router";
import DragDropThumbnail from "../assets/drag-thumbnail.png";

const {
  sortBy,
  sortAsc,
  selectedDirectory,
  rootDirectory,
  directoryIndex,
  selectedFiles,
  setSortBy,
  setSortAsc,
  setFileFilter,
  setErrors,
  setSelectedFiles,
  setItemsInPage,
  setSelectedDirectory,
  setSelectedAll,
} = useGlobals();
const { fileCopy, fileListMove, deleteFileItem, deleteFileList, renameFile, createDirectory, deleteDirectory, loadDirectoryFiles } = useFileLibraryManager();
const { on, emit } = useEventBus();

export const useEventBusService = () => {
  const dragDropThumbnail: HTMLImageElement = new Image();
  dragDropThumbnail.src = DragDropThumbnail;

  const changeSort = (newSort: string) => {
    if (sortBy.value == newSort) {
      setSortAsc(!sortAsc.value);
    } else {
      setSortAsc(true);
      setSortBy(newSort);
    }
  };

  const toggleSelectionOfFile = (file: IFileLibraryItemDto) => {
    if (isFileSelected(file) == true) {
      selectedFiles.value.splice(selectedFiles.value.indexOf(file), 1);
      setSelectedFiles(selectedFiles.value);
    } else {
      selectedFiles.value.push(file);
      setSelectedFiles(selectedFiles.value);
    }
  };

  const selectRootDirectory = () => {
    setSelectedDirectory(rootDirectory.value);
  };

  const setDirectoryFiles = async (directory: string) => {
    await loadDirectoryFiles(directory);
  };

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const handleDragStart = (element: any) => {
    const files = [] as IFileLibraryItemDto[];
    selectedFiles.value.forEach((item) => {
      files.push(item);
    });

    if (isFileSelected(element.file) == false) {
      files.push(element.file);
    }

    element.e.dataTransfer.setData("files", JSON.stringify(files));
    element.e.dataTransfer.setData("sourceFolder", selectedDirectory.value.directoryPath);
    element.e.dataTransfer.setDragImage(dragDropThumbnail, 10, 10);
    element.e.dataTransfer.effectAllowed = "move";
  };

  on("PagerEvent", (files: IFileLibraryItemDto[]) => {
    setItemsInPage(files);
  });

  on("IsUploading", () => {
    setSelectedFiles([]);
    setSelectedAll(false);
  });

  on("DirSelected", (directory: IFileLibraryItemDto) => {
    setSelectedDirectory(directory);
    setItemsInPage([]);
    setDirectoryFiles(directory.directoryPath);

    if (directory.directoryPath && directory.directoryPath != "") {
      router.push({ name: "folder", params: { path: directory.directoryPath } });
    } else {
      router.push({ name: "home" });
    }

    setSelectedAll(false);
    setFileFilter("");
    setSelectedFiles([]);
    setErrors([]);

    const uploadInput = document.querySelector("#fileupload");
    uploadInput?.setAttribute("multiple", "true");

    emit("AfterDirSelected", directory);
  });

  on("DirDeleted", (folder: IFileLibraryItemDto) => {
    const directory = folder.directoryPath.substring(0, folder.directoryPath.lastIndexOf("/"));

    if (directory) {
      const parentDirectory = directoryIndex.value.get(directory);
      emit("DirSelected", parentDirectory as IFileLibraryItemDto);
    } else {
      selectRootDirectory();
    }
  });

  on("DirAdded", (directory: IHFileLibraryItemDto) => {
    directory.selected = true;
    setSelectedDirectory(directory);
  });

  on("FileListMove", (elem: IFileListMoveDto) => {
    fileListMove(elem);
  });

  on("FileListMoved", async () => {
    setSelectedFiles([]);
    setErrors([]);
  });

  on("FileCopy", (elem: IFileCopyDto) => {
    fileCopy(elem);
  });

  on("FileRenamed", () => {
    setSelectedFiles([]);
    setErrors([]);
  });

  on("DirCreateReq", (directory: IFileLibraryItemDto) => {
    createDirectory(directory);
  });

  on("DirDeleteReq", (directory: IFileLibraryItemDto) => {
    deleteDirectory(directory);
  });

  on("FileSortChangeReq", (newSort: string) => {
    changeSort(newSort);
  });

  on("FileSelectReq", (file: IFileLibraryItemDto) => {
    toggleSelectionOfFile(file);
    setSelectedAll(false);
  });

  on("FileRenameReq", (file: IRenameFileLibraryItemDto) => {
    renameFile(file);
  });

  on("FileDeleteReq", (file: IFileLibraryItemDto) => {
    deleteFileItem(file);
  });

  on("FilesDeleteReq", () => {
    deleteFileList();
  });

  on("FileDragReq", (file: IFileLibraryItemDto) => {
    handleDragStart(file);
  });
};
