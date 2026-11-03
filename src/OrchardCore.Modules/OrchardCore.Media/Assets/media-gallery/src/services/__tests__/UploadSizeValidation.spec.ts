import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { flushPromises, mount, type VueWrapper } from "@vue/test-utils";
import { defineComponent, h } from "vue";
import type UppyType from "@uppy/core";

const { notify } = vi.hoisted(() => ({ notify: vi.fn() }));

vi.mock("../FileLibraryManager", () => ({ invalidateFileCache: vi.fn() }));
vi.mock("@bloom/services/notifications/notifier", () => ({
  notify,
  NotificationMessage: class {
    constructor(public data: unknown) {}
  },
}));

describe("Gallery upload size validation", () => {
  let Uppy: typeof UppyType;
  const wrappers: VueWrapper[] = [];

  beforeEach(async () => {
    vi.resetModules();
    Uppy = (await import("@uppy/core")).default;
    notify.mockClear();
  });

  afterEach(() => {
    wrappers.splice(0).forEach(wrapper => wrapper.unmount());
    document.body.innerHTML = "";
    vi.restoreAllMocks();
  });

  async function mountUploader(maxFileSize = 10, tusEnabled = false, allowedExtensions = ".txt") {
    const { useGlobals } = await import("../Globals");
    const globals = useGlobals();
    globals.setSelectedDirectory({ name: "root", directoryPath: "", isDirectory: true });
    globals.setAssetsStore([]);
    globals.setFileItems([]);
    globals.setUploadFilesUrl("https://orchard.example/tenant/api/media/Upload");
    const { useFileUpload } = await import("../UppyFileUpload");
    const wrapper = mount(defineComponent({
      setup() {
        useFileUpload({
          maxFileSize,
          allowedExtensions,
          maxUploadChunkSize: 0,
          debugEnabled: false,
          tusEnabled,
          tusEndpointUrl: tusEnabled ? "https://orchard.example/tenant/tus" : "",
          tusFileInfoUrl: "",
        });
        return () => h("input", { id: "fileupload", type: "file" });
      },
    }), { attachTo: document.body });
    wrappers.push(wrapper);
    return wrapper;
  }

  function selectFile(size: number, name = "file.txt") {
    const input = document.querySelector<HTMLInputElement>("#fileupload")!;
    Object.defineProperty(input, "files", { configurable: true, value: [new File(["x".repeat(size)], name)] });
    input.dispatchEvent(new Event("change"));
  }

  it.each([false, true])("rejects over-limit selections without uploading (TUS: %s)", async (tus) => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    await mountUploader(10, tus);
    selectFile(11);
    await flushPromises();
    expect(notify).toHaveBeenCalledTimes(1);
    expect(upload).not.toHaveBeenCalled();
  });

  it.each([false, true])("accepts exact-limit selections (TUS: %s)", async (tus) => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    await mountUploader(10, tus);
    selectFile(10);
    await flushPromises();
    expect(upload).toHaveBeenCalledTimes(1);
    expect(notify).not.toHaveBeenCalled();
  });

  it("rejects over-limit files added by DropTarget", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const setOptions = vi.spyOn(Uppy.prototype, "setOptions");
    await mountUploader();
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
  });

  it.each([0, -1, undefined])("resets size and extension restrictions on remount with limit %s", async (limit) => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const first = await mountUploader();
    selectFile(11);
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    first.unmount();
    first.element.remove();

    await mountUploader(limit ?? 0, false, "");
    selectFile(11, "file.jpg");
    await flushPromises();
    expect(upload).toHaveBeenCalled();
  });

  it.each([false, true])("keeps default duplicate checking for pending drops (TUS: %s)", async (tus) => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockReturnValue(new Promise(() => {}));
    const setOptions = vi.spyOn(Uppy.prototype, "setOptions");
    await mountUploader(10, tus);
    const uppy = setOptions.mock.contexts[0];
    const file = {
      source: "DropTarget",
      name: "file.txt",
      data: new File(["x"], "file.txt", { lastModified: 1 }),
    };
    uppy.addFiles([file]);
    await flushPromises();
    const pending = uppy.getFiles()[0];
    uppy.addFiles([file]);
    await flushPromises();
    expect(uppy.getFiles()).toEqual([pending]);
    expect(upload).toHaveBeenCalledTimes(1);
  });

  it("applies a stricter limit on the next mount", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    const first = await mountUploader(100);
    selectFile(11);
    await flushPromises();
    expect(upload).toHaveBeenCalledTimes(1);
    first.unmount();
    first.element.remove();
    upload.mockClear();

    await mountUploader(10);
    selectFile(11);
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    expect(notify).toHaveBeenCalledTimes(1);
  });

  it("preserves extension restrictions", async () => {
    const upload = vi.spyOn(Uppy.prototype, "upload").mockResolvedValue(undefined);
    await mountUploader();
    selectFile(1, "file.exe");
    await flushPromises();
    expect(upload).not.toHaveBeenCalled();
    expect(notify).toHaveBeenCalledTimes(1);
  });
});
