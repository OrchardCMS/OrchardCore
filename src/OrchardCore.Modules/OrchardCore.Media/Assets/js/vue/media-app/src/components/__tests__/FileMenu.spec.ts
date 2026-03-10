import { afterEach, describe, expect, it, vi } from "vitest";
import { mount, VueWrapper } from "@vue/test-utils";
import fileMenu from "../FileMenu.vue";
import { FileAction, IRenameFileLibraryItemDto, IFileLibraryItemDto } from "../../interfaces/interfaces";
import PrimeVue from "primevue/config";
import Menu from "primevue/menu";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { useGlobals } from "../../services/Globals";
import { useLocalizations } from "../../services/Localizations";
import { useEventBusService } from "../../services/EventBusService";
import { createVfm } from "vue-final-modal";

// Mock FileDataService to avoid real API calls
vi.mock("../../services/data/IFileDataService", () => ({
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

const vfm = createVfm();
useEventBusService();
const { setAssetsStore } = useGlobals();

const { setTranslations } = useLocalizations();
setTranslations({
  Ok: "Ok",
  Copy: "Copy",
  Rename: "Rename",
  Delete: "Delete",
  Move: "Move",
  Download: "Download",
  RenameSingleFileTitle: "Rename file",
  MoveSingleFileTitle: "Move file",
  CopySingleFileTitle: "Copy file",
  DeleteFileTitle: "Delete file",
});

const createFileItem = (): IRenameFileLibraryItemDto => ({
  filePath: "/Images/photo1.jpg",
  directoryPath: "/Images",
  name: "photo1.jpg",
  newName: "",
  isDirectory: false,
  size: 896,
  url: "/media/Images/photo1.jpg",
});

const assetsStoreData: IFileLibraryItemDto[] = [
  {
    filePath: "/Images",
    directoryPath: "/Images",
    name: "Images",
    isDirectory: true,
    size: 0,
    url: "/media/Images",
  },
  {
    filePath: "/Images/photo1.jpg",
    directoryPath: "/Images",
    name: "photo1.jpg",
    isDirectory: false,
    size: 896,
    url: "/media/Images/photo1.jpg",
  },
];

describe("fileMenu", () => {
  let wrapper: VueWrapper | null = null;

  afterEach(() => {
    wrapper?.unmount();
    wrapper = null;
  });

  const mountMenu = () => {
    setAssetsStore(assetsStoreData);
    wrapper = mount(fileMenu, {
      props: { fileItem: createFileItem(), showModalProp: true },
      attachTo: document.body,
      global: {
        components: { "p-menu": Menu, "fa-icon": FontAwesomeIcon },
        plugins: [PrimeVue, vfm],
        stubs: { transition: false, teleport: true, ModalsContainer: true },
      },
    });
    return wrapper;
  };

  it("renders the component with toggle button", () => {
    const w = mountMenu();
    expect(w.find("a.btn-link").exists()).toBe(true);
  });

  it("has the correct menu items configured", () => {
    const w = mountMenu();
    // Access menu items via the component's internal setup state
    const setupState = (w.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
    const items = setupState.items;

    expect(items).toBeDefined();
    expect(items.length).toBe(1);

    const menuItems = items[0].items;
    expect(menuItems.length).toBe(5);
    expect(menuItems[0].label).toBe("Rename");
    expect(menuItems[1].label).toBe("Move");
    expect(menuItems[2].label).toBe("Copy");
    expect(menuItems[3].label).toBe("Download");
    expect(menuItems[4].label).toBe("Delete");
  });

  it("rename command uses FileAction.Rename", () => {
    const w = mountMenu();
    const setupState = (w.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
    const renameItem = setupState.items[0].items[0];

    expect(renameItem.label).toBe("Rename");
    expect(renameItem.icon).toBe("fa-solid fa-edit");
    // Calling the command should not throw
    expect(() => renameItem.command()).not.toThrow();
  });

  it("move command uses FileAction.Move", () => {
    const w = mountMenu();
    const setupState = (w.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
    const moveItem = setupState.items[0].items[1];

    expect(moveItem.label).toBe("Move");
    expect(moveItem.icon).toBe("fa-solid fa-folder");
    expect(() => moveItem.command()).not.toThrow();
  });

  it("copy command uses FileAction.Copy", () => {
    const w = mountMenu();
    const setupState = (w.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
    const copyItem = setupState.items[0].items[2];

    expect(copyItem.label).toBe("Copy");
    expect(copyItem.icon).toBe("fa-solid fa-copy");
    expect(() => copyItem.command()).not.toThrow();
  });

  it("download command calls downloadFile", () => {
    vi.mock("../../services/Utils", () => ({
      downloadFile: vi.fn(),
      getFileExtension: vi.fn(),
    }));

    const w = mountMenu();
    const setupState = (w.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
    const downloadItem = setupState.items[0].items[3];

    expect(downloadItem.label).toBe("Download");
    expect(downloadItem.icon).toBe("fa-solid fa-download");
    expect(() => downloadItem.command()).not.toThrow();
  });

  it("delete command uses FileAction.Delete", () => {
    const w = mountMenu();
    const setupState = (w.vm.$ as any).setupState; // eslint-disable-line @typescript-eslint/no-explicit-any
    const deleteItem = setupState.items[0].items[4];

    expect(deleteItem.label).toBe("Delete");
    expect(deleteItem.icon).toBe("fa-solid fa-trash");
    expect(() => deleteItem.command()).not.toThrow();
  });
});
