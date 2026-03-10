import { beforeAll, describe, expect, it } from "vitest";
import { useGlobals } from "../Globals";
import { assetsStoreData } from "../../__tests__/mockdata";
import { useHierarchicalTreeBuilder } from "../HierarchicalTreeBuilder";
import { IFileLibraryItemDto } from "../../interfaces/interfaces";

const { hierarchicalDirectories, setAssetsStore, setSelectedDirectory, assetsStore } = useGlobals();
const { getDirectoryTreeNode, setHierarchicalDirectories, getFileTreeNode } = useHierarchicalTreeBuilder();

describe("Hierarchical Tree Builder", () => {
  beforeAll(() => {
    setAssetsStore(assetsStoreData);
  });

  it("getDirectoryTreeNode", () => {
    const treeNode = getDirectoryTreeNode();

    expect(treeNode[0].key).toEqual("");
    expect(treeNode[0].data).toEqual({});
    expect(treeNode[0].icon).toEqual("fa-solid fa-folder");
    expect(treeNode[0].selectable).toBe(true);
    expect(treeNode[0].children.length).toEqual(2);
    expect(treeNode[0].children[0].label).toEqual("Images");
    expect(treeNode[0].children[1].label).toEqual("Documents");
  });

  it("getDirectoryTreeNode folder not selectable when current", () => {
    setSelectedDirectory(assetsStore.value.find(x => x.isDirectory && x.directoryPath == "/Images") as IFileLibraryItemDto);
    const treeNode = getDirectoryTreeNode();

    const imagesNode = treeNode[0].children.find(c => c.label === "Images");
    expect(imagesNode).toBeDefined();
    expect(imagesNode!.selectable).toBe(false);
  });

  it("setHierarchicalDirectories", () => {
    setHierarchicalDirectories(assetsStore.value);

    expect(hierarchicalDirectories.value.directoryPath).toEqual("");
    expect(hierarchicalDirectories.value.isDirectory).toBe(true);
    expect(hierarchicalDirectories.value.selected).toBe(true);
    expect(hierarchicalDirectories.value.children.length).toBe(2);
    expect(hierarchicalDirectories.value.children[0].name).toEqual("Images");
    expect(hierarchicalDirectories.value.children[1].name).toEqual("Documents");
  });

  it("should get file tree node", () => {
    const storeItems: IFileLibraryItemDto[] = [
      { filePath: "/file1.txt", name: "file1.txt", directoryPath: "/", isDirectory: false },
      { filePath: "/file2.txt", name: "file2.txt", directoryPath: "/", isDirectory: false },
      { filePath: "/folder1", name: "folder1", directoryPath: "/", isDirectory: true },
      { filePath: "/folder1/file3.txt", name: "file3.txt", directoryPath: "/folder1", isDirectory: false },
    ];

    const treeNode = getFileTreeNode(storeItems);

    expect(treeNode.length).toBe(1);
    expect(treeNode[0].key).toEqual("/");
    expect(treeNode[0].data).toEqual({ isDirectory: true });
    expect(treeNode[0].icon).toEqual("fa-solid fa-folder");
    expect(treeNode[0].selectable).toBe(false);
    expect(treeNode[0].children.length).toBe(3);
    expect(treeNode[0].children[0].label).toEqual("file1.txt");
    expect(treeNode[0].children[0].selectable).toBe(true);
    expect(treeNode[0].children[1].label).toEqual("file2.txt");
    expect(treeNode[0].children[2].label).toEqual("folder1");
    expect(treeNode[0].children[2].children.length).toBe(1);
    expect(treeNode[0].children[2].children[0].label).toEqual("file3.txt");
  });

  it("should get file tree node with allowed file extensions", () => {
    const storeItems: IFileLibraryItemDto[] = [
      { filePath: "/file1.txt", name: "file1.txt", directoryPath: "/", isDirectory: false },
      { filePath: "/file2.pdf", name: "file2.pdf", directoryPath: "/", isDirectory: false },
      { filePath: "/folder1", name: "folder1", directoryPath: "/", isDirectory: true },
      { filePath: "/folder1/file3.txt", name: "file3.txt", directoryPath: "/folder1", isDirectory: false },
    ];

    const treeNode = getFileTreeNode(storeItems, ["txt"]);
    expect(treeNode.length).toBe(1);
    expect(treeNode[0].children.length).toBe(2);
    expect(treeNode[0].children[0].label).toEqual("file1.txt");
    expect(treeNode[0].children[1].label).toEqual("folder1");
    expect(treeNode[0].children[1].children.length).toBe(1);
    expect(treeNode[0].children[1].children[0].label).toEqual("file3.txt");
  });

  it("should get file tree node with empty assets store", () => {
    const treeNode = getFileTreeNode([]);
    expect(treeNode.length).toBe(0);
  });
});
