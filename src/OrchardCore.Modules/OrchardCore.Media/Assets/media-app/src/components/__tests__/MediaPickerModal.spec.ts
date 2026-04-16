import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount, flushPromises } from "@vue/test-utils";
import { nextTick } from "vue";
import type { VueWrapper } from "@vue/test-utils";

vi.mock("vue-final-modal", () => ({
  VueFinalModal: {
    name: "VueFinalModal",
    template: "<div><slot /></div>",
    props: ["modelValue", "contentClass", "contentStyle"],
    emits: ["update:modelValue", "opened", "closed"],
  },
}));

const mockUnmount = vi.fn();
const mockGetSelectedFiles = vi.fn(() => [] as any[]);
const mockClearSelection = vi.fn();

function makeMockHandle() {
  return {
    getSelectedFiles: mockGetSelectedFiles,
    clearSelection: mockClearSelection,
    unmount: mockUnmount,
  };
}

const mockMountPicker = vi.fn((_container: HTMLElement, _config: any) => makeMockHandle());

import MediaPickerModal from "../MediaPickerModal.vue";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";

const defaultConfig = {
  translations: JSON.stringify({ selectMedia: "Select Media", ok: "OK", cancel: "Cancel" }),
  basePath: "/",
  uploadFilesUrl: "/api/upload",
  allowedExtensions: ".jpg,.png",
  allowMultiple: true,
};

function createWrapper(configOverrides: Record<string, unknown> = {}, onResolve?: (files: IFileLibraryItemDto[]) => void) {
  return mount(MediaPickerModal, {
    props: {
      config: { ...defaultConfig, ...configOverrides },
      mountPicker: mockMountPicker,
      onResolve: onResolve ?? vi.fn(),
    },
    global: {
      stubs: {
        VueFinalModal: {
          name: "VueFinalModal",
          template: "<div><slot /></div>",
          props: ["modelValue", "contentClass", "contentStyle"],
          emits: ["update:modelValue", "opened", "closed"],
        },
      },
    },
  });
}

function setContainerRef(wrapper: VueWrapper) {
  const state = (wrapper.vm as any).$.setupState;
  if (state && "pickerContainer" in state) {
    state.pickerContainer = document.createElement("div");
  }
}

async function getModalProps(wrapper: VueWrapper) {
  wrapper.vm.$forceUpdate();
  await nextTick();
  return wrapper.findComponent({ name: "VueFinalModal" }).props();
}

describe("MediaPickerModal", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockMountPicker.mockImplementation((_container: HTMLElement, _config: any) => makeMockHandle());
  });

  it("is initially visible (visible starts true)", async () => {
    const wrapper = createWrapper();
    const props = await getModalProps(wrapper);
    expect(props.modelValue).toBe(true);
  });

  it("renders translated labels from config", async () => {
    const wrapper = createWrapper();
    await nextTick();
    expect(wrapper.text()).toContain("Select Media");
    expect(wrapper.text()).toContain("OK");
    expect(wrapper.text()).toContain("Cancel");
  });

  it("falls back to default labels when translations JSON is empty", async () => {
    const wrapper = createWrapper({ translations: "{}" });
    await nextTick();
    expect(wrapper.text()).toContain("Select Media");
    expect(wrapper.text()).toContain("OK");
    expect(wrapper.text()).toContain("Cancel");
  });

  it("falls back to default labels when translations JSON is invalid", async () => {
    const wrapper = createWrapper({ translations: "not-json" });
    await nextTick();
    expect(wrapper.text()).toContain("Select Media");
  });

  it("calls mountPicker on opened event", async () => {
    const wrapper = createWrapper();
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    expect(mockMountPicker).toHaveBeenCalledTimes(1);
  });

  it("passes config props to mountPicker", async () => {
    const wrapper = createWrapper({ allowMultiple: false, allowedExtensions: ".gif" });
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    const callArgs = mockMountPicker.mock.calls[0][1];
    expect(callArgs.allowMultiple).toBe(false);
    expect(callArgs.allowedExtensions).toBe(".gif");
  });

  it("mountPicker receives onSelectionChange callback", async () => {
    const wrapper = createWrapper();
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    const callArgs = mockMountPicker.mock.calls[0][1];
    expect(typeof callArgs.onSelectionChange).toBe("function");
  });

  it("onSelectionChange updates selectedCount", async () => {
    const wrapper = createWrapper();
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    const callArgs = mockMountPicker.mock.calls[0][1];
    callArgs.onSelectionChange(5);
    await nextTick();

    const internal = (wrapper.vm as any).$.setupState;
    expect(internal.selectedCount).toBe(5);
  });

  it("resolves with empty array and closes on cancel", async () => {
    const onResolve = vi.fn();
    const wrapper = createWrapper({}, onResolve);

    const internal = (wrapper.vm as any).$.setupState;
    internal.cancel();
    await nextTick();

    const props = await getModalProps(wrapper);
    expect(props.modelValue).toBe(false);
    expect(onResolve).toHaveBeenCalledWith([]);
  });

  it("resolves with selected files on confirm", async () => {
    const selectedFiles: IFileLibraryItemDto[] = [
      { name: "photo.jpg", mime: "image/jpeg", filePath: "uploads/photo.jpg", url: "/media/photo.jpg", size: 2048 } as any,
    ];
    mockGetSelectedFiles.mockReturnValue(selectedFiles);

    const onResolve = vi.fn();
    const wrapper = createWrapper({}, onResolve);
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    const internal = (wrapper.vm as any).$.setupState;
    internal.confirm();
    await nextTick();

    expect(onResolve).toHaveBeenCalledWith(selectedFiles);
  });

  it("resolves only once even if cancel and onClosed both fire", async () => {
    const onResolve = vi.fn();
    const wrapper = createWrapper({}, onResolve);

    const internal = (wrapper.vm as any).$.setupState;
    internal.cancel();
    await nextTick();

    internal.onClosed();
    await flushPromises();

    expect(onResolve).toHaveBeenCalledTimes(1);
  });

  it("unmounts picker handle on closed event", async () => {
    const wrapper = createWrapper();
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    const internal = (wrapper.vm as any).$.setupState;
    internal.onClosed();
    await flushPromises();

    expect(mockUnmount).toHaveBeenCalled();
  });

  it("resets selectedCount on closed", async () => {
    const wrapper = createWrapper();
    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    const callArgs = mockMountPicker.mock.calls[0][1];
    callArgs.onSelectionChange(3);
    await nextTick();

    const internal = (wrapper.vm as any).$.setupState;
    expect(internal.selectedCount).toBe(3);

    internal.onClosed();
    await flushPromises();

    expect(internal.selectedCount).toBe(0);
  });

  it("resolves with empty array when container ref is null on opened", async () => {
    const onResolve = vi.fn();
    const wrapper = createWrapper({}, onResolve);

    // do NOT setContainerRef, leaving pickerContainer null
    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    expect(mockMountPicker).not.toHaveBeenCalled();
    expect(onResolve).toHaveBeenCalledWith([]);
  });
});
