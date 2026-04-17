import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

// ---------- Hoisted mock state ----------
// vi.hoisted() ensures these are available when vi.mock factories run (hoisted above imports)
const {
  uppyEventHandlers,
  mockGetFiles,
  mockGetFile,
  mockSetFileMeta,
  mockSetMeta,
  mockUpload,
  mockClear,
  mockAddFiles,
  mockPauseResume,
  mockSetOptions,
  mockGetPlugin,
  mockUse,
  mockUppyInstance,
  mockNotify,
} = vi.hoisted(() => {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const uppyEventHandlers: Record<string, ((...args: any[]) => any)[]> = {};
  const mockGetFiles = vi.fn(() => []);
  const mockGetFile = vi.fn();
  const mockSetFileMeta = vi.fn();
  const mockSetMeta = vi.fn();
  const mockUpload = vi.fn(() => Promise.resolve());
  const mockClear = vi.fn();
  const mockAddFiles = vi.fn();
  const mockPauseResume = vi.fn();
  const mockSetOptions = vi.fn();
  const mockGetPlugin = vi.fn(() => null);
  const mockUse = vi.fn();
  const mockNotify = vi.fn();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  function mockUppyOn(event: string, handler: (...args: any[]) => any) {
    if (!uppyEventHandlers[event]) {
      uppyEventHandlers[event] = [];
    }
    uppyEventHandlers[event].push(handler);
  }

  const mockUppyInstance = {
    on: vi.fn(mockUppyOn),
    getFiles: mockGetFiles,
    getFile: mockGetFile,
    setFileMeta: mockSetFileMeta,
    setMeta: mockSetMeta,
    upload: mockUpload,
    clear: mockClear,
    addFiles: mockAddFiles,
    pauseResume: mockPauseResume,
    setOptions: mockSetOptions,
    getPlugin: mockGetPlugin,
    use: mockUse,
    opts: { restrictions: {} },
  };

  return {
    uppyEventHandlers,
    mockGetFiles,
    mockGetFile,
    mockSetFileMeta,
    mockSetMeta,
    mockUpload,
    mockClear,
    mockAddFiles,
    mockPauseResume,
    mockSetOptions,
    mockGetPlugin,
    mockUse,
    mockUppyInstance,
    mockNotify,
  };
});

// ---------- Mock modules ----------
vi.mock("@uppy/core", () => {
  const MockUppy = vi.fn(() => mockUppyInstance);
  return {
    default: MockUppy,
    debugLogger: {},
  };
});

vi.mock("@uppy/drop-target", () => ({ default: vi.fn() }));
vi.mock("@uppy/xhr-upload", () => ({ default: vi.fn() }));
vi.mock("@uppy/tus", () => ({ default: vi.fn() }));
vi.mock("@uppy/locales/lib/en_US", () => ({ default: { strings: {} } }));
vi.mock("@uppy/locales/lib/fr_FR", () => ({ default: { strings: {} } }));
vi.mock("@uppy/locales/lib/it_IT", () => ({ default: { strings: {} } }));
vi.mock("@uppy/locales/lib/es_ES", () => ({ default: { strings: {} } }));
vi.mock("@uppy/drop-target/dist/style.css", () => ({}));

vi.mock("@bloom/services/notifications/notifier", () => ({
  notify: (...args: any[]) => mockNotify(...args), // eslint-disable-line @typescript-eslint/no-explicit-any
  NotificationMessage: class {
    summary?: string;
    detail?: string;
    severity?: string;
    constructor(data?: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
      if (data) {
        this.summary = data.summary;
        this.detail = data.detail;
        this.severity = data.severity;
      }
    }
  },
}));

// ---------- Imports (run after mocks are set up) ----------
import { useEventBus } from "../UseEventBus";
import { setTranslations } from "@bloom/helpers/localizations";
import { translationsData } from "../../__tests__/mockdata";
import { updateUploadOptions, useFileUpload } from "../UppyFileUpload";
import { useGlobals } from "../Globals";
import Uppy from "@uppy/core";
import XHRUpload from "@uppy/xhr-upload";
import Tus from "@uppy/tus";
import DropTarget from "@uppy/drop-target";

// Set up localizations
setTranslations({
  ...translationsData,
  ValidationError: "Validation Error",
  ValidationErrorUploadFileExist: "A file with this name already exists.",
});

// Helper to emit events on the mock Uppy instance
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function emitUppyEvent(event: string, ...args: any[]) {
  (uppyEventHandlers[event] ?? []).forEach((handler) => handler(...args));
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
async function emitUppyEventAsync(event: string, ...args: any[]) {
  for (const handler of (uppyEventHandlers[event] ?? [])) {
    await handler(...args);
  }
}

describe("UppyFileUpload", () => {
  it("should initialize Uppy instance", () => {
    expect(Uppy).toHaveBeenCalled();
  });

  it("should have event bus with upload events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadFileAdded", (data) => {
      receivedData = data;
    });

    emit("UploadFileAdded", { name: "test.jpg" });
    expect(receivedData).toEqual({ name: "test.jpg" });
  });

  it("should emit upload progress events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadProgress", (data) => {
      receivedData = data;
    });

    emit("UploadProgress", { name: "test.jpg", percentage: 50 });
    expect(receivedData).toEqual({ name: "test.jpg", percentage: 50 });
  });

  it("should emit upload success events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadSuccess", (data) => {
      receivedData = data;
    });

    emit("UploadSuccess", { name: "test.jpg" });
    expect(receivedData).toEqual({ name: "test.jpg" });
  });

  it("should emit upload error events", () => {
    const { on, emit } = useEventBus();

    let receivedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
    on("UploadError", (data) => {
      receivedData = data;
    });

    emit("UploadError", { name: "test.jpg", errorMessage: "Upload failed" });
    expect(receivedData).toEqual({ name: "test.jpg", errorMessage: "Upload failed" });
  });

  describe("updateUploadOptions", () => {
    it("should be a function", () => {
      expect(typeof updateUploadOptions).toBe("function");
    });

    it("should call setOptions on the Uppy instance", () => {
      updateUploadOptions({
        restrictions: {
          maxFileSize: 5 * 1024 * 1024,
          maxNumberOfFiles: 10,
          allowedFileTypes: ["image/*"],
        },
      });
      expect(mockSetOptions).toHaveBeenCalled();
    });

    it("should update upload options with meta and restrictions", () => {
      updateUploadOptions({
        meta: { destinationPath: "/uploads" },
        restrictions: {
          maxFileSize: 10 * 1024 * 1024,
        },
      });
      expect(mockSetOptions).toHaveBeenCalledWith(
        expect.objectContaining({
          meta: { destinationPath: "/uploads" },
        }),
      );
    });

    it("should accept empty options object", () => {
      updateUploadOptions({});
      expect(mockSetOptions).toHaveBeenCalledWith({});
    });
  });

  describe("AfterDirSelected event", () => {
    it("should handle AfterDirSelected event without throwing", () => {
      const { emit } = useEventBus();

      expect(() => {
        emit("AfterDirSelected", {
          name: "Images",
          directoryPath: "/Images",
          filePath: "/Images",
          isDirectory: true,
        });
      }).not.toThrow();
    });
  });

  describe("useFileUpload", () => {
    let globals: ReturnType<typeof useGlobals>;
    let fileInput: HTMLInputElement;
    let eventBus: ReturnType<typeof useEventBus>;

    const xhrModel = {
      maxUploadChunkSize: 0,
      tusEnabled: false,
      tusEndpointUrl: "",
      tusFileInfoUrl: "",
    };

    const tusModel = {
      maxUploadChunkSize: 5 * 1024 * 1024,
      tusEnabled: true,
      tusEndpointUrl: "http://localhost/tus",
      tusFileInfoUrl: "http://localhost/tus/fileinfo",
    };

    async function mountWithFileUpload(model: Parameters<typeof useFileUpload>[0]) {
      const { mount } = await import("@vue/test-utils");
      const { defineComponent, h } = await import("vue");

      const TestComponent = defineComponent({
        setup() {
          useFileUpload(model);
          return () => h("div");
        },
      });

      return mount(TestComponent, { attachTo: document.body });
    }

    beforeEach(() => {
      vi.clearAllMocks();
      mockNotify.mockClear();

      // Clear accumulated Uppy event handlers from previous tests
      Object.keys(uppyEventHandlers).forEach((key) => delete uppyEventHandlers[key]);

      // Re-wire the on mock to continue capturing handlers
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      mockUppyInstance.on.mockImplementation((event: string, handler: (...args: any[]) => any) => {
        if (!uppyEventHandlers[event]) {
          uppyEventHandlers[event] = [];
        }
        uppyEventHandlers[event].push(handler);
      });

      // Reset getPlugin to return null (no plugins installed)
      mockGetPlugin.mockReturnValue(null);
      mockUpload.mockResolvedValue(undefined);
      mockGetFiles.mockReturnValue([]);

      globals = useGlobals();
      globals.setSelectedDirectory({
        name: "Images",
        directoryPath: "/Images",
        filePath: "/Images",
        isDirectory: true,
      });
      globals.setAssetsStore([]);
      globals.setFileItems([]);
      globals.setUploadFilesUrl("/api/media/upload");

      eventBus = useEventBus();

      // Create mock file input
      fileInput = document.createElement("input");
      fileInput.id = "fileupload";
      fileInput.type = "file";
      document.body.appendChild(fileInput);
    });

    afterEach(() => {
      const el = document.getElementById("fileupload");
      if (el) el.remove();
      eventBus.all.clear();
    });

    describe("XHR mode initialization", () => {
      it("should mount without errors", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);
        expect(wrapper.exists()).toBe(true);
        wrapper.unmount();
      });

      it("should register AfterDirSelected handler", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        eventBus.emit("AfterDirSelected", {
          name: "Documents",
          directoryPath: "/Documents",
          filePath: "/Documents",
          isDirectory: true,
        });

        wrapper.unmount();
      });

      it("should install XHRUpload plugin", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);
        expect(mockUse).toHaveBeenCalledWith(XHRUpload, expect.any(Object));
        wrapper.unmount();
      });

      it("should install DropTarget plugin", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);
        expect(mockUse).toHaveBeenCalledWith(DropTarget, expect.objectContaining({
          target: document.body,
        }));
        wrapper.unmount();
      });

      it("should not re-register XHRUpload plugin if already installed", async () => {
        const mockXhrPlugin = { setOptions: vi.fn() };
        mockGetPlugin.mockImplementation((name: string) => {
          if (name === "XHRUpload") return mockXhrPlugin;
          return null;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        expect(mockUse).not.toHaveBeenCalledWith(XHRUpload, expect.any(Object));
        expect(mockXhrPlugin.setOptions).toHaveBeenCalled();

        wrapper.unmount();
      });

      it("should not re-register DropTarget plugin if already installed", async () => {
        mockGetPlugin.mockImplementation((name: string) => {
          if (name === "DropTarget") return {};
          return null;
        });

        const wrapper = await mountWithFileUpload(xhrModel);
        expect(mockUse).not.toHaveBeenCalledWith(DropTarget, expect.any(Object));
        wrapper.unmount();
      });
    });

    describe("TUS mode initialization", () => {
      it("should mount without errors", async () => {
        const wrapper = await mountWithFileUpload(tusModel);
        expect(wrapper.exists()).toBe(true);
        wrapper.unmount();
      });

      it("should install TUS plugin", async () => {
        const wrapper = await mountWithFileUpload(tusModel);
        expect(mockUse).toHaveBeenCalledWith(Tus, expect.objectContaining({
          endpoint: "http://localhost/tus",
        }));
        wrapper.unmount();
      });

      it("should use maxUploadChunkSize when greater than 0", async () => {
        const wrapper = await mountWithFileUpload({
          ...tusModel,
          maxUploadChunkSize: 10 * 1024 * 1024,
        });
        expect(mockUse).toHaveBeenCalledWith(Tus, expect.objectContaining({
          chunkSize: 10 * 1024 * 1024,
        }));
        wrapper.unmount();
      });

      it("should use default 5MB chunk size when maxUploadChunkSize is 0", async () => {
        const wrapper = await mountWithFileUpload({
          ...tusModel,
          maxUploadChunkSize: 0,
        });
        expect(mockUse).toHaveBeenCalledWith(Tus, expect.objectContaining({
          chunkSize: 5 * 1024 * 1024,
        }));
        wrapper.unmount();
      });

      it("should not re-register TUS plugin if already installed", async () => {
        mockGetPlugin.mockImplementation((name: string) => {
          if (name === "Tus") return {};
          return null;
        });

        const wrapper = await mountWithFileUpload(tusModel);
        expect(mockUse).not.toHaveBeenCalledWith(Tus, expect.any(Object));
        wrapper.unmount();
      });

      it("should fall back to XHR when tusEnabled but tusEndpointUrl is empty", async () => {
        const wrapper = await mountWithFileUpload({
          maxUploadChunkSize: 0,
          tusEnabled: true,
          tusEndpointUrl: "",
          tusFileInfoUrl: "",
        });

        expect(mockUse).toHaveBeenCalledWith(XHRUpload, expect.any(Object));
        expect(mockUse).not.toHaveBeenCalledWith(Tus, expect.any(Object));
        wrapper.unmount();
      });
    });

    describe("fileInput change handler", () => {
      it("should call uppy.clear and uppy.addFiles on change", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const changeEvent = new Event("change", { bubbles: true });
        Object.defineProperty(changeEvent, "target", {
          value: {
            files: [
              new File(["content"], "test.jpg", { type: "image/jpeg" }),
            ],
          },
        });

        fileInput.dispatchEvent(changeEvent);

        expect(mockClear).toHaveBeenCalled();
        expect(mockAddFiles).toHaveBeenCalledWith([
          expect.objectContaining({
            source: "file input",
            name: "test.jpg",
            type: "image/jpeg",
          }),
        ]);

        wrapper.unmount();
      });

      it("should handle multiple files", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const changeEvent = new Event("change", { bubbles: true });
        Object.defineProperty(changeEvent, "target", {
          value: {
            files: [
              new File(["a"], "file1.jpg", { type: "image/jpeg" }),
              new File(["b"], "file2.png", { type: "image/png" }),
            ],
          },
        });

        fileInput.dispatchEvent(changeEvent);

        expect(mockAddFiles).toHaveBeenCalledWith([
          expect.objectContaining({ name: "file1.jpg" }),
          expect.objectContaining({ name: "file2.png" }),
        ]);

        wrapper.unmount();
      });

      it("should handle restriction error from addFiles", async () => {
        const restrictionError = new Error("File too large");
        (restrictionError as any).isRestriction = true; // eslint-disable-line @typescript-eslint/no-explicit-any
        mockAddFiles.mockImplementationOnce(() => { throw restrictionError; });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        const changeEvent = new Event("change", { bubbles: true });
        Object.defineProperty(changeEvent, "target", {
          value: {
            files: [new File(["content"], "big.jpg", { type: "image/jpeg" })],
          },
        });

        fileInput.dispatchEvent(changeEvent);

        expect(consoleSpy).toHaveBeenCalledWith("Restriction error:", expect.any(Error));
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should handle non-restriction error from addFiles", async () => {
        const error = new Error("Unknown error");
        mockAddFiles.mockImplementationOnce(() => { throw error; });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        const changeEvent = new Event("change", { bubbles: true });
        Object.defineProperty(changeEvent, "target", {
          value: {
            files: [new File(["content"], "bad.jpg", { type: "image/jpeg" })],
          },
        });

        fileInput.dispatchEvent(changeEvent);

        expect(consoleSpy).toHaveBeenCalledWith("error:", expect.any(Error));
        consoleSpy.mockRestore();
        wrapper.unmount();
      });
    });

    describe("restriction-failed handler", () => {
      it("should notify with validation error", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("restriction-failed", { name: "test.jpg" }, { message: "File too big" });

        expect(mockNotify).toHaveBeenCalledWith(
          expect.objectContaining({
            summary: "Validation Error",
            detail: "File too big",
          }),
        );

        wrapper.unmount();
      });
    });

    describe("files-added handler", () => {
      it("should set meta with destinationPath in XHR mode", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "newfile.jpg" },
        ]);

        expect(mockSetMeta).toHaveBeenCalledWith({ destinationPath: "/Images" });
        wrapper.unmount();
      });

      it("should set per-file meta in TUS mode", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "tusfile.jpg" },
        ]);

        expect(mockSetFileMeta).toHaveBeenCalledWith("f1", {
          destinationPath: "/Images",
          fileName: "tusfile.jpg",
        });
        wrapper.unmount();
      });

      it("should notify when uploading a file that already exists", async () => {
        globals.setFileItems([
          {
            filePath: "/Images/existing.jpg",
            directoryPath: "/Images",
            name: "existing.jpg",
            isDirectory: false,
          },
        ]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "existing.jpg" },
        ]);

        expect(mockNotify).toHaveBeenCalledWith(
          expect.objectContaining({
            summary: "Validation Error",
            detail: "A file with this name already exists.",
          }),
        );

        expect(mockUpload).not.toHaveBeenCalled();
        wrapper.unmount();
      });

      it("should detect existing file at root directory path", async () => {
        globals.setSelectedDirectory({
          name: "/",
          directoryPath: "/",
          filePath: "/",
          isDirectory: true,
        });
        globals.setFileItems([
          {
            filePath: "/rootfile.jpg",
            directoryPath: "/",
            name: "rootfile.jpg",
            isDirectory: false,
          },
        ]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "rootfile.jpg" },
        ]);

        expect(mockNotify).toHaveBeenCalledWith(
          expect.objectContaining({
            detail: "A file with this name already exists.",
          }),
        );
        wrapper.unmount();
      });

      it("should detect existing file in subdirectory", async () => {
        globals.setSelectedDirectory({
          name: "Docs",
          directoryPath: "/Docs",
          filePath: "/Docs",
          isDirectory: true,
        });
        globals.setFileItems([
          {
            filePath: "/Docs/readme.txt",
            directoryPath: "/Docs",
            name: "readme.txt",
            isDirectory: false,
          },
        ]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "readme.txt" },
        ]);

        expect(mockNotify).toHaveBeenCalled();
        expect(mockUpload).not.toHaveBeenCalled();
        wrapper.unmount();
      });

      it("should add placeholder to assetsStore and emit UploadFileAdded", async () => {
        globals.setAssetsStore([]);
        globals.setFileItems([]);

        let addedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadFileAdded", (data) => {
          addedData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "newfile.jpg" },
        ]);

        expect(globals.assetsStore.value).toEqual(
          expect.arrayContaining([
            expect.objectContaining({
              name: "newfile.jpg",
              directoryPath: "/Images",
              isDirectory: false,
            }),
          ]),
        );
        expect(addedData).toEqual({ name: "newfile.jpg" });
        wrapper.unmount();
      });

      it("should construct correct filePath for root directory placeholder", async () => {
        globals.setSelectedDirectory({
          name: "/",
          directoryPath: "/",
          filePath: "/",
          isDirectory: true,
        });
        globals.setAssetsStore([]);
        globals.setFileItems([]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "rootfile.jpg" },
        ]);

        expect(globals.assetsStore.value).toEqual(
          expect.arrayContaining([
            expect.objectContaining({
              filePath: "/rootfile.jpg",
            }),
          ]),
        );
        wrapper.unmount();
      });

      it("should construct correct filePath for subdirectory placeholder", async () => {
        globals.setAssetsStore([]);
        globals.setFileItems([]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "photo.jpg" },
        ]);

        expect(globals.assetsStore.value).toEqual(
          expect.arrayContaining([
            expect.objectContaining({
              filePath: "/Images/photo.jpg",
            }),
          ]),
        );
        wrapper.unmount();
      });

      it("should not add duplicate placeholder if already in assetsStore", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/photo.jpg",
            directoryPath: "/Images",
            name: "photo.jpg",
            isDirectory: false,
          },
        ]);
        globals.setFileItems([]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "photo.jpg" },
        ]);

        const count = globals.assetsStore.value.filter(
          (a) => a.name === "photo.jpg",
        ).length;
        expect(count).toBe(1);
        wrapper.unmount();
      });

      it("should call uppy.upload and uppy.clear after adding files", async () => {
        globals.setAssetsStore([]);
        globals.setFileItems([]);

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "newfile.jpg" },
        ]);

        expect(mockUpload).toHaveBeenCalled();
        expect(mockClear).toHaveBeenCalled();
        wrapper.unmount();
      });

      it("should handle upload error gracefully", async () => {
        globals.setAssetsStore([]);
        globals.setFileItems([]);
        mockUpload.mockRejectedValueOnce(new Error("Upload failed"));

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        await emitUppyEventAsync("files-added", [
          { id: "f1", name: "fail.jpg" },
        ]);

        expect(consoleSpy).toHaveBeenCalledWith("upload error", expect.any(Error));
        consoleSpy.mockRestore();
        wrapper.unmount();
      });
    });

    describe("upload-progress handler", () => {
      it("should emit UploadProgress with file progress data", async () => {
        let progressData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadProgress", (data) => {
          progressData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-progress",
          { name: "file1.jpg", progress: { percentage: 50 } },
          { bytesUploaded: 5000, bytesTotal: 10000 },
        );

        expect(progressData).toEqual({
          name: "file1.jpg",
          percentage: 50,
          bytesUploaded: 5000,
          bytesTotal: 10000,
        });
        wrapper.unmount();
      });

      it("should use 0 for percentage when file progress has no percentage", async () => {
        let progressData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadProgress", (data) => {
          progressData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-progress",
          { name: "file1.jpg", progress: {} },
          { bytesUploaded: 100, bytesTotal: 200 },
        );

        expect(progressData).toEqual({
          name: "file1.jpg",
          percentage: 0,
          bytesUploaded: 100,
          bytesTotal: 200,
        });
        wrapper.unmount();
      });

      it("should use empty string when file name is undefined", async () => {
        let progressData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadProgress", (data) => {
          progressData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-progress",
          { progress: { percentage: 30 } },
          { bytesUploaded: 300, bytesTotal: 1000 },
        );

        expect(progressData).toEqual({
          name: "",
          percentage: 30,
          bytesUploaded: 300,
          bytesTotal: 1000,
        });
        wrapper.unmount();
      });

      it("should use 0 for bytesUploaded and bytesTotal when not provided", async () => {
        let progressData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadProgress", (data) => {
          progressData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-progress",
          { name: "file.jpg", progress: { percentage: 10 } },
          {},
        );

        expect(progressData).toEqual({
          name: "file.jpg",
          percentage: 10,
          bytesUploaded: 0,
          bytesTotal: 0,
        });
        wrapper.unmount();
      });

      it("should not emit when file is null/falsy", async () => {
        let progressData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadProgress", (data) => {
          progressData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-progress", null, { bytesUploaded: 0, bytesTotal: 0 });

        expect(progressData).toBeNull();
        wrapper.unmount();
      });
    });

    describe("file-removed handler", () => {
      it("should clear file input value", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("file-removed");

        expect(fileInput.value).toBe("");
        wrapper.unmount();
      });
    });

    describe("upload-error handler", () => {
      it("should emit UploadError with error message", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error",
          { name: "fail.jpg" },
          { message: "Server error" },
          undefined,
        );

        expect(errorData).toEqual({
          name: "fail.jpg",
          errorMessage: "Server error",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should use response body when it is a short string", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error",
          { name: "fail.mkv" },
          { message: "Upload rejected" },
          { body: "File extension not allowed: .mkv" },
        );

        expect(errorData).toEqual({
          name: "fail.mkv",
          errorMessage: "File extension not allowed: .mkv",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should ignore response body when it is too long", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        const longBody = "x".repeat(501);
        emitUppyEvent("upload-error",
          { name: "fail.jpg" },
          { message: "Error occurred" },
          { body: longBody },
        );

        expect(errorData).toEqual({
          name: "fail.jpg",
          errorMessage: "Error occurred",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should ignore response body when it is empty string", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error",
          { name: "fail.jpg" },
          { message: "Generic error" },
          { body: "" },
        );

        expect(errorData).toEqual({
          name: "fail.jpg",
          errorMessage: "Generic error",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should ignore response body when it is not a string", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error",
          { name: "fail.jpg" },
          { message: "Error" },
          { body: { json: true } },
        );

        expect(errorData).toEqual({
          name: "fail.jpg",
          errorMessage: "Error",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should use empty string for name when file.name is undefined", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error", {}, { message: "Error" }, undefined);

        expect(errorData).toEqual({
          name: "",
          errorMessage: "Error",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should not emit when file is null", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error", null, { message: "Error" }, undefined);

        expect(errorData).toBeNull();
        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should fall back to translation Error when error.message is undefined", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("upload-error", { name: "fail.jpg" }, {}, undefined);

        expect(errorData).toEqual({
          name: "fail.jpg",
          errorMessage: "Error",
        });
        consoleSpy.mockRestore();
        wrapper.unmount();
      });
    });

    describe("TUS upload-success handler", () => {
      it("should fetch file info and update placeholder on success", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/tusfile.jpg",
            directoryPath: "/Images",
            name: "tusfile.jpg",
            isDirectory: false,
          },
        ]);

        const serverFile = {
          name: "tusfile.jpg",
          directoryPath: "/Images",
          filePath: "/Images/tusfile.jpg",
          size: 12345,
          lastModifiedUtc: "2024-10-01T00:00:00Z",
          url: "/media/Images/tusfile.jpg",
          mime: "image/jpeg",
        };

        global.fetch = vi.fn().mockResolvedValue({
          ok: true,
          json: () => Promise.resolve(serverFile),
        });

        let successData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => {
          successData = data;
        });

        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("upload-success",
          { id: "f1", name: "tusfile.jpg" },
          { uploadURL: "http://localhost/tus/abc123" },
        );

        expect(global.fetch).toHaveBeenCalledWith(
          "http://localhost/tus/fileinfo/abc123",
        );

        const placeholder = globals.assetsStore.value.find((a) => a.name === "tusfile.jpg");
        expect(placeholder?.size).toBe(12345);
        expect(placeholder?.url).toBe("/media/Images/tusfile.jpg");
        expect(placeholder?.mime).toBe("image/jpeg");

        expect(successData).toEqual({ name: "tusfile.jpg", resumed: false });
        wrapper.unmount();
      });

      it("should mark upload as resumed when file was previously resumed", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/resumed.jpg",
            directoryPath: "/Images",
            name: "resumed.jpg",
            isDirectory: false,
          },
        ]);

        global.fetch = vi.fn().mockResolvedValue({
          ok: true,
          json: () => Promise.resolve({
            name: "resumed.jpg",
            directoryPath: "/Images",
            filePath: "/Images/resumed.jpg",
            size: 5000,
          }),
        });

        // Simulate the "upload" event that detects resumed files
        const wrapper = await mountWithFileUpload(tusModel);

        mockGetFile.mockReturnValue({ id: "f1", name: "resumed.jpg", tus: { uploadUrl: "http://localhost/tus/existing" } });
        emitUppyEvent("upload", { fileIDs: ["f1"] });

        let successData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => {
          successData = data;
        });

        await emitUppyEventAsync("upload-success",
          { id: "f1", name: "resumed.jpg" },
          { uploadURL: "http://localhost/tus/existing" },
        );

        expect(successData).toEqual({ name: "resumed.jpg", resumed: true });
        wrapper.unmount();
      });

      it("should skip when file is null", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("upload-success", null, { uploadURL: "http://localhost/tus/abc" });

        wrapper.unmount();
      });

      it("should skip when response has no uploadURL", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("upload-success", { id: "f1", name: "file.jpg" }, {});

        wrapper.unmount();
      });

      it("should handle fetch failure gracefully", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/fetchfail.jpg",
            directoryPath: "/Images",
            name: "fetchfail.jpg",
            isDirectory: false,
          },
        ]);

        global.fetch = vi.fn().mockRejectedValue(new Error("Network error"));

        const consoleSpy = vi.spyOn(console, "debug").mockImplementation(() => {});

        let successData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => {
          successData = data;
        });

        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("upload-success",
          { id: "f2", name: "fetchfail.jpg" },
          { uploadURL: "http://localhost/tus/fail123" },
        );

        expect(consoleSpy).toHaveBeenCalledWith("Failed to fetch TUS file info:", expect.any(Error));
        expect(successData).toEqual({ name: "fetchfail.jpg", resumed: false });

        consoleSpy.mockRestore();
        wrapper.unmount();
      });

      it("should handle non-ok fetch response", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/notfound.jpg",
            directoryPath: "/Images",
            name: "notfound.jpg",
            isDirectory: false,
          },
        ]);

        global.fetch = vi.fn().mockResolvedValue({
          ok: false,
          status: 404,
        });

        let successData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => {
          successData = data;
        });

        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("upload-success",
          { id: "f3", name: "notfound.jpg" },
          { uploadURL: "http://localhost/tus/notfound" },
        );

        expect(successData).toEqual({ name: "notfound.jpg", resumed: false });

        const placeholder = globals.assetsStore.value.find((a) => a.name === "notfound.jpg");
        expect(placeholder?.size).toBeUndefined();

        wrapper.unmount();
      });

      it("should handle case where placeholder is not found", async () => {
        globals.setAssetsStore([]);

        global.fetch = vi.fn().mockResolvedValue({
          ok: true,
          json: () => Promise.resolve({
            name: "orphan.jpg",
            directoryPath: "/Images",
            filePath: "/Images/orphan.jpg",
            size: 999,
          }),
        });

        const wrapper = await mountWithFileUpload(tusModel);

        await emitUppyEventAsync("upload-success",
          { id: "f4", name: "orphan.jpg" },
          { uploadURL: "http://localhost/tus/orphan" },
        );

        wrapper.unmount();
      });
    });

    describe("TUS upload event - resume detection", () => {
      it("should not mark as resumed when tus.uploadUrl is not set", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        mockGetFile.mockReturnValue({ id: "f1", name: "fresh.jpg" });
        emitUppyEvent("upload", { fileIDs: ["f1"] });

        global.fetch = vi.fn().mockResolvedValue({
          ok: true,
          json: () => Promise.resolve({
            name: "fresh.jpg",
            directoryPath: "/Images",
          }),
        });

        globals.setAssetsStore([{
          filePath: "/Images/fresh.jpg",
          directoryPath: "/Images",
          name: "fresh.jpg",
          isDirectory: false,
        }]);

        let successData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => {
          successData = data;
        });

        await emitUppyEventAsync("upload-success",
          { id: "f1", name: "fresh.jpg" },
          { uploadURL: "http://localhost/tus/fresh" },
        );

        expect(successData?.resumed).toBe(false);
        wrapper.unmount();
      });

      it("should handle upload event with empty fileIDs", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        emitUppyEvent("upload", { fileIDs: [] });
        emitUppyEvent("upload", {});

        wrapper.unmount();
      });
    });

    describe("TUS pause/resume handler", () => {
      it("should pause a file on first toggle", async () => {
        let pauseData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadPaused", (data) => {
          pauseData = data;
        });

        mockGetFiles.mockReturnValue([
          { id: "f1", name: "tusfile.jpg" },
        ]);

        const wrapper = await mountWithFileUpload(tusModel);

        eventBus.emit("UploadPauseToggle", { name: "tusfile.jpg" });

        expect(pauseData).toEqual({ name: "tusfile.jpg", paused: true });
        expect(mockPauseResume).toHaveBeenCalledWith("f1");
        wrapper.unmount();
      });

      it("should resume a file on second toggle", async () => {
        const pauseEvents: any[] = []; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadPaused", (data) => {
          pauseEvents.push(data);
        });

        mockGetFiles.mockReturnValue([
          { id: "f1", name: "tusfile.jpg" },
        ]);

        const wrapper = await mountWithFileUpload(tusModel);

        eventBus.emit("UploadPauseToggle", { name: "tusfile.jpg" });
        expect(pauseEvents[0]).toEqual({ name: "tusfile.jpg", paused: true });

        eventBus.emit("UploadPauseToggle", { name: "tusfile.jpg" });
        expect(pauseEvents[1]).toEqual({ name: "tusfile.jpg", paused: false });

        expect(mockPauseResume).toHaveBeenCalledTimes(2);
        wrapper.unmount();
      });

      it("should not pause when file is not found in Uppy", async () => {
        let pauseData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadPaused", (data) => {
          pauseData = data;
        });

        mockGetFiles.mockReturnValue([]);

        const wrapper = await mountWithFileUpload(tusModel);

        eventBus.emit("UploadPauseToggle", { name: "nonexistent.jpg" });

        expect(pauseData).toBeNull();
        expect(mockPauseResume).not.toHaveBeenCalled();
        wrapper.unmount();
      });
    });

    describe("XHR complete handler", () => {
      it("should update placeholders from server response on successful upload", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/uploaded.jpg",
            directoryPath: "/Images",
            name: "uploaded.jpg",
            isDirectory: false,
          },
        ]);

        let successData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => {
          successData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [
            {
              name: "uploaded.jpg",
              response: {
                body: {
                  files: [
                    {
                      name: "uploaded.jpg",
                      directoryPath: "/Images",
                      filePath: "/Images/uploaded.jpg",
                      size: 54321,
                      lastModifiedUtc: "2024-11-01T00:00:00Z",
                      url: "/media/Images/uploaded.jpg",
                      mime: "image/jpeg",
                    },
                  ],
                },
              },
            },
          ],
          failed: [],
        });

        const placeholder = globals.assetsStore.value.find((a) => a.name === "uploaded.jpg");
        expect(placeholder?.size).toBe(54321);
        expect(placeholder?.url).toBe("/media/Images/uploaded.jpg");
        expect(placeholder?.mime).toBe("image/jpeg");
        expect(successData).toEqual({ name: "uploaded.jpg" });
        expect(fileInput.value).toBe("");

        wrapper.unmount();
      });

      it("should handle failed uploads", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [],
          failed: [
            {
              name: "badfile.jpg",
              error: "Server rejected the file",
            },
          ],
        });

        expect(errorData).toEqual({
          name: "badfile.jpg",
          errorMessage: "Server rejected the file",
        });

        wrapper.unmount();
      });

      it("should use translation Error when file.error is undefined", async () => {
        let errorData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadError", (data) => {
          errorData = data;
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [],
          failed: [
            { name: "badfile.jpg" },
          ],
        });

        expect(errorData).toEqual({
          name: "badfile.jpg",
          errorMessage: "Error",
        });

        wrapper.unmount();
      });

      it("should handle successful upload with no server files array", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [
            {
              name: "noresponse.jpg",
              response: { body: {} },
            },
          ],
          failed: [],
        });

        wrapper.unmount();
      });

      it("should handle successful upload with no response body", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [
            {
              name: "nobody.jpg",
              response: {},
            },
          ],
          failed: [],
        });

        wrapper.unmount();
      });

      it("should handle successful upload with no response", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [
            { name: "noresp.jpg" },
          ],
          failed: [],
        });

        wrapper.unmount();
      });

      it("should handle case where placeholder is not found in assetsStore", async () => {
        globals.setAssetsStore([]);

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [
            {
              name: "orphan.jpg",
              response: {
                body: {
                  files: [
                    {
                      name: "orphan.jpg",
                      directoryPath: "/Images",
                      filePath: "/Images/orphan.jpg",
                      size: 100,
                    },
                  ],
                },
              },
            },
          ],
          failed: [],
        });

        wrapper.unmount();
      });

      it("should handle both successful and failed in same result", async () => {
        globals.setAssetsStore([
          {
            filePath: "/Images/good.jpg",
            directoryPath: "/Images",
            name: "good.jpg",
            isDirectory: false,
          },
        ]);

        const successEvents: any[] = []; // eslint-disable-line @typescript-eslint/no-explicit-any
        const errorEvents: any[] = []; // eslint-disable-line @typescript-eslint/no-explicit-any
        eventBus.on("UploadSuccess", (data) => successEvents.push(data));
        eventBus.on("UploadError", (data) => errorEvents.push(data));

        const wrapper = await mountWithFileUpload(xhrModel);

        emitUppyEvent("complete", {
          successful: [
            {
              name: "good.jpg",
              response: {
                body: {
                  files: [
                    {
                      name: "good.jpg",
                      directoryPath: "/Images",
                      filePath: "/Images/good.jpg",
                      size: 200,
                    },
                  ],
                },
              },
            },
          ],
          failed: [
            {
              name: "bad.jpg",
              error: "Rejected",
            },
          ],
        });

        expect(successEvents).toEqual([{ name: "good.jpg" }]);
        expect(errorEvents).toEqual([{ name: "bad.jpg", errorMessage: "Rejected" }]);

        wrapper.unmount();
      });
    });

    describe("setUppyUrl", () => {
      it("should update XHRUpload plugin options when plugin is installed", async () => {
        const mockXhrPlugin = { setOptions: vi.fn() };
        mockGetPlugin.mockImplementation((name: string) => {
          if (name === "XHRUpload") return mockXhrPlugin;
          return null;
        });

        globals.setUploadFilesUrl("/api/upload");
        globals.setSelectedDirectory({
          name: "Test",
          directoryPath: "/Test",
          filePath: "/Test",
          isDirectory: true,
        });

        const wrapper = await mountWithFileUpload(xhrModel);

        // Trigger AfterDirSelected which calls setUppyUrl
        eventBus.emit("AfterDirSelected", {
          name: "NewDir",
          directoryPath: "/NewDir",
          filePath: "/NewDir",
          isDirectory: true,
        });

        expect(mockXhrPlugin.setOptions).toHaveBeenCalledWith(
          expect.objectContaining({
            endpoint: "/api/upload?path=%2FTest",
          }),
        );

        wrapper.unmount();
      });

      it("should not throw when XHRUpload plugin is not installed", async () => {
        mockGetPlugin.mockReturnValue(null);

        const wrapper = await mountWithFileUpload(xhrModel);

        expect(() => {
          eventBus.emit("AfterDirSelected", {
            name: "Dir",
            directoryPath: "/Dir",
            filePath: "/Dir",
            isDirectory: true,
          });
        }).not.toThrow();

        wrapper.unmount();
      });
    });

    describe("DropTarget onDragLeave", () => {
      it("should clear uppy and stop propagation", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        expect(mockUse).toHaveBeenCalledWith(
          DropTarget,
          expect.objectContaining({
            target: document.body,
            onDragLeave: expect.any(Function),
          }),
        );

        const dropTargetCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === DropTarget,
        );
        const onDragLeave = dropTargetCall?.[1]?.onDragLeave;
        expect(onDragLeave).toBeDefined();

        const mockEvent = { stopPropagation: vi.fn() };
        onDragLeave(mockEvent);

        expect(mockClear).toHaveBeenCalled();
        expect(mockEvent.stopPropagation).toHaveBeenCalled();

        wrapper.unmount();
      });
    });

    describe("XHR onAfterResponse callback", () => {
      it("should notify on 400 response", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        const onAfterResponse = xhrCall?.[1]?.onAfterResponse;

        onAfterResponse({
          status: 400,
          response: JSON.stringify({ title: "Bad Request", detail: "Invalid file" }),
        });

        expect(mockNotify).toHaveBeenCalledWith(
          expect.objectContaining({
            summary: "Bad Request",
            detail: "Invalid file",
          }),
        );
        expect(fileInput.value).toBe("");

        wrapper.unmount();
      });

      it("should notify on 401 response", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        const onAfterResponse = xhrCall?.[1]?.onAfterResponse;

        onAfterResponse({
          status: 401,
          response: JSON.stringify({ title: "Unauthorized" }),
        });

        expect(mockNotify).toHaveBeenCalled();
        wrapper.unmount();
      });

      it("should notify on 403 response", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        const onAfterResponse = xhrCall?.[1]?.onAfterResponse;

        onAfterResponse({
          status: 403,
          response: JSON.stringify({ title: "Forbidden", detail: "Access denied" }),
        });

        expect(mockNotify).toHaveBeenCalled();
        wrapper.unmount();
      });

      it("should notify on 500 response", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        const onAfterResponse = xhrCall?.[1]?.onAfterResponse;

        onAfterResponse({
          status: 500,
          response: JSON.stringify({ detail: "Internal error" }),
        });

        expect(mockNotify).toHaveBeenCalledWith(
          expect.objectContaining({
            summary: "Error",
            detail: "Internal error",
          }),
        );
        wrapper.unmount();
      });

      it("should use raw response when JSON has no title or detail", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        const onAfterResponse = xhrCall?.[1]?.onAfterResponse;

        const rawResponse = JSON.stringify({ something: "else" });
        onAfterResponse({
          status: 400,
          response: rawResponse,
        });

        expect(mockNotify).toHaveBeenCalledWith(
          expect.objectContaining({
            detail: rawResponse,
          }),
        );
        wrapper.unmount();
      });

      it("should not notify on 200 response", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        const onAfterResponse = xhrCall?.[1]?.onAfterResponse;

        onAfterResponse({
          status: 200,
          response: JSON.stringify({ ok: true }),
        });

        expect(mockNotify).not.toHaveBeenCalled();
        wrapper.unmount();
      });
    });

    describe("TUS configuration", () => {
      it("should configure TUS with onShouldRetry that always returns true", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        const tusCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === Tus,
        );
        expect(tusCall?.[1]?.onShouldRetry()).toBe(true);

        wrapper.unmount();
      });

      it("should configure TUS with removeFingerprintOnSuccess", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        const tusCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === Tus,
        );
        expect(tusCall?.[1]?.removeFingerprintOnSuccess).toBe(true);

        wrapper.unmount();
      });

      it("should configure TUS with retryDelays", async () => {
        const wrapper = await mountWithFileUpload(tusModel);

        const tusCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === Tus,
        );
        expect(tusCall?.[1]?.retryDelays).toEqual([0, 1000, 3000, 5000]);

        wrapper.unmount();
      });
    });

    describe("XHR shouldRetry", () => {
      it("should configure XHR with shouldRetry that always returns false", async () => {
        const wrapper = await mountWithFileUpload(xhrModel);

        const xhrCall = mockUse.mock.calls.find(
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          (call: any[]) => call[0] === XHRUpload,
        );
        expect(xhrCall?.[1]?.shouldRetry()).toBe(false);

        wrapper.unmount();
      });
    });
  });
});
