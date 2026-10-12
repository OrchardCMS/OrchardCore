import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount } from "@vue/test-utils";
import { defineComponent, h } from "vue";
import type UppyType from "@uppy/core";

const { getUploadOptions, notify } = vi.hoisted(() => ({
  getUploadOptions: vi.fn(),
  notify: vi.fn(),
}));

vi.mock("../apiClient", () => ({
  getSharedAxios: () => ({ get: getUploadOptions }),
}));
vi.mock("../FileLibraryManager", () => ({ invalidateFileCache: vi.fn() }));
vi.mock("@bloom/services/notifications/notifier", () => ({
  notify,
  NotificationMessage: class {
    constructor(public data: unknown) {}
  },
}));

describe("Gallery upload size validation", () => {
  let Uppy: typeof UppyType;
  beforeEach(async () => {
    vi.resetModules();
    Uppy = (await import("@uppy/core")).default;
    getUploadOptions.mockReset();
    getUploadOptions.mockResolvedValue({ data: { maxFileSize: 10 } });
    notify.mockClear();
  });

  afterEach(() => vi.restoreAllMocks());

  async function mountUploader(tusEnabled = false, allowedExtensions = ".txt") {
    const { useGlobals } = await import("../Globals");
    const globals = useGlobals();
    globals.setBasePath("https://orchard.example/tenant/");
    globals.setSelectedDirectory({ name: "root", directoryPath: "", isDirectory: true });
    globals.setAssetsStore([]);
    globals.setFileItems([]);
    globals.setUploadFilesUrl("https://orchard.example/tenant/api/media/Upload");
    const { useFileUpload } = await import("../UppyFileUpload");
    const wrapper = mount(defineComponent({
      setup() {
        const { uploadReady } = useFileUpload({
          maxFileSize: 0, // Picker and standalone mounts do not supply the tenant's size.
          allowedExtensions,
          maxUploadChunkSize: 0,
          debugEnabled: false,
          tusEnabled,
          tusEndpointUrl: tusEnabled ? "https://orchard.example/tenant/tus" : "",
          tusFileInfoUrl: "",
        });
        return () => h("input", { id: "fileupload", type: "file", disabled: !uploadReady.value });
      },
    }), { attachTo: document.body });
    return wrapper;
  }

  function selectFile(size: number, name = "file.txt") {
    const input = document.querySelector<HTMLInputElement>("#fileupload")!;
    Object.defineProperty(input, "files", { configurable: true, value: [new File(["x".repeat(size)], name)] });
    input.dispatchEvent(new Event("change"));
  }

  it.each([false, true])("rejects over-limit files without starting an upload (TUS: %s)", async (tus) => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const wrapper = await mountUploader(tus);
    await flushPromises();
    expect(getUploadOptions).toHaveBeenCalledWith("https://orchard.example/tenant/api/media/GetUploadOptions");
    expect(wrapper.element.disabled).toBe(false);
    selectFile(11);
    await flushPromises();
    expect(notify).toHaveBeenCalledTimes(1);
    expect(upload).not.toHaveBeenCalled();
    wrapper.unmount();
  });

  it.each([false, true])("accepts exact-limit files (TUS: %s)", async (tus) => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const wrapper = await mountUploader(tus);
    await flushPromises();
    selectFile(10);
    await flushPromises();
    expect(upload).toHaveBeenCalledTimes(1);
    expect(notify).not.toHaveBeenCalled();
    wrapper.unmount();
  });

  it("rejects over-limit files added directly by DropTarget", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const setOptions = vi.spyOn(Uppy.prototype, "setOptions");
    const wrapper = await mountUploader();
    await flushPromises();
    const uppy = setOptions.mock.contexts[0];
    expect(() => uppy.addFile({
      source: "DropTarget",
      name: "large.txt",
      data: new File(["x".repeat(11)], "large.txt"),
    })).toThrow();
    await flushPromises();
    expect(uppy.getFiles()).toHaveLength(0);
    expect(upload).not.toHaveBeenCalled();
    expect(notify).toHaveBeenCalledTimes(1);
    wrapper.unmount();
  });

  it("blocks file input and dropped files while options are pending", async () => {
    getUploadOptions.mockReturnValue(new Promise(() => {}));
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const setOptions = vi.spyOn(Uppy.prototype, "setOptions");
    const wrapper = await mountUploader();
    expect(wrapper.element.disabled).toBe(true);
    selectFile(1);
    await flushPromises();
    const gate = setOptions.mock.calls.find(([options]) => options.onBeforeFileAdded)?.[0].onBeforeFileAdded;
    expect(gate?.({} as never, {})).toBe(false); // DropTarget uses the same Uppy gate.
    expect(upload).not.toHaveBeenCalled();
    wrapper.unmount();
  });

  it.each([null, { maxFileSize: "10" }, { maxFileSize: NaN }])("keeps uploads disabled for invalid options: %s", async (data) => {
    getUploadOptions.mockResolvedValue({ data });
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const wrapper = await mountUploader();
    await flushPromises();
    expect(wrapper.element.disabled).toBe(true);
    expect(notify).toHaveBeenCalledTimes(1);
    selectFile(1);
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    wrapper.unmount();
  });

  it("keeps uploads disabled when the options request fails", async () => {
    getUploadOptions.mockRejectedValue(new Error("Unavailable"));
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const wrapper = await mountUploader();
    await flushPromises();
    expect(notify).toHaveBeenCalledTimes(1);
    selectFile(1);
    await flushPromises();
    expect(wrapper.element.disabled).toBe(true);
    expect(upload).not.toHaveBeenCalled();
    wrapper.unmount();
  });

  it("refreshes the singleton restrictions on repeated mounts, including an unlimited size", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const first = await mountUploader();
    await flushPromises();
    selectFile(11);
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    first.unmount();

    getUploadOptions.mockResolvedValue({ data: { maxFileSize: 0 } });
    const second = await mountUploader(false, "");
    await flushPromises();
    selectFile(11, "file.jpg");
    await flushPromises();
    expect(upload).toHaveBeenCalled();
    expect(getUploadOptions).toHaveBeenCalledTimes(2);
    second.unmount();
  });

  it("ignores an old options response after a picker is remounted", async () => {
    let resolveFirst!: (response: { data: { maxFileSize: number } }) => void;
    getUploadOptions.mockReturnValueOnce(new Promise(resolve => { resolveFirst = resolve; }));
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const first = await mountUploader();
    first.unmount();
    const second = await mountUploader();
    await flushPromises();
    resolveFirst({ data: { maxFileSize: 100 } });
    await flushPromises();
    selectFile(11);
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    second.unmount();
  });

  it("applies a stricter tenant limit on the next mount", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    getUploadOptions.mockResolvedValueOnce({ data: { maxFileSize: 100 } });
    const first = await mountUploader();
    await flushPromises();
    selectFile(11);
    await flushPromises();
    expect(upload).toHaveBeenCalled();
    first.unmount();
    upload.mockClear();
    const second = await mountUploader();
    await flushPromises();
    selectFile(11);
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    expect(notify).toHaveBeenCalledTimes(1);
    second.unmount();
  });

  it("preserves the mount's extension restrictions", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const wrapper = await mountUploader();
    await flushPromises();
    selectFile(1, "file.exe");
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    expect(notify).toHaveBeenCalledTimes(1);
    wrapper.unmount();
  });
});
