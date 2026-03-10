import { afterEach, beforeEach, describe, expect, it } from "vitest";
import { mount, VueWrapper } from "@vue/test-utils";
import ModalFilePicker from "../ModalFilePicker.vue";
import { TreeNode } from "@bloom/media/interfaces";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { createVfm } from "vue-final-modal";
import { useLocalizations } from "@bloom/helpers/localizations";

const translationsData = {
  Ok: "Ok",
  Cancel: "Cancel",
  SelectFile: "Select a file",
  ValidationFilenameRequired: "File name is required",
  ValidationFileExtensionRequired: "File extension is not allowed",
};

const { setTranslations, translations } = useLocalizations();
setTranslations(translationsData);
const t = translations;

describe("ModalFilePicker", () => {
  let wrapper: VueWrapper | null = null;

  afterEach(() => {
    wrapper?.unmount();
  });

  beforeEach(() => {
    const vfm = createVfm();
    wrapper = mount(ModalFilePicker, {
      props: {
        title: "Test Title",
        modalName: "test-modal",
        files: [{ key: "test", label: "Test", data: {} }] as TreeNode[],
        showModalProp: true,
        allowedExtensions: [".gc3"],
      },
      global: {
        plugins: [vfm],
        stubs: {
          "fa-icon": FontAwesomeIcon,
          "p-treeselect": true,
        },
      },
    });
  });

  it("receives the correct title and modal name props", async () => {
    await wrapper?.vm.$nextTick();

    expect(wrapper?.props("title")).toBe("Test Title");
    expect(wrapper?.props("modalName")).toBe("test-modal");
    expect(wrapper?.props("allowedExtensions")).toEqual([".gc3"]);
    expect(wrapper?.props("files")).toEqual([{ key: "test", label: "Test", data: {} }]);
  });

  it("validate file extension when input value is empty", async () => {
    wrapper?.vm.validate({ inputValue: null });
    expect(wrapper?.vm.errorMessage).toBe(t.ValidationFilenameRequired);
    wrapper?.vm.onFileChange();

    wrapper?.vm.validate({ inputValue: { "test.gc4": true } });
    expect(wrapper?.vm.errorMessage).toBe(t.ValidationFileExtensionRequired);
    wrapper?.vm.onFileChange();

    wrapper?.vm.validate({ inputValue: { "test.gc3": true } });
    expect(wrapper?.vm.errorMessage).toBe("");
  });
});
