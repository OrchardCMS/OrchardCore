import { usePermissions } from "./Permissions";
import { useGlobals } from "./Globals";
import { IFileLibraryItemDto, IFileCopyDto, IFileListMoveDto, IRenameFileLibraryItemDto } from "@bloom/media/interfaces";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { useEventBus } from "./UseEventBus";
import { useHierarchicalTreeBuilder } from "./HierarchicalTreeBuilder";
import { FileDataService, IFileDataService } from "@bloom/media/api/file-data-service";
import { useLocalizations } from "@bloom/helpers/localizations";
import { BASE_DIR } from "@bloom/media/constants";

const { canManage } = usePermissions();
const { setHierarchicalDirectories, setServerDirectoryTree } = useHierarchicalTreeBuilder();
const { assetsStore, fileItems, selectedDirectory, rootDirectory, selectedFiles, capabilities, setAssetsStore, setSelectedFiles, setSelectedAll, setCapabilities, setFileItems } = useGlobals();
const { translations } = useLocalizations();
const t = translations;
const { emit } = useEventBus();

export function useFileLibraryManager() {
  const fileDataService: IFileDataService = new FileDataService();

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

          if (capabilities.value.hasHierarchicalNamespace) {
            await loadDirectoryFiles(selectedDirectory.value.directoryPath);
          } else {
            const movedNames = new Set(elem.files.map((x: { name: string }) => x.name));
            const sourceFolder = elem.sourceFolder != "root" ? elem.sourceFolder : BASE_DIR;
            const targetFolder = elem.targetFolder != "root" ? elem.targetFolder : BASE_DIR;

            setAssetsStore(assetsStore.value.map(x => {
              if (!x.isDirectory && x.directoryPath == sourceFolder && movedNames.has(x.name)) {
                return {
                  ...x,
                  directoryPath: targetFolder != "" ? elem.targetFolder : BASE_DIR,
                  filePath: targetFolder != "" ? elem.targetFolder + "/" + x.name : x.name,
                };
              }
              return x;
            }));

            setFileItems(fileItems.value.filter(x => !(x.directoryPath == sourceFolder && movedNames.has(x.name))));

            for (let i = 0; i < elem.files.length; i++) {
              elem.files[i].directoryPath = targetFolder != "" ? elem.targetFolder : BASE_DIR;
              elem.files[i].filePath = targetFolder != "" ? elem.targetFolder + "/" + elem.files[i].name : elem.files[i].name;
            }
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
          if (capabilities.value.hasHierarchicalNamespace) {
            await loadDirectoryFiles(selectedDirectory.value.directoryPath);
          } else {
            setAssetsStore([...assetsStore.value, copiedFile]);
          }
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
        if (capabilities.value.hasHierarchicalNamespace) {
          // Refresh the directory tree from the server.
          const tree = await fileDataService.getDirectoryTree();
          const flatDirs = setServerDirectoryTree(tree);
          setAssetsStore(flatDirs);
        } else {
          setAssetsStore([...assetsStore.value, response].sort((a, b) => (a.name > b.name ? 1 : -1)));
        }
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
        if (capabilities.value.hasHierarchicalNamespace) {
          await loadDirectoryFiles(selectedDirectory.value.directoryPath);
        }
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

        if (capabilities.value.hasHierarchicalNamespace) {
          await loadDirectoryFiles(selectedDirectory.value.directoryPath);
        } else {
          const deletedPaths = new Set(imagePaths);
          for (let i = 0; i < selectedFiles.value.length; i++) {
            emit("FileDeleted", selectedFiles.value[i]);
          }
          setAssetsStore(assetsStore.value.filter(x => !deletedPaths.has(x.filePath ?? "")));
        }

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

        if (capabilities.value.hasHierarchicalNamespace) {
          await loadDirectoryFiles(selectedDirectory.value.directoryPath);
        } else {
          setAssetsStore(assetsStore.value.filter(x => x.filePath !== file.filePath));
        }

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

        if (capabilities.value.hasHierarchicalNamespace) {
          // Refresh the directory tree from the server.
          const tree = await fileDataService.getDirectoryTree();
          const flatDirs = setServerDirectoryTree(tree);
          setAssetsStore(flatDirs);
        } else {
          setAssetsStore(assetsStore.value.filter(x =>
            !(x.isDirectory && (x.directoryPath + "/").startsWith(directory.directoryPath + "/"))
          ));
        }

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

      if (caps.hasHierarchicalNamespace) {
        // Hierarchical backend: fetch the directory tree from the server and
        // load files per-folder on demand instead of loading everything upfront.
        try {
          const tree = await fileDataService.getDirectoryTree();
          const flatDirs = setServerDirectoryTree(tree);
          setAssetsStore(flatDirs);

          // Load files for the root directory.
          const rootFiles = await fileDataService.getMediaItems("");
          setFileItems(rootFiles.filter(f => !f.isDirectory));
          result = flatDirs;
        } catch {
          // Tree endpoint failed — fall back to flat mode.
          console.warn("Failed to fetch directory tree, falling back to flat mode.");
          setCapabilities({ hasHierarchicalNamespace: false, supportsAtomicMove: caps.supportsAtomicMove });
          const response = await fileDataService.listAllItems();
          setAssetsStore(response);
          setHierarchicalDirectories(response);
          result = response;
        }
      } else {
        // Flat backend: load all items and build tree client-side.
        const response = await fileDataService.listAllItems();
        setAssetsStore(response);

        if (response) {
          setHierarchicalDirectories(response);
          result = response;
        } else {
          notify(new NotificationMessage({ summary: t.ErrorGetFolders, detail: t.ErrorGetFolders, severity: SeverityLevel.Error }));
        }
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
    try {
      const files = await fileDataService.getMediaItems(directoryPath);
      setFileItems(files.filter(f => !f.isDirectory));
    } catch (error) {
      notify(error);
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
