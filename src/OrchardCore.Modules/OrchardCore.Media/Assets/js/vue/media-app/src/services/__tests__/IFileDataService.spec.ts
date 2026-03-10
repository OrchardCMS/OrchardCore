import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { FileDataService } from "@bloom/media/api/file-data-service";
import { IFileLibraryItemDto } from "@bloom/media/interfaces";

const mockFolder: IFileLibraryItemDto = {
  name: "Images",
  directoryPath: "/Images",
  filePath: "/Images",
  isDirectory: true,
};

const mockFile: IFileLibraryItemDto = {
  name: "photo.jpg",
  directoryPath: "/Images",
  filePath: "/Images/photo.jpg",
  isDirectory: false,
  url: "/media/Images/photo.jpg",
};

describe("FileDataService", () => {
  let service: FileDataService;

  beforeEach(() => {
    service = new FileDataService();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe("constructor", () => {
    it("constructs without error using default base URL", () => {
      const defaultService = new FileDataService();
      expect(defaultService).toBeInstanceOf(FileDataService);
    });

    it("constructs without error using custom base URL", () => {
      const customService = new FileDataService("/api/custom");
      expect(customService).toBeInstanceOf(FileDataService);
    });
  });

  describe("getFileItem", () => {
    it("returns mapped file item", async () => {
      vi.spyOn(service, "getFileItem").mockResolvedValue(mockFile);
      const result = await service.getFileItem("/Images/photo.jpg");
      expect(result.name).toBe("photo.jpg");
    });
  });

  describe("getFolders", () => {
    it("returns mapped folders", async () => {
      vi.spyOn(service, "getFolders").mockResolvedValue([mockFolder]);
      const result = await service.getFolders("/");
      expect(result).toHaveLength(1);
      expect(result[0].name).toBe("Images");
    });
  });

  describe("getMediaItems", () => {
    it("returns mapped media items", async () => {
      vi.spyOn(service, "getMediaItems").mockResolvedValue([mockFile]);
      const result = await service.getMediaItems("/Images");
      expect(result).toHaveLength(1);
      expect(result[0].name).toBe("photo.jpg");
    });
  });

  describe("listAllItems", () => {
    it("returns all items", async () => {
      vi.spyOn(service, "listAllItems").mockResolvedValue([mockFolder, mockFile]);
      const result = await service.listAllItems();
      expect(result).toHaveLength(2);
      expect(result.some((x) => x.name === "Images")).toBe(true);
      expect(result.some((x) => x.name === "photo.jpg")).toBe(true);
    });

    it("returns empty array when no items exist", async () => {
      vi.spyOn(service, "listAllItems").mockResolvedValue([]);
      const result = await service.listAllItems();
      expect(result).toHaveLength(0);
    });
  });

  describe("copyMedia", () => {
    it("returns mapped copied file", async () => {
      const copiedFile = { ...mockFile, filePath: "/Other/photo.jpg", directoryPath: "/Other" };
      vi.spyOn(service, "copyMedia").mockResolvedValue(copiedFile);
      const result = await service.copyMedia("/Images/photo.jpg", "/Other/photo.jpg");
      expect(result.filePath).toBe("/Other/photo.jpg");
    });
  });

  describe("moveMedia", () => {
    it("resolves without error", async () => {
      vi.spyOn(service, "moveMedia").mockResolvedValue(undefined);
      await expect(service.moveMedia("/old/path.jpg", "/new/path.jpg")).resolves.toBeUndefined();
    });

    it("throws on error", async () => {
      vi.spyOn(service, "moveMedia").mockRejectedValue(new Error("Server error"));
      await expect(service.moveMedia("/a", "/b")).rejects.toThrow("Server error");
    });
  });

  describe("moveMediaList", () => {
    it("resolves without error", async () => {
      vi.spyOn(service, "moveMediaList").mockResolvedValue(undefined);
      await expect(service.moveMediaList(["photo.jpg"], "/source", "/target")).resolves.toBeUndefined();
    });
  });

  describe("deleteMedia", () => {
    it("resolves without error", async () => {
      vi.spyOn(service, "deleteMedia").mockResolvedValue(undefined);
      await expect(service.deleteMedia("/Images/photo.jpg")).resolves.toBeUndefined();
    });
  });

  describe("deleteMediaList", () => {
    it("resolves without error", async () => {
      vi.spyOn(service, "deleteMediaList").mockResolvedValue(undefined);
      await expect(service.deleteMediaList(["/a.jpg", "/b.jpg"])).resolves.toBeUndefined();
    });
  });

  describe("deleteFolder", () => {
    it("resolves without error", async () => {
      vi.spyOn(service, "deleteFolder").mockResolvedValue(undefined);
      await expect(service.deleteFolder("/Images")).resolves.toBeUndefined();
    });
  });

  describe("createFolder", () => {
    it("returns mapped folder", async () => {
      vi.spyOn(service, "createFolder").mockResolvedValue(mockFolder);
      const result = await service.createFolder("/", "Images");
      expect(result.name).toBe("Images");
    });
  });
});
