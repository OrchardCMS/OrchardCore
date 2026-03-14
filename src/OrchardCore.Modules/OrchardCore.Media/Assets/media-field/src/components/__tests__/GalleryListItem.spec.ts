import { ref } from "vue";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount } from "@vue/test-utils";
import GalleryListItem from "../GalleryListItem.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem } from "../../__tests__/mockdata";

vi.mock("vue-final-modal", () => ({
  VueFinalModal: { name: "VueFinalModal", template: "<div><slot /></div>", props: ["modelValue"] },
  createVfm: () => ({ install: vi.fn() }),
}));

vi.mock("../../composables/useLocalizations", () => {
  const translations = ref<Record<string, string>>({});
  return {
    useLocalizations: () => ({
      translations,
      setTranslations: (t: Record<string, string>) => { translations.value = t; },
    }),
  };
});

function mountComponent(props: Partial<InstanceType<typeof GalleryListItem>["$props"]> = {}) {
  return mount(GalleryListItem, {
    props: {
      media: makeMediaItem(),
      allowMultiple: true,
      allowMediaText: false,
      allowAnchors: false,
      ...props,
    },
    global: getGlobalMountOptions(),
  });
}

describe("GalleryListItem", () => {
  beforeEach(() => {
    setupTranslations();
  });

  it("renders media name for normal item", () => {
    const wrapper = mountComponent({ media: makeMediaItem({ name: "photo.png" }) });
    expect(wrapper.text()).toContain("photo.png");
  });

  it("shows 'Temporarily unavailable' for transient error", () => {
    const wrapper = mountComponent({ media: makeMediaItem({ errorType: "transient" }) });
    expect(wrapper.text()).toContain("Temporarily unavailable");
  });

  it("shows 'Media not found' for not-found error", () => {
    const wrapper = mountComponent({ media: makeMediaItem({ errorType: "not-found" }) });
    expect(wrapper.text()).toContain("Media not found");
  });

  it("hidden when isRemoved is true", () => {
    const wrapper = mountComponent({ media: makeMediaItem({ isRemoved: true }) });
    // v-if removes the element from DOM
    expect(wrapper.find(".mf-gallery-list-item").exists()).toBe(false);
  });

  it("shows image thumbnail for image mime", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ mime: "image/png", url: "/media/pic.png" }),
    });
    expect(wrapper.find("img").exists()).toBe(true);
  });

  it("shows icon for non-image", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ mime: "application/pdf", name: "doc.pdf" }),
    });
    expect(wrapper.find("img").exists()).toBe(false);
    expect(wrapper.find(".mf-gallery-list-preview i").exists()).toBe(true);
  });

  it("shows mediaText button when allowMediaText and no error", () => {
    const wrapper = mountComponent({
      allowMediaText: true,
      media: makeMediaItem(),
    });
    const btn = wrapper.find('button[title="Edit media text"]');
    expect(btn.exists()).toBe(true);
  });

  it("hides mediaText button when error", () => {
    const wrapper = mountComponent({
      allowMediaText: true,
      media: makeMediaItem({ errorType: "transient" }),
    });
    const btn = wrapper.find('button[title="Edit media text"]');
    expect(btn.exists()).toBe(false);
  });

  it("shows anchor button for images when allowAnchors", () => {
    const wrapper = mountComponent({
      allowAnchors: true,
      media: makeMediaItem({ mime: "image/jpeg", url: "/media/img.jpg" }),
    });
    const btn = wrapper.find('button[title="Set anchor"]');
    expect(btn.exists()).toBe(true);
  });

  it("hides anchor button for non-images", () => {
    const wrapper = mountComponent({
      allowAnchors: true,
      media: makeMediaItem({ mime: "application/pdf", name: "doc.pdf" }),
    });
    const btn = wrapper.find('button[title="Set anchor"]');
    expect(btn.exists()).toBe(false);
  });

  it("emits editMediaText on button click", async () => {
    const media = makeMediaItem();
    const wrapper = mountComponent({ allowMediaText: true, media });
    await wrapper.find('button[title="Edit media text"]').trigger("click");
    expect(wrapper.emitted("editMediaText")).toBeTruthy();
    expect(wrapper.emitted("editMediaText")![0][0]).toEqual(media);
  });

  it("emits editAnchor on button click", async () => {
    const media = makeMediaItem({ mime: "image/jpeg", url: "/media/img.jpg" });
    const wrapper = mountComponent({ allowAnchors: true, media });
    await wrapper.find('button[title="Set anchor"]').trigger("click");
    expect(wrapper.emitted("editAnchor")).toBeTruthy();
    expect(wrapper.emitted("editAnchor")![0][0]).toEqual(media);
  });

  it("emits deleteMedia on trash click", async () => {
    const media = makeMediaItem();
    const wrapper = mountComponent({ media });
    await wrapper.find('button[title="Remove"]').trigger("click");
    expect(wrapper.emitted("deleteMedia")).toBeTruthy();
    expect(wrapper.emitted("deleteMedia")![0][0]).toEqual(media);
  });

  it("is draggable when allowMultiple", () => {
    const wrapper = mountComponent({ allowMultiple: true });
    expect(wrapper.find(".mf-gallery-list-item").attributes("draggable")).toBe("true");
  });

  it("is not draggable when allowMultiple is false", () => {
    const wrapper = mountComponent({ allowMultiple: false });
    expect(wrapper.find(".mf-gallery-list-item").attributes("draggable")).toBe("false");
  });
});
