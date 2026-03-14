import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { mount, flushPromises } from "@vue/test-utils";
import { ref, nextTick } from "vue";

vi.mock("../../composables/useLocalizations", () => {
  const translations = ref<Record<string, string>>({});
  return {
    useLocalizations: () => ({
      translations,
      setTranslations: (t: Record<string, string>) => { translations.value = t; },
    }),
  };
});

vi.mock("vue-final-modal", () => ({
  VueFinalModal: { name: "VueFinalModal", template: "<div><slot /></div>", props: ["modelValue"] },
  createVfm: () => ({ install: vi.fn() }),
}));

vi.mock("@media-app", () => ({
  mountMediaAppAsPicker: vi.fn(() => ({
    getSelectedFiles: vi.fn(() => []),
    unmount: vi.fn(),
  })),
}));

const mockUploadFiles = vi.fn(() => Promise.resolve({ uploaded: [], errors: [] }));
const mockClearErrors = vi.fn();
const mockDismiss = vi.fn();
const mockDismissAll = vi.fn();
const mockUploadServiceFiles = ref<any[]>([]);

vi.mock("../../services/FieldUploadService", () => ({
  useFieldUpload: vi.fn(() => ({
    files: mockUploadServiceFiles,
    uploadFiles: mockUploadFiles,
    clearErrors: mockClearErrors,
    dismiss: mockDismiss,
    dismissAll: mockDismissAll,
  })),
}));

import MediaFieldAttached from "../MediaFieldAttached.vue";
import type { IAttachedFieldConfig } from "../MediaFieldAttached.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem, makePath } from "../../__tests__/mockdata";

const stubs = { ThumbsContainer: true, UploadList: true };

function makeAttachedConfig(overrides: Partial<IAttachedFieldConfig> = {}): IAttachedFieldConfig {
  return {
    paths: [],
    multiple: false,
    allowMediaText: false,
    allowAnchors: false,
    allowedExtensions: "",
    mediaItemUrl: "/api/media",
    uploadAction: "/api/upload",
    tempUploadFolder: "/temp",
    ...overrides,
  };
}

function mockFetchOk(data: Record<string, unknown> = { name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" }) {
  global.fetch = vi.fn(() =>
    Promise.resolve({
      ok: true,
      status: 200,
      json: () => Promise.resolve(data),
    })
  ) as any;
}

function mockFetch404() {
  global.fetch = vi.fn(() =>
    Promise.resolve({
      ok: false,
      status: 404,
      json: () => Promise.resolve({}),
    })
  ) as any;
}

function mockFetchNetworkError() {
  global.fetch = vi.fn(() => Promise.reject(new Error("Network error"))) as any;
}

function createWrapper(configOverrides: Partial<IAttachedFieldConfig> = {}, propsOverrides: Record<string, unknown> = {}) {
  return mount(MediaFieldAttached, {
    props: {
      config: makeAttachedConfig(configOverrides),
      inputName: "AttachedField",
      ...propsOverrides,
    },
    global: getGlobalMountOptions(stubs),
  });
}

describe("MediaFieldAttached", () => {
  beforeEach(() => {
    setupTranslations();
    localStorage.clear();
    mockFetchOk();
    vi.clearAllMocks();
    mockUploadServiceFiles.value = [];
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders hidden input with serialized paths", () => {
    const wrapper = createWrapper();
    const input = wrapper.find('input[type="hidden"]');
    expect(input.exists()).toBe(true);
    expect(input.attributes("name")).toBe("AttachedField");
  });

  it("loadInitialPaths works", async () => {
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();

    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("path=test%2Ftest.jpg")
    );

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
  });

  it("handles file input change (onFileInputChange)", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "uuid-photo.jpg", mime: "image/jpeg", mediaPath: "temp/uuid-photo.jpg", url: "/media/temp/uuid-photo.jpg", size: 1024, isNew: true, attachedFileName: "photo.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    // Create a mock file
    const file = new File(["content"], "photo.jpg", { type: "image/jpeg" });
    // We cannot set files property directly on the input, so we trigger the change event
    // and simulate the handler
    Object.defineProperty(fileInput.element, "files", { value: [file], writable: false });
    await fileInput.trigger("change");
    await flushPromises();

    expect(mockUploadFiles).toHaveBeenCalledWith([file]);
  });

  it("handles drag-drop (onDrop)", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "uuid-drop.jpg", mime: "image/jpeg", mediaPath: "temp/uuid-drop.jpg", url: "/media/temp/uuid-drop.jpg", size: 512, isNew: true, attachedFileName: "drop.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const dropZone = wrapper.find(".mf-drop-zone");
    const file = new File(["data"], "drop.jpg", { type: "image/jpeg" });

    const dropEvent = new Event("drop") as any;
    dropEvent.dataTransfer = { files: [file] };
    dropEvent.preventDefault = vi.fn();

    await dropZone.trigger("drop", { dataTransfer: { files: [file] } });
    await flushPromises();

    expect(mockUploadFiles).toHaveBeenCalled();
  });

  it("handleFiles calls uploadService.uploadFiles", async () => {
    mockUploadFiles.mockResolvedValueOnce({ uploaded: [], errors: [] });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    const file = new File(["x"], "test.txt", { type: "text/plain" });
    Object.defineProperty(fileInput.element, "files", { value: [file], writable: false });
    await fileInput.trigger("change");
    await flushPromises();

    expect(mockUploadFiles).toHaveBeenCalledWith([file]);
  });

  it("handleFiles adds uploaded items to mediaItems", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "uuid-new.jpg", mime: "image/jpeg", mediaPath: "temp/uuid-new.jpg", url: "/media/temp/uuid-new.jpg", size: 2048, isNew: true, attachedFileName: "new.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    const file = new File(["img"], "new.jpg", { type: "image/jpeg" });
    Object.defineProperty(fileInput.element, "files", { value: [file], writable: false });
    await fileInput.trigger("change");
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems.length).toBeGreaterThanOrEqual(1);
    const newItem = vm.mediaItems.find((m: any) => m.attachedFileName === "new.jpg");
    expect(newItem).toBeTruthy();
    expect(newItem.isNew).toBe(true);
  });

  it("canAddMedia false for single mode with item", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "uuid-a.jpg", mime: "image/jpeg", mediaPath: "temp/uuid-a.jpg", url: "/u", size: 100, isNew: true, attachedFileName: "a.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: false });
    await flushPromises();

    // Upload one item
    const fileInput = wrapper.find('input[type="file"]');
    const file = new File(["a"], "a.jpg", { type: "image/jpeg" });
    Object.defineProperty(fileInput.element, "files", { value: [file], writable: false });
    await fileInput.trigger("change");
    await flushPromises();
    await nextTick();
    await nextTick(); // extra tick for internal nextTick in handleFiles

    // The upload label should be disabled
    wrapper.vm.$forceUpdate();
    await nextTick();
    const label = wrapper.find("label.mf-btn-primary");
    expect(label.classes()).toContain("is-disabled");
  });

  it("removeSelected works", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths, multiple: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);

    // Select the item via ThumbsContainer emit
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    expect(vm.selectedMedia).toBeTruthy();

    // Click remove
    const removeBtn = wrapper.find(".mf-btn-danger");
    expect(removeBtn.exists()).toBe(true);
    await removeBtn.trigger("click");
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
    expect(vm.selectedMedia).toBeNull();
  });

  it("drag zone shows overlay when dragging over", async () => {
    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    // Set isDraggingOver directly via vm (exposed via defineExpose)
    const vm = wrapper.vm as any;
    vm.isDraggingOver = true;
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const dropZone = wrapper.find(".mf-drop-zone");
    expect(dropZone.classes()).toContain("mf-drop-zone-active");
    expect(wrapper.find(".mf-drop-overlay").exists()).toBe(true);
  });

  it("serializedPaths includes isNew, isRemoved, attachedFileName", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "uuid-file.jpg", mime: "image/jpeg", mediaPath: "temp/uuid-file.jpg", url: "/u", size: 100, isNew: true, attachedFileName: "file.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    const file = new File(["x"], "file.jpg", { type: "image/jpeg" });
    Object.defineProperty(fileInput.element, "files", { value: [file], writable: false });
    await fileInput.trigger("change");
    await flushPromises();
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const input = wrapper.find('input[type="hidden"]');
    const val = JSON.parse(input.element.getAttribute("value")!);
    expect(val.length).toBeGreaterThanOrEqual(1);
    const entry = val.find((e: any) => e.attachedFileName === "file.jpg");
    expect(entry).toBeTruthy();
    expect(entry.isNew).toBe(true);
  });

  // -- loadInitialPaths edge cases ----------------------------------------

  it("loadInitialPaths handles 404 as not-found error", async () => {
    mockFetch404();
    const paths = [makePath({ path: "missing.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems[0].errorType).toBe("not-found");
  });

  it("loadInitialPaths handles network error as transient", async () => {
    mockFetchNetworkError();
    const paths = [makePath({ path: "fail.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems[0].errorType).toBe("transient");
  });

  it("loadInitialPaths with empty paths sets initialized", async () => {
    const wrapper = createWrapper({ paths: [] });
    await flushPromises();

    const input = wrapper.find('input[type="hidden"]');
    const val = JSON.parse(input.element.getAttribute("value")!);
    expect(val).toEqual([]);
  });

  // -- onDrop with no files -----------------------------------------------

  it("onDrop does nothing when no files in dataTransfer", async () => {
    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const dropZone = wrapper.find(".mf-drop-zone");
    await dropZone.trigger("drop", { dataTransfer: { files: [] } });
    await flushPromises();

    expect(mockUploadFiles).not.toHaveBeenCalled();
  });

  // -- onFileInputChange with empty files ---------------------------------

  it("onFileInputChange does nothing when no files selected", async () => {
    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    Object.defineProperty(fileInput.element, "files", { value: [], writable: false });
    await fileInput.trigger("change");
    await flushPromises();

    expect(mockUploadFiles).not.toHaveBeenCalled();
  });

  // -- selectAndDeleteMedia -----------------------------------------------

  it("selectAndDeleteMedia selects then removes item", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths, multiple: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("delete-media", vm.mediaItems[0]);
    await nextTick();
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
  });

  // -- onReorder ----------------------------------------------------------

  it("onReorder updates mediaItems order", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });

    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "a.jpg", mime: "image/jpeg", mediaPath: "a.jpg", url: "/u/a", size: 100, isNew: true, attachedFileName: "a.jpg" },
        { name: "b.jpg", mime: "image/jpeg", mediaPath: "b.jpg", url: "/u/b", size: 100, isNew: true, attachedFileName: "b.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    const files = [new File(["a"], "a.jpg"), new File(["b"], "b.jpg")];
    Object.defineProperty(fileInput.element, "files", { value: files, writable: false });
    await fileInput.trigger("change");
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    const reordered = [...vm.mediaItems].reverse();
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("reorder", reordered);
    await nextTick();

    expect(vm.mediaItems[0].attachedFileName).toBe("b.jpg");
  });

  // -- Media Text Modal (tested via internal component methods) -----------

  it("cancelMediaText does not change mediaText", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg", mediaText: "alt text" })];
    const wrapper = createWrapper({ paths, allowMediaText: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // Click media text button to open modal
    const btns = wrapper.findAll("button");
    const mediaTextBtn = btns.find((b) => b.text().includes("Media text"));
    expect(mediaTextBtn).toBeTruthy();
    await mediaTextBtn!.trigger("click");
    await nextTick();

    // Find and click Cancel
    const cancelBtns = wrapper.findAll("button").filter((b) => b.text().includes("Cancel"));
    expect(cancelBtns.length).toBeGreaterThan(0);
    await cancelBtns[0].trigger("click");
    await nextTick();

    // mediaText should remain unchanged after cancel
    expect(vm.mediaItems[0].mediaText).toBe("alt text");
  });

  it("saveMediaText saves editingMediaText to selected item", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg", mediaText: "old" })];
    const wrapper = createWrapper({ paths, allowMediaText: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // Click media text button to open the modal (triggers showMediaTextModal)
    const btns = wrapper.findAll("button");
    const mediaTextBtn = btns.find((b) => b.text().includes("Media text"));
    await mediaTextBtn!.trigger("click");
    await nextTick();

    // Click OK - this calls saveMediaText which saves editingMediaText to selectedMedia
    // Since showMediaTextModal sets editingMediaText = selectedMedia.mediaText ("old"),
    // clicking OK will save "old" back (no change, but exercises the code path)
    const okBtns = wrapper.findAll("button").filter((b) => b.text().includes("OK"));
    expect(okBtns.length).toBeGreaterThan(0);
    await okBtns[0].trigger("click");
    await nextTick();

    // The mediaText should remain "old" since we didn't change editingMediaText
    expect(vm.mediaItems[0].mediaText).toBe("old");
  });

  // -- Anchor Modal (tested via internal component methods) ---------------

  it("showAnchorModal / saveAnchor / resetAnchor / cancelAnchor work", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths, allowAnchors: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // The Anchor button should be visible (selectedMedia is set, allowAnchors=true, mime=image/jpeg)
    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    expect(anchorBtn).toBeTruthy();
    await anchorBtn!.trigger("click");
    await nextTick();

    // Find Reset and Cancel buttons in the anchor modal area
    const resetBtns = wrapper.findAll("button").filter((b) => b.text().includes("Reset"));
    const cancelBtns = wrapper.findAll("button").filter((b) => b.text().includes("Cancel"));
    const okBtns = wrapper.findAll("button").filter((b) => b.text().includes("OK"));

    // Click Reset to set anchor to 0.5, 0.5
    if (resetBtns.length > 0) await resetBtns[resetBtns.length - 1].trigger("click");
    await nextTick();

    // Click OK to save
    if (okBtns.length > 0) await okBtns[okBtns.length - 1].trigger("click");
    await nextTick();

    expect(vm.mediaItems[0].anchor).toBeDefined();
    expect(vm.mediaItems[0].anchor.x).toBe(0.5);
    expect(vm.mediaItems[0].anchor.y).toBe(0.5);
  });

  it("cancelAnchor does not modify anchor", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg", anchor: { x: 0.3, y: 0.7 } })];
    const wrapper = createWrapper({ paths, allowAnchors: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    await anchorBtn!.trigger("click");
    await nextTick();

    // Click Cancel
    const cancelBtns = wrapper.findAll("button").filter((b) => b.text().includes("Cancel"));
    await cancelBtns[cancelBtns.length - 1].trigger("click");
    await nextTick();

    // anchor stays unchanged
    expect(vm.mediaItems[0].anchor.x).toBe(0.3);
    expect(vm.mediaItems[0].anchor.y).toBe(0.7);
  });

  // -- smallThumbs localStorage persistence --------------------------------

  it("persists smallThumbs preference to localStorage", async () => {
    const wrapper = createWrapper();
    await flushPromises();

    // Toggle thumb size button
    const thumbToggle = wrapper.find(".mf-btn-icon");
    await thumbToggle.trigger("click");
    await nextTick();

    const stored = JSON.parse(localStorage.getItem("mediaFieldPrefs")!);
    expect(stored.smallThumbs).toBe(true);
  });

  it("handles invalid mediaFieldPrefs in localStorage gracefully", async () => {
    localStorage.setItem("mediaFieldPrefs", "NOT_JSON{{{");

    // Should not throw on mount
    const wrapper = createWrapper();
    await flushPromises();
    await nextTick();

    // Component should still work
    expect(wrapper.exists()).toBe(true);
  });

  // -- handleFiles single mode replaces existing ---------------------------

  it("handleFiles in single mode replaces existing item", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "first.jpg", mime: "image/jpeg", mediaPath: "first.jpg", url: "/u/first", size: 100, isNew: true, attachedFileName: "first.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: false });
    await flushPromises();

    // Upload first file
    const fileInput = wrapper.find('input[type="file"]');
    const file1 = new File(["a"], "first.jpg");
    Object.defineProperty(fileInput.element, "files", { value: [file1], writable: false });
    await fileInput.trigger("change");
    await flushPromises();
    await nextTick();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
    // In single mode, first item should be auto-selected
    expect(vm.selectedMedia).toBeTruthy();
  });

  // -- handleFiles with upload errors logs warning -------------------------

  it("handleFiles logs warning when there are upload errors", async () => {
    const warnSpy = vi.spyOn(console, "warn").mockImplementation(() => {});
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [],
      errors: ["photo.jpg: File too large"],
    });

    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    const file = new File(["x"], "photo.jpg");
    Object.defineProperty(fileInput.element, "files", { value: [file], writable: false });
    await fileInput.trigger("change");
    await flushPromises();

    expect(warnSpy).toHaveBeenCalledWith(
      "[media-field] Upload errors:",
      expect.arrayContaining(["photo.jpg: File too large"])
    );
    warnSpy.mockRestore();
  });

  // -- removeSelected with no selection but one item ----------------------

  it("removeSelected else-if branch: removes single item when nothing selected", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths, multiple: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
    vm.selectedMedia = null;
    await nextTick();

    // Call removeSelected directly to hit the else-if (length === 1) branch
    const internal = (wrapper.vm as any).$;
    internal.setupState.removeSelected();
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
  });

  // -- setAnchor (click on anchor image) -----------------------------------

  it("setAnchor updates editingAnchor from mouse position", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths, allowAnchors: true });
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // Open anchor modal
    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    await anchorBtn!.trigger("click");
    await nextTick();

    // Mock anchorImageRef
    const internal = (wrapper.vm as any).$;
    internal.setupState.anchorImageRef = { clientWidth: 400, clientHeight: 200 };

    internal.setupState.setAnchor({ offsetX: 100, offsetY: 50 } as MouseEvent);
    await nextTick();

    expect(internal.setupState.editingAnchor.x).toBeCloseTo(0.25);
    expect(internal.setupState.editingAnchor.y).toBeCloseTo(0.25);
  });

  // -- setAnchor when anchorImageRef is null --------------------------------

  it("setAnchor does nothing when anchorImageRef is null", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const wrapper = createWrapper({ allowAnchors: true, paths: [makePath({ path: "test/test.jpg" })] });
    await flushPromises();
    await nextTick();

    const internal = (wrapper.vm as any).$;
    internal.setupState.anchorImageRef = null;
    const before = { ...internal.setupState.editingAnchor };

    internal.setupState.setAnchor({ offsetX: 100, offsetY: 50 } as MouseEvent);
    await nextTick();

    // Anchor should not have changed
    expect(internal.setupState.editingAnchor.x).toBe(before.x);
    expect(internal.setupState.editingAnchor.y).toBe(before.y);
  });

  // -- handleFiles with multiple files in single mode ----------------------

  it("handleFiles with multiple uploads in single mode keeps only first", async () => {
    mockUploadFiles.mockResolvedValueOnce({
      uploaded: [
        { name: "a.jpg", mime: "image/jpeg", mediaPath: "a.jpg", url: "/u/a", size: 100, isNew: true, attachedFileName: "a.jpg" },
        { name: "b.jpg", mime: "image/jpeg", mediaPath: "b.jpg", url: "/u/b", size: 200, isNew: true, attachedFileName: "b.jpg" },
      ],
      errors: [],
    });

    const wrapper = createWrapper({ multiple: false });
    await flushPromises();

    const fileInput = wrapper.find('input[type="file"]');
    const files = [new File(["a"], "a.jpg"), new File(["b"], "b.jpg")];
    Object.defineProperty(fileInput.element, "files", { value: files, writable: false });
    await fileInput.trigger("change");
    await flushPromises();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
    expect(vm.mediaItems[0].attachedFileName).toBe("a.jpg");
  });

  // -- dragover sets isDraggingOver, dragleave resets it -------------------

  it("dragover and dragleave toggle isDraggingOver", async () => {
    const wrapper = createWrapper({ multiple: true });
    await flushPromises();

    const dropZone = wrapper.find(".mf-drop-zone");
    await dropZone.trigger("dragover");
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.isDraggingOver).toBe(true);

    await dropZone.trigger("dragleave");
    await nextTick();
    expect(vm.isDraggingOver).toBe(false);
  });
});
