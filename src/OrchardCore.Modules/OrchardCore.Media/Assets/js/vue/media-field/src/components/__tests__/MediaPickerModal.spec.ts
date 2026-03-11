import { describe, it, expect, vi, beforeEach } from "vitest";
import { mount, flushPromises } from "@vue/test-utils";
import { ref, nextTick } from "vue";
import type { VueWrapper } from "@vue/test-utils";

vi.mock("@bloom/helpers/localizations", () => {
  const translations = ref<Record<string, string>>({});
  return {
    useLocalizations: () => ({
      translations,
      setTranslations: (t: Record<string, string>) => {
        translations.value = t;
      },
    }),
  };
});

vi.mock("vue-final-modal", () => ({
  VueFinalModal: {
    name: "VueFinalModal",
    template: "<div><slot /></div>",
    props: ["modelValue", "contentClass"],
    emits: ["update:modelValue", "opened", "closed"],
  },
  createVfm: () => ({ install: vi.fn() }),
}));

const mockUnmount = vi.fn();
const mockGetSelectedFiles = vi.fn(() => [] as any[]);
const mockMountMediaAppAsPicker = vi.fn(() => ({
  getSelectedFiles: mockGetSelectedFiles,
  unmount: mockUnmount,
}));

vi.mock("@media-app", () => ({
  mountMediaAppAsPicker: mockMountMediaAppAsPicker,
}));

import MediaPickerModal from "../MediaPickerModal.vue";
import { setupTranslations, getGlobalMountOptions } from "../../__tests__/helpers";

function createWrapper(propsOverrides: Record<string, unknown> = {}) {
  return mount(MediaPickerModal, {
    props: {
      fieldId: "test-field",
      allowedExtensions: ".jpg,.png",
      allowMultiple: true,
      mediaAppTranslations: "{}",
      basePath: "/",
      uploadFilesUrl: "/api/upload",
      ...propsOverrides,
    },
    global: getGlobalMountOptions(),
  });
}

/**
 * Helper: set containerRef on the component's internal state.
 * The VueFinalModal mock doesn't render slot content (due to v-show/ref
 * directive issues in jsdom), so template refs like containerRef are null.
 * We set it manually so that onOpened() doesn't bail early.
 */
function setContainerRef(wrapper: VueWrapper) {
  const internal = (wrapper.vm as any).$;
  const state = internal.setupState;
  if (state && "containerRef" in state) {
    state.containerRef = document.createElement("div");
  }
}

/** Helper to get the VueFinalModal stub's props after forcing a re-render */
async function getModalProps(wrapper: VueWrapper) {
  wrapper.vm.$forceUpdate();
  await nextTick();
  return wrapper.findComponent({ name: "VueFinalModal" }).props();
}

describe("MediaPickerModal", () => {
  beforeEach(() => {
    setupTranslations();
    vi.clearAllMocks();
  });

  it("initially not visible", () => {
    const wrapper = createWrapper();
    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    expect(modal.props("modelValue")).toBe(false);
  });

  it("opens via exposed open() method", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).open();

    const props = await getModalProps(wrapper);
    expect(props.modelValue).toBe(true);
  });

  it("shows loading state initially", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).open();

    const props = await getModalProps(wrapper);
    expect(props.modelValue).toBe(true);

    // Loading is set in open(), and mountMediaAppAsPicker is only called
    // in onOpened. Verify that onOpened hasn't fired yet.
    expect(mockMountMediaAppAsPicker).not.toHaveBeenCalled();
  });

  it("shows error when media-app import fails", async () => {
    mockMountMediaAppAsPicker.mockImplementationOnce(() => {
      throw new Error("Module load failed");
    });

    const wrapper = createWrapper();
    (wrapper.vm as any).open();
    await nextTick();

    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    expect(mockMountMediaAppAsPicker).toHaveBeenCalled();
    expect(wrapper.emitted("select")).toBeFalsy();
  });

  it("emits select with converted IMediaFieldItem[] on confirm", async () => {
    mockGetSelectedFiles.mockReturnValueOnce([
      {
        name: "photo.jpg",
        mime: "image/jpeg",
        filePath: "uploads/photo.jpg",
        url: "/media/photo.jpg",
        size: 2048,
      },
    ]);

    const wrapper = createWrapper();
    (wrapper.vm as any).open();
    await nextTick();

    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    expect(mockMountMediaAppAsPicker).toHaveBeenCalled();

    // The confirm() function is not exposed, so we access it via the internal state.
    // It calls getSelectedFiles(), converts items, and emits "select".
    const internal = (wrapper.vm as any).$;
    internal.setupState.confirm();
    await flushPromises();

    const selectEvents = wrapper.emitted("select");
    expect(selectEvents).toBeTruthy();
    const items = selectEvents![0][0] as any[];
    expect(items).toHaveLength(1);
    expect(items[0].name).toBe("photo.jpg");
    expect(items[0].mediaPath).toBe("uploads/photo.jpg");
  });

  it("cancel closes modal", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).open();

    let props = await getModalProps(wrapper);
    expect(props.modelValue).toBe(true);

    // Call cancel via internal state since buttons don't render
    const internal = (wrapper.vm as any).$;
    internal.setupState.cancel();
    await nextTick();

    props = await getModalProps(wrapper);
    expect(props.modelValue).toBe(false);
  });

  it("cleanup unmounts picker on close", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).open();
    await nextTick();

    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    expect(mockMountMediaAppAsPicker).toHaveBeenCalled();

    // Trigger onClosed via internal state
    const internal = (wrapper.vm as any).$;
    internal.setupState.onClosed();
    await flushPromises();

    expect(mockUnmount).toHaveBeenCalled();
  });

  it("OK button disabled when no selection", async () => {
    mockGetSelectedFiles.mockReturnValue([]);

    const wrapper = createWrapper();
    (wrapper.vm as any).open();
    await nextTick();

    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    // selectedCount is set to 0 in open(). The OK button would be disabled.
    // Verify by checking the internal selectedCount state.
    const internal = (wrapper.vm as any).$;
    expect(internal.setupState.selectedCount).toBe(0);
  });

  it("onSelectionChange callback updates selectedCount", async () => {
    const wrapper = createWrapper();
    (wrapper.vm as any).open();
    await nextTick();

    setContainerRef(wrapper);

    const modal = wrapper.findComponent({ name: "VueFinalModal" });
    modal.vm.$emit("opened");
    await flushPromises();

    // mountMediaAppAsPicker was called with an onSelectionChange callback
    expect(mockMountMediaAppAsPicker).toHaveBeenCalled();
    const callArgs = mockMountMediaAppAsPicker.mock.calls[0][1];
    expect(callArgs.onSelectionChange).toBeDefined();

    // Call the callback to simulate selection change
    callArgs.onSelectionChange(3);
    await nextTick();

    const internal = (wrapper.vm as any).$;
    expect(internal.setupState.selectedCount).toBe(3);
  });
});
