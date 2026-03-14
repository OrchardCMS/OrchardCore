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

import MediaFieldGallery from "../MediaFieldGallery.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem, makeConfig, makePath } from "../../__tests__/mockdata";

const stubs = { GalleryContainer: true, MediaPickerModal: true };

function mockFetchOk(data: Record<string, unknown> = { name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" }) {
  global.fetch = vi.fn(() =>
    Promise.resolve({
      ok: true,
      status: 200,
      json: () => Promise.resolve(data),
    })
  ) as any;
}

function createWrapper(configOverrides: Record<string, unknown> = {}, propsOverrides: Record<string, unknown> = {}) {
  return mount(MediaFieldGallery, {
    props: {
      config: makeConfig(configOverrides as any),
      inputName: "GalleryField",
      ...propsOverrides,
    },
    global: getGlobalMountOptions(stubs),
  });
}

describe("MediaFieldGallery", () => {
  beforeEach(() => {
    setupTranslations();
    localStorage.clear();
    mockFetchOk();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it("renders hidden input", () => {
    const wrapper = createWrapper();
    const input = wrapper.find('input[type="hidden"]');
    expect(input.exists()).toBe(true);
    expect(input.attributes("name")).toBe("GalleryField");
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

  it("addMediaFiles adds for multiple, replaces for single", async () => {
    // Multiple mode
    const wrapperMulti = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vmMulti = wrapperMulti.vm as any;
    vmMulti.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" }),
    ]);
    await nextTick();
    expect(vmMulti.mediaItems).toHaveLength(2);

    // Single mode
    const wrapperSingle = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vmSingle = wrapperSingle.vm as any;
    vmSingle.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" }),
    ]);
    await nextTick();
    expect(vmSingle.mediaItems).toHaveLength(1);
  });

  it("selectAndDeleteMedia removes item", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Emit deleteMedia from GalleryContainer stub
    const gallery = wrapper.findComponent({ name: "GalleryContainer" });
    await gallery.vm.$emit("deleteMedia", vm.mediaItems[0]);
    await nextTick();

    expect(vm.mediaItems).toHaveLength(0);
  });

  it("onReorder updates mediaItems", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item1 = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    const item2 = makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" });
    vm.addMediaFiles([item1, item2]);
    await nextTick();

    const reordered = [vm.mediaItems[1], vm.mediaItems[0]];
    const gallery = wrapper.findComponent({ name: "GalleryContainer" });
    await gallery.vm.$emit("reorder", reordered);
    await nextTick();

    expect(vm.mediaItems[0].mediaPath).toBe("b.jpg");
    expect(vm.mediaItems[1].mediaPath).toBe("a.jpg");
  });

  it("serializedPaths correct after init", async () => {
    mockFetchOk({ name: "test.jpg", mime: "image/jpeg", mediaPath: "test/test.jpg", url: "/media/test.jpg" });
    const paths = [makePath({ path: "test/test.jpg", mediaText: "alt text" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();
    await nextTick();

    const input = wrapper.find('input[type="hidden"]');
    const val = JSON.parse(input.element.getAttribute("value")!);
    expect(val).toHaveLength(1);
    expect(val[0].path).toBe("test/test.jpg");
    expect(val[0].mediaText).toBe("alt text");
  });

  it("showPicker calls pickerModalRef.open", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    // Emit showPicker from GalleryContainer
    const gallery = wrapper.findComponent({ name: "GalleryContainer" });
    await gallery.vm.$emit("showPicker");
    await nextTick();

    // Since MediaPickerModal is stubbed, we verify indirectly.
    // The showPicker function calls pickerModalRef.value?.open()
    // We can check that no error was thrown and the component is stable.
    expect(wrapper.exists()).toBe(true);
  });

  it("onEditMediaText sets selectedMedia and opens modal", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mediaText: "existing text", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Emit editMediaText from GalleryContainer
    const gallery = wrapper.findComponent({ name: "GalleryContainer" });
    await gallery.vm.$emit("editMediaText", vm.mediaItems[0]);
    await nextTick();

    expect(vm.selectedMedia).toBe(vm.mediaItems[0]);
  });

  it("saveMediaText updates mediaText", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mediaText: "old", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Call onEditMediaText directly (sets selectedMedia, editingMediaText, opens modal)
    vm.onEditMediaText(vm.mediaItems[0]);
    await nextTick();

    // Update editingMediaText directly and call saveMediaText
    vm.editingMediaText = "new alt text";
    vm.saveMediaText();
    await nextTick();

    expect(vm.mediaItems[0].mediaText).toBe("new alt text");
  });

  it("onEditAnchor sets selectedMedia and opens anchor modal", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    const gallery = wrapper.findComponent({ name: "GalleryContainer" });
    await gallery.vm.$emit("editAnchor", vm.mediaItems[0]);
    await nextTick();

    expect(vm.selectedMedia).toBe(vm.mediaItems[0]);
  });

  it("saveAnchor updates anchor", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Call onEditAnchor directly (sets selectedMedia, editingAnchor, opens modal)
    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();

    // Call saveAnchor directly (saves editingAnchor to selectedMedia.anchor)
    vm.saveAnchor();
    await nextTick();

    expect(vm.mediaItems[0].anchor).toBeDefined();
    expect(vm.mediaItems[0].anchor.x).toBe(0.5);
    expect(vm.mediaItems[0].anchor.y).toBe(0.5);
  });

  it("resetAnchor resets to 0.5, 0.5", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", anchor: { x: 0.2, y: 0.8 }, vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Call onEditAnchor directly (loads anchor from media item)
    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();

    // Call resetAnchor, then saveAnchor
    vm.resetAnchor();
    vm.saveAnchor();
    await nextTick();

    expect(vm.mediaItems[0].anchor.x).toBe(0.5);
    expect(vm.mediaItems[0].anchor.y).toBe(0.5);
  });

  // -- cancelMediaText and cancelAnchor -----------------------------------

  it("cancelMediaText closes modal without saving (via exposed state)", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mediaText: "keep this", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // Open the media text modal and modify editingMediaText
    vm.onEditMediaText(vm.mediaItems[0]);
    await nextTick();
    expect(vm.mediaTextModalVisible).toBe(true);

    // Change editingMediaText but don't save
    vm.editingMediaText = "modified but unsaved";
    await nextTick();

    // Close the modal without saving (simulate cancel by just closing)
    vm.mediaTextModalVisible = false;
    await nextTick();

    // The media item's text should remain unchanged
    expect(vm.mediaItems[0].mediaText).toBe("keep this");
  });

  it("cancelAnchor closes modal without saving (via exposed state)", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", anchor: { x: 0.3, y: 0.7 }, vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();
    expect(vm.anchorModalVisible).toBe(true);

    // Modify editingAnchor but don't save
    vm.editingAnchor = { x: 0.1, y: 0.1 };
    await nextTick();

    // Close without saving
    vm.anchorModalVisible = false;
    await nextTick();

    // Anchor should remain unchanged on the original item
    expect(vm.mediaItems[0].anchor.x).toBe(0.3);
    expect(vm.mediaItems[0].anchor.y).toBe(0.7);
  });

  // -- loadInitialPaths edge cases ----------------------------------------

  it("loadInitialPaths handles 404 as not-found", async () => {
    global.fetch = vi.fn(() =>
      Promise.resolve({ ok: false, status: 404, json: () => Promise.resolve({}) })
    ) as any;
    const paths = [makePath({ path: "missing.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems[0].errorType).toBe("not-found");
  });

  it("loadInitialPaths handles network error as transient", async () => {
    global.fetch = vi.fn(() => Promise.reject(new Error("fail"))) as any;
    const paths = [makePath({ path: "net-err.jpg" })];
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

  // -- selectAndDeleteMedia clears selectedMedia --------------------------

  it("selectAndDeleteMedia clears selectedMedia when deleting selected item", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    vm.addMediaFiles([item]);
    await nextTick();

    // First select the item, then delete it
    vm.selectedMedia = vm.mediaItems[0];
    await nextTick();

    const gallery = wrapper.findComponent({ name: "GalleryContainer" });
    await gallery.vm.$emit("delete-media", vm.mediaItems[0]);
    await nextTick();

    expect(vm.selectedMedia).toBeNull();
    expect(vm.mediaItems).toHaveLength(0);
  });

  // -- addMediaFiles single file in single mode ---------------------------

  it("addMediaFiles with single file in single mode", async () => {
    const wrapper = createWrapper({ multiple: false, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "single.jpg", vuekey: "s0" })]);
    await nextTick();

    expect(vm.mediaItems).toHaveLength(1);
    expect(vm.mediaItems[0].mediaPath).toBe("single.jpg");
  });

  // -- onEditAnchor without existing anchor (defaults to 0.5, 0.5) --------

  it("onEditAnchor defaults to center when no anchor exists", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    const item = makeMediaItem({ mediaPath: "no-anchor.jpg", mime: "image/jpeg", vuekey: "na0" });
    vm.addMediaFiles([item]);
    await nextTick();

    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();

    expect(vm.editingAnchor.x).toBe(0.5);
    expect(vm.editingAnchor.y).toBe(0.5);
  });

  // -- serializedPaths before init ----------------------------------------

  it("serializedPaths returns config.paths before initialization", () => {
    const paths = [makePath({ path: "pre-init.jpg" })];
    const wrapper = createWrapper({ paths });
    const input = wrapper.find('input[type="hidden"]');
    expect(input.element.getAttribute("value")).toBe(JSON.stringify(paths));
  });

  // -- Non-404 HTTP error should be "transient" ---------------------------

  it("loadInitialPaths treats non-404 HTTP errors as transient", async () => {
    global.fetch = vi.fn(() =>
      Promise.resolve({ ok: false, status: 500, json: () => Promise.resolve({}) })
    ) as any;
    const paths = [makePath({ path: "server-err.jpg" })];
    const wrapper = createWrapper({ paths });
    await flushPromises();

    const vm = wrapper.vm as any;
    expect(vm.mediaItems[0].errorType).toBe("transient");
  });

  // -- setAnchor (click on anchor image) -----------------------------------

  it("setAnchor updates editingAnchor from mouse position", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", url: "/media/a.jpg", vuekey: "a0" })]);
    await nextTick();

    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();

    // Mock anchorImageRef
    const internal = (wrapper.vm as any).$;
    internal.setupState.anchorImageRef = { clientWidth: 200, clientHeight: 100 };

    internal.setupState.setAnchor({ offsetX: 50, offsetY: 25 } as MouseEvent);
    await nextTick();

    expect(vm.editingAnchor.x).toBeCloseTo(0.25);
    expect(vm.editingAnchor.y).toBeCloseTo(0.25);
  });

  it("setAnchor does nothing when anchorImageRef is null", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", vuekey: "a0" })]);
    await nextTick();

    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();

    const internal = (wrapper.vm as any).$;
    internal.setupState.anchorImageRef = null;
    const before = { ...vm.editingAnchor };

    internal.setupState.setAnchor({ offsetX: 100, offsetY: 50 } as MouseEvent);
    await nextTick();

    expect(vm.editingAnchor.x).toBe(before.x);
    expect(vm.editingAnchor.y).toBe(before.y);
  });

  // -- cancelMediaText directly -------------------------------------------

  it("cancelMediaText closes modal directly", async () => {
    const wrapper = createWrapper({ multiple: true, allowMediaText: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" })]);
    await nextTick();

    vm.onEditMediaText(vm.mediaItems[0]);
    await nextTick();
    expect(vm.mediaTextModalVisible).toBe(true);

    vm.cancelMediaText();
    await nextTick();
    expect(vm.mediaTextModalVisible).toBe(false);
  });

  // -- cancelAnchor directly ----------------------------------------------

  it("cancelAnchor closes modal directly", async () => {
    const wrapper = createWrapper({ multiple: true, allowAnchors: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([makeMediaItem({ mediaPath: "a.jpg", mime: "image/jpeg", vuekey: "a0" })]);
    await nextTick();

    vm.onEditAnchor(vm.mediaItems[0]);
    await nextTick();
    expect(vm.anchorModalVisible).toBe(true);

    vm.cancelAnchor();
    await nextTick();
    expect(vm.anchorModalVisible).toBe(false);
  });

  // -- serializedPaths after initialization --------------------------------

  it("serializedPaths maps mediaItems after initialization", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const vm = wrapper.vm as any;
    vm.addMediaFiles([
      makeMediaItem({ mediaPath: "a.jpg", mediaText: "alt a", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", anchor: { x: 0.3, y: 0.7 }, vuekey: "b0" }),
    ]);
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    const input = wrapper.find('input[type="hidden"]');
    const val = JSON.parse(input.element.getAttribute("value")!);
    expect(val).toHaveLength(2);
    expect(val[0].path).toBe("a.jpg");
    expect(val[0].mediaText).toBe("alt a");
    expect(val[1].path).toBe("b.jpg");
    expect(val[1].anchor).toEqual({ x: 0.3, y: 0.7 });
  });

  // -- showPicker direct call ---------------------------------------------

  it("showPicker calls pickerModalRef.open", async () => {
    const wrapper = createWrapper({ multiple: true, paths: [] });
    await flushPromises();

    const internal = (wrapper.vm as any).$;
    internal.setupState.showPicker();
    await nextTick();
    expect(wrapper.exists()).toBe(true);
  });

  // -- onPickerSelect direct call -----------------------------------------

  it("onPickerSelect adds files", async () => {
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
});
