import { FileAction, IConfirmViewModel, IModalFileEvent } from "../interfaces/interfaces";
import { useEventBus } from "./UseEventBus";
import { useModal } from "vue-final-modal";
import ModalConfirm from "../components/ModalConfirm.vue";
import { useLocalizations } from "./Localizations";

const { emit } = useEventBus();

export const useConfirmModal = () => {
  const showConfirmModal = (modalFileEvent: IModalFileEvent) => {
    if (modalFileEvent.files.length > 0) {
      const { translations } = useLocalizations();
      const t = translations.value;
      let defaultSlotMessage = "";

      if (modalFileEvent.action == FileAction.Delete) {
        if (modalFileEvent.files.length > 1) {
          defaultSlotMessage += `<p class="mb-4">${t.DeleteMultipleFilesMessage}</p>`;
        } else {
          defaultSlotMessage += `<p class="mb-4">${t.DeleteSingleFileMessage}</p>`;
        }
      } else if (modalFileEvent.action == FileAction.Move) {
        defaultSlotMessage += `<p class="mb-4">${t.MoveFileMessage}</p>`;
      }

      const { open, destroy } = useModal({
        defaultModelValue: false,
        keepAlive: false,
        component: ModalConfirm,
        slots: {
          default: defaultSlotMessage,
          submit: t.Ok,
        },
        attrs: {
          title: modalFileEvent.modalTitle,
          modalName: modalFileEvent.uuid,
          files: modalFileEvent.files,
          action: modalFileEvent.action ?? FileAction.Rename,
          targetFolder: modalFileEvent.targetFolder,
          onConfirm(action: IConfirmViewModel) {
            confirmModal(action);
            destroy();
          },
          onClosed() {
            destroy();
          },
        },
      });

      open();
    }
  };

  const confirmModal = (confirmAction: IConfirmViewModel) => {
    if (confirmAction.action == FileAction.Delete) {
      if (confirmAction.files && confirmAction.files.length > 0) {
        if (confirmAction.files.length > 1) {
          emit("FilesDeleteReq");
        } else {
          emit("FileDeleteReq", confirmAction.files[0]);
        }
      }
    } else if (confirmAction.action == FileAction.Move) {
      if (confirmAction.files && confirmAction.files.length > 0) {
        const moveAssetsState = {
          files: confirmAction.files,
          sourceFolder: confirmAction.files[0].directoryPath,
          targetFolder: confirmAction.targetFolder,
        };

        emit("FileListMove", moveAssetsState);
      }
    }
  };

  return { showConfirmModal, confirmModal };
};
