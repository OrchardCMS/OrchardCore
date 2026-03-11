import { ref } from "vue";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount } from "@vue/test-utils";
import ThumbsContainer from "../ThumbsContainer.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem } from "../../__tests__/mockdata";

vi.mock("vue-final-modal", () => ({
  VueFinalModal: { name: "VueFinalModal", template: "<div><slot /></div>", props: ["modelValue"] },
  createVfm: () => ({ install: vi.fn() }),
}));

vi.mock("@bloom/helpers/localizations", () => {
  const translations = ref<Record<string, string>>({});
  return {
    useLocalizations: () => ({
      translations,
      setTranslations: (t: Record<string, string>) => { translations.value = t; },
    }),
  };
});

function mountComponent(props: Partial<InstanceType<typeof ThumbsContainer>["$props"]> = {}) {
  return mount(ThumbsContainer, {
    props: {
      mediaItems: [],
      selectedMedia: null,
      thumbSize: 120,
      allowMultiple: true,
      ...props,
    },
    global: getGlobalMountOptions(),
  });
}

describe("ThumbsContainer", () => {
  beforeEach(() => {
    setupTranslations();
  });

  it("shows 'No Files' empty state when mediaItems is empty", () => {
    const wrapper = mountComponent({ mediaItems: [] });
    expect(wrapper.find(".mf-empty-card").exists()).toBe(true);
    expect(wrapper.text()).toContain("No Files");
  });

  it("renders visible items (filters out isRemoved)", () => {
    const items = [
      makeMediaItem({ name: "a.jpg", vuekey: "a" }),
      makeMediaItem({ name: "b.jpg", vuekey: "b", isRemoved: true }),
      makeMediaItem({ name: "c.jpg", vuekey: "c" }),
    ];
    const wrapper = mountComponent({ mediaItems: items });
    const thumbItems = wrapper.findAll(".mf-thumb-item");
    expect(thumbItems).toHaveLength(2);
  });

  it("shows image preview for image mime types", () => {
    const items = [makeMediaItem({ name: "photo.jpg", mime: "image/jpeg", url: "/media/photo.jpg", vuekey: "p1" })];
    const wrapper = mountComponent({ mediaItems: items });
    const img = wrapper.find("img");
    expect(img.exists()).toBe(true);
    expect(img.attributes("data-mime")).toBe("image/jpeg");
  });

  it("shows icon for non-image files", () => {
    const items = [makeMediaItem({ name: "doc.pdf", mime: "application/pdf", url: "/media/doc.pdf", vuekey: "d1" })];
    const wrapper = mountComponent({ mediaItems: items });
    expect(wrapper.find("img").exists()).toBe(false);
    const icon = wrapper.find(".mf-thumb-preview i");
    expect(icon.exists()).toBe(true);
  });

  it("applies active class on selected item", () => {
    const item = makeMediaItem({ vuekey: "sel1" });
    const wrapper = mountComponent({ mediaItems: [item], selectedMedia: item });
    expect(wrapper.find(".mf-thumb-item-active").exists()).toBe(true);
  });

  it("emits selectMedia on item click", async () => {
    const item = makeMediaItem({ vuekey: "click1" });
    const wrapper = mountComponent({ mediaItems: [item] });
    await wrapper.find(".mf-thumb-item").trigger("click");
    expect(wrapper.emitted("selectMedia")).toBeTruthy();
    expect(wrapper.emitted("selectMedia")![0][0]).toEqual(item);
  });

  it("emits deleteMedia on trash button click", async () => {
    const item = makeMediaItem({ vuekey: "del1" });
    const wrapper = mountComponent({ mediaItems: [item] });
    await wrapper.find(".mf-btn-delete").trigger("click");
    expect(wrapper.emitted("deleteMedia")).toBeTruthy();
    expect(wrapper.emitted("deleteMedia")![0][0]).toEqual(item);
  });

  it("shows transient error state with warning icon", () => {
    const item = makeMediaItem({ name: "err.jpg", vuekey: "te1", errorType: "transient" });
    const wrapper = mountComponent({ mediaItems: [item] });
    expect(wrapper.find(".fa-triangle-exclamation").exists()).toBe(true);
    expect(wrapper.text()).toContain("Temporarily unavailable");
  });

  it("shows not-found error state with ban icon", () => {
    const item = makeMediaItem({ name: "gone.jpg", vuekey: "nf1", errorType: "not-found" });
    const wrapper = mountComponent({ mediaItems: [item] });
    expect(wrapper.find(".fa-ban").exists()).toBe(true);
    expect(wrapper.text()).toContain("Media not found");
  });

  it("strips GUID prefix from name when isNew is true (substring(36))", () => {
    const guid = "12345678-1234-1234-1234-123456789012";
    const item = makeMediaItem({ name: `${guid}myfile.jpg`, vuekey: "new1", isNew: true });
    const wrapper = mountComponent({ mediaItems: [item] });
    expect(wrapper.find(".mf-filename").text()).toBe("myfile.jpg");
  });

  it("emits reorder with rearranged array on drop", async () => {
    const a = makeMediaItem({ name: "a.jpg", vuekey: "a" });
    const b = makeMediaItem({ name: "b.jpg", vuekey: "b" });
    const c = makeMediaItem({ name: "c.jpg", vuekey: "c" });
    const items = [a, b, c];
    const wrapper = mountComponent({ mediaItems: items, allowMultiple: true });

    const thumbItems = wrapper.findAll(".mf-thumb-item");

    // Simulate drag-and-drop: dragstart on first item, then drop on third
    await thumbItems[0].trigger("dragstart");
    await thumbItems[2].trigger("drop");

    expect(wrapper.emitted("reorder")).toBeTruthy();
    const reordered = wrapper.emitted("reorder")![0][0] as any[];
    expect(reordered.map((i: any) => i.name)).toEqual(["b.jpg", "c.jpg", "a.jpg"]);
  });

  it("does not allow drag when allowMultiple is false", () => {
    const item = makeMediaItem({ vuekey: "nd1" });
    const wrapper = mountComponent({ mediaItems: [item], allowMultiple: false });
    expect(wrapper.find(".mf-thumb-item").attributes("draggable")).toBe("false");
  });

  it("onDragEnd resets draggedItem (subsequent drop is no-op)", async () => {
    const a = makeMediaItem({ name: "a.jpg", vuekey: "a" });
    const b = makeMediaItem({ name: "b.jpg", vuekey: "b" });
    const wrapper = mountComponent({ mediaItems: [a, b], allowMultiple: true });

    const thumbItems = wrapper.findAll(".mf-thumb-item");
    await thumbItems[0].trigger("dragstart");
    await thumbItems[0].trigger("dragend");
    await thumbItems[1].trigger("drop");
    expect(wrapper.emitted("reorder")).toBeFalsy();
  });

  it("buildMediaUrl appends width/height params (check img src)", () => {
    const item = makeMediaItem({
      name: "img.jpg",
      mime: "image/jpeg",
      url: "/media/img.jpg",
      vuekey: "url1",
    });
    const wrapper = mountComponent({ mediaItems: [item], thumbSize: 200 });
    const img = wrapper.find("img");
    expect(img.attributes("src")).toBe("/media/img.jpg?width=200&height=200");
  });

  it("emits deleteMedia on trash click for transient error item", async () => {
    const item = makeMediaItem({ name: "err.jpg", vuekey: "te2", errorType: "transient" });
    const wrapper = mountComponent({ mediaItems: [item] });
    await wrapper.find(".mf-btn-delete").trigger("click");
    expect(wrapper.emitted("deleteMedia")).toBeTruthy();
    expect(wrapper.emitted("deleteMedia")![0][0]).toEqual(item);
  });

  it("emits deleteMedia on trash click for not-found error item", async () => {
    const item = makeMediaItem({ name: "gone.jpg", vuekey: "nf2", errorType: "not-found" });
    const wrapper = mountComponent({ mediaItems: [item] });
    await wrapper.find(".mf-btn-delete").trigger("click");
    expect(wrapper.emitted("deleteMedia")).toBeTruthy();
    expect(wrapper.emitted("deleteMedia")![0][0]).toEqual(item);
  });

  it("onDragStart sets dataTransfer.effectAllowed when dataTransfer exists", async () => {
    const a = makeMediaItem({ name: "a.jpg", vuekey: "dt1" });
    const b = makeMediaItem({ name: "b.jpg", vuekey: "dt2" });
    const wrapper = mountComponent({ mediaItems: [a, b], allowMultiple: true });
    const thumbItems = wrapper.findAll(".mf-thumb-item");

    // Create a mock dataTransfer object
    const dataTransfer = { effectAllowed: "" };
    await thumbItems[0].trigger("dragstart", { dataTransfer });
    expect(dataTransfer.effectAllowed).toBe("move");
  });
});
