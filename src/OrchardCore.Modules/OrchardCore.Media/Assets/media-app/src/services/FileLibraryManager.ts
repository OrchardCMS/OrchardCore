import { usePermissions } from "./Permissions";
import { useGlobals } from "./Globals";
import { IFileLibraryItemDto, IFileCopyDto, IFileListMoveDto, IRenameFileLibraryItemDto, IHFileLibraryItemDto } from "@bloom/media/interfaces";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { useEventBus } from "./UseEventBus";
import { useHierarchicalTreeBuilder } from "./HierarchicalTreeBuilder";
import { FileDataService, IFileDataService } from "@bloom/media/api/file-data-service";
import { useLocalizations } from "@bloom/helpers/localizations";
const { canManage } = usePermissions();
const { setHierarchicalDirectories, setServerDirectoryTree } = useHierarchicalTreeBuilder();
const { assetsStore, basePath, selectedDirectory, rootDirectory, selectedFiles, fileItems, hierarchicalDirectories, setAssetsStore, setSelectedFiles, setSelectedAll, setCapabilities, setFileItems, setHierarchicalData, setRootDirectory, setFolderLoaded, setIsLoadingFiles } = useGlobals();
const { translations } = useLocalizations();
const t = translations;
const { emit } = useEventBus();

/**
 * Finds a node in the hierarchical tree by its directoryPath.
 */
function findNodeByPath(root: IHFileLibraryItemDto, path: string): IHFileLibraryItemDto | null {
  if (root.directoryPath === path) return root;
  for (const child of root.children) {
    const found = findNodeByPath(child, path);
    if (found) return found;
  }
  return null;
}

/**
 * Converts a flat folder DTO into a hierarchical tree child node.
 */
function toHierarchicalChild(f: IFileLibraryItemDto): IHFileLibraryItemDto {
  return {
    name: f.name,
    directoryPath: f.directoryPath,
    filePath: "",
    isDirectory: true,
    selected: false,
    hasChildren: f.hasChildren ?? false,
    children: [],
  };
}

/**
 * Builds the root IHFileLibraryItemDto node from a flat list of root-level folders.
 * Used for lazy loading — children of these folders are loaded on demand.
 */
function buildLazyRootNode(rootFolders: IFileLibraryItemDto[], hasChildren: boolean): IHFileLibraryItemDto {
  const { translations } = useLocalizations();
  const t = translations;

  return {
    name: t.FileLibrary ?? "Media Library",
    directoryPath: "",
    filePath: "",
    isDirectory: true,
    selected: true,
    hasChildren,
    children: rootFolders
      .filter(f => f.isDirectory)
      .map(toHierarchicalChild),
  };
}

// Module-level state shared across all useFileLibraryManager() callers.
const ROOT_FOLDER_PAGE_SIZE = 50;
let rootFoldersLoaded = 0;
let moreRootFoldersAvailable = false;
let isLoadingMoreRoots = false;
let loadRequestId = 0;
const fileCache = new Map<string, IFileLibraryItemDto[]>();

export function useFileLibraryManager() {
  const fileDataService: IFileDataService = new FileDataService(basePath.value);

  const getFileItem = async (path: string): Promise<IFileLibraryItemDto> => {
    let result = {} as IFileLibraryItemDto;

    try {
      const response = await fileDataService.getFileItem(path);
      const hasExisting = assetsStore.value.some((x: IFileLibraryItemDto) => x.filePath == response.filePath);

      if (hasExisting) {
        setAssetsStore([
          ...assetsStore.value.filter(x => x.filePath !== response.filePath),
          response,
        ]);
      }

      result = response;
    } catch (error) {
      notify(error);
    }

    return result;
  };

  const fileListMove = async (elem: IFileListMoveDto): Promise<void> => {
    if (canManage.value) {
      if (elem) {
        try {
          const movedNames = elem.files.map((x: { name: string }) => x.name);
          await fileDataService.moveMediaList(
            movedNames,
            elem.sourceFolder || "root",
            elem.targetFolder || "root",
          );

          // Remove moved files from the current view.
          const movedSet = new Set(movedNames);
          const updated = fileItems.value.filter(f => !movedSet.has(f.name));
          setFileItems(updated);
          invalidateFileCache(selectedDirectory.value.directoryPath);
          // Also invalidate target folder cache so it picks up moved files.
          invalidateFileCache(elem.targetFolder === "root" ? "" : elem.targetFolder);
          emit("FileListMoved", elem);
          notify(new NotificationMessage({ summary: t.Success ?? "Success", detail: t.FilesMoved ?? "File(s) moved successfully.", severity: SeverityLevel.Success }));
        } catch (error) {
          notify(error);
        }
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFile, severity: SeverityLevel.Warn }));
    }
  };

  const fileCopy = async (elem: IFileCopyDto): Promise<void> => {
    if (canManage.value) {
      if (elem) {
        try {
          const copiedFile = await fileDataService.copyMedia(elem.oldPath, elem.newPath);

          // Add the copied file to the UI immediately.
          const updated = [...fileItems.value, copiedFile];
          setFileItems(updated);
          invalidateFileCache(selectedDirectory.value.directoryPath);
          emit("FileCopied", copiedFile);
          notify(new NotificationMessage({ summary: t.Success ?? "Success", detail: t.FileCopied ?? "File copied successfully.", severity: SeverityLevel.Success }));
        } catch (error) {
          notify(error);
        }
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFile, severity: SeverityLevel.Warn }));
    }
  };

  const createDirectory = async (directory: IFileLibraryItemDto): Promise<void> => {
    if (directory.name === "") {
      return;
    }

    if (canManage.value) {
      try {
        const response = await fileDataService.createFolder(selectedDirectory.value.directoryPath, directory.name);
        // Add the new folder as a child of the current directory in the tree.
        const parentPath = selectedDirectory.value.directoryPath;
        const parentNode = findNodeByPath(hierarchicalDirectories.value, parentPath);
        if (parentNode) {
          const newChild: IHFileLibraryItemDto = {
            name: response.name,
            directoryPath: response.directoryPath,
            filePath: "",
            isDirectory: true,
            selected: false,
            hasChildren: false,
            children: [],
          };
          parentNode.children.push(newChild);
          parentNode.hasChildren = true;
          setHierarchicalData({ ...hierarchicalDirectories.value });
        }
        setAssetsStore([...assetsStore.value, response]);
        emit("DirAddReq", { selectedDirectory: selectedDirectory.value, data: response });
      } catch (error) {
        notify(error);
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFolder, severity: SeverityLevel.Warn }));
    }
  };

  const renameFile = async (element: IRenameFileLibraryItemDto): Promise<void> => {
    const newName = element.newName;
    const file = element;

    if (canManage.value) {
      const oldPath = file.filePath;
      // Replace only the filename (last segment) to avoid replacing matching directory names.
      const lastSlash = oldPath.lastIndexOf("/");
      const newPath = lastSlash >= 0 ? oldPath.substring(0, lastSlash + 1) + newName : newName;

      try {
        await fileDataService.moveMedia(oldPath, newPath);

        // Update the file name and path in the UI immediately.
        const updated = fileItems.value.map(f =>
          f.filePath === oldPath ? { ...f, name: newName, filePath: newPath } : f
        );
        setFileItems(updated);
        invalidateFileCache(selectedDirectory.value.directoryPath);
        emit("FileRenamed", { newName: newName, newPath: newPath, oldPath: oldPath });
      } catch (error) {
        notify(error);
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFiles, severity: SeverityLevel.Warn }));
    }
  };

  const deleteFileList = async (): Promise<void> => {
    const files = selectedFiles.value;

    if (canManage.value) {
      if (files.length < 1) {
        return;
      }

      const imagePaths: string[] = [];
      for (let i = 0; i < files.length; i++) {
        imagePaths.push(files[i].filePath ?? "");
      }

      try {
        await fileDataService.deleteMediaList(imagePaths);

        // Remove deleted files from the UI immediately.
        const deletedPaths = new Set(imagePaths);
        const updated = fileItems.value.filter(f => !deletedPaths.has(f.filePath ?? ""));
        setFileItems(updated);
        invalidateFileCache(selectedDirectory.value.directoryPath);
        setSelectedFiles([]);
        setSelectedAll(false);
      } catch (error) {
        notify(error);
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFiles, severity: SeverityLevel.Warn }));
    }
  };

  const deleteFileItem = async (file: IFileLibraryItemDto): Promise<void> => {
    if (!file) {
      return;
    }

    if (canManage.value) {
      try {
        await fileDataService.deleteMedia(file.filePath);

        // Remove the file from the UI immediately.
        const updated = fileItems.value.filter(f => f.filePath !== file.filePath);
        setFileItems(updated);
        invalidateFileCache(selectedDirectory.value.directoryPath);
        emit("FileDeleted", file);
        setSelectedFiles([]);
        setSelectedAll(false);
      } catch (error) {
        notify(error);
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFile, severity: SeverityLevel.Warn }));
    }
  };

  const deleteDirectory = async (directory: IFileLibraryItemDto): Promise<void> => {
    if (directory.directoryPath == rootDirectory.value.directoryPath) {
      notify(new NotificationMessage({ summary: t.ErrorDeleteRootFolder, detail: t.ErrorDeleteRootFolder, severity: SeverityLevel.Warn }));
      return;
    }

    if (canManage.value) {
      try {
        await fileDataService.deleteFolder(directory.directoryPath);

        // Remove the folder from the tree in-place.
        const parentPath = directory.directoryPath.substring(0, directory.directoryPath.lastIndexOf("/"));
        const parentNode = findNodeByPath(hierarchicalDirectories.value, parentPath || "");
        if (parentNode) {
          parentNode.children = parentNode.children.filter(c => c.directoryPath !== directory.directoryPath);
          parentNode.hasChildren = parentNode.children.length > 0;
          setHierarchicalData({ ...hierarchicalDirectories.value });
        }
        setAssetsStore(assetsStore.value.filter(x =>
          !(x.isDirectory && (x.directoryPath + "/").startsWith(directory.directoryPath + "/"))
        ));

        emit("DirDelete", directory);
      } catch (error) {
        notify(error);
      }
    }
    /* v8 ignore next 3 -- canManage is always true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFolder, severity: SeverityLevel.Warn }));
    }
  };

  const getFileLibraryStoreAsync = async (): Promise<IFileLibraryItemDto[]> => {
    let result: IFileLibraryItemDto[] = [];

    try {
      // Fetch capabilities, root folders, and root files all in parallel.
      const currentDir = selectedDirectory.value?.directoryPath ?? "";
      const [caps, foldersResult, currentFiles] = await Promise.all([
        fileDataService.getCapabilities(),
        fileDataService.getFolders("", 0, ROOT_FOLDER_PAGE_SIZE).catch(() => null),
        fileDataService.getMediaItems(currentDir),
      ]);

      setCapabilities(caps);
      setFileItems(currentFiles.filter(f => !f.isDirectory));

      if (foldersResult) {
        moreRootFoldersAvailable = foldersResult.hasMore;
        rootFoldersLoaded = foldersResult.items.length;

        const rootHNode = buildLazyRootNode(foldersResult.items, foldersResult.items.length > 0 || foldersResult.hasMore);
        setRootDirectory({ ...rootHNode, children: [] } as unknown as IFileLibraryItemDto);
        setHierarchicalData(rootHNode);
        setFolderLoaded("");

        // Register root folders in assetsStore for breadcrumb lookups.
        const flatDirs = foldersResult.items.filter(f => f.isDirectory);
        setAssetsStore(flatDirs);
        result = flatDirs;
      } else {
        // Lazy endpoint failed — fall back to loading all items.
        console.warn("Failed to fetch root folders, falling back to flat mode.");
        const response = await fileDataService.listAllItems();
        setAssetsStore(response);
        setHierarchicalDirectories(response);
        result = response;
      }
    } catch (error) {
      notify(error);
    }

    return result;
  };

  /**
   * Loads the next page of root-level folders and appends them to the tree.
   */
  const loadMoreRootFolders = async (): Promise<void> => {
    if (isLoadingMoreRoots || !moreRootFoldersAvailable) {
      return;
    }

    isLoadingMoreRoots = true;
    try {
      const result = await fileDataService.getFolders("", rootFoldersLoaded, ROOT_FOLDER_PAGE_SIZE);
      const newFolders = result.items.filter(f => f.isDirectory);
      rootFoldersLoaded += result.items.length;
      moreRootFoldersAvailable = result.hasMore;

      // Append new children to the root node in the tree.
      const root = hierarchicalDirectories.value;
      for (const f of newFolders) {
        root.children.push(toHierarchicalChild(f));
      }
      root.hasChildren = root.children.length > 0;
      setHierarchicalData({ ...root });

      // Also register in assetsStore for breadcrumb lookups.
      setAssetsStore([...assetsStore.value, ...newFolders]);
    } catch (error) {
      notify(error);
    } finally {
      isLoadingMoreRoots = false;
    }
  };

  /**
   * Returns true if there are more root folders available to load.
   */
  const hasMoreRootFolders = (): boolean => {
    return moreRootFoldersAvailable;
  };

  /**
   * Loads files (and optionally folders) for a directory via a single combined API call.
   * If cached files exist, shows them immediately and refreshes in the background.
   * Rapid clicks cancel previous in-flight requests — only the latest wins.
   *
   * Returns the folder list from the response so callers (e.g. FolderTree) can
   * populate children without a second request.
   */
  const loadDirectoryFiles = async (directoryPath: string, silent = false): Promise<IFileLibraryItemDto[] | null> => {
    const requestId = ++loadRequestId;

    const cached = fileCache.get(directoryPath);
    if (cached) {
      // Show cached files instantly — clear any loading state from a prior uncached request.
      setFileItems(cached);
      setIsLoadingFiles(false);
      // Refresh silently in the background.
      try {
        const content = await fileDataService.getDirectoryContent(directoryPath);
        if (requestId !== loadRequestId) return null; // stale
        fileCache.set(directoryPath, content.files);
        setFileItems(content.files);
        return content.folders;
      } catch (error) {
        if (requestId !== loadRequestId) return null;
        notify(error);
      }
      return null;
    }

    if (!silent) {
      setIsLoadingFiles(true);
    }
    try {
      const content = await fileDataService.getDirectoryContent(directoryPath);
      if (requestId !== loadRequestId) return null; // stale
      fileCache.set(directoryPath, content.files);
      setFileItems(content.files);
      return content.folders;
    } catch (error) {
      if (requestId !== loadRequestId) return null;
      notify(error);
    } finally {
      if (requestId === loadRequestId) {
        setIsLoadingFiles(false);
      }
    }
    return null;
  };

  /**
   * Invalidates the file cache for a specific directory.
   */
  const invalidateFileCache = (directoryPath: string) => {
    fileCache.delete(directoryPath);
  };

  return {
    fileCopy,
    fileListMove,
    getFileItem,
    deleteFileItem,
    deleteFileList,
    renameFile,
    createDirectory,
    deleteDirectory,
    getFileLibraryStoreAsync,
    loadDirectoryFiles,
    loadMoreRootFolders,
    hasMoreRootFolders,
  };
}
