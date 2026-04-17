import { describe, expect, it, vi } from "vitest";
import { mount, VueWrapper } from "@vue/test-utils";
import ModalFolderAction from "../ModalFolderAction.vue";
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { setTranslations } from "@bloom/helpers/localizations";
import { useEventBusService } from "../../services/EventBusService";
import { createVfm } from "vue-final-modal";

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

setTranslations({
  Cancel: "Cancel",
  CreateSubFolder: "Create sub-folder",
  Delete: "Delete",
});

const vfm = createVfm();

/**
 * Access internal setup state.
 */
const ss = (wrapper: VueWrapper) => {
  return (wrapper.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
};

describe("ModalFolderAction", () => {
  it("onPressEnter clicks btn-submit", () => {
    const clickMock = vi.fn();
    vi.spyOn(document, "getElementById").mockReturnValue({
      click: clickMock,
    } as any); // eslint-disable-line @typescript-eslint/no-explicit-any

    const wrapper = mount(ModalFolderAction, {
      props: {
        title: "Folder action",
        modalName: "folder-action-modal",
        folder: { name: "Test", filePath: "/Test", directoryPath: "/Test", isDirectory: true } as IFileLibraryItemDto,
      },
      slots: { default: "<p>test slot</p>", submit: "OK" },
      global: {
        plugins: [vfm],
        stubs: {
          "fa-icon": true,
          teleport: true,
        },
      },
    });

    ss(wrapper).onPressEnter();

    expect(document.getElementById).toHaveBeenCalledWith("btn-submit");
    expect(clickMock).toHaveBeenCalled();
    vi.restoreAllMocks();
  });
});
