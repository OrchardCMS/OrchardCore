import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { mount, flushPromises } from "@vue/test-utils";
import { nextTick } from "vue";

vi.mock("@bloom/helpers/localizations", () => {
  const store: Record<string, string> = {};
  return {
    getTranslations: () => store,
    setTranslations: (t: Record<string, string>) => Object.assign(store, t),
  };
});

vi.mock("vue-final-modal", () => ({
  VueFinalModal: { name: "VueFinalModal", template: "<div><slot /></div>", props: ["modelValue"] },
  createVfm: () => ({ install: vi.fn() }),
}));

vi.mock("@media-gallery", () => ({
  mountMediaAppAsPicker: vi.fn(() => ({
    getSelectedFiles: vi.fn(() => []),
    unmount: vi.fn(),
  })),
}));

import MediaFieldBasic from "../MediaFieldBasic.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem, makeConfig, makePath } from "../../__tests__/mockdata";

const stubs = { MediaPickerModal: true, ThumbsContainer: true };

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

function createWrapper(configOverrides: Record<string, unknown> = {}, propsOverrides: Record<string, unknown> = {}) {
  return mount(MediaFieldBasic, {
    props: {
      config: makeConfig(configOverrides as any),
      inputName: "MediaField",
      ...propsOverrides,
    },
    global: getGlobalMountOptions(stubs),
  });
}

describe("MediaFieldBasic", () => {
  beforeEach(() => {
    setupTranslations();
    localStorage.clear();
    mockFetchOk();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders hidden input with serialized paths", () => {
    const wrapper = createWrapper();
    const input = wrapper.find('input[type="hidden"]');
    expect(input.exists()).toBe(true);
    expect(input.attributes("name")).toBe("MediaField");
  });

  it("serializedPaths returns config.paths before initialization", () => {
    const paths = [makePath({ path: "foo/bar.jpg" })];
    const wrapper = createWrapper({ paths });
    const input = wrapper.find('input[type="hidden"]');
    expect(input.element.getAttribute("value")).toBe(JSON.stringify(paths));
  });

  it("serializedPaths returns mediaItems after initialization", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();
    await nextTick();

    const input = wrapper.find('input[type="hidden"]');
    const val = JSON.parse(input.element.getAttribute("value")!);
    expect(val).toHaveLength(1);
    expect(val[0].path).toBe("test/test.jpg");
  });

  it("loadInitialPaths fetches media details from API", async () => {
    const paths = [makePath({ path: "test/test.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();

    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("path=test%2Ftest.jpg")
    );
  });

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

  it("loadInitialPaths batches requests when mediaItemsUrl is configured", async () => {
    global.fetch = vi.fn(() =>
      Promise.resolve({
        ok: true,
        status: 200,
        json: () => Promise.resolve([
          { name: "test.jpg", mime: "image/jpeg", filePath: "test/test.jpg", url: "/media/test.jpg" },
        ]),
      })
    ) as any;

    const paths = [makePath({ path: "test/test.jpg" })];
    createWrapper({ paths, mediaItemsUrl: "/api/media/GetMediaFieldItems" });
    await flushPromises();

    expect(global.fetch).toHaveBeenCalledWith(
      expect.stringContaining("/api/media/GetMediaFieldItems?paths=test%2Ftest.jpg")
    );
  });

  it("loadInitialPaths with empty paths sets initialized", async () => {
    const wrapper = createWrapper({ paths: [] });
    await flushPromises();

    const input = wrapper.find('input[type="hidden"]');
    const val = JSON.parse(input.element.getAttribute("value")!);
    expect(val).toEqual([]);
  });

  it("addMediaFiles adds items for multiple mode", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" }),
    ]);
    await nextTick();

    expect(vm.mediaItems).toHaveLength(2);
  });

  it("addMediaFiles replaces for single mode", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" }),
    ]);
    await nextTick();

    expect(vm.mediaItems).toHaveLength(1);
  });

  it("addMediaFiles ignores duplicate mediaPath entries", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();
    expect(vm.mediaItems).toHaveLength(1);

    // Same path again → no-op
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a1" })]);
    await nextTick();
    expect(vm.mediaItems).toHaveLength(1);

    // Batch mixing duplicate + new → only new is added
    vm.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a2" }),
      makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" }),
    ]);
    await nextTick();
    expect(vm.mediaItems).toHaveLength(2);
    expect(vm.mediaItems.map((i: any) => i.mediaPath)).toEqual(["a.jpg", "b.jpg"]);
  });

  it("removeSelected removes selected item", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Select via ThumbsContainer emit
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    expect(vm.selectedMedia).toBeTruthy();

    // Find and click the Remove button
    const removeBtn = wrapper.find(".mf-btn-danger");
    expect(removeBtn.exists()).toBe(true);
    await removeBtn.trigger("click");
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
    expect(vm.selectedMedia).toBeNull();
  });

  it("selectMedia sets selectedMedia", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Emit select-media from ThumbsContainer stub
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();

    expect(vm.selectedMedia).toBe(vm.mediaItems[0]);
  });

  it("selectMedia deselects when clicking the same image twice", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    expect(vm.selectedMedia).toBe(vm.mediaItems[0]);

    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    expect(vm.selectedMedia).toBeNull();
  });

  it("canAddMedia false when single and has item", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const addBtn = wrapper.find(".mf-btn-primary");
    expect(addBtn.attributes("disabled")).toBeDefined();
  });

  it("canAddMedia true when multiple", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const addBtn = wrapper.find(".mf-btn-primary");
    expect(addBtn.attributes("disabled")).toBeUndefined();
  });

  it("shows Remove button when selectedMedia", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    // Select via ThumbsContainer emit
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // Verify selection worked
    expect(vm.selectedMedia).toBeTruthy();
    expect(wrapper.find(".mf-btn-danger").exists()).toBe(true);
  });

  it("shows Media Text button when selectedMedia and allowMediaText", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    // Select via ThumbsContainer emit
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const mediaTextBtn = btns.find((b) => b.text().includes("Media text"));
    expect(mediaTextBtn).toBeTruthy();
  });

  it("shows Anchor button when selectedMedia, allowAnchors, and image type", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", vuekey: "a0" })]);
    await nextTick();

    // Select via ThumbsContainer emit
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    expect(anchorBtn).toBeTruthy();
  });

  it("smallThumbs toggle changes thumbSize", async () => {
    const wrapper = createWrapper({ paths: [] });
    await flushPromises();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    expect(thumbs.props("thumbSize")).toBe(240);

    // Toggle smallThumbs via vm (exposed via defineExpose)
    const vm = wrapper.vm as any;
    vm.smallThumbs = true;
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    expect(thumbs.props("thumbSize")).toBe(120);
  });

  // -- selectAndDeleteMedia -----------------------------------------------

  it("selectAndDeleteMedia selects then removes item", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("delete-media", vm.mediaItems[0]);
    await nextTick();
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
    expect(vm.selectedMedia).toBeNull();
  });

  // -- onReorder ----------------------------------------------------------

  it("onReorder updates mediaItems order", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" }),
    ]);
    await nextTick();

    const reordered = [vm.mediaItems[1], vm.mediaItems[0]];
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("reorder", reordered);
    await nextTick();

    expect(vm.mediaItems[0].mediaPath).toBe("b.jpg");
    expect(vm.mediaItems[1].mediaPath).toBe("a.jpg");
  });

  // -- addMediaFiles single item for single mode --------------------------

  it("addMediaFiles with single file for single mode", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "only.jpg", vuekey: "o0" })]);
    await nextTick();
    await nextTick();

    expect(vm.mediaItems).toHaveLength(1);
    // Single mode auto-selects
    expect(vm.selectedMedia).toBeTruthy();
    expect(vm.selectedMedia.mediaPath).toBe("only.jpg");
  });

  // -- showPicker / onPickerSelect ----------------------------------------

  it("showPicker button is disabled when canAddMedia is false", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // canAddMedia is false (single mode with item), button should be disabled
    const addBtn = wrapper.find(".mf-btn-primary");
    expect(addBtn.classes()).toContain("is-disabled");
  });

  it("onPickerSelect adds files via addMediaFiles", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const picker = wrapper.findComponent({ name: "MediaPickerModal" });
    await picker.vm.$emit("select", [
      makeMediaItem({ mediaPath: "picked.jpg", vuekey: "p0" }),
    ]);
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
    expect(vm.mediaItems[0].mediaPath).toBe("picked.jpg");
  });

  it("onPickerSelect keeps only one item in single mode", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const picker = wrapper.findComponent({ name: "MediaPickerModal" });
    await picker.vm.$emit("select", [
      makeMediaItem({ mediaPath: "picked-a.jpg", vuekey: "p0" }),
      makeMediaItem({ mediaPath: "picked-b.jpg", vuekey: "p1" }),
    ]);
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
    expect(vm.mediaItems[0].mediaPath).toBe("picked-a.jpg");
  });

  // -- Media Text Modal (via button clicks) --------------------------------

  it("cancelMediaText does not change mediaText", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mediaText: "hello", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const mediaTextBtn = btns.find((b) => b.text().includes("Media text"));
    expect(mediaTextBtn).toBeTruthy();
    await mediaTextBtn!.trigger("click");
    await nextTick();

    const cancelBtns = wrapper.findAll("button").filter((b) => b.text().includes("Cancel"));
    expect(cancelBtns.length).toBeGreaterThan(0);
    await cancelBtns[0].trigger("click");
    await nextTick();

    expect(vm.mediaItems[0].mediaText).toBe("hello");
  });

  it("saveMediaText saves editingMediaText to selected item", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mediaText: "existing", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // Click media text button to open modal (sets editingMediaText = "existing")
    const btns = wrapper.findAll("button");
    const mediaTextBtn = btns.find((b) => b.text().includes("Media text"));
    await mediaTextBtn!.trigger("click");
    await nextTick();

    // Click OK to save - exercises saveMediaText code path
    const okBtns = wrapper.findAll("button").filter((b) => b.text().includes("OK"));
    expect(okBtns.length).toBeGreaterThan(0);
    await okBtns[0].trigger("click");
    await nextTick();

    // mediaText should remain "existing" (editingMediaText was set from it)
    expect(vm.mediaItems[0].mediaText).toBe("existing");
  });

  // -- Anchor Modal (via exposed methods) ---------------------------------

  it("showAnchorModal / saveAnchor / resetAnchor / cancelAnchor", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    expect(anchorBtn).toBeTruthy();
    await anchorBtn!.trigger("click");
    await nextTick();

    // Reset and save
    const resetBtns = wrapper.findAll("button").filter((b) => b.text().includes("Reset"));
    if (resetBtns.length > 0) await resetBtns[resetBtns.length - 1].trigger("click");
    await nextTick();

    const okBtns = wrapper.findAll("button").filter((b) => b.text().includes("OK"));
    if (okBtns.length > 0) await okBtns[okBtns.length - 1].trigger("click");
    await nextTick();

    expect(vm.mediaItems[0].anchor).toBeDefined();
    expect(vm.mediaItems[0].anchor.x).toBe(0.5);
    expect(vm.mediaItems[0].anchor.y).toBe(0.5);
  });

  it("cancelAnchor does not modify anchor", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", anchor: { x: 0.3, y: 0.7 }, vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    await anchorBtn!.trigger("click");
    await nextTick();

    const cancelBtns = wrapper.findAll("button").filter((b) => b.text().includes("Cancel"));
    await cancelBtns[cancelBtns.length - 1].trigger("click");
    await nextTick();

    expect(vm.mediaItems[0].anchor.x).toBe(0.3);
    expect(vm.mediaItems[0].anchor.y).toBe(0.7);
  });

  // -- localStorage persistence ------------------------------------------

  it("persists smallThumbs to localStorage on change", async () => {
    const wrapper = createWrapper({ paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.smallThumbs = true;
    await nextTick();

    const stored = JSON.parse(localStorage.getItem("mediaFieldPrefs")!);
    expect(stored.smallThumbs).toBe(true);
  });

  it("handles invalid mediaFieldPrefs in localStorage gracefully", async () => {
    localStorage.setItem("mediaFieldPrefs", "INVALID{{{");

    // Should not throw on mount
    const wrapper = createWrapper({ paths: [] });
    await flushPromises();
    await nextTick();

    expect(wrapper.exists()).toBe(true);
  });

  // -- auto-select in single mode on initial load -------------------------

  it("auto-selects first item in single mode on initial load", async () => {
    mockFetchOk({ name: "auto.jpg", mime: "image/jpeg", mediaPath: "test/auto.jpg", url: "/media/auto.jpg" });
    const paths = [makePath({ path: "test/auto.jpg" })];
    const wrapper = createWrapper({ multiple: false, paths });
    await flushPromises();
    await nextTick();
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.selectedMedia).toBeTruthy();
  });

  // -- onDocumentClick (click outside deselects) ----------------------------

  it("onDocumentClick deselects when clicking outside", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    expect(vm.selectedMedia).toBeTruthy();

    // Simulate click outside (on document body)
    const outsideEl = document.createElement("div");
    document.body.appendChild(outsideEl);
    const clickEvent = new MouseEvent("click", { bubbles: true });
    outsideEl.dispatchEvent(clickEvent);
    await nextTick();

    expect(vm.selectedMedia).toBeNull();
    document.body.removeChild(outsideEl);
  });

  it("onDocumentClick does NOT deselect when clicking inside .mf-thumb-item", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    expect(vm.selectedMedia).toBeTruthy();

    // Simulate click inside a thumb item
    const thumbEl = document.createElement("div");
    thumbEl.className = "mf-thumb-item";
    document.body.appendChild(thumbEl);
    const clickEvent = new MouseEvent("click", { bubbles: true });
    thumbEl.dispatchEvent(clickEvent);
    await nextTick();

    // Should NOT deselect
    expect(vm.selectedMedia).toBeTruthy();
    document.body.removeChild(thumbEl);
  });

  // -- setAnchor (click on anchor image) -----------------------------------

  it("setAnchor updates editingAnchor from mouse position", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", url: "/media/a.jpg", vuekey: "a0" })]);
    await nextTick();

    // Select item and open anchor modal
    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const btns = wrapper.findAll("button");
    const anchorBtn = btns.find((b) => b.text().includes("Anchor"));
    await anchorBtn!.trigger("click");
    await nextTick();

    // Mock the anchorImageRef with clientWidth/clientHeight
    const internal = (wrapper.vm as any).$;
    internal.setupState.anchorImageRef = {
      clientWidth: 200,
      clientHeight: 100,
    };

    // Call setAnchor with a mock MouseEvent
    internal.setupState.setAnchor({ offsetX: 50, offsetY: 25 } as MouseEvent);
    await nextTick();

    expect(vm.editingAnchor.x).toBeCloseTo(0.25);
    expect(vm.editingAnchor.y).toBeCloseTo(0.25);
  });

  // -- cancelMediaText directly -------------------------------------------

  it("cancelMediaText closes modal via direct call", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mediaText: "txt", vuekey: "a0" })]);
    await nextTick();

    const internal = (wrapper.vm as any).$;
    internal.setupState.mediaTextModalVisible = true;
    await nextTick();

    internal.setupState.cancelMediaText();
    await nextTick();
    expect(internal.setupState.mediaTextModalVisible).toBe(false);
  });

  // -- cancelAnchor directly ----------------------------------------------

  it("cancelAnchor closes modal via direct call", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const internal = (wrapper.vm as any).$;
    internal.setupState.anchorModalVisible = true;
    await nextTick();

    internal.setupState.cancelAnchor();
    await nextTick();
    expect(internal.setupState.anchorModalVisible).toBe(false);
  });

  // -- removeSelected else-if branch (no selection, 1 item) ----------------

  it("removeSelected removes single item when nothing is selected", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();
    vm.selectedMedia = null;
    await nextTick();

    // Call removeSelected directly — exercises the else-if branch
    const internal = (wrapper.vm as any).$;
    internal.setupState.removeSelected();
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
  });

  // -- showPicker direct call ---------------------------------------------

  it("showPicker calls pickerModalRef.open (no-op when ref is stub)", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const internal = (wrapper.vm as any).$;
    // showPicker should not throw even when pickerModalRef is a stub
    internal.setupState.showPicker();
    await nextTick();
    expect(wrapper.exists()).toBe(true);
  });

  it("showPicker does nothing when canAddMedia is false", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    const internal = (wrapper.vm as any).$;
    internal.setupState.showPicker();
    await nextTick();
    expect(wrapper.exists()).toBe(true);
  });

  // -- showMediaTextModal direct call ------------------------------------

  it("showMediaTextModal does nothing when no selectedMedia", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const internal = (wrapper.vm as any).$;
    internal.setupState.showMediaTextModal();
    await nextTick();
    expect(internal.setupState.mediaTextModalVisible).toBe(false);
  });

  // -- saveMediaText direct call -----------------------------------------

  it("saveMediaText updates mediaText on selectedMedia via direct call", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mediaText: "old", vuekey: "a0" })]);
    await nextTick();

    const thumbs = wrapper.findComponent({ name: "ThumbsContainer" });
    await thumbs.vm.$emit("select-media", vm.mediaItems[0]);
    await nextTick();

    const internal = (wrapper.vm as any).$;
    internal.setupState.editingMediaText = "updated";
    internal.setupState.saveMediaText();
    await nextTick();

    expect(vm.mediaItems[0].mediaText).toBe("updated");
    expect(internal.setupState.mediaTextModalVisible).toBe(false);
  });

  // -- onPickerSelect direct call ----------------------------------------

  it("onPickerSelect adds files via addMediaFiles", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const internal = (wrapper.vm as any).$;
    internal.setupState.onPickerSelect([
      makeMediaItem({ mediaPath: "picked.jpg", vuekey: "p0" }),
    ]);
    await nextTick();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems).toHaveLength(1);
  });

  it("onBeforeUnmount removes document click listener", async () => {
    const spy = vi.spyOn(document, "removeEventListener");
    const wrapper = createWrapper();
    await flushPromises();
    wrapper.unmount();
    expect(spy).toHaveBeenCalledWith("click", expect.any(Function));
    spy.mockRestore();
  });
});
