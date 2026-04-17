import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { useFieldUpload } from "../FieldUploadService";
import type { IUploadFileEntry } from "../FieldUploadService";

// --- MockUppy (for TUS tests) ------------------------------------------

type UppyEventCallback = (...args: any[]) => void;

class MockUppy {
  static instances: MockUppy[] = [];
  options: any;
  files: any[] = [];
  eventHandlers: Record<string, UppyEventCallback[]> = {};
  useFn = vi.fn();
  addFileFn = vi.fn();
  uploadFn = vi.fn();
  destroyFn = vi.fn();
  getFilesFn = vi.fn(() => this.files);

  constructor(options?: any) {
    this.options = options;
    MockUppy.instances.push(this);
  }

  use(plugin: any, opts: any) {
    this.useFn(plugin, opts);
    return this;
  }

  addFile(fileDescriptor: any) {
    this.addFileFn(fileDescriptor);
    this.files.push({
      name: fileDescriptor.name,
      type: fileDescriptor.type,
      data: fileDescriptor.data,
      progress: { percentage: 0 },
    });
  }

  on(event: string, cb: UppyEventCallback) {
    if (!this.eventHandlers[event]) this.eventHandlers[event] = [];
    this.eventHandlers[event].push(cb);
  }

  getFiles() {
    return this.getFilesFn();
  }

  upload() {
    this.uploadFn();
  }

  destroy() {
    this.destroyFn();
  }

  // Test helpers
  triggerEvent(event: string, ...args: any[]) {
    const handlers = this.eventHandlers[event] || [];
    for (const h of handlers) h(...args);
  }
}

vi.mock("@uppy/core", () => ({
  default: vi.fn((...args: any[]) => new MockUppy(...args)),
}));

vi.mock("@uppy/tus", () => ({
  default: { name: "Tus" },
}));

// --- MockXHR -----------------------------------------------------------

type XHREventCallback = (event: Partial<ProgressEvent>) => void;

class MockXHR {
  upload = { addEventListener: vi.fn() };
  addEventListener = vi.fn();
  open = vi.fn();
  send = vi.fn();
  status = 200;
  responseText = "";

  /** Helper: fire the "load" event listener registered via addEventListener */
  triggerLoad() {
    const loadCb = this.addEventListener.mock.calls.find(
      ([type]: [string]) => type === "load"
    );
    if (loadCb) (loadCb[1] as XHREventCallback)({});
  }

  /** Helper: fire the "error" event listener */
  triggerError() {
    const errorCb = this.addEventListener.mock.calls.find(
      ([type]: [string]) => type === "error"
    );
    if (errorCb) (errorCb[1] as XHREventCallback)({});
  }

  /** Helper: fire the "progress" event on upload */
  triggerProgress(loaded: number, total: number) {
    const progressCb = this.upload.addEventListener.mock.calls.find(
      ([type]: [string]) => type === "progress"
    );
    if (progressCb)
      (progressCb[1] as XHREventCallback)({
        lengthComputable: true,
        loaded,
        total,
      });
  }
}

// -----------------------------------------------------------------------

const defaultConfig = {
  uploadAction: "/api/media/upload",
  tempUploadFolder: "/tmp",
};

function makeFile(name: string, content = "data"): File {
  return new File([content], name, { type: "application/octet-stream" });
}

describe("useFieldUpload", () => {
  let xhrInstances: MockXHR[];
  let OriginalXHR: typeof XMLHttpRequest;

  beforeEach(() => {
    xhrInstances = [];
    OriginalXHR = global.XMLHttpRequest;
    global.XMLHttpRequest = vi.fn(() => {
      const instance = new MockXHR();
      xhrInstances.push(instance);
      return instance;
    }) as unknown as typeof XMLHttpRequest;
  });

  afterEach(() => {
    global.XMLHttpRequest = OriginalXHR;
    document.body.innerHTML = "";
    MockUppy.instances = [];
    vi.restoreAllMocks();
  });

  it("returns reactive files ref that is initially empty", () => {
    const { files } = useFieldUpload(defaultConfig);
    expect(files.value).toEqual([]);
  });

  // -- clearErrors / dismiss / dismissAll --------------------------------

  describe("clearErrors", () => {
    it("removes only entries with error messages", () => {
      const { files, clearErrors } = useFieldUpload(defaultConfig);
      const ok: IUploadFileEntry = {
        name: "a.jpg",
        percentage: 100,
        errorMessage: "",
        success: true,
      };
      const err: IUploadFileEntry = {
        name: "b.jpg",
        percentage: 0,
        errorMessage: "fail",
        success: false,
      };
      files.value = [ok, err];

      clearErrors();

      expect(files.value).toEqual([ok]);
    });
  });

  describe("dismiss", () => {
    it("removes the specific entry", () => {
      const { files, dismiss } = useFieldUpload(defaultConfig);
      files.value = [
        { name: "a.jpg", percentage: 0, errorMessage: "", success: false },
        { name: "b.jpg", percentage: 0, errorMessage: "", success: false },
      ];

      // Use the reactive reference from files.value so reference equality works
      const a = files.value[0];
      dismiss(a);

      expect(files.value).toHaveLength(1);
      expect(files.value[0].name).toBe("b.jpg");
    });
  });

  describe("dismissAll", () => {
    it("clears all entries", () => {
      const { files, dismissAll } = useFieldUpload(defaultConfig);
      files.value = [
        { name: "a.jpg", percentage: 0, errorMessage: "", success: false },
        { name: "b.jpg", percentage: 0, errorMessage: "err", success: false },
      ];

      dismissAll();

      expect(files.value).toEqual([]);
    });
  });

  // -- uploadFiles -------------------------------------------------------

  describe("uploadFiles", () => {
    it("returns uploaded result on successful upload", async () => {
      const { uploadFiles, files } = useFieldUpload(defaultConfig);
      const file = makeFile("photo.jpg");

      const promise = uploadFiles([file]);

      // Wait a tick so the XHR is created
      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [
          { name: "uuid-photo.jpg", size: 1024, url: "/media/photo.jpg", mediaPath: "photo.jpg", mime: "image/jpeg" },
        ],
      });
      xhr.triggerLoad();

      const result = await promise;

      expect(result.uploaded).toHaveLength(1);
      expect(result.uploaded[0]).toMatchObject({
        name: "uuid-photo.jpg",
        isNew: true,
        attachedFileName: "photo.jpg",
      });
      expect(result.errors).toHaveLength(0);

      // The entry should be marked as success
      const entry = files.value.find((f) => f.name === "photo.jpg");
      expect(entry?.success).toBe(true);
      expect(entry?.percentage).toBe(100);
    });

    it("uses filePath when XHR response omits mediaPath", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("fallback.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [
          {
            name: "uuid-fallback.jpg",
            size: 1024,
            url: "/media/fallback.jpg",
            filePath: "MediaFields/temp/fallback.jpg",
            mime: "image/jpeg",
          },
        ],
      });
      xhr.triggerLoad();

      const result = await promise;

      expect(result.uploaded).toHaveLength(1);
      expect(result.uploaded[0].mediaPath).toBe("MediaFields/temp/fallback.jpg");
    });

    it("captures server-side file error", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("bad.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [{ error: "File type not allowed" }],
      });
      xhr.triggerLoad();

      const result = await promise;

      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("File type not allowed");
      expect(result.uploaded).toHaveLength(0);
    });

    it("handles HTTP error status", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("fail.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.status = 500;
      xhr.responseText = "Internal Server Error";
      xhr.triggerLoad();

      const result = await promise;

      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("500");
      expect(result.uploaded).toHaveLength(0);
    });

    it("handles network error", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("net.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.triggerError();

      const result = await promise;

      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("Network error");
      expect(result.uploaded).toHaveLength(0);
    });

    it("handles invalid JSON response", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("corrupt.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.status = 200;
      xhr.responseText = "not json at all";
      xhr.triggerLoad();

      const result = await promise;

      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("Invalid response");
      expect(result.uploaded).toHaveLength(0);
    });

    it("handles empty files array in response", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("empty.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      xhr.status = 200;
      xhr.responseText = JSON.stringify({ files: [] });
      xhr.triggerLoad();

      const result = await promise;

      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("No files in response");
      expect(result.uploaded).toHaveLength(0);
    });

    it("tracks upload progress percentage", async () => {
      const { uploadFiles, files } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("big.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      // Simulate progress events
      xhr.triggerProgress(250, 1000);
      expect(files.value[0].percentage).toBe(25);

      xhr.triggerProgress(500, 1000);
      expect(files.value[0].percentage).toBe(50);

      xhr.triggerProgress(1000, 1000);
      expect(files.value[0].percentage).toBe(100);

      // Complete the upload so the promise resolves
      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [{ name: "big.jpg", size: 1000, url: "/media/big.jpg", mediaPath: "big.jpg", mime: "image/jpeg" }],
      });
      xhr.triggerLoad();

      await promise;
    });

    it("opens XHR with correct method and URL", async () => {
      const { uploadFiles } = useFieldUpload({
        uploadAction: "/custom/upload",
        tempUploadFolder: "/my-temp",
      });

      const promise = uploadFiles([makeFile("test.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      expect(xhr.open).toHaveBeenCalledWith("POST", "/custom/upload");

      // Resolve the promise
      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [{ name: "test.jpg", size: 100, url: "/u/test.jpg", mediaPath: "test.jpg", mime: "image/jpeg" }],
      });
      xhr.triggerLoad();
      await promise;
    });
  });

  // -- getAntiForgeryToken (accessed indirectly via uploadFiles) ----------

  describe("anti-forgery token", () => {
    it("reads token from DOM input and includes it in FormData", async () => {
      const input = document.createElement("input");
      input.name = "__RequestVerificationToken";
      input.value = "test-token-123";
      document.body.appendChild(input);

      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("tok.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      // Verify send was called with a FormData that contains the token
      expect(xhr.send).toHaveBeenCalledTimes(1);
      const formData = xhr.send.mock.calls[0][0] as FormData;
      expect(formData.get("__RequestVerificationToken")).toBe("test-token-123");

      // Resolve
      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [{ name: "tok.jpg", size: 10, url: "/u/tok.jpg", mediaPath: "tok.jpg", mime: "image/jpeg" }],
      });
      xhr.triggerLoad();
      await promise;
    });

    it("returns empty string when no token input exists in DOM", async () => {
      const { uploadFiles } = useFieldUpload(defaultConfig);

      const promise = uploadFiles([makeFile("notoken.jpg")]);

      await vi.waitFor(() => expect(xhrInstances).toHaveLength(1));
      const xhr = xhrInstances[0];

      const formData = xhr.send.mock.calls[0][0] as FormData;
      expect(formData.get("__RequestVerificationToken")).toBe("");

      // Resolve
      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [{ name: "notoken.jpg", size: 10, url: "/u/x.jpg", mediaPath: "x.jpg", mime: "image/jpeg" }],
      });
      xhr.triggerLoad();
      await promise;
    });
  });

  // -- TUS upload tests ---------------------------------------------------

  describe("uploadFiles with TUS enabled", () => {
    const tusConfig = {
      uploadAction: "/api/media/upload",
      tempUploadFolder: "/tmp",
      tusEnabled: true,
      tusEndpointUrl: "https://tus.example.com/files/",
      tusFileInfoUrl: "https://tus.example.com/fileinfo",
      maxUploadChunkSize: 1024,
    };

    it("uses TUS when tusEnabled is true", async () => {
      global.fetch = vi.fn(() =>
        Promise.resolve({
          ok: true,
          json: () => Promise.resolve({ name: "tus-photo.jpg", size: 2048, url: "/media/tus-photo.jpg", mediaPath: "tus-photo.jpg", mime: "image/jpeg" }),
        })
      ) as any;

      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("photo.jpg");
      const promise = uploadFiles([file]);

      // Wait for Uppy instance to be created
      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      // Simulate upload-success with uploadURL
      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/abc123" });

      const result = await promise;
      expect(result.uploaded).toHaveLength(1);
      expect(result.uploaded[0]).toMatchObject({
        name: "tus-photo.jpg",
        isNew: true,
        attachedFileName: "photo.jpg",
      });
      expect(result.errors).toHaveLength(0);
      expect(uppy.destroyFn).toHaveBeenCalled();
    });

    it("uses filePath when TusFileInfo omits mediaPath", async () => {
      global.fetch = vi.fn(() =>
        Promise.resolve({
          ok: true,
          json: () => Promise.resolve({
            name: "tus-fallback.jpg",
            size: 2048,
            url: "/media/tus-fallback.jpg",
            filePath: "MediaFields/temp/tus-fallback.jpg",
            mime: "image/jpeg",
          }),
        })
      ) as any;

      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("fallback.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/fallback123" });

      const result = await promise;

      expect(result.uploaded).toHaveLength(1);
      expect(result.uploaded[0].mediaPath).toBe("MediaFields/temp/tus-fallback.jpg");
    });

    it("handles TUS progress event", async () => {
      global.fetch = vi.fn(() =>
        Promise.resolve({
          ok: true,
          json: () => Promise.resolve({ name: "tus.jpg", size: 100, url: "/u", mediaPath: "x", mime: "image/jpeg" }),
        })
      ) as any;

      const { uploadFiles, files } = useFieldUpload(tusConfig);
      const file = makeFile("progress.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      // Set percentage on the file
      uppy.files[0].progress = { percentage: 50 };
      uppy.triggerEvent("progress", 50);
      expect(files.value[0].percentage).toBe(50);

      // Finish upload
      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/xyz" });
      await promise;
    });

    it("rejects when TUS response has no uploadURL", async () => {
      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("nouploadurl.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      // upload-success with no uploadURL
      uppy.triggerEvent("upload-success", uppy.files[0], {});

      const result = await promise;
      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("No upload URL in TUS response");
    });

    it("rejects when TUS file info request fails", async () => {
      global.fetch = vi.fn(() =>
        Promise.resolve({
          ok: false,
          status: 500,
        })
      ) as any;

      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("failinfo.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/def456" });

      const result = await promise;
      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("TUS file info request failed: 500");
      expect(uppy.destroyFn).toHaveBeenCalled();
    });

    it("rejects when TUS file info fetch throws a network error", async () => {
      global.fetch = vi.fn(() => Promise.reject(new Error("Network down"))) as any;

      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("netfail.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/ghi789" });

      const result = await promise;
      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("Network down");
      expect(uppy.destroyFn).toHaveBeenCalled();
    });

    it("handles TUS upload-error event", async () => {
      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("tus-error.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      uppy.triggerEvent("upload-error", uppy.files[0], new Error("TUS connection lost"));

      const result = await promise;
      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("TUS connection lost");
      expect(uppy.destroyFn).toHaveBeenCalled();
    });

    it("handles TUS upload-error with null error", async () => {
      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("tus-null-err.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      uppy.triggerEvent("upload-error", uppy.files[0], null);

      const result = await promise;
      expect(result.errors).toHaveLength(1);
      expect(result.errors[0]).toContain("TUS upload failed");
    });

    it("uses default chunk size when maxUploadChunkSize is 0", async () => {
      global.fetch = vi.fn(() =>
        Promise.resolve({
          ok: true,
          json: () => Promise.resolve({ name: "chunk.jpg", size: 100, url: "/u", mediaPath: "x", mime: "image/jpeg" }),
        })
      ) as any;

      const configNoChunk = { ...tusConfig, maxUploadChunkSize: 0 };
      const { uploadFiles } = useFieldUpload(configNoChunk);
      const file = makeFile("chunk.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      // Verify TUS was configured with default chunk size (5 MB)
      const tusOpts = uppy.useFn.mock.calls[0][1];
      expect(tusOpts.chunkSize).toBe(5 * 1024 * 1024);

      // Finish
      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/chk" });
      await promise;
    });

    it("uses configured chunk size when maxUploadChunkSize > 0", async () => {
      global.fetch = vi.fn(() =>
        Promise.resolve({
          ok: true,
          json: () => Promise.resolve({ name: "chunk2.jpg", size: 100, url: "/u", mediaPath: "x", mime: "image/jpeg" }),
        })
      ) as any;

      const { uploadFiles } = useFieldUpload(tusConfig);
      const file = makeFile("chunk2.jpg");
      const promise = uploadFiles([file]);

      await vi.waitFor(() => expect(MockUppy.instances).toHaveLength(1));
      const uppy = MockUppy.instances[0];

      const tusOpts = uppy.useFn.mock.calls[0][1];
      expect(tusOpts.chunkSize).toBe(1024);

      uppy.triggerEvent("upload-success", uppy.files[0], { uploadURL: "https://tus.example.com/files/chk2" });
      await promise;
    });
  });

  // -- successful upload auto-removes entry after timeout -----------------

  describe("auto-remove on success", () => {
    it("schedules entry removal via setTimeout and callback removes entry", async () => {
      // Use fake timers from the start
      vi.useFakeTimers();

      const { uploadFiles, files } = useFieldUpload(defaultConfig);
      const file = makeFile("auto.jpg");

      // Start upload - don't await yet
      let resolved = false;
      const promise = uploadFiles([file]).then(() => { resolved = true; });

      // Flush microtasks so the XHR setup runs
      await vi.advanceTimersByTimeAsync(0);

      expect(xhrInstances).toHaveLength(1);
      const xhr = xhrInstances[0];

      xhr.status = 200;
      xhr.responseText = JSON.stringify({
        files: [{ name: "auto.jpg", size: 100, url: "/u", mediaPath: "auto.jpg", mime: "image/jpeg" }],
      });
      xhr.triggerLoad();

      // Flush microtasks for promise resolution
      await vi.advanceTimersByTimeAsync(0);
      expect(resolved).toBe(true);

      // Entry exists immediately after upload
      expect(files.value).toHaveLength(1);
      expect(files.value[0].success).toBe(true);

      // Advance past the 3000ms auto-remove delay
      await vi.advanceTimersByTimeAsync(3100);

      // The setTimeout callback (line 203) should have removed the entry
      expect(files.value).toHaveLength(0);

      vi.useRealTimers();
    });
  });
});
