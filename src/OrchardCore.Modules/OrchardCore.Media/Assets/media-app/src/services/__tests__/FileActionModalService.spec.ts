import { beforeEach, describe, expect, it, vi } from "vitest";
import { useFileActionModal } from "../FileActionModalService";
import { useModal } from "vue-final-modal";
import { FileAction, IConfirmFileActionViewModel, IModalFileEvent } from "@bloom/media/interfaces";
import { useLocalizations } from "../../composables/useLocalizations";
import { translationsData } from "../../__tests__/mockdata";
import { useEventBus } from "../UseEventBus";

const { setTranslations } = useLocalizations();
setTranslations(translationsData);

let onConfirmCallback: ((action: IConfirmFileActionViewModel) => void) | null = null;
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

const singleFile = {
  name: "photo.jpg",
  newName: "photo.jpg",
  filePath: "/Images/photo.jpg",
  directoryPath: "/Images",
  isDirectory: false,
};

describe("FileActionModalService", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    onConfirmCallback = null;
    onClosedCallback = null;
  });

  describe("showFileActionModal", () => {
    it("returns an object with showFileActionModal function", () => {
      const { showFileActionModal } = useFileActionModal();
      expect(typeof showFileActionModal).toBe("function");
    });

    it("does nothing when files array is empty", () => {
      const { showFileActionModal } = useFileActionModal();
      const event: IModalFileEvent = {
        modalName: "rename",
        isEdit: false,
        files: [],
        action: FileAction.Rename,
        modalTitle: "Rename",
        uuid: "1",
      };
      showFileActionModal(event);
      expect(useModal).not.toHaveBeenCalled();
    });

    it("opens a modal when files are provided", () => {
      const { showFileActionModal } = useFileActionModal();
      const event: IModalFileEvent = {
        modalName: "rename",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Rename,
        modalTitle: "Rename file",
        uuid: "rename-1",
      };
      showFileActionModal(event);
      expect(useModal).toHaveBeenCalledTimes(1);
      expect(mockOpen).toHaveBeenCalledTimes(1);
    });

    it("passes the correct title and action to the modal", () => {
      const { showFileActionModal } = useFileActionModal();
      const event: IModalFileEvent = {
        modalName: "move",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Move,
        modalTitle: "Move file",
        uuid: "move-1",
      };
      showFileActionModal(event);
      expect(useModal).toHaveBeenCalledWith(
        expect.objectContaining({
          attrs: expect.objectContaining({
            title: "Move file",
            action: FileAction.Move,
            fileItem: singleFile,
          }),
        }),
      );
    });

    it("defaults to FileAction.Rename when action is undefined", () => {
      const { showFileActionModal } = useFileActionModal();
      const event: IModalFileEvent = {
        modalName: "rename",
        isEdit: false,
        files: [singleFile],
        action: undefined,
        modalTitle: "Rename",
        uuid: "rename-2",
      };
      showFileActionModal(event);
      expect(useModal).toHaveBeenCalledWith(
        expect.objectContaining({
          attrs: expect.objectContaining({
            action: FileAction.Rename,
          }),
        }),
      );
    });
  });

  describe("confirmFileActionModal - Rename", () => {
    it("emits FileRenameReq with the new name", () => {
      const { on } = useEventBus();
      let emittedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileRenameReq", (data) => {
        emittedData = data;
      });

      const { showFileActionModal } = useFileActionModal();
      showFileActionModal({
        modalName: "rename",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Rename,
        modalTitle: "Rename file",
        uuid: "rename-3",
      });

      // Trigger onConfirm callback
      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Rename,
          file: singleFile,
          inputValue: "new-photo.jpg",
        });
      }

      expect(emittedData).not.toBeNull();
      expect(emittedData.newName).toBe("new-photo.jpg");
    });

    it("uses file.name as newName when inputValue is undefined", () => {
      const { on } = useEventBus();
      let emittedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileRenameReq", (data) => {
        emittedData = data;
      });

      const { showFileActionModal } = useFileActionModal();
      showFileActionModal({
        modalName: "rename",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Rename,
        modalTitle: "Rename file",
        uuid: "rename-4",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Rename,
          file: singleFile,
          inputValue: undefined,
        });
      }

      expect(emittedData).not.toBeNull();
      expect(emittedData.newName).toBe("photo.jpg");
    });
  });

  describe("confirmFileActionModal - Move", () => {
    it("emits FileListMove with correct source/target folder", () => {
      const { on } = useEventBus();
      let emittedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileListMove", (data) => {
        emittedData = data;
      });

      const { showFileActionModal } = useFileActionModal();
      showFileActionModal({
        modalName: "move",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Move,
        modalTitle: "Move file",
        uuid: "move-2",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Move,
          file: singleFile,
          inputValue: { "/Documents": true } as any, // eslint-disable-line @typescript-eslint/no-explicit-any
        });
      }

      expect(emittedData).not.toBeNull();
      expect(emittedData.targetFolder).toBe("/Documents");
      expect(emittedData.files).toContain(singleFile);
    });

    it("does nothing when file is undefined for move action", () => {
      const { on } = useEventBus();
      let emittedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileListMove", (data) => {
        emittedData = data;
      });

      const { showFileActionModal } = useFileActionModal();
      showFileActionModal({
        modalName: "move",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Move,
        modalTitle: "Move file",
        uuid: "move-3",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Move,
          file: null as any, // eslint-disable-line @typescript-eslint/no-explicit-any
          inputValue: { "/Documents": true } as any, // eslint-disable-line @typescript-eslint/no-explicit-any
        });
      }

      // Should not emit since file is falsy
      expect(emittedData).toBeNull();
    });
  });

  describe("confirmFileActionModal - Copy", () => {
    it("emits FileCopy with correct paths", () => {
      const { on } = useEventBus();
      let emittedData: any = null; // eslint-disable-line @typescript-eslint/no-explicit-any
      on("FileCopy", (data) => {
        emittedData = data;
      });

      const { showFileActionModal } = useFileActionModal();
      showFileActionModal({
        modalName: "copy",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Copy,
        modalTitle: "Copy file",
        uuid: "copy-1",
      });

      if (onConfirmCallback) {
        onConfirmCallback({
          action: FileAction.Copy,
          file: singleFile,
          inputValue: { "/Documents": true } as any, // eslint-disable-line @typescript-eslint/no-explicit-any
        });
      }

      expect(emittedData).not.toBeNull();
      expect(emittedData.oldPath).toBe("/Images/photo.jpg");
      expect(emittedData.newPath).toBe("/Documents/photo.jpg");
    });
  });

  describe("onClosed callback", () => {
    it("calls destroy when modal is closed", () => {
      const { showFileActionModal } = useFileActionModal();
      showFileActionModal({
        modalName: "rename",
        isEdit: false,
        files: [singleFile],
        action: FileAction.Rename,
        modalTitle: "Rename",
        uuid: "close-1",
      });

      if (onClosedCallback) {
        onClosedCallback();
      }

      expect(mockDestroy).toHaveBeenCalledTimes(1);
    });
  });
});
