import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { FileDataService } from "../data/IFileDataService";
import { IFileLibraryItemDto } from "../../interfaces/interfaces";

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

function makeOkResponse(data: unknown) {
  return {
    ok: true,
    text: () => Promise.resolve(JSON.stringify(data)),
    json: () => Promise.resolve(data),
  } as unknown as Response;
}

function makeEmptyOkResponse() {
  return {
    ok: true,
    text: () => Promise.resolve(""),
    json: () => Promise.resolve({}),
  } as unknown as Response;
}

function makeErrorResponse(status: number, body: object) {
  return {
    ok: false,
    status,
    statusText: "Error",
    text: () => Promise.resolve(JSON.stringify(body)),
    json: () => Promise.resolve(body),
  } as unknown as Response;
}

describe("FileDataService", () => {
  let service: FileDataService;

  beforeEach(() => {
    service = new FileDataService("/api/media-gen2");
    global.fetch = vi.fn();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe("constructor", () => {
    it("uses default base URL when none is provided", () => {
      const defaultService = new FileDataService();
      // Just verify it constructs without error
      expect(defaultService).toBeInstanceOf(FileDataService);
    });

    it("accepts a custom base URL", () => {
      const customService = new FileDataService("/api/custom");
      expect(customService).toBeInstanceOf(FileDataService);
    });
  });

  describe("getFileItem", () => {
    it("fetches a single file item by path", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeOkResponse(mockFile));

      const result = await service.getFileItem("/Images/photo.jpg");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/GetMediaItem?path=%2FImages%2Fphoto.jpg",
        expect.objectContaining({ headers: expect.objectContaining({ "Content-Type": "application/json" }) }),
      );
      expect(result.name).toBe("photo.jpg");
    });

    it("throws on error response", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeErrorResponse(404, { title: "Not found", detail: "File not found" }));
      await expect(service.getFileItem("/notfound.jpg")).rejects.toMatchObject({ title: "Not found" });
    });
  });

  describe("getFolders", () => {
    it("fetches folders for a given path", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeOkResponse([mockFolder]));

      const result = await service.getFolders("/");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/GetFolders?path=%2F",
        expect.anything(),
      );
      expect(result).toHaveLength(1);
      expect(result[0].name).toBe("Images");
    });
  });

  describe("getMediaItems", () => {
    it("fetches media items for a given path", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeOkResponse([mockFile]));

      const result = await service.getMediaItems("/Images");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/GetMediaItems?path=%2FImages",
        expect.anything(),
      );
      expect(result).toHaveLength(1);
      expect(result[0].name).toBe("photo.jpg");
    });
  });

  describe("listAllItems", () => {
    it("fetches all items from GetAllMediaItems endpoint", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeOkResponse([mockFolder, mockFile]));

      const result = await service.listAllItems();

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/GetAllMediaItems",
        expect.objectContaining({ headers: expect.objectContaining({ "Content-Type": "application/json" }) }),
      );
      expect(result).toHaveLength(2);
      expect(result.some((x) => x.name === "Images")).toBe(true);
      expect(result.some((x) => x.name === "photo.jpg")).toBe(true);
    });

    it("returns empty array when no items exist", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeOkResponse([]));

      const result = await service.listAllItems();
      expect(result).toHaveLength(0);
    });
  });

  describe("moveMedia", () => {
    it("posts to MoveMedia endpoint", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeEmptyOkResponse());

      await service.moveMedia("/old/path.jpg", "/new/path.jpg");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/MoveMedia?oldPath=%2Fold%2Fpath.jpg&newPath=%2Fnew%2Fpath.jpg",
        expect.objectContaining({ method: "POST" }),
      );
    });

    it("throws on error", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeErrorResponse(500, { title: "Server error", detail: "Internal" }));
      await expect(service.moveMedia("/a", "/b")).rejects.toMatchObject({ title: "Server error" });
    });
  });

  describe("moveMediaList", () => {
    it("posts to MoveMediaList endpoint with JSON body", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeEmptyOkResponse());

      await service.moveMediaList(["photo.jpg"], "/source", "/target");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/MoveMediaList",
        expect.objectContaining({
          method: "POST",
          body: JSON.stringify({ mediaNames: ["photo.jpg"], sourceFolder: "/source", targetFolder: "/target" }),
        }),
      );
    });
  });

  describe("deleteMedia", () => {
    it("posts to DeleteMedia endpoint", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeEmptyOkResponse());

      await service.deleteMedia("/Images/photo.jpg");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/DeleteMedia?path=%2FImages%2Fphoto.jpg",
        expect.objectContaining({ method: "POST" }),
      );
    });
  });

  describe("deleteMediaList", () => {
    it("posts to DeleteMediaList endpoint with JSON body", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeEmptyOkResponse());

      await service.deleteMediaList(["/Images/photo1.jpg", "/Images/photo2.jpg"]);

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/DeleteMediaList",
        expect.objectContaining({
          method: "POST",
          body: JSON.stringify(["/Images/photo1.jpg", "/Images/photo2.jpg"]),
        }),
      );
    });
  });

  describe("deleteFolder", () => {
    it("posts to DeleteFolder endpoint", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeEmptyOkResponse());

      await service.deleteFolder("/Images");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/DeleteFolder?path=%2FImages",
        expect.objectContaining({ method: "POST" }),
      );
    });
  });

  describe("createFolder", () => {
    it("posts to CreateFolder endpoint and returns the new folder", async () => {
      vi.mocked(global.fetch).mockResolvedValue(makeOkResponse(mockFolder));

      const result = await service.createFolder("/", "Images");

      expect(global.fetch).toHaveBeenCalledWith(
        "/api/media-gen2/CreateFolder?path=%2F&name=Images",
        expect.objectContaining({ method: "POST" }),
      );
      expect(result.name).toBe("Images");
    });
  });

  describe("fetchJson error handling", () => {
    it("falls back to statusText when response.json() fails", async () => {
      const badErrorResponse = {
        ok: false,
        status: 500,
        statusText: "Internal Server Error",
        json: () => Promise.reject(new Error("invalid json")),
        text: () => Promise.resolve("not json"),
      } as unknown as Response;

      vi.mocked(global.fetch).mockResolvedValue(badErrorResponse);

      await expect(service.getFileItem("/test.jpg")).rejects.toMatchObject({
        title: "Error",
        detail: "Internal Server Error",
      });
    });
  });
});
