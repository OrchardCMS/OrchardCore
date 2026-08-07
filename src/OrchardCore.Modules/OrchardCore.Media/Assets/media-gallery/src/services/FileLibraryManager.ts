import { usePermissions } from "./Permissions";
import { useGlobals } from "./Globals";
import { IFileLibraryItemDto, IFileCopyDto, IFileListMoveDto, IRenameFileLibraryItemDto, IHFileLibraryItemDto } from "@bloom/media/interfaces";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { useEventBus } from "./UseEventBus";
import { useHierarchicalTreeBuilder } from "./HierarchicalTreeBuilder";
import { FileDataService, IFileDataService } from "@bloom/media/api/file-data-service";
import { getTranslations } from "@bloom/helpers/localizations";
import { getSharedAxios } from "./apiClient";

function isNotFoundError(error: unknown): boolean {
  return typeof error === "object" && error !== null && "status" in error && (error as { status: number }).status === 404;
}
const { canManage } = usePermissions();
const { setServerDirectoryTree } = useHierarchicalTreeBuilder();
const { assetsStore, basePath, selectedDirectory, rootDirectory, selectedFiles, fileItems, hierarchicalDirectories, allowedExtensions, setAssetsStore, setSelectedFiles, setSelectedAll, setFileItems, setHierarchicalData, setRootDirectory, markAllFoldersLoaded, setIsLoadingFiles } = useGlobals();
const t = getTranslations();
const { emit } = useEventBus();

/**
 * Finds a node in the hierarchical tree by its directoryPath.
 */
function findNodeByPath(root: IHFileLibraryItemDto, path: string): IHFileLibraryItemDto | null {
  if (!root || !root.children) return null;
  if (root.directoryPath === path) return root;
  for (const child of root.children) {
    const found = findNodeByPath(child, path);
    if (found) return found;
  }
  return null;
}

// Module-level state shared across all useFileLibraryManager() callers.
let loadRequestId = 0;
const fileCache = new Map<string, IFileLibraryItemDto[]>();
// Tracks directories currently being deleted so that SignalR-triggered loads
// skip them instead of firing a request that will 404.
const pendingDeletes = new Set<string>();

function isBeingDeleted(path: string): boolean {
  for (const p of pendingDeletes) {
    if (path === p || path.startsWith(p + "/")) return true;
  }
  return false;
}

/**
 * A change to the media library, described independently of what caused it.
 *
 * The same change can originate locally — this user copied a file — or remotely, from a media event
 * broadcast by the server. Both are applied through {@link useFileLibraryManager}'s `applyChange`, so the
 * rules for how the store changes live in exactly one place.
 */
export type MediaChange =
  | { kind: "fileAdded"; item: IFileLibraryItemDto }
  | { kind: "fileRemoved"; filePath: string }
  | { kind: "filesRemoved"; filePaths: string[] }
  | { kind: "fileMoved"; oldPath: string; item: IFileLibraryItemDto }
  | { kind: "directoryAdded"; parentPath: string; item: IFileLibraryItemDto }
  | { kind: "directoryRemoved"; directoryPath: string };

function directoryOf(path: string): string {
  const index = path.lastIndexOf("/");

  return index >= 0 ? path.substring(0, index) : "";
}

export function useFileLibraryManager() {
  const fileDataService: IFileDataService = new FileDataService(basePath.value, getSharedAxios());

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

          const sourceFolder = elem.sourceFolder === "root" ? "" : (elem.sourceFolder ?? "");
          const targetFolder = elem.targetFolder === "root" ? "" : (elem.targetFolder ?? "");

          for (const name of movedNames) {
            const oldPath = sourceFolder ? `${sourceFolder}/${name}` : name;
            const moved = fileItems.value.find(f => f.name === name);

            applyChange({
              kind: "fileMoved",
              oldPath,
              item: {
                ...(moved ?? {} as IFileLibraryItemDto),
                name,
                directoryPath: targetFolder,
                filePath: targetFolder ? `${targetFolder}/${name}` : name,
              },
            });
          }

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

          applyChange({ kind: "fileAdded", item: copiedFile });
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
        // directory.directoryPath is the PARENT path (set by useFolderModal), which
        // may be the folder whose ellipsis menu was clicked rather than the
        // currently selected one.
        const parentPath = directory.directoryPath;
        const response = await fileDataService.createFolder(parentPath, directory.name);

        applyChange({ kind: "directoryAdded", parentPath, item: response });
        emit("DirAddReq", { selectedDirectory: { ...selectedDirectory.value, directoryPath: parentPath } as IFileLibraryItemDto, data: response });
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

        const renamed = fileItems.value.find(f => f.filePath === oldPath);

        applyChange({
          kind: "fileMoved",
          oldPath,
          item: { ...(renamed ?? {} as IFileLibraryItemDto), name: newName, filePath: newPath, directoryPath: directoryOf(newPath) },
        });

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

        applyChange({ kind: "filesRemoved", filePaths: imagePaths });
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

        applyChange({ kind: "fileRemoved", filePath: file.filePath });
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
      // Mark the directory as pending-delete so SignalR-triggered loads skip it.
      pendingDeletes.add(directory.directoryPath);
      try {
        await fileDataService.deleteFolder(directory.directoryPath);

        // Navigating away is local to whoever performed the deletion.
        emit("DirDelete", directory);

        applyChange({ kind: "directoryRemoved", directoryPath: directory.directoryPath });
      } catch (error) {
        notify(error);
      } finally {
        pendingDeletes.delete(directory.directoryPath);
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
      // Fetch directory tree and current files in parallel.
      const currentDir = selectedDirectory.value?.directoryPath ?? "";
      const [tree, currentFiles] = await Promise.all([
        fileDataService.getDirectoryTree(),
        fileDataService.getMediaItems(currentDir, allowedExtensions.value),
      ]);

      setFileItems(currentFiles.filter(f => !f.isDirectory));

      // Convert the server-cached tree into the client hierarchy.
      const flatDirs = setServerDirectoryTree(tree);
      setAssetsStore(flatDirs);
      markAllFoldersLoaded(hierarchicalDirectories.value);
      result = flatDirs;
    } catch (error) {
      notify(error);
    }

    return result;
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
    if (directoryPath == null) return null;

    // Skip directories that are mid-deletion to avoid a 404 from the SignalR race.
    if (isBeingDeleted(directoryPath)) return null;

    const requestId = ++loadRequestId;

    const cached = fileCache.get(directoryPath);
    if (cached) {
      // Show cached files instantly — clear any loading state from a prior uncached request.
      setFileItems(cached);
      setIsLoadingFiles(false);
      // Refresh silently in the background.
      try {
        const content = await fileDataService.getDirectoryContent(directoryPath, allowedExtensions.value);
        if (requestId !== loadRequestId) return null; // stale
        fileCache.set(directoryPath, content.files);
        setFileItems(content.files);
        return content.folders;
      } catch (error) {
        if (requestId !== loadRequestId) return null;
        // Suppress 404s — the folder may have been deleted (e.g. SignalR race).
        if (!isNotFoundError(error)) {
          notify(error);
        }
      }
      return null;
    }

    if (!silent) {
      setIsLoadingFiles(true);
    }
    try {
      const content = await fileDataService.getDirectoryContent(directoryPath, allowedExtensions.value);
      if (requestId !== loadRequestId) return null; // stale
      fileCache.set(directoryPath, content.files);
      setFileItems(content.files);
      return content.folders;
    } catch (error) {
      if (requestId !== loadRequestId) return null;
      // Suppress 404s — the folder may have been deleted (e.g. SignalR race).
      if (!isNotFoundError(error)) {
        notify(error);
      }
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

  /**
   * Applies a change to the store, and nothing else.
   *
   * Deliberately free of side effects: no API calls, no notifications, no event-bus emissions and no
   * navigation. Those belong to the operation that caused the change, because they are local to the user
   * who performed it — a remote change must not pop a success toast, clear someone else's selection, or
   * navigate them away from the folder they are looking at.
   */
  const applyChange = (change: MediaChange): void => {
    const currentDirectory = selectedDirectory.value?.directoryPath ?? "";

    switch (change.kind) {
      case "fileAdded": {
        const directory = change.item.directoryPath ?? "";
        invalidateFileCache(directory);

        if (directory === currentDirectory) {
          setFileItems([...fileItems.value.filter(f => f.filePath !== change.item.filePath), change.item]);
        }

        break;
      }

      case "fileRemoved": {
        invalidateFileCache(directoryOf(change.filePath));

        if (directoryOf(change.filePath) === currentDirectory) {
          setFileItems(fileItems.value.filter(f => f.filePath !== change.filePath));
        }

        break;
      }

      case "filesRemoved": {
        const removed = new Set(change.filePaths);

        for (const filePath of change.filePaths) {
          invalidateFileCache(directoryOf(filePath));
        }

        setFileItems(fileItems.value.filter(f => !removed.has(f.filePath ?? "")));

        break;
      }

      case "fileMoved": {
        const from = directoryOf(change.oldPath);
        const to = change.item.directoryPath ?? "";

        invalidateFileCache(from);
        invalidateFileCache(to);

        let updated = fileItems.value;

        if (from === currentDirectory) {
          updated = updated.filter(f => f.filePath !== change.oldPath);
        }

        if (to === currentDirectory) {
          updated = [...updated.filter(f => f.filePath !== change.item.filePath), change.item];
        }

        if (updated !== fileItems.value) {
          setFileItems(updated);
        }

        break;
      }

      case "directoryAdded": {
        const parentNode = findNodeByPath(hierarchicalDirectories.value, change.parentPath);

        if (parentNode) {
          const newChild: IHFileLibraryItemDto = {
            name: change.item.name,
            directoryPath: change.item.directoryPath,
            filePath: "",
            isDirectory: true,
            selected: false,
            hasChildren: false,
            children: [],
          };

          // Insert in sorted position (case-insensitive).
          const insertIndex = parentNode.children.findIndex(
            c => c.name.localeCompare(newChild.name, undefined, { sensitivity: "base" }) > 0
          );

          if (insertIndex === -1) {
            parentNode.children.push(newChild);
          } else {
            parentNode.children.splice(insertIndex, 0, newChild);
          }

          parentNode.hasChildren = true;
          setHierarchicalData({ ...hierarchicalDirectories.value });
        }

        if (!assetsStore.value.some(x => x.isDirectory && x.directoryPath === change.item.directoryPath)) {
          setAssetsStore([...assetsStore.value, change.item]);
        }

        break;
      }

      case "directoryRemoved": {
        const deletedPrefix = change.directoryPath + "/";

        for (const key of fileCache.keys()) {
          if (key === change.directoryPath || key.startsWith(deletedPrefix)) {
            fileCache.delete(key);
          }
        }

        const parentNode = findNodeByPath(hierarchicalDirectories.value, directoryOf(change.directoryPath));

        if (parentNode) {
          parentNode.children = parentNode.children.filter(c => c.directoryPath !== change.directoryPath);
          parentNode.hasChildren = parentNode.children.length > 0;
          setHierarchicalData({ ...hierarchicalDirectories.value });
        }

        setAssetsStore(assetsStore.value.filter(x =>
          !(x.isDirectory && (x.directoryPath + "/").startsWith(deletedPrefix))
        ));

        break;
      }
    }
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
    invalidateFileCache,
    applyChange,
  };
}
