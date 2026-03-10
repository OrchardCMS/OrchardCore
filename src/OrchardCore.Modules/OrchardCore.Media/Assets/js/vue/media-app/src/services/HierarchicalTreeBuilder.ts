import { IHFileLibraryItemDto, TreeNode, IFileLibraryItemDto } from "../interfaces/interfaces";
import { BASE_DIR } from "../interfaces/contants";
import { useGlobals } from "./Globals";
import { useLocalizations } from "./Localizations";
import { getFileExtension } from "./Utils";

const { assetsStore, selectedDirectory, hierarchicalDirectories, rootDirectory, setHierarchicalData, setRootDirectory } = useGlobals();
const { translations } = useLocalizations();
const t = translations.value;

export function useHierarchicalTreeBuilder() {
  const convertToHierarchyTreeNode = (fileLibraryItems: IFileLibraryItemDto[]) => {
    const rootNode: TreeNode = {
      key: "",
      label: t.FileLibrary ?? "Media Library",
      data: {},
      icon: "fa-solid fa-folder",
      selectable: selectedDirectory.value.directoryPath != "",
      children: [],
    };

    for (const fileLibraryItem of fileLibraryItems) {
      const folderPath = fileLibraryItem.directoryPath.replace(/^\//, "");
      if (!folderPath) continue;
      buildTreeNodeRecursive(rootNode, folderPath.split("/"), fileLibraryItem, 0);
    }

    return rootNode;
  };

  const buildTreeNodeRecursive = (node: TreeNode, paths: string[], fileLibraryItem: IFileLibraryItemDto, idx: number) => {
    if (idx < paths.length) {
      const item = paths[idx];
      let dir = node.children.find((child: TreeNode) => child.label == item);
      if (!dir) {
        node.children.push(
          (dir = {
            label: fileLibraryItem.name,
            key: fileLibraryItem.directoryPath,
            data: fileLibraryItem,
            icon: "fa-solid fa-folder",
            selectable: selectedDirectory.value.directoryPath == fileLibraryItem.directoryPath ? false : true,
            children: [],
          }),
        );
      }
      buildTreeNodeRecursive(dir, paths, fileLibraryItem, idx + 1);
    }
  };

  const getDirectoryTreeNode = (): TreeNode[] => {
    const directories = assetsStore.value.filter((x) => x.isDirectory);
    const result = convertToHierarchyTreeNode(directories);
    return [result];
  };

  const convertToHierarchy = (fileLibraryItems: IFileLibraryItemDto[]) => {
    const rootNode: IHFileLibraryItemDto = { name: t.FileLibrary ?? "Media Library", directoryPath: "", filePath: "", isDirectory: true, selected: true, children: [] };
    setRootDirectory({ ...rootNode });

    for (const fileLibraryItem of fileLibraryItems) {
      const folderPath = fileLibraryItem.directoryPath.replace(/^\//, "");
      if (!folderPath) continue;
      buildNodeRecursive(rootNode, folderPath.split("/"), fileLibraryItem, 0);
    }

    return rootNode;
  };

  const buildNodeRecursive = (node: IHFileLibraryItemDto, paths: string[], fileLibraryItem: IFileLibraryItemDto, idx: number) => {
    if (idx < paths.length) {
      const item = paths[idx];
      let dir = node.children.find((child: IHFileLibraryItemDto) => child.name == item);
      if (!dir) {
        node.children.push(
          (dir = {
            name: fileLibraryItem.name,
            directoryPath: fileLibraryItem.directoryPath,
            filePath: fileLibraryItem.filePath,
            isDirectory: fileLibraryItem.isDirectory,
            selected: false,
            children: [],
          }),
        );
      }
      buildNodeRecursive(dir, paths, fileLibraryItem, idx + 1);
    }
  };

  const setHierarchicalDirectories = (elements: IFileLibraryItemDto[]) => {
    const directories = elements.filter((x) => x.isDirectory);
    const foundHierarchicalDirectories = convertToHierarchy(directories);
    hierarchicalDirectories.value = foundHierarchicalDirectories;
    setHierarchicalData(hierarchicalDirectories.value);
  };

  const convertToFileHierarchyTreeNode = (fileLibraryItems: IFileLibraryItemDto[]) => {
    const rootNode: TreeNode = {
      key: "/",
      label: t.FileLibrary ?? "Media Library",
      data: { isDirectory: true },
      icon: "fa-solid fa-folder",
      selectable: false,
      children: [],
    };

    for (const fileLibraryItem of fileLibraryItems) {
      const itemPath = (fileLibraryItem.isDirectory ? fileLibraryItem.directoryPath : fileLibraryItem.filePath).replace(/^\//, "");
      buildFileTreeNodeRecursive(rootNode, itemPath.split("/"), fileLibraryItem, 0);
    }

    return rootNode;
  };

  const buildFileTreeNodeRecursive = (node: TreeNode, paths: string[], fileLibraryItem: IFileLibraryItemDto, idx: number) => {
    if (idx < paths.length) {
      const item = paths[idx];
      let dir = node.children.find((child: TreeNode) => child.label == item);
      if (!dir) {
        node.children.push(
          (dir = {
            label: fileLibraryItem.name,
            key: fileLibraryItem.isDirectory ? fileLibraryItem.directoryPath : fileLibraryItem.filePath,
            data: fileLibraryItem,
            icon: fileLibraryItem.isDirectory ? "fa-solid fa-folder" : "fa-solid fa-file",
            selectable: !fileLibraryItem.isDirectory,
            children: [],
          }),
        );
      }
      buildFileTreeNodeRecursive(dir, paths, fileLibraryItem, idx + 1);
    }
  };

  const getFileTreeNode = (storeItems: IFileLibraryItemDto[], allowedFileExtensions?: string[]): TreeNode[] => {
    let assets = storeItems;

    if (allowedFileExtensions) {
      assets = assets.filter((node) => {
        if (node.isDirectory) {
          return node;
        } else if (allowedFileExtensions.some((x: string) => x.replace(".", "") == getFileExtension(node.filePath))) {
          return node;
        }
      });

      assets = assets.filter((node) => {
        if (node.isDirectory) {
          if (assets.some((x) => !x.isDirectory && x.directoryPath == node.directoryPath)) {
            return node;
          }
        } else {
          return node;
        }
      });
    }

    if (assets.length > 1) {
      const hierarchicalFiles = convertToFileHierarchyTreeNode(assets);
      return [hierarchicalFiles];
    } else {
      return [];
    }
  };

  return { getDirectoryTreeNode, setHierarchicalDirectories, getFileTreeNode };
}
