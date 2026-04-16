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

export const useEventBusService = () => {
  const {
    sortBy,
    sortAsc,
    selectedDirectory,
    rootDirectory,
    directoryIndex,
    selectedFiles,
    allowMultipleSelection,
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
    } else if (!allowMultipleSelection.value) {
      setSelectedFiles([file]);
    } else {
      selectedFiles.value.push(file);
      setSelectedFiles(selectedFiles.value);
    }
  };

  const setDirectoryFiles = async (directory: string) => {
    const folders = await loadDirectoryFiles(directory);
    if (folders !== null) {
      emit("DirChildrenLoaded", { directoryPath: directory, folders });
    }
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
    if (allowMultipleSelection.value) {
      uploadInput?.setAttribute("multiple", "true");
    } else {
      uploadInput?.removeAttribute("multiple");
    }

    emit("AfterDirSelected", directory);
  });

  on("DirDelete", (folder: IFileLibraryItemDto) => {
    const lastSlash = folder.directoryPath.lastIndexOf("/");
    const parentPath = lastSlash > 0 ? folder.directoryPath.substring(0, lastSlash) : "";
    const parentDirectory = parentPath ? directoryIndex.value.get(parentPath) : null;

    // Always emit DirSelected so the route, file list, and selection state update correctly.
    emit("DirSelected", parentDirectory ?? rootDirectory.value);
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
