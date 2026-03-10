// EventBusService.spec.ts
import { beforeEach, describe, expect, it, vi } from "vitest";
import {
  IFileCopyDto,
  IFileListMoveDto,
  IHFileLibraryItemDto,
  IRenameFileLibraryItemDto,
  IFileLibraryItemDto,
} from "../../interfaces/interfaces";
import { useEventBusService } from "../EventBusService";
import { useEventBus } from "../../services/UseEventBus";
import { useGlobals } from "../Globals";
import { assetsStoreData } from "../../__tests__/mockdata";

useEventBusService();
const { on, emit } = useEventBus();
const { setAssetsStore, setSelectedFiles, setSortBy, selectedFiles, sortBy, sortAsc } = useGlobals();

setAssetsStore(assetsStoreData);

describe("EventBusService", () => {
  it('should emit "FileRenameReq" event', () => {
    const spy = vi.fn();
    on("FileRenameReq", spy);

    const file: IRenameFileLibraryItemDto = { name: "Name", newName: "New Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: false };
    emit("FileRenameReq", file);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(file);
  });

  it('should emit "FileDeleteReq" event', () => {
    const spy = vi.fn();
    on("FileDeleteReq", spy);

    const file: IFileLibraryItemDto = { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: false };
    emit("FileDeleteReq", file);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(file);
  });

  it('should emit "PagerEvent" event', () => {
    const spy = vi.fn();
    on("PagerEvent", spy);

    const files: IFileLibraryItemDto[] = [{ name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: false }];
    emit("PagerEvent", files);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(files);
  });

  it('should emit "IsUploading" event', () => {
    const spy = vi.fn();
    on("IsUploading", spy);

    emit("IsUploading");

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should emit "DirSelected" event', () => {
    const spy = vi.fn();
    on("DirSelected", spy);

    const file: IFileLibraryItemDto = { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: true };
    emit("DirSelected", file);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(file);
  });

  it('should emit "DirDeleted" event', () => {
    const spy = vi.fn();
    on("DirDeleted", spy);

    const directory: IFileLibraryItemDto = {
      filePath: "/Images",
      directoryPath: "/Images",
      name: "Images",
      isDirectory: true,
      size: 0,
      url: "/media/Images",
    };
    emit("DirDeleted", directory);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(directory);
  });

  it('should emit "DirAdded" event', () => {
    const spy = vi.fn();
    on("DirAdded", spy);

    const model: IHFileLibraryItemDto = { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: true, selected: true, children: [] };
    emit("DirAdded", model);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(model);
  });

  it('should emit "FileListMove" event', () => {
    const spy = vi.fn();
    on("FileListMove", spy);

    const model: IFileListMoveDto = {
      files: [{ name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: false }],
      targetFolder: "/target",
      sourceFolder: "/source",
    };
    emit("FileListMove", model);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(model);
  });

  it('should emit "FileCopy" event', () => {
    const spy = vi.fn();
    on("FileCopy", spy);

    const model: IFileCopyDto = { newPath: "/new/path", oldPath: "/old/path" };
    emit("FileCopy", model);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(model);
  });

  it('should emit "FileListMoved" event', () => {
    const spy = vi.fn();
    on("FileListMoved", spy);

    emit("FileListMoved", { files: [], targetFolder: "/target", sourceFolder: "/source" } as IFileListMoveDto);

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should emit "FileRenamed" event', () => {
    const spy = vi.fn();
    on("FileRenamed", spy);

    emit("FileRenamed");

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should emit "DirCreateReq" event', () => {
    const spy = vi.fn();
    on("DirCreateReq", spy);

    const model: IFileLibraryItemDto = { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: true };
    emit("DirCreateReq", model);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(model);
  });

  it('should emit "DirDeleteReq" event', () => {
    const spy = vi.fn();
    on("DirDeleteReq", spy);

    const model: IFileLibraryItemDto = { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: true };
    emit("DirDeleteReq", model);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(model);
  });

  it('should emit "FileSortChangeReq" event', () => {
    const spy = vi.fn();
    on("FileSortChangeReq", spy);

    emit("FileSortChangeReq", "name");

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith("name");
  });

  it('should emit "FileSelectReq" event', () => {
    const spy = vi.fn();
    on("FileSelectReq", spy);

    const model: IFileLibraryItemDto = { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file", isDirectory: false };
    emit("FileSelectReq", model);

    expect(spy).toHaveBeenCalledTimes(1);
    expect(spy).toHaveBeenCalledWith(model);
  });

  it('should emit "FilesDeleteReq" event', () => {
    const spy = vi.fn();
    on("FilesDeleteReq", spy);

    emit("FilesDeleteReq");

    expect(spy).toHaveBeenCalledTimes(1);
  });

  it('should emit upload events', () => {
    const addedSpy = vi.fn();
    const progressSpy = vi.fn();
    const successSpy = vi.fn();
    const errorSpy = vi.fn();

    on("UploadFileAdded", addedSpy);
    on("UploadProgress", progressSpy);
    on("UploadSuccess", successSpy);
    on("UploadError", errorSpy);

    emit("UploadFileAdded", { name: "file.jpg" });
    emit("UploadProgress", { name: "file.jpg", percentage: 50 });
    emit("UploadSuccess", { name: "file.jpg" });
    emit("UploadError", { name: "file.jpg", errorMessage: "Upload failed" });

    expect(addedSpy).toHaveBeenCalledWith({ name: "file.jpg" });
    expect(progressSpy).toHaveBeenCalledWith({ name: "file.jpg", percentage: 50 });
    expect(successSpy).toHaveBeenCalledWith({ name: "file.jpg" });
    expect(errorSpy).toHaveBeenCalledWith({ name: "file.jpg", errorMessage: "Upload failed" });
  });

  describe("changeSort - FileSortChangeReq toggle", () => {
    it("toggles sort direction when same sort field is selected again", () => {
      // First set a sort by emitting once
      setSortBy("name");
      const initialSortAsc = sortAsc.value;

      // Now emit the same sort again to trigger the toggle branch
      emit("FileSortChangeReq", "name");

      // Wait for event to process
      expect(sortAsc.value).toBe(!initialSortAsc);
    });

    it("changes sort field when a different sort is requested", () => {
      setSortBy("name");
      emit("FileSortChangeReq", "size");
      expect(sortBy.value).toBe("size");
    });
  });

  describe("FileSelectReq - deselect file", () => {
    beforeEach(() => {
      setSelectedFiles([]);
    });

    it("deselects a file when it is already selected", () => {
      const file: IFileLibraryItemDto = {
        name: "photo.jpg",
        directoryPath: "/Images",
        filePath: "/Images/photo.jpg",
        isDirectory: false,
        url: "/media/Images/photo.jpg",
      };

      // Select the file first
      setSelectedFiles([file]);

      // Now request to select again (should deselect)
      emit("FileSelectReq", file);

      // File should be deselected
      expect(selectedFiles.value).toHaveLength(0);
    });

    it("selects a file when it is not selected", () => {
      const file: IFileLibraryItemDto = {
        name: "photo.jpg",
        directoryPath: "/Images",
        filePath: "/Images/photo.jpg",
        isDirectory: false,
        url: "/media/Images/photo.jpg",
      };

      setSelectedFiles([]);
      emit("FileSelectReq", file);

      expect(selectedFiles.value.length).toBeGreaterThan(0);
    });
  });

  describe("FileDragReq - handleDragStart", () => {
    it("handles drag start event with a mock dataTransfer", () => {
      const file: IFileLibraryItemDto = {
        name: "photo.jpg",
        directoryPath: "/Images",
        filePath: "/Images/photo.jpg",
        isDirectory: false,
        url: "/media/Images/photo.jpg",
      };

      const mockDataTransfer = {
        setData: vi.fn(),
        setDragImage: vi.fn(),
        effectAllowed: "",
      };

      expect(() => {
        emit("FileDragReq", {
          file,
          e: { dataTransfer: mockDataTransfer },
        });
      }).not.toThrow();

      expect(mockDataTransfer.setData).toHaveBeenCalledWith("files", expect.any(String));
      expect(mockDataTransfer.setData).toHaveBeenCalledWith("sourceFolder", expect.any(String));
      expect(mockDataTransfer.setDragImage).toHaveBeenCalled();
      expect(mockDataTransfer.effectAllowed).toBe("move");
    });

    it("handles drag start when file is already selected (does not add duplicate)", () => {
      const file: IFileLibraryItemDto = {
        name: "photo.jpg",
        directoryPath: "/Images",
        filePath: "/Images/photo.jpg",
        isDirectory: false,
        url: "/media/Images/photo.jpg",
      };

      setSelectedFiles([file]);

      const mockDataTransfer = {
        setData: vi.fn(),
        setDragImage: vi.fn(),
        effectAllowed: "",
      };

      expect(() => {
        emit("FileDragReq", {
          file,
          e: { dataTransfer: mockDataTransfer },
        });
      }).not.toThrow();

      // files in drag data should contain the already-selected file (from selectedFiles)
      const setDataCall = mockDataTransfer.setData.mock.calls.find((c) => c[0] === "files");
      if (setDataCall) {
        const dragFiles = JSON.parse(setDataCall[1]);
        // Should not be duplicated
        expect(dragFiles.length).toBe(1);
      }
    });
  });
});
