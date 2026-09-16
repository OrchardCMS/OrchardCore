import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount } from "@vue/test-utils";
import GalleryCardItem from "../GalleryCardItem.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeMediaItem } from "../../__tests__/mockdata";

vi.mock("vue-final-modal", () => ({
  VueFinalModal: { name: "VueFinalModal", template: "<div><slot /></div>", props: ["modelValue"] },
  createVfm: () => ({ install: vi.fn() }),
}));

vi.mock("@bloom/helpers/localizations", () => {
  const store: Record<string, string> = {};
  return {
    getTranslations: () => store,
    setTranslations: (t: Record<string, string>) => Object.assign(store, t),
  };
});

function mountComponent(props: Partial<InstanceType<typeof GalleryCardItem>["$props"]> = {}) {
  return mount(GalleryCardItem, {
    props: {
      media: makeMediaItem(),
      allowMultiple: true,
      allowMediaText: false,
      allowAnchors: false,
      size: "sm" as const,
      ...props,
    },
    global: getGlobalMountOptions(),
  });
}

describe("GalleryCardItem", () => {
  beforeEach(() => {
    setupTranslations();
  });

  it("renders for normal image item", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ mime: "image/jpeg", url: "/media/photo.jpg" }),
    });
    expect(wrapper.find("img").exists()).toBe(true);
    expect(wrapper.find(".mf-gallery-card").exists()).toBe(true);
  });

  it("renders file icon for non-image", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ mime: "application/pdf", name: "report.pdf" }),
    });
    expect(wrapper.find("img").exists()).toBe(false);
    expect(wrapper.find(".mf-gallery-card-icon i").exists()).toBe(true);
    expect(wrapper.text()).toContain("report.pdf");
  });

  it("shows transient warning", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ errorType: "transient" }),
    });
    expect(wrapper.find(".mf-item-warning").exists()).toBe(true);
    expect(wrapper.text()).toContain("Temporarily unavailable");
  });

  it("shows not-found error", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ errorType: "not-found" }),
    });
    expect(wrapper.find(".mf-item-danger").exists()).toBe(true);
    expect(wrapper.text()).toContain("Media not found");
  });

  it("hidden when isRemoved", () => {
    const wrapper = mountComponent({
      media: makeMediaItem({ isRemoved: true }),
    });
    expect(wrapper.find(".mf-gallery-card-item").exists()).toBe(false);
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

  it("uses 120px thumb size for 'sm'", () => {
    const wrapper = mountComponent({
      size: "sm",
      media: makeMediaItem({ mime: "image/jpeg", url: "/media/img.jpg" }),
    });
    const img = wrapper.find("img");
    expect(img.attributes("src")).toContain("width=120");
  });

  it("uses 240px thumb size for 'lg'", () => {
    const wrapper = mountComponent({
      size: "lg",
      media: makeMediaItem({ mime: "image/jpeg", url: "/media/img.jpg" }),
    });
    const img = wrapper.find("img");
    expect(img.attributes("src")).toContain("width=240");
  });

  it("shows filled comment icon when mediaText exists", () => {
    const wrapper = mountComponent({
      allowMediaText: true,
      media: makeMediaItem({ mediaText: "Alt text here" }),
    });
    expect(wrapper.find(".fa-solid.fa-comment").exists()).toBe(true);
    expect(wrapper.find(".fa-regular.fa-comment").exists()).toBe(false);
  });

  it("shows outline comment icon when no mediaText", () => {
    const wrapper = mountComponent({
      allowMediaText: true,
      media: makeMediaItem({ mediaText: undefined }),
    });
    expect(wrapper.find(".fa-regular.fa-comment").exists()).toBe(true);
    expect(wrapper.find(".fa-solid.fa-comment").exists()).toBe(false);
  });
});
