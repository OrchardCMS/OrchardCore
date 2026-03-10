import { describe, expect, it, vi } from "vitest";
import { FileAction, IModalFileEvent } from "../../interfaces/interfaces";
import { useConfirmModal } from "../../services/ConfirmModalService";
import { useModal } from "vue-final-modal";
import ModalConfirm from "../../components/ModalConfirm.vue";
import { useLocalizations } from "../../services/Localizations";
import { translationsData } from "../../__tests__/mockdata";

const { showConfirmModal } = useConfirmModal();
const { setTranslations } = useLocalizations();
setTranslations(translationsData);

vi.mock("vue-final-modal", () => ({
  useModal: vi.fn(() => ({
    open: vi.fn(() => Promise.resolve()),
    destroy: vi.fn(),
  })),
}));

describe("ConfirmationDialogDeleteFile", () => {
  it("renders the confirm dialog", () => {
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
        default: '<p class="tw-m-0">Are you sure you want to delete this file?</p>',
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
});
