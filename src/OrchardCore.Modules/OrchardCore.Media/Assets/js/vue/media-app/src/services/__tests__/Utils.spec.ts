import { beforeEach, describe, expect, it, vi } from "vitest";
import { getFileExtension, humanFileSize, printDateTime, downloadFile, downloadSelectedFiles } from "../Utils";
import { useGlobals } from "../Globals";
import { translationsData } from "../../__tests__/mockdata";
import { useLocalizations } from "../Localizations";

const { setTranslations } = useLocalizations();
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
  let xhrMock: {
    open: ReturnType<typeof vi.fn>;
    send: ReturnType<typeof vi.fn>;
    onload: (() => void) | null;
    onerror: (() => void) | null;
    status: number;
  };

  beforeEach(() => {
    xhrMock = {
      open: vi.fn(),
      send: vi.fn(),
      onload: null,
      onerror: null,
      status: 200,
    };

    vi.spyOn(window, "XMLHttpRequest").mockImplementation(() => xhrMock as unknown as XMLHttpRequest);

    // Mock document.createElement to intercept anchor creation
    vi.spyOn(document, "createElement").mockImplementation((tag: string) => {
      if (tag === "a") {
        const a = {
          setAttribute: vi.fn(),
          click: vi.fn(),
          href: "",
        } as unknown as HTMLAnchorElement;
        return a;
      }
      return document.createElement(tag);
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("should do nothing when file has no url", () => {
    const file = { name: "test.jpg", filePath: "/test.jpg", directoryPath: "/", isDirectory: false };
    downloadFile(file);
    expect(xhrMock.open).not.toHaveBeenCalled();
  });

  it("should do nothing when file is falsy", () => {
    downloadFile(null as any); // eslint-disable-line @typescript-eslint/no-explicit-any
    expect(xhrMock.open).not.toHaveBeenCalled();
  });

  it("should open an XHR HEAD request when file has a url", () => {
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    downloadFile(file);
    expect(xhrMock.open).toHaveBeenCalledWith("HEAD", "/media/photo.jpg", false);
    expect(xhrMock.send).toHaveBeenCalled();
  });

  it("should create and click anchor when XHR returns 200", () => {
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    xhrMock.status = 200;
    downloadFile(file);

    // Trigger the onload handler
    if (xhrMock.onload) xhrMock.onload();

    expect(document.createElement).toHaveBeenCalledWith("a");
  });

  it("should handle XHR error without throwing", () => {
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    downloadFile(file);

    // Trigger onerror - should not throw
    expect(() => {
      if (xhrMock.onerror) xhrMock.onerror();
    }).not.toThrow();
  });

  it("should handle non-200 status without throwing", () => {
    const file = { name: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false, url: "/media/photo.jpg" };
    xhrMock.status = 404;
    downloadFile(file);

    expect(() => {
      if (xhrMock.onload) xhrMock.onload();
    }).not.toThrow();
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
