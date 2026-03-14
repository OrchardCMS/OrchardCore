import { beforeEach, describe, expect, it, vi } from "vitest";
import { useConfirmModal } from "../ConfirmModalService";
import { useModal } from "vue-final-modal";
import { FileAction, IConfirmViewModel, IModalFileEvent } from "@bloom/media/interfaces";
import ModalConfirm from "../../components/ModalConfirm.vue";
import { useLocalizations } from "../../composables/useLocalizations";
import { translationsData } from "../../__tests__/mockdata";
import { useEventBus } from "../UseEventBus";

const { showConfirmModal } = useConfirmModal();
const { setTranslations } = useLocalizations();
setTranslations(translationsData);

let onConfirmCallback: ((action: IConfirmViewModel) => void) | null = null;
let onClosedCallback: (() => void) | null = null;
const mockOpen = vi.fn(() => Promise.resolve());
const mockDestroy = vi.fn();

vi.mock("vue-final-modal", () => ({
  useModal: vi.fn((opts: any) => { // eslint-disable-line @typescript-eslint/no-explicit-any
    onConfirmCallback = opts?.attrs?.onConfirm ?? null;
    onClosedCallback = opts?.attrs?.onClosed ?? null;
    return {
      open: mockOpen,
      destroy: mockDestroy,
    };
  }),
}));

describe("ConfirmModalService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    onConfirmCallback = null;
    onClosedCallback = null;
  });

  it("returns an object with a showConfirmModal property", () => {
    const { showConfirmModal } = useConfirmModal();
    expect(typeof showConfirmModal).toBe("function");
  });

  it("opens a modal with the correct title and message (single file delete)", async () => {
    const modalFileEvent: IModalFileEvent = {
      modalName: "delete-file",
      isEdit: false,
      files: [{ name: "file1", newName: "file2", filePath: "/file1", directoryPath: "/", isDirectory: false }],
      action: FileAction.Delete,
      modalTitle: "Delete File",
      uuid: "1",
      targetFolder: "/",
    };

    showConfirmModal(modalFileEvent);

    expect(useModal).toHaveBeenCalledTimes(1);
    expect(useModal).toHaveBeenCalledWith({
      defaultModelValue: false,
      keepAlive: false,
      component: ModalConfirm,
      slots: {
        default: '<p class="tw:m-0">Are you sure you want to delete this file?</p>',
        submit: "Ok",
      },
      attrs: {
        title: "Delete File",
        modalName: "1",
        files: [{ name: "file1", newName: "file2", filePath: "/file1", directoryPath: "/", isDirectory: false }],
        action: FileAction.Delete,
        targetFolder: "/",
        onClosed: expect.anything(),
        onConfirm: expect.anything(),
      },
    });
  });

  it("opens a modal with the correct message (multiple files delete)", async () => {
    const modalFileEvent: IModalFileEvent = {
      modalName: "delete-file",
      isEdit: false,
      files: [
        { name: "file1", newName: "file3", filePath: "/file1", directoryPath: "/", isDirectory: false },
        { name: "file2", newName: "file4", filePath: "/file2", directoryPath: "/", isDirectory: false },
      ],
      action: FileAction.Delete,
      modalTitle: "Delete File",
      uuid: "2",
      targetFolder: "/",
    };

    showConfirmModal(modalFileEvent);

    expect(useModal).toHaveBeenCalledTimes(1);
    expect(useModal).toHaveBeenCalledWith(
      expect.objectContaining({
        slots: {
          default: '<p class="tw:m-0">Are you sure you want to delete these files?</p>',
          submit: "Ok",
        },
      }),
    );
  });

  it("opens a modal with the correct message (move files)", async () => {
    const modalFileEvent: IModalFileEvent = {
      modalName: "move-file",
      isEdit: false,
      files: [
        { name: "file1", newName: "file3", filePath: "/file1", directoryPath: "/", isDirectory: false },
        { name: "file2", newName: "file4", filePath: "/file2", directoryPath: "/", isDirectory: false },
      ],
      action: FileAction.Move,
      modalTitle: "Move File",
      uuid: "3",
      targetFolder: "/",
    };

    showConfirmModal(modalFileEvent);

    expect(useModal).toHaveBeenCalledTimes(1);
    expect(useModal).toHaveBeenCalledWith(
      expect.objectContaining({
        slots: {
          default: '<p class="tw:m-0">Are you sure you want to move the selected file(s) to this folder?</p>',
          submit: "Ok",
        },
      }),
    );
  });

  it("returns confirmModal function", () => {
    const { confirmModal } = useConfirmModal();
    expect(typeof confirmModal).toBe("function");
  });

  it("does nothing when files array is empty", () => {
    const { showConfirmModal } = useConfirmModal();
    showConfirmModal({
      modalName: "delete",
      isEdit: false,
      files: [],
      action: FileAction.Delete,
      modalTitle: "Delete",
      uuid: "empty-1",
    });
    expect(useModal).not.toHaveBeenCalled();
  });

  describe("confirmModal - Delete", () => {
    it("emits FileDeleteReq for single file delete", () => {
      const { on } = useEventBus();
      let emittedFile: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileDeleteReq", (data) => {
        emittedFile = data;
      });

      const fileToDelete = { name: "photo.jpg", newName: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false };

      const { showConfirmModal } = useConfirmModal();
      showConfirmModal({
        modalName: "delete",
        isEdit: false,
        files: [fileToDelete],
        action: FileAction.Delete,
        modalTitle: "Delete",
        uuid: "del-1",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Delete,
          files: [fileToDelete],
        });
      }

      expect(emittedFile).not.toBeNull();
      expect(emittedFile.name).toBe("photo.jpg");
    });

    it("emits FilesDeleteReq for multiple files delete", () => {
      const { on } = useEventBus();
      let called = false;
      on("FilesDeleteReq", () => {
        called = true;
      });

      const files = [
        { name: "photo1.jpg", newName: "photo1.jpg", filePath: "/photo1.jpg", directoryPath: "/", isDirectory: false },
        { name: "photo2.jpg", newName: "photo2.jpg", filePath: "/photo2.jpg", directoryPath: "/", isDirectory: false },
      ];

      const { showConfirmModal } = useConfirmModal();
      showConfirmModal({
        modalName: "delete",
        isEdit: false,
        files,
        action: FileAction.Delete,
        modalTitle: "Delete",
        uuid: "del-2",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Delete,
          files,
        });
      }

      expect(called).toBe(true);
    });

    it("does nothing when confirmAction.files is empty or undefined for Delete", () => {
      const { showConfirmModal } = useConfirmModal();
      const file = { name: "photo.jpg", newName: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false };

      showConfirmModal({
        modalName: "delete",
        isEdit: false,
        files: [file],
        action: FileAction.Delete,
        modalTitle: "Delete",
        uuid: "del-3",
      });

      // Should not throw
      expect(() => {
        if (onConfirmCallback) {
          onConfirmCallback({ action: FileAction.Delete, files: [] });
        }
      }).not.toThrow();
    });
  });

  describe("confirmModal - Move", () => {
    it("emits FileListMove with correct state for move action", () => {
      const { on } = useEventBus();
      let emittedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileListMove", (data) => {
        emittedData = data;
      });

      const files = [
        { name: "photo.jpg", newName: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false },
      ];

      const { showConfirmModal } = useConfirmModal();
      showConfirmModal({
        modalName: "move",
        isEdit: false,
        files,
        action: FileAction.Move,
        modalTitle: "Move",
        uuid: "move-1",
        targetFolder: "/Documents",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Move,
          files,
          targetFolder: "/Documents",
        });
      }

      expect(emittedData).not.toBeNull();
      expect(emittedData.targetFolder).toBe("/Documents");
      expect(emittedData.sourceFolder).toBe("/");
    });

    it("does nothing for Move when files is empty", () => {
      const { showConfirmModal } = useConfirmModal();
      const file = { name: "photo.jpg", newName: "photo.jpg", filePath: "/photo.jpg", directoryPath: "/", isDirectory: false };

      showConfirmModal({
        modalName: "move",
        isEdit: false,
        files: [file],
        action: FileAction.Move,
        modalTitle: "Move",
        uuid: "move-2",
      });

      expect(() => {
        if (onConfirmCallback) {
          onConfirmCallback({ action: FileAction.Move, files: [] });
        }
      }).not.toThrow();
    });
  });

  describe("onClosed callback", () => {
    it("calls destroy when modal is closed", () => {
      const { showConfirmModal } = useConfirmModal();
      showConfirmModal({
        modalName: "delete",
        isEdit: false,
        files: [{ name: "f.jpg", newName: "f.jpg", filePath: "/f.jpg", directoryPath: "/", isDirectory: false }],
        action: FileAction.Delete,
        modalTitle: "Delete",
        uuid: "close-1",
      });

      if (onClosedCallback) {
        onClosedCallback();
      }

      expect(mockDestroy).toHaveBeenCalled();
    });
  });
});
