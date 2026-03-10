import { beforeEach, describe, expect, it, vi } from "vitest";
import { useFileLibraryManager } from "../FileLibraryManager";
import { useGlobals } from "../Globals";
import { assetsStoreData } from "../../__tests__/mockdata";
import { IFileLibraryItemDto, IRenameFileLibraryItemDto } from "../../interfaces/interfaces";

// Mock the FileDataService
vi.mock("../data/IFileDataService", () => {
  return {
    FileDataService: vi.fn().mockImplementation(() => ({
      getFileItem: vi.fn().mockResolvedValue({
        filePath: "/Images/photo1.jpg",
        directoryPath: "/Images",
        name: "photo1.jpg",
        isDirectory: false,
        size: 896,
        url: "/media/Images/photo1.jpg",
      } as IFileLibraryItemDto),
      getFolders: vi.fn().mockResolvedValue([]),
      getMediaItems: vi.fn().mockResolvedValue([]),
      listAllItems: vi.fn().mockResolvedValue(assetsStoreData),
      moveMedia: vi.fn().mockResolvedValue(undefined),
      moveMediaList: vi.fn().mockResolvedValue(undefined),
      deleteMedia: vi.fn().mockResolvedValue(undefined),
      deleteMediaList: vi.fn().mockResolvedValue(undefined),
      deleteFolder: vi.fn().mockResolvedValue(undefined),
      copyMedia: vi.fn().mockResolvedValue({
        filePath: "/Other/photo1.jpg",
        directoryPath: "/Other",
        name: "photo1.jpg",
        isDirectory: false,
        size: 896,
        url: "/media/Other/photo1.jpg",
      } as IFileLibraryItemDto),
      createFolder: vi.fn().mockResolvedValue({
        filePath: "/NewFolder",
        directoryPath: "/NewFolder",
        name: "NewFolder",
        isDirectory: true,
        size: 0,
      } as IFileLibraryItemDto),
    })),
  };
});

// Mock the notification system
vi.mock("@bloom/services/notifications/notifier", () => ({
  notify: vi.fn(),
  NotificationMessage: vi.fn().mockImplementation((data: any) => data), // eslint-disable-line @typescript-eslint/no-explicit-any
}));

const { setAssetsStore, setSelectedFiles, setSelectedDirectory, assetsStore, setRootDirectory } = useGlobals();

describe("FileLibraryManager", () => {
  beforeEach(() => {
    setAssetsStore(JSON.parse(JSON.stringify(assetsStoreData)));
    setRootDirectory({ filePath: "", directoryPath: "", name: "Media Library", isDirectory: true });
    setSelectedDirectory({ filePath: "/Images", directoryPath: "/Images", name: "Images", isDirectory: true });
  });

  it("should get a file item", async () => {
    const { getFileItem } = useFileLibraryManager();
    const fileItem = await getFileItem("/Images/photo1.jpg");

    expect(fileItem).toBeDefined();
    expect(fileItem.filePath).toBe("/Images/photo1.jpg");
    expect(fileItem.name).toBe("photo1.jpg");
  });

  it("should move a list of files", async () => {
    const { fileListMove } = useFileLibraryManager();
    const files = assetsStore.value.filter((x) => !x.isDirectory && x.directoryPath === "/Images");

    await fileListMove({
      files: files,
      sourceFolder: "/Images",
      targetFolder: "/Documents",
    });

    // Files should have updated paths
    files.forEach((file) => {
      expect(file.directoryPath).toBe("/Documents");
      expect(file.filePath).toContain("/Documents/");
    });
  });

  it("should delete a file item", async () => {
    const { deleteFileItem } = useFileLibraryManager();
    const file = assetsStore.value.find((x) => x.filePath === "/Images/photo1.jpg") as IFileLibraryItemDto;
    const initialLength = assetsStore.value.length;

    await deleteFileItem(file);

    expect(assetsStore.value.length).toBe(initialLength - 1);
    expect(assetsStore.value.find((x) => x.filePath === "/Images/photo1.jpg")).toBeUndefined();
  });

  it("should not delete a null file item", async () => {
    const { deleteFileItem } = useFileLibraryManager();
    const initialLength = assetsStore.value.length;

    await deleteFileItem(null as unknown as IFileLibraryItemDto);

    expect(assetsStore.value.length).toBe(initialLength);
  });

  it("should delete a file item list", async () => {
    const { deleteFileList } = useFileLibraryManager();
    const files = assetsStore.value.filter((x) => !x.isDirectory && x.directoryPath === "/Images");
    setSelectedFiles(files);

    await deleteFileList();

    files.forEach((file) => {
      expect(assetsStore.value.find((x) => x.filePath === file.filePath)).toBeUndefined();
    });
  });

  it("should not delete an empty file list", async () => {
    const { deleteFileList } = useFileLibraryManager();
    setSelectedFiles([]);
    const initialLength = assetsStore.value.length;

    await deleteFileList();

    expect(assetsStore.value.length).toBe(initialLength);
  });

  it("should rename a file", async () => {
    const { renameFile } = useFileLibraryManager();
    const file: IRenameFileLibraryItemDto = {
      filePath: "/Images/photo1.jpg",
      directoryPath: "/Images",
      name: "photo1.jpg",
      newName: "renamed.jpg",
      isDirectory: false,
    };

    await renameFile(file);

    // renameFile calls moveMedia on the data service - just verify no throw
  });

  it("should create a directory", async () => {
    const { createDirectory } = useFileLibraryManager();
    const directory: IFileLibraryItemDto = {
      filePath: "/NewFolder",
      directoryPath: "/NewFolder",
      name: "NewFolder",
      isDirectory: true,
    };

    await createDirectory(directory);

    expect(assetsStore.value.find((x) => x.isDirectory && x.name === "NewFolder")).toBeDefined();
  });

  it("should not create a directory with empty name", async () => {
    const { createDirectory } = useFileLibraryManager();
    const directory: IFileLibraryItemDto = {
      filePath: "",
      directoryPath: "",
      name: "",
      isDirectory: true,
    };
    const initialLength = assetsStore.value.length;

    await createDirectory(directory);

    expect(assetsStore.value.length).toBe(initialLength);
  });

  it("should delete a directory", async () => {
    const { deleteDirectory } = useFileLibraryManager();
    const directory = assetsStore.value.find((x) => x.isDirectory && x.directoryPath === "/Images") as IFileLibraryItemDto;

    await deleteDirectory(directory);

    expect(assetsStore.value.find((x) => x.isDirectory && x.directoryPath === "/Images")).toBeUndefined();
  });

  it("should not delete root directory", async () => {
    const { deleteDirectory } = useFileLibraryManager();
    const rootDir: IFileLibraryItemDto = { filePath: "", directoryPath: "", name: "Media Library", isDirectory: true };
    const initialLength = assetsStore.value.length;

    await deleteDirectory(rootDir);

    expect(assetsStore.value.length).toBe(initialLength);
  });

  it("should get file library store", async () => {
    const { getFileLibraryStoreAsync } = useFileLibraryManager();

    const result = await getFileLibraryStoreAsync();

    expect(result).toBeDefined();
    expect(result.length).toBeGreaterThan(0);
  });

  it("should notify info for file copy (not yet supported)", async () => {
    const { fileCopy } = useFileLibraryManager();

    await fileCopy({ oldPath: "/Images/photo1.jpg", newPath: "/Documents/photo1.jpg" });

    // Should not throw - just shows notification
  });
});
