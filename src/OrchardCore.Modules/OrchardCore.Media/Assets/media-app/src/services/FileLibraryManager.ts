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
const { assetsStore, basePath, selectedDirectory, rootDirectory, selectedFiles, hierarchicalDirectories, setAssetsStore, setSelectedFiles, setSelectedAll, setCapabilities, setFileItems, setHierarchicalData, setRootDirectory, setFolderLoaded, setIsLoadingFiles } = useGlobals();
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
 * Builds the root IHFileLibraryItemDto node from a flat list of root-level folders.
 * Used for lazy loading — children of these folders are loaded on demand.
 */
function buildLazyRootNode(rootFolders: IFileLibraryItemDto[]): IHFileLibraryItemDto {
  const { translations } = useLocalizations();
  const t = translations;

  return {
    name: t.FileLibrary ?? "Media Library",
    directoryPath: "",
    filePath: "",
    isDirectory: true,
    selected: true,
    hasChildren: rootFolders.length > 0,
    children: rootFolders
      .filter(f => f.isDirectory)
      .map(f => ({
        name: f.name,
        directoryPath: f.directoryPath,
        filePath: "",
        isDirectory: true,
        selected: false,
        hasChildren: f.hasChildren ?? false,
        children: [],
      })),
  };
}

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
          await fileDataService.moveMediaList(
            elem.files.map((x: { name: string }) => x.name),
            elem.sourceFolder || "root",
            elem.targetFolder || "root",
          );

          await loadDirectoryFiles(selectedDirectory.value.directoryPath);

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
          await loadDirectoryFiles(selectedDirectory.value.directoryPath);
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
        await loadDirectoryFiles(selectedDirectory.value.directoryPath);
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
        await loadDirectoryFiles(selectedDirectory.value.directoryPath);

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
        await loadDirectoryFiles(selectedDirectory.value.directoryPath);

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
      // Fetch storage capabilities first.
      const caps = await fileDataService.getCapabilities();
      setCapabilities(caps);

      // Lazy-load only root-level folders.
      // Children are fetched on-demand when folders are expanded.
      try {
        const rootFolders = await fileDataService.getFolders("");
        const rootHNode = buildLazyRootNode(rootFolders);
        setRootDirectory({ ...rootHNode, children: [] } as unknown as IFileLibraryItemDto);
        setHierarchicalData(rootHNode);
        setFolderLoaded("");

        // Register root folders in assetsStore for breadcrumb lookups.
        const flatDirs = rootFolders.filter(f => f.isDirectory);
        setAssetsStore(flatDirs);

        // Load files for the root directory.
        const rootFiles = await fileDataService.getMediaItems("");
        setFileItems(rootFiles.filter(f => !f.isDirectory));
        result = flatDirs;
      } catch {
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
   * Loads files for a specific directory from the server.
   * Used in hierarchical mode for per-folder loading.
   */
  const loadDirectoryFiles = async (directoryPath: string): Promise<void> => {
    setIsLoadingFiles(true);
    try {
      const files = await fileDataService.getMediaItems(directoryPath);
      setFileItems(files.filter(f => !f.isDirectory));
    } catch (error) {
      notify(error);
    } finally {
      setIsLoadingFiles(false);
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
  };
}
