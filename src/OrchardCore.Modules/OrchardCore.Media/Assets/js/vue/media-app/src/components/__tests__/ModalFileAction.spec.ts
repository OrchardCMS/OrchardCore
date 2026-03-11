import { describe, expect, it, vi } from "vitest";
import { mount, VueWrapper } from "@vue/test-utils";
import ModalFileAction from "../ModalFileAction.vue";
import { FileAction, IFileLibraryItemDto, IRenameFileLibraryItemDto } from "@bloom/media/interfaces";
import { useLocalizations } from "@bloom/helpers/localizations";
import { useEventBusService } from "../../services/EventBusService";
import { VueFinalModal, createVfm } from "vue-final-modal";

// Mock FileDataService
vi.mock("@bloom/media/api/file-data-service", () => ({
  FileDataService: vi.fn().mockImplementation(() => ({
    getFileItem: vi.fn().mockResolvedValue({} as IFileLibraryItemDto),
    moveMedia: vi.fn().mockResolvedValue(undefined),
    moveMediaList: vi.fn().mockResolvedValue(undefined),
    deleteMedia: vi.fn().mockResolvedValue(undefined),
    deleteMediaList: vi.fn().mockResolvedValue(undefined),
    deleteFolder: vi.fn().mockResolvedValue(undefined),
    copyMedia: vi.fn().mockResolvedValue({} as IFileLibraryItemDto),
    createFolder: vi.fn().mockResolvedValue({} as IFileLibraryItemDto),
    listAllItems: vi.fn().mockResolvedValue([]),
    getFolders: vi.fn().mockResolvedValue([]),
    getMediaItems: vi.fn().mockResolvedValue([]),
  })),
}));

vi.mock("@bloom/services/notifications/notifier", () => ({
  notify: vi.fn(),
  NotificationMessage: vi.fn().mockImplementation((data: any) => data), // eslint-disable-line @typescript-eslint/no-explicit-any
}));

useEventBusService();

const { setTranslations } = useLocalizations();
setTranslations({
  Filename: "Filename",
  SelectFolder: "Select a folder",
  Cancel: "Cancel",
  ValidationFolderRequired: "Folder is required",
  ValidationFilenameRequired: "File name is required",
  ValidationFileExtensionRequired: "File extension is not allowed",
});

const vfm = createVfm();

const createFileItem = (): IRenameFileLibraryItemDto => ({
  filePath: "/Images/photo1.jpg",
  directoryPath: "/Images",
  name: "photo1.jpg",
  newName: "",
  isDirectory: false,
  size: 896,
  url: "/media/Images/photo1.jpg",
});

/**
 * Access internal setup state.
 */
const ss = (wrapper: VueWrapper) => {
  return (wrapper.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
};

const mountComponent = (action: FileAction) => {
  return mount(ModalFileAction, {
    props: {
      title: action === FileAction.Rename ? "Rename file" : action === FileAction.Copy ? "Copy file" : "Move file",
      modalName: "test-modal",
      action,
      fileItem: createFileItem(),
    },
    slots: { default: "<p>test slot</p>", submit: "OK" },
    global: {
      plugins: [vfm],
      stubs: {
        "fa-icon": true,
        "p-treeselect": true,
        teleport: true,
      },
    },
  });
};

describe("ModalFileAction", () => {
  it("renders with Rename action and file name is set in inputValue", () => {
    const wrapper = mountComponent(FileAction.Rename);
    expect(ss(wrapper).fileActionEntry.inputValue).toBe("photo1.jpg");
  });

  it("renders with Copy action and inputValue is empty", () => {
    const wrapper = mountComponent(FileAction.Copy);
    expect(ss(wrapper).fileActionEntry.inputValue).toBe("");
  });

  it("renders with Move action and inputValue is empty", () => {
    const wrapper = mountComponent(FileAction.Move);
    expect(ss(wrapper).fileActionEntry.inputValue).toBe("");
  });

  it("onPressEnter clicks btn-submit", () => {
    const clickMock = vi.fn();
    vi.spyOn(document, "getElementById").mockReturnValue({
      click: clickMock,
    } as any); // eslint-disable-line @typescript-eslint/no-explicit-any

    const wrapper = mountComponent(FileAction.Rename);
    ss(wrapper).onPressEnter();

    expect(document.getElementById).toHaveBeenCalledWith("btn-submit");
    expect(clickMock).toHaveBeenCalled();
    vi.restoreAllMocks();
  });

  it("validate with Copy action and empty inputValue shows ValidationFolderRequired error", () => {
    const wrapper = mountComponent(FileAction.Copy);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Copy, file: createFileItem(), inputValue: "" });

    expect(state.errorMessage).toBe("Folder is required");
  });

  it("validate with Copy action and null inputValue shows ValidationFolderRequired error", () => {
    const wrapper = mountComponent(FileAction.Copy);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Copy, file: createFileItem(), inputValue: null });

    expect(state.errorMessage).toBe("Folder is required");
  });

  it("validate with Copy action and empty object inputValue shows ValidationFolderRequired error", () => {
    const wrapper = mountComponent(FileAction.Copy);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Copy, file: createFileItem(), inputValue: {} });

    expect(state.errorMessage).toBe("Folder is required");
  });

  it("validate with Copy action and valid inputValue emits confirm", () => {
    const wrapper = mountComponent(FileAction.Copy);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Copy, file: createFileItem(), inputValue: "/Documents" });

    expect(wrapper.emitted("confirm")).toBeTruthy();
    expect(wrapper.emitted("confirm")![0][0]).toEqual(
      expect.objectContaining({ action: FileAction.Copy, inputValue: "/Documents" }),
    );
  });

  it("validate with Move action and empty inputValue shows ValidationFolderRequired error", () => {
    const wrapper = mountComponent(FileAction.Move);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Move, file: createFileItem(), inputValue: "" });

    expect(state.errorMessage).toBe("Folder is required");
  });

  it("validate with Move action and valid inputValue emits confirm", () => {
    const wrapper = mountComponent(FileAction.Move);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Move, file: createFileItem(), inputValue: "/Documents" });

    expect(wrapper.emitted("confirm")).toBeTruthy();
    expect(wrapper.emitted("confirm")![0][0]).toEqual(
      expect.objectContaining({ action: FileAction.Move, inputValue: "/Documents" }),
    );
  });

  it("validate with Rename action and empty inputValue shows ValidationFilenameRequired error", () => {
    const wrapper = mountComponent(FileAction.Rename);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Rename, file: createFileItem(), inputValue: "" });

    expect(state.errorMessage).toBe("File name is required");
  });

  it("validate with Rename action and null inputValue shows ValidationFilenameRequired error", () => {
    const wrapper = mountComponent(FileAction.Rename);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Rename, file: createFileItem(), inputValue: null });

    expect(state.errorMessage).toBe("File name is required");
  });

  it("validate with Rename action and invalid extension shows ValidationFileExtensionRequired error", () => {
    const wrapper = mountComponent(FileAction.Rename);
    const state = ss(wrapper);

    // "noextension" has no dot, so isValidFileExtension returns false
    state.validate({ action: FileAction.Rename, file: createFileItem(), inputValue: "noextension" });

    expect(state.errorMessage).toBe("File extension is not allowed");
  });

  it("validate with Rename action and valid name emits confirm", () => {
    const wrapper = mountComponent(FileAction.Rename);
    const state = ss(wrapper);

    state.validate({ action: FileAction.Rename, file: createFileItem(), inputValue: "newname.jpg" });

    expect(wrapper.emitted("confirm")).toBeTruthy();
    expect(wrapper.emitted("confirm")![0][0]).toEqual(
      expect.objectContaining({ action: FileAction.Rename, inputValue: "newname.jpg" }),
    );
  });
});
