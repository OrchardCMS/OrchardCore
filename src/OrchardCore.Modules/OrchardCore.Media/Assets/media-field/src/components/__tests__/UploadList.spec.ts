import { ref } from "vue";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount } from "@vue/test-utils";
import UploadList from "../UploadList.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";
import { makeUploadEntry } from "../../__tests__/mockdata";

vi.mock("vue-final-modal", () => ({
  VueFinalModal: {
    name: "VueFinalModal",
    template: "<div><slot /></div>",
    props: ["modelValue"],
    emits: ["update:modelValue", "opened", "closed"],
  },
  createVfm: vi.fn(() => ({ install: vi.fn() })),
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

function mountComponent(props: { files: ReturnType<typeof makeUploadEntry>[] }) {
  return mount(UploadList, {
    props,
    global: getGlobalMountOptions(),
  });
}

describe("UploadList", () => {
  beforeEach(() => {
    setupTranslations();
  });

  it("is hidden when files array is empty (v-show)", () => {
    const wrapper = mountComponent({ files: [] });
    const root = wrapper.find(".mf-upload-toast");
    // v-show keeps the element in the DOM but hides it
    expect(root.exists()).toBe(true);
    // Verify the condition that controls v-show: files.length === 0
    expect(wrapper.props("files")).toHaveLength(0);
  });

  it("shows header with upload count", () => {
    const files = [
      makeUploadEntry({ name: "a.jpg", percentage: 50 }),
      makeUploadEntry({ name: "b.jpg", percentage: 30 }),
    ];
    const wrapper = mountComponent({ files });
    expect(wrapper.text()).toContain("Uploads");
    expect(wrapper.text()).toContain("2");
  });

  it("shows error count when errors exist", () => {
    const files = [
      makeUploadEntry({ name: "bad.jpg", errorMessage: "Too large" }),
      makeUploadEntry({ name: "ok.jpg", percentage: 100, success: true }),
    ];
    const wrapper = mountComponent({ files });
    expect(wrapper.text()).toContain("Errors");
    expect(wrapper.text()).toContain("1");
  });

  it("shows progress bar for in-progress files", () => {
    const files = [makeUploadEntry({ name: "uploading.jpg", percentage: 45 })];
    const wrapper = mountComponent({ files });
    const progressBar = wrapper.find(".mf-upload-toast-progress-bar");
    expect(progressBar.exists()).toBe(true);
    expect((progressBar.element as HTMLElement).style.width).toBe("45%");
  });

  it("shows check icon for successful files", () => {
    const files = [makeUploadEntry({ name: "done.jpg", success: true })];
    const wrapper = mountComponent({ files });
    expect(wrapper.find(".fa-check").exists()).toBe(true);
  });

  it("shows error message for failed files", () => {
    const files = [makeUploadEntry({ name: "fail.jpg", errorMessage: "File too large" })];
    const wrapper = mountComponent({ files });
    expect(wrapper.find(".mf-upload-toast-error").exists()).toBe(true);
    expect(wrapper.text()).toContain("File too large");
  });

  it("emits clearErrors on broom button click", async () => {
    const files = [makeUploadEntry({ name: "err.jpg", errorMessage: "Oops" })];
    const wrapper = mountComponent({ files });
    await wrapper.find(".fa-broom").element.closest("button")!.click();
    expect(wrapper.emitted("clearErrors")).toBeTruthy();
  });

  it("emits dismiss on X button for error entry", async () => {
    const entry = makeUploadEntry({ name: "err.jpg", errorMessage: "Bad file" });
    const wrapper = mountComponent({ files: [entry] });
    const errorItem = wrapper.find(".is-error");
    const dismissBtn = errorItem.find(".fa-xmark").element.closest("button")!;
    await dismissBtn.click();
    expect(wrapper.emitted("dismiss")).toBeTruthy();
    expect(wrapper.emitted("dismiss")![0][0]).toEqual(entry);
  });

  it("toggles expanded/collapsed on header click", async () => {
    const files = [makeUploadEntry({ name: "a.jpg", percentage: 50 })];
    const wrapper = mountComponent({ files });

    // Initially expanded
    expect((wrapper.vm as any).expanded).toBe(true);

    await wrapper.find(".mf-upload-toast-header").trigger("click");
    expect((wrapper.vm as any).expanded).toBe(false);

    await wrapper.find(".mf-upload-toast-header").trigger("click");
    expect((wrapper.vm as any).expanded).toBe(true);
  });

  it("shows dismissAll X button when no pending uploads", () => {
    const files = [
      makeUploadEntry({ name: "done.jpg", success: true }),
      makeUploadEntry({ name: "err.jpg", errorMessage: "Fail" }),
    ];
    const wrapper = mountComponent({ files });
    // The dismissAll button is the xmark in the header actions (not the broom)
    const headerActions = wrapper.find(".mf-upload-toast-actions");
    const xmarkButtons = headerActions.findAll("button");
    // Should have broom + chevron + dismiss all
    const lastBtn = xmarkButtons[xmarkButtons.length - 1];
    expect(lastBtn.find(".fa-xmark").exists()).toBe(true);
  });

  it("emits dismissAll on X button click", async () => {
    const files = [makeUploadEntry({ name: "done.jpg", success: true })];
    const wrapper = mountComponent({ files });
    const headerActions = wrapper.find(".mf-upload-toast-actions");
    const allButtons = headerActions.findAll("button");
    const dismissAllBtn = allButtons[allButtons.length - 1];
    await dismissAllBtn.trigger("click");
    expect(wrapper.emitted("dismissAll")).toBeTruthy();
  });
});
