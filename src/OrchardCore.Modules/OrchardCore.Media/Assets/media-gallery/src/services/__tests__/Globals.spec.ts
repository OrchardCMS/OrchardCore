import { describe, expect, it } from "vitest";
import { useGlobals } from "../Globals";

describe("Globals", () => {
  const {
    uploadFilesUrl,
    isLoading,
    basePath,
    assetsStore,
    selectedDirectory,
    setUploadFilesUrl,
    setIsLoading,
    setBasePath,
    setAssetsStore,
    setSelectedDirectory,
  } = useGlobals();

  describe("setUploadFilesUrl", () => {
    it("sets the upload base URL and computes uploadFilesUrl", () => {
      setUploadFilesUrl("https://example.com/upload");
      setSelectedDirectory({ name: "root", directoryPath: "/Images", filePath: "/Images", isDirectory: true });

      expect(uploadFilesUrl.value).toBe("https://example.com/upload?path=%2FImages");
    });

    it("computes uploadFilesUrl with empty directoryPath", () => {
      setUploadFilesUrl("https://example.com/upload");
      setSelectedDirectory({ name: "root", directoryPath: "", filePath: "", isDirectory: true });

      expect(uploadFilesUrl.value).toBe("https://example.com/upload?path=");
    });
  });

  describe("setIsLoading", () => {
    it("sets isLoading to true", () => {
      setIsLoading(true);
      expect(isLoading.value).toBe(true);
    });

    it("sets isLoading to false", () => {
      setIsLoading(false);
      expect(isLoading.value).toBe(false);
    });
  });

  describe("setBasePath", () => {
    it("sets the base path", () => {
      setBasePath("/my/base/path");
      expect(basePath.value).toBe("/my/base/path");
    });

    it("sets the base path to empty string", () => {
      setBasePath("");
      expect(basePath.value).toBe("");
    });
  });

  describe("setIsDownloading", () => {
    const { isDownloading, setIsDownloading } = useGlobals();

    it("sets isDownloading to true", () => {
      setIsDownloading(true);
      expect(isDownloading.value).toBe(true);
    });

    it("sets isDownloading to false", () => {
      setIsDownloading(false);
      expect(isDownloading.value).toBe(false);
    });
  });

  describe("setAssetsStore", () => {
    it("sets the assets store", () => {
      const items = [
        { name: "test.jpg", directoryPath: "/Images", filePath: "/Images/test.jpg", isDirectory: false },
      ];
      setAssetsStore(items);
      expect(assetsStore.value).toEqual(items);
    });

    it("sets the assets store to empty array", () => {
      setAssetsStore([]);
      expect(assetsStore.value).toEqual([]);
    });
  });
});
