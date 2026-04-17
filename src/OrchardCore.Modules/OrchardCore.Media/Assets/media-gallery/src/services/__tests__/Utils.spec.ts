import { beforeEach, describe, expect, it, vi } from "vitest";
import { getFileExtension, humanFileSize, printDateTime, downloadFile, downloadSelectedFiles } from "../Utils";
import { useGlobals } from "../Globals";
import { translationsData } from "../../__tests__/mockdata";
import { setTranslations } from "@bloom/helpers/localizations";

setTranslations(translationsData);

describe("GetFileExtension", () => {
  it("should return a correct file extension when file has extension", () => {
    expect(getFileExtension("file.jpg")).toBe("jpg");
  });

  it("should return empty string when file has no extension", () => {
    expect(getFileExtension("file")).toBe("");
  });

  it("should return the last extension when file has a dot in his name", () => {
    expect(getFileExtension("file.dot.jpg")).toBe("jpg");
  });

  it("should return empty string for undefined input", () => {
    expect(getFileExtension(undefined)).toBe("");
  });

  it("should return empty string for empty string input", () => {
    expect(getFileExtension("")).toBe("");
  });
});

describe("HumanFileSize", () => {
  it("should return correct size with SI units", () => {
    expect(humanFileSize(100000000, true, 2)).toBe("100.00 MB");
  });

  it("should round correctly with SI units", () => {
    expect(humanFileSize(100000001, true, 2)).toBe("100.00 MB");
    expect(humanFileSize(100001000, true, 2)).toBe("100.00 MB");
    expect(humanFileSize(100010000, true, 2)).toBe("100.01 MB");
    expect(humanFileSize(100100000, true, 2)).toBe("100.10 MB");
  });

  it("should return correct size without decimals with SI units", () => {
    expect(humanFileSize(100000000, true, 0)).toBe("100 MB");
  });

  it("should return correct size with binary (IEC) units", () => {
    expect(humanFileSize(100000000, false, 0)).toBe("95 MiB");
  });

  it("should return bytes for small values", () => {
    expect(humanFileSize(500, true)).toBe("500 B");
    expect(humanFileSize(0, true)).toBe("0 B");
  });

  it("should throw error for null input", () => {
    expect(() => humanFileSize(null as unknown as number)).toThrow("humanFileSize: bytes is null or undefined");
  });

  it("should throw error for undefined input", () => {
    expect(() => humanFileSize(undefined as unknown as number)).toThrow("humanFileSize: bytes is null or undefined");
  });

  it("should handle kilobytes correctly", () => {
    expect(humanFileSize(1500, true, 1)).toBe("1.5 kB");
  });

  it("should handle gigabytes correctly", () => {
    expect(humanFileSize(1000000000, true, 0)).toBe("1 GB");
  });
});

describe("PrintDateTime", () => {
  it("should format a date string", () => {
    const result = printDateTime("2024-01-15T10:30:00Z");
    expect(result).toBeTruthy();
    expect(typeof result).toBe("string");
    expect(result.length).toBeGreaterThan(0);
  });

  it("should format a timestamp number", () => {
    const result = printDateTime(1705312200000);
    expect(result).toBeTruthy();
    expect(typeof result).toBe("string");
  });

  it("should format a Date object", () => {
    const result = printDateTime(new Date(2024, 0, 15, 10, 30));
    expect(result).toBeTruthy();
    expect(typeof result).toBe("string");
  });

  it("should return empty string for null", () => {
    expect(printDateTime(null)).toBe("");
  });

  it("should return empty string for undefined", () => {
    expect(printDateTime(undefined)).toBe("");
  });

  it("should return empty string for empty string", () => {
    expect(printDateTime("")).toBe("");
  });
});

describe("downloadFile", () => {
  let anchorMock: { setAttribute: ReturnType<typeof vi.fn>; click: ReturnType<typeof vi.fn>; href: string };

  beforeEach(() => {
    anchorMock = { setAttribute: vi.fn(), click: vi.fn(), href: "" };

    vi.spyOn(document, "createElement").mockImplementation((tag: string) => {
      if (tag === "a") return anchorMock as unknown as HTMLAnchorElement;
      return document.createElement(tag);
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("should do nothing when file has no url", async () => {
    global.fetch = vi.fn();
    const file = { name: "test.jpg", filePath: "/test.jpg", directoryPath: "/", isDirectory: false };
    await downloadFile(file);
    expect(global.fetch).not.toHaveBeenCalled();
  });

  it("should do nothing when file is falsy", async () => {
    global.fetch = vi.fn();
    await downloadFile(null as any); // eslint-disable-line @typescript-eslint/no-explicit-any
    expect(global.fetch).not.toHaveBeenCalled();
  });

  it("should send a HEAD fetch request when file has a url", async () => {
    global.fetch = vi.fn(() => Promise.resolve({ ok: true } as Response));
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    await downloadFile(file);
    expect(global.fetch).toHaveBeenCalledWith("/media/photo.jpg", { method: "HEAD" });
  });

  it("should create and click anchor when fetch returns ok", async () => {
    global.fetch = vi.fn(() => Promise.resolve({ ok: true } as Response));
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    await downloadFile(file);
    expect(document.createElement).toHaveBeenCalledWith("a");
    expect(anchorMock.click).toHaveBeenCalled();
  });

  it("should handle fetch error without throwing", async () => {
    global.fetch = vi.fn(() => Promise.reject(new Error("network error")));
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    await expect(downloadFile(file)).resolves.toBeUndefined();
  });

  it("should handle non-ok response without throwing", async () => {
    global.fetch = vi.fn(() => Promise.resolve({ ok: false, status: 404 } as Response));
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    await expect(downloadFile(file)).resolves.toBeUndefined();
  });
});

describe("downloadSelectedFiles", () => {
  const { setSelectedFiles, setSelectedAll, setIsDownloading } = useGlobals();

  beforeEach(() => {
    // Mock fetch to return a blob
    global.fetch = vi.fn(() =>
      Promise.resolve({
        blob: () => Promise.resolve(new Blob(["data"], { type: "image/jpeg" })),
      } as Response),
    );

    // Mock URL.createObjectURL and revokeObjectURL
    global.URL.createObjectURL = vi.fn(() => "blob:mock-url");
    global.URL.revokeObjectURL = vi.fn();

    // Mock document.body.appendChild and element.remove
    vi.spyOn(document.body, "appendChild").mockImplementation((el) => el);
    vi.spyOn(document.body, "removeChild").mockImplementation((el) => el);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    setSelectedFiles([]);
    setSelectedAll(false);
    setIsDownloading(false);
  });

  it("should download selected files and clear selection", async () => {
    setSelectedFiles([
      { name: "photo1.jpg", filePath: "/photo1.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo1.jpg" },
      { name: "photo2.jpg", filePath: "/photo2.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo2.jpg" },
    ]);

    await downloadSelectedFiles();

    expect(global.fetch).toHaveBeenCalledTimes(2);
    expect(global.fetch).toHaveBeenCalledWith("/media/photo1.jpg");
    expect(global.fetch).toHaveBeenCalledWith("/media/photo2.jpg");
    expect(global.URL.createObjectURL).toHaveBeenCalledTimes(2);
    expect(global.URL.revokeObjectURL).toHaveBeenCalledTimes(2);
  });

  it("should skip files without a url", async () => {
    setSelectedFiles([
      { name: "photo1.jpg", filePath: "/photo1.jpg", directoryPath: "/", isDirectory: false }, // no url
      { name: "photo2.jpg", filePath: "/photo2.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo2.jpg" },
    ]);

    await downloadSelectedFiles();

    expect(global.fetch).toHaveBeenCalledTimes(1);
    expect(global.fetch).toHaveBeenCalledWith("/media/photo2.jpg");
  });

  it("should set isDownloading to false after completion", async () => {
    const { isDownloading } = useGlobals();
    setSelectedFiles([
      { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" },
    ]);

    await downloadSelectedFiles();

    expect(isDownloading.value).toBe(false);
  });

  it("should work with empty selection", async () => {
    setSelectedFiles([]);
    await downloadSelectedFiles();
    expect(global.fetch).not.toHaveBeenCalled();
  });
});
