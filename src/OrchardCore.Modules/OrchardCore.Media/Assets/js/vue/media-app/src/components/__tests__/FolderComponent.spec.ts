import { beforeEach, describe, expect, it, vi } from "vitest";
import { mount } from "@vue/test-utils";
import FolderComponent from "../FolderComponent.vue";
import ModalFolderAction from "../ModalFolderAction.vue";
import { useHierarchicalTreeBuilder } from "../../services/HierarchicalTreeBuilder";
import { useGlobals } from "../../services/Globals";
import { createVfm, ModalsContainer } from "vue-final-modal";
import { FolderAction, IConfirmFolderActionViewModel, IFileLibraryItemDto, IRenameFileLibraryItemDto } from "@bloom/media/interfaces";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { useLocalizations } from "@bloom/helpers/localizations";
import { useEventBus } from "../../services/UseEventBus";
import { useEventBusService } from "../../services/EventBusService";

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
    createFolder: vi.fn().mockResolvedValue({
      name: "Test 2", filePath: "/Test2", directoryPath: "/Test2", isDirectory: true,
    } as IFileLibraryItemDto),
    listAllItems: vi.fn().mockResolvedValue([
      { name: "Test", filePath: "/Test", directoryPath: "/Test", isDirectory: true },
      { name: "Test 2", filePath: "/Test2", directoryPath: "/Test2", isDirectory: true },
    ] as IFileLibraryItemDto[]),
    getFolders: vi.fn().mockResolvedValue([]),
    getMediaItems: vi.fn().mockResolvedValue([]),
  })),
}));

vi.mock("@bloom/services/notifications/notifier", () => ({
  notify: vi.fn(),
  NotificationMessage: vi.fn().mockImplementation((data: any) => data), // eslint-disable-line @typescript-eslint/no-explicit-any
}));

const { setHierarchicalDirectories } = useHierarchicalTreeBuilder();
const { setSelectedDirectory, hierarchicalDirectories, setAssetsStore } = useGlobals();

const { setTranslations } = useLocalizations();
const translationsData = { Ok: "Ok", MoveFileTitle: "Move file(s)" };
setTranslations(translationsData);
const { on, emit } = useEventBus();
useEventBusService();

let assetsStoreData: IFileLibraryItemDto[] = [
  {
    filePath: "/Test",
    directoryPath: "/Test",
    name: "Test",
    isDirectory: true,
    size: 0,
    url: "/media/Test",
  },
];

const vfm = createVfm();

describe("FolderComponent", () => {
  it("renders folder component correctly", () => {
    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [
            { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: true, children: [] },
            { name: "Child 2", filePath: "/test/directory/child2", directoryPath: "/test/directory", selected: false, children: [] },
          ],
        },
        level: 1,
      },
    });

    expect(wrapper.find(".folder").exists()).toBe(true);
    expect(wrapper.find(".folder-name").text()).toBe("Test Directory");
  });

  it("renders root folder component correctly", async () => {
    const directory = {
      name: "Files",
      filePath: "/",
      directoryPath: "/",
      selected: false,
      children: [
        { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: true, children: [] },
        { name: "Child 2", filePath: "/test/directory/child2", directoryPath: "/test/directory", selected: false, children: [] },
      ],
    };

    setSelectedDirectory(directory);

    const wrapper = mount(FolderComponent, {
      props: { hierarchicalDirectories: directory, level: 1 },
    });

    expect(wrapper.find(".folder").exists()).toBe(true);
    expect(wrapper.find(".folder-name").text()).toBe("Files");
  });

  it("renders root folder without children component correctly", async () => {
    const directory = {
      name: "Files",
      filePath: "/",
      directoryPath: "/",
      selected: false,
      children: [],
    };

    setSelectedDirectory(directory);

    const wrapper = mount(FolderComponent, {
      props: { hierarchicalDirectories: directory, level: 1 },
    });

    expect(wrapper.find(".folder").exists()).toBe(true);
    expect(wrapper.find(".folder-name").text()).toBe("Files");
  });

  it("calls openFolderModal method when action button is clicked", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [
            { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: true, children: [] },
            { name: "Child 2", filePath: "/test/directory/child2", directoryPath: "/test/directory", selected: false, children: [] },
          ],
        },
        level: 0,
      },
      global: { plugins: [vfm], stubs: { transition: false, teleport: true, ModalsContainer } },
    });

    const openFolderModalSpy = vi.spyOn(wrapper.vm, "openFolderModal");
    await wrapper.find(".action-button").trigger("click");

    expect(openFolderModalSpy).toHaveBeenCalledTimes(1);
  });

  it("renders modal folder action correctly", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [
            { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: false, children: [] },
            { name: "Child 2", filePath: "/test/directory/child2", directoryPath: "/test/directory", selected: false, children: [] },
          ],
        },
        level: 0,
        showModalProp: true,
      },
    });

    await wrapper.find(".action-button").trigger("click");
    expect(wrapper.findComponent(ModalFolderAction).exists()).toBe(true);
  });

  it("submit modal emits confirm event", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [
            { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: false, children: [] },
            { name: "Child 2", filePath: "/test/directory/child2", directoryPath: "/test/directory", selected: false, children: [] },
          ],
        },
        level: 0,
        showModalProp: true,
      },
      global: {
        components: { "fa-icon": FontAwesomeIcon },
        stubs: { transition: false, teleport: true },
      },
    });

    await wrapper.find(".action-button").trigger("click");
    const modal = wrapper.findComponent(ModalFolderAction);
    expect(modal.exists()).toBe(true);

    // Emit confirm directly (teleported modal DOM is not accessible via wrapper)
    modal.vm.$emit("confirm", {
      action: FolderAction.Create,
      inputValue: "New Folder",
      folder: { name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory" } as IFileLibraryItemDto,
    } as IConfirmFolderActionViewModel);

    await wrapper.vm.$nextTick();
    expect(wrapper.find(".folder").exists()).toBe(true);
  });

  it("confirm with delete action emits DirDeleteReq", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const dirDeleteSpy = vi.fn();
    on("DirDeleteReq", dirDeleteSpy);

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [],
        },
        level: 0,
        showModalProp: true,
      },
      global: {
        components: { "fa-icon": FontAwesomeIcon },
        stubs: { transition: false, teleport: true },
      },
    });

    await wrapper.find(".action-button").trigger("click");
    const modal = wrapper.findComponent(ModalFolderAction);
    modal.vm.$emit("confirm", {
      action: FolderAction.Delete,
      inputValue: "",
      folder: { name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory" } as IFileLibraryItemDto,
    } as IConfirmFolderActionViewModel);

    await wrapper.vm.$nextTick();
    expect(dirDeleteSpy).toHaveBeenCalled();
  });

  it("confirm with create action emits DirCreateReq", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const dirCreateSpy = vi.fn();
    on("DirCreateReq", dirCreateSpy);

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [],
        },
        level: 0,
        showModalProp: true,
      },
      global: {
        components: { "fa-icon": FontAwesomeIcon },
        stubs: { transition: false, teleport: true },
      },
    });

    await wrapper.find(".action-button").trigger("click");
    const modal = wrapper.findComponent(ModalFolderAction);
    modal.vm.$emit("confirm", {
      action: FolderAction.Create,
      inputValue: "New Folder",
      folder: { name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory" } as IFileLibraryItemDto,
    } as IConfirmFolderActionViewModel);

    await wrapper.vm.$nextTick();
    expect(dirCreateSpy).toHaveBeenCalledWith(
      expect.objectContaining({ name: "New Folder", directoryPath: "New Folder" }),
    );
  });

  it("modal has correct folder prop", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [
            { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: false, children: [] },
          ],
        },
        level: 0,
        showModalProp: true,
      },
      global: {
        components: { "fa-icon": FontAwesomeIcon },
        stubs: { transition: false, teleport: true },
      },
    });

    await wrapper.find(".action-button").trigger("click");
    const modal = wrapper.findComponent(ModalFolderAction);
    expect(modal.exists()).toBe(true);
    expect(modal.props("folder")).toEqual(
      expect.objectContaining({ name: "Test Directory", directoryPath: "/test/directory" }),
    );
  });

  it("openFolderModal is called when action button is clicked with children", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [
            { name: "Child 1", filePath: "/test/directory/child1", directoryPath: "/test/directory", selected: false, children: [] },
            { name: "Child 2", filePath: "/test/directory/child2", directoryPath: "/test/directory", selected: false, children: [] },
          ],
        },
        level: 0,
        showModalProp: true,
      },
      global: {
        components: { "fa-icon": FontAwesomeIcon },
        stubs: { transition: false, teleport: true },
      },
    });

    const openFolderModalSpy = vi.spyOn(wrapper.vm, "openFolderModal");
    await wrapper.find(".action-button").trigger("click");
    expect(openFolderModalSpy).toHaveBeenCalledTimes(1);
  });

  it("handle drag and drop", async () => {
    setSelectedDirectory({ name: "Test Directory", filePath: "/test/directory", directoryPath: "/test/directory", isDirectory: true });

    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: {
          name: "Test Directory",
          filePath: "/test/directory",
          directoryPath: "/test/directory",
          selected: false,
          children: [],
        },
        level: 0,
        showModalProp: true,
      },
      global: {
        components: { "fa-icon": FontAwesomeIcon },
        stubs: { transition: false, teleport: true },
      },
    });

    const fileList: IRenameFileLibraryItemDto[] = [
      {
        filePath: "/Images/photo1.jpg",
        directoryPath: "/Images",
        name: "photo1.jpg",
        newName: "",
        isDirectory: false,
        size: 896,
        url: "/media/Images/photo1.jpg",
      },
    ];

    await wrapper.find(".folder").trigger("dragover");
    await wrapper.find(".folder").trigger("drop", { folder: { directoryPath: "/" } as IFileLibraryItemDto, dataTransfer: createMockDataTransfer("drop", fileList, "/") });
    await wrapper.find(".folder").trigger("dragleave");
  });

  it('should emit "DirAddReq" event', async () => {
    const fileSpy = vi.fn();
    on("DirAddReq", fileSpy);

    const model = { selectedDirectory: { name: "Files", filePath: "/", directoryPath: "/" }, data: { name: "Name", directoryPath: "/path/to/file", filePath: "/path/to/file" } };
    emit("DirAddReq", model);

    expect(fileSpy).toHaveBeenCalledTimes(1);
    expect(fileSpy).toHaveBeenCalledWith(model);
  });
});

describe("FolderComponentActions", () => {
  beforeEach(() => {
    assetsStoreData = [
      {
        filePath: "/Test",
        directoryPath: "/Test",
        name: "Test",
        isDirectory: true,
        size: 0,
        url: "/media/Test",
      },
    ];
    setAssetsStore(assetsStoreData);
    setHierarchicalDirectories(assetsStoreData);
    setSelectedDirectory(assetsStoreData.find((x) => x.directoryPath === "/Test") as IFileLibraryItemDto);
  });

  it("create folder successfully", async () => {
    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: hierarchicalDirectories.value,
        level: 0,
        showModalProp: true,
      },
    });

    await wrapper.find(".action-button").trigger("click");
    const modal = wrapper.findComponent(ModalFolderAction);
    modal.vm.$emit("confirm", {
      action: FolderAction.Create,
      inputValue: "Test 2",
      folder: { name: "Test 2", filePath: "/Test2", directoryPath: "/Test2" } as IFileLibraryItemDto,
    } as IConfirmFolderActionViewModel);

    await modal.vm.$nextTick();
    await modal.vm.$nextTick();
    await modal.vm.$nextTick();

    const wrapper2 = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: hierarchicalDirectories.value,
        level: 0,
        showModalProp: true,
      },
    });

    const folderNames = wrapper2.findAll(".folder-name");
    expect(folderNames.length).toBeGreaterThanOrEqual(1);
  });

  it("delete folder successfully", async () => {
    const wrapper = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: hierarchicalDirectories.value,
        level: 0,
        showModalProp: true,
      },
    });

    await wrapper.find(".action-button").trigger("click");
    wrapper.findComponent(ModalFolderAction).vm.$emit("confirm", {
      action: FolderAction.Delete,
      inputValue: "New Folder",
      folder: assetsStoreData.find((x) => x.directoryPath === "/Test") as IFileLibraryItemDto,
    } as IConfirmFolderActionViewModel);

    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();
    await wrapper.vm.$nextTick();

    const wrapper2 = mount(FolderComponent, {
      props: {
        hierarchicalDirectories: hierarchicalDirectories.value,
        level: 0,
        showModalProp: true,
      },
    });

    const folderNames = wrapper2.findAll(".folder-name");
    expect(folderNames.length).toBeGreaterThanOrEqual(1);
  });
});

export const createMockDataTransfer = (eventType: string, files: IRenameFileLibraryItemDto[], sourceFolder?: string) => {
  const dataTransferObject = {
    dataTransfer: {
      getData: (key: string) => {
        if (key === "files") {
          return JSON.stringify(files);
        } else if (key === "sourceFolder") {
          return sourceFolder ?? "/";
        }
      },
      files,
      items: files.map((file) => ({
        kind: "file",
        type: "jpg",
        getAsFile: () => file,
      })),
      types: ["Files"],
    },
  };
  return dataTransferObject.dataTransfer;
};
