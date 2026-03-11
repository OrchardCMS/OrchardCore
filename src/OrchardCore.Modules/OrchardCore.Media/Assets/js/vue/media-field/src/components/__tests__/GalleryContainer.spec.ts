import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount, flushPromises } from "@vue/test-utils";
import { ref, nextTick } from "vue";

vi.mock("@bloom/helpers/localizations", () => {
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

import GalleryContainer from "../GalleryContainer.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem, makeConfig, makePath } from "../../__tests__/mockdata";

const GalleryCardItemStub = {
  name: "GalleryCardItem",
  template: '<li class="gallery-card-stub" draggable="true" @dragstart="$emit(\'dragstart\', $event)" @drop="$emit(\'drop\', $event)" @dragend="$emit(\'dragend\', $event)"></li>',
  props: ["media", "allowMultiple", "allowMediaText", "allowAnchors", "size"],
  emits: ["editMediaText", "editAnchor", "deleteMedia", "dragstart", "drop", "dragend"],
};
const stubs = { GalleryListItem: true, GalleryCardItem: GalleryCardItemStub };

function createWrapper(propsOverrides: Record<string, unknown> = {}) {
  return mount(GalleryContainer, {
    props: {
      mediaItems: [],
      allowMultiple: true,
      allowMediaText: false,
      allowAnchors: false,
      idPrefix: "test-field",
      ...propsOverrides,
    },
    global: getGlobalMountOptions(stubs),
  });
}

describe("GalleryContainer", () => {
  beforeEach(() => {
    setupTranslations();
    localStorage.clear();
  });

  it("renders in grid view by default", () => {
    const wrapper = createWrapper();
    expect(wrapper.find(".mf-gallery-cards").exists()).toBe(true);
    expect(wrapper.find(".mf-gallery-list").exists()).toBe(false);
  });

  it("shows add card in grid view", () => {
    const wrapper = createWrapper();
    expect(wrapper.find(".mf-gallery-add-card").exists()).toBe(true);
  });

  it("toggles to list view on button click", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).gridView = false;
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();
    expect(wrapper.find(".mf-gallery-list").exists()).toBe(true);
    expect(wrapper.find(".mf-gallery-cards").exists()).toBe(false);
  });

  it("shows 'Add media' empty item in list when no items", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).gridView = false;
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();
    expect(wrapper.find(".mf-gallery-list-empty").exists()).toBe(true);
  });

  it("de-duplicates items by mediaPath", () => {
    const items = [
      makeMediaItem({ mediaPath: "a.jpg", name: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "a.jpg", name: "a-dup.jpg", vuekey: "a1" }),
      makeMediaItem({ mediaPath: "b.jpg", name: "b.jpg", vuekey: "b0" }),
    ];
    const wrapper = createWrapper({ mediaItems: items });
    // In grid view, GalleryCardItem stubs are rendered for unique items
    const cards = wrapper.findAllComponents({ name: "GalleryCardItem" });
    expect(cards).toHaveLength(2);
  });

  it("filters out isRemoved items", () => {
    const items = [
      makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" }),
      makeMediaItem({ mediaPath: "b.jpg", isRemoved: true, vuekey: "b0" }),
    ];
    const wrapper = createWrapper({ mediaItems: items });
    const cards = wrapper.findAllComponents({ name: "GalleryCardItem" });
    expect(cards).toHaveLength(1);
  });

  it("emits showPicker on add button click", async () => {
    const wrapper = createWrapper();
    await wrapper.find(".mf-btn-primary").trigger("click");
    expect(wrapper.emitted("showPicker")).toBeTruthy();
  });

  it("emits reorder on drag-drop (test via component method call)", async () => {
    const item1 = makeMediaItem({ mediaPath: "a.jpg", name: "a.jpg", vuekey: "a0" });
    const item2 = makeMediaItem({ mediaPath: "b.jpg", name: "b.jpg", vuekey: "b0" });
    const items = [item1, item2];
    const wrapper = createWrapper({ mediaItems: items });

    // Call exposed drag handlers directly with items matched by mediaPath
    const vm = wrapper.vm as any;
    vm.onDragStart({ dataTransfer: { effectAllowed: "" } }, item1);
    vm.onDrop({}, item2);

    // The component should emit reorder
    expect(wrapper.emitted("reorder")).toBeTruthy();
  });

  it("saves/loads view prefs from localStorage", async () => {
    // Set prefs
    localStorage.setItem(
      "mediaFieldGallery_test-field",
      JSON.stringify({ size: "sm", gridView: false })
    );

    const wrapper = createWrapper();
    await flushPromises();
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();

    // After mount, should load saved prefs
    expect(wrapper.find(".mf-gallery-list").exists()).toBe(true);

    // Now toggle to grid via vm and check localStorage updated
    (wrapper.vm as any).gridView = true;
    await nextTick();
    const saved = JSON.parse(localStorage.getItem("mediaFieldGallery_test-field")!);
    expect(saved.gridView).toBe(true);
  });

  it("enforces single item for non-multiple mode (watch fires reorder)", async () => {
    const item1 = makeMediaItem({ mediaPath: "a.jpg", vuekey: "a0" });
    const item2 = makeMediaItem({ mediaPath: "b.jpg", vuekey: "b0" });

    // Pass 2 items with allowMultiple: false - the immediate watch should emit reorder
    const wrapper = createWrapper({
      allowMultiple: false,
      mediaItems: [item1, item2],
    });
    await flushPromises();
    await nextTick();

    const reorderEvents = wrapper.emitted("reorder");
    expect(reorderEvents).toBeTruthy();
    // Should emit with only the last item
    const lastEmit = reorderEvents![reorderEvents!.length - 1];
    expect((lastEmit[0] as any[])).toHaveLength(1);
  });

  it("handles invalid localStorage gracefully (catch block)", async () => {
    localStorage.setItem("mediaFieldGallery_test-field", "NOT_JSON{{{");
    const wrapper = createWrapper();
    await flushPromises();
    expect(wrapper.exists()).toBe(true);
  });

  it("onDragEnd resets draggedItem", async () => {
    const item1 = makeMediaItem({ mediaPath: "a.jpg", name: "a.jpg", vuekey: "a0" });
    const item2 = makeMediaItem({ mediaPath: "b.jpg", name: "b.jpg", vuekey: "b0" });
    const wrapper = createWrapper({ mediaItems: [item1, item2] });
    const vm = wrapper.vm as any;

    vm.onDragStart({ dataTransfer: { effectAllowed: "" } }, item1);
    vm.onDragEnd();
    vm.onDrop({}, item2);
    expect(wrapper.emitted("reorder")).toBeFalsy();
  });

  it("grid size toggle between sm and lg", async () => {
    const wrapper = createWrapper();
    // Default is lg
    expect(wrapper.find(".mf-size-lg").exists()).toBe(true);

    // Toggle size via vm
    (wrapper.vm as any).size = "sm";
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();
    expect(wrapper.find(".mf-size-sm").exists()).toBe(true);

    // Toggle back to lg
    (wrapper.vm as any).size = "lg";
    await nextTick();
    wrapper.vm.$forceUpdate();
    await nextTick();
    expect(wrapper.find(".mf-size-lg").exists()).toBe(true);
  });
});
