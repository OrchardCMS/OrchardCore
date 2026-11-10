import { computed } from "vue";
import { IHFileLibraryItemDto, TreeNode, IFileLibraryItemDto, IDirectoryTreeNode } from "@bloom/media/interfaces";
import { useGlobals } from "./Globals";
import { getTranslations } from "@bloom/helpers/localizations";
import { getFileExtension } from "@bloom/media/utils";

/**
 * A flattened tree node for virtual scrolling.
 * Contains the item data plus depth for indentation.
 */
export interface IFlatTreeNode {
  item: IHFileLibraryItemDto;
  depth: number;
  id: string; // directoryPath used as unique key
}

const { assetsStore, selectedDirectory, hierarchicalDirectories, expandedFolders, setHierarchicalData, setRootDirectory } = useGlobals();
const t = getTranslations();

/**
 * Extracts the parent path from a given path.
 * e.g. "Photos/2024/Summer" -> "Photos/2024", "Photos" -> ""
 */
function getParentPath(path: string): string {
  const lastSlash = path.lastIndexOf("/");
  return lastSlash > 0 ? path.substring(0, lastSlash) : "";
}

export function useHierarchicalTreeBuilder() {
  /**
   * Builds a TreeNode hierarchy from a flat list of directory items.
   * Uses a Map for O(1) parent lookups instead of recursive .find() traversal.
   */
  const convertToHierarchyTreeNode = (fileLibraryItems: IFileLibraryItemDto[]) => {
    const rootNode: TreeNode = {
      key: "",
      label: t.FileLibrary ?? "Media Library",
      data: {},
      icon: "fa-solid fa-folder",
      selectable: selectedDirectory.value.directoryPath != "",
      children: [],
    };

    const nodeMap = new Map<string, TreeNode>();
    nodeMap.set("", rootNode);

    // Sort by depth so parents are created before children.
    const sorted = [...fileLibraryItems].sort(
      (a, b) => a.directoryPath.split("/").length - b.directoryPath.split("/").length,
    );

    for (const item of sorted) {
      const folderPath = item.directoryPath.replace(/^\//, "");
      if (!folderPath) continue;

      // Skip if we've already created a node for this path.
      if (nodeMap.has(folderPath)) continue;

      const parentPath = getParentPath(folderPath);
      const parent = nodeMap.get(parentPath) ?? rootNode;

      const node: TreeNode = {
        label: item.name,
        key: item.directoryPath,
        data: item,
        icon: "fa-solid fa-folder",
        selectable: selectedDirectory.value.directoryPath !== item.directoryPath,
        children: [],
      };

      parent.children.push(node);
      nodeMap.set(folderPath, node);
    }

    return rootNode;
  };

  const getDirectoryTreeNode = (): TreeNode[] => {
    const directories = assetsStore.value.filter((x) => x.isDirectory);
    const result = convertToHierarchyTreeNode(directories);
    return [result];
  };

  /**
   * Builds an IHFileLibraryItemDto hierarchy from a flat list of directory items.
   * Uses a Map for O(1) parent lookups.
   */
  const convertToHierarchy = (fileLibraryItems: IFileLibraryItemDto[]) => {
    const rootNode: IHFileLibraryItemDto = {
      name: t.FileLibrary ?? "Media Library",
      directoryPath: "",
      filePath: "",
      isDirectory: true,
      selected: true,
      children: [],
    };
    setRootDirectory({ ...rootNode });

    const nodeMap = new Map<string, IHFileLibraryItemDto>();
    nodeMap.set("", rootNode);

    const sorted = [...fileLibraryItems].sort(
      (a, b) => a.directoryPath.split("/").length - b.directoryPath.split("/").length,
    );

    for (const item of sorted) {
      const folderPath = item.directoryPath.replace(/^\//, "");
      if (!folderPath) continue;

      if (nodeMap.has(folderPath)) continue;

      const parentPath = getParentPath(folderPath);
      const parent = nodeMap.get(parentPath) ?? rootNode;

      const node: IHFileLibraryItemDto = {
        name: item.name,
        directoryPath: item.directoryPath,
        filePath: item.filePath,
        isDirectory: item.isDirectory,
        selected: false,
        children: [],
      };

      parent.children.push(node);
      nodeMap.set(folderPath, node);
    }

    return rootNode;
  };

  const setHierarchicalDirectories = (elements: IFileLibraryItemDto[]) => {
    const directories = elements.filter((x) => x.isDirectory);
    const foundHierarchicalDirectories = convertToHierarchy(directories);
    hierarchicalDirectories.value = foundHierarchicalDirectories;
    setHierarchicalData(hierarchicalDirectories.value);
  };

  /**
   * Builds a TreeNode hierarchy including both files and directories.
   * Uses a Map for O(1) parent lookups.
   */
  const convertToFileHierarchyTreeNode = (fileLibraryItems: IFileLibraryItemDto[]) => {
    const rootNode: TreeNode = {
      key: "/",
      label: t.FileLibrary ?? "Media Library",
      data: { isDirectory: true },
      icon: "fa-solid fa-folder",
      selectable: false,
      children: [],
    };

    const nodeMap = new Map<string, TreeNode>();
    nodeMap.set("", rootNode);

    // First pass: register all directory nodes in the map (sorted by depth so parents exist first).
    const dirs = fileLibraryItems.filter((x) => x.isDirectory);
    dirs.sort((a, b) => {
      const aPath = a.directoryPath.replace(/^\//, "");
      const bPath = b.directoryPath.replace(/^\//, "");
      return aPath.split("/").length - bPath.split("/").length;
    });

    for (const item of dirs) {
      // For directories in the file tree, the actual folder path comes from directoryPath
      // but for top-level folders directoryPath is "/" (parent). Use name for the key segment.
      const parentDir = item.directoryPath.replace(/^\//, "");
      const folderKey = parentDir ? `${parentDir}/${item.name}` : item.name;
      if (nodeMap.has(folderKey)) continue;

      nodeMap.set(folderKey, {
        label: item.name,
        key: item.isDirectory ? item.directoryPath : item.filePath,
        data: item,
        icon: "fa-solid fa-folder",
        selectable: false,
        children: [],
      });
    }

    // Second pass: attach all items to their parents in original order.
    for (const item of fileLibraryItems) {
      if (item.isDirectory) {
        const parentDir = item.directoryPath.replace(/^\//, "");
        const folderKey = parentDir ? `${parentDir}/${item.name}` : item.name;
        const node = nodeMap.get(folderKey);
        if (node) {
          const parent = nodeMap.get(parentDir) ?? rootNode;
          if (!parent.children.includes(node)) {
            parent.children.push(node);
          }
        }
      } else {
        const dirPath = item.directoryPath.replace(/^\//, "");
        const parent = nodeMap.get(dirPath) ?? rootNode;
        parent.children.push({
          label: item.name,
          key: item.filePath,
          data: item,
          icon: "fa-solid fa-file",
          selectable: true,
          children: [],
        });
      }
    }

    return rootNode;
  };

  const getFileTreeNode = (storeItems: IFileLibraryItemDto[], allowedFileExtensions?: string[]): TreeNode[] => {
    let assets = storeItems;

    if (allowedFileExtensions) {
      assets = assets.filter((node) => {
        if (node.isDirectory) {
          return true;
        }
        return allowedFileExtensions.some((x: string) => x.replace(".", "") == getFileExtension(node.filePath));
      });

      assets = assets.filter((node) => {
        if (node.isDirectory) {
          return assets.some((x) => !x.isDirectory && x.directoryPath == node.directoryPath);
        }
        return true;
      });
    }

    if (assets.length > 1) {
      const hierarchicalFiles = convertToFileHierarchyTreeNode(assets);
      return [hierarchicalFiles];
    } else {
      return [];
    }
  };

  /**
   * Converts a server-built directory tree (IDirectoryTreeNode) into the
   * IHFileLibraryItemDto hierarchy used by FolderComponent.
   * Also collects all directories as flat IFileLibraryItemDto entries for assetsStore.
   */
  const setServerDirectoryTree = (tree: IDirectoryTreeNode): IFileLibraryItemDto[] => {
    const flatDirectories: IFileLibraryItemDto[] = [];

    const convertNode = (node: IDirectoryTreeNode): IHFileLibraryItemDto => {
      const hNode: IHFileLibraryItemDto = {
        name: node.name || t.FileLibrary || "Media Library",
        directoryPath: node.path,
        filePath: "",
        isDirectory: true,
        selected: node.path === "",
        hasChildren: node.hasChildren || node.children.length > 0,
        children: node.children.map(convertNode),
      };

      // Collect non-root directories as flat entries for assetsStore/breadcrumbs.
      if (node.path !== "") {
        flatDirectories.push({
          name: node.name,
          directoryPath: node.path,
          filePath: "",
          isDirectory: true,
        });
      }

      return hNode;
    };

    const rootNode = convertNode(tree);
    setRootDirectory({ ...rootNode, children: [] } as unknown as IFileLibraryItemDto);
    hierarchicalDirectories.value = rootNode;
    setHierarchicalData(rootNode);

    return flatDirectories;
  };

  /**
   * Flattens the hierarchical directory tree into a list of visible nodes
   * based on which folders are currently expanded. Used for virtual scrolling.
   */
  const visibleFolderNodes = computed((): IFlatTreeNode[] => {
    const root = hierarchicalDirectories.value;
    if (!root || !root.name) return [];

    const expanded = expandedFolders.value;
    const result: IFlatTreeNode[] = [];

    const walk = (node: IHFileLibraryItemDto, depth: number) => {
      result.push({ item: node, depth, id: node.directoryPath });

      if (expanded.has(node.directoryPath) && node.children) {
        for (const child of node.children) {
          walk(child, depth + 1);
        }
      }
    };

    walk(root, 0);
    return result;
  });

  return { getDirectoryTreeNode, setHierarchicalDirectories, setServerDirectoryTree, getFileTreeNode, visibleFolderNodes };
}
