import { FileAction, IConfirmFileActionViewModel, IModalFileEvent, IRenameFileLibraryItemDto } from "@bloom/media/interfaces";
import { useEventBus } from "./UseEventBus";
import { useModal } from "vue-final-modal";
import ModalFileAction from "../components/ModalFileAction.vue";
import { useLocalizations } from "../composables/useLocalizations";

const { emit } = useEventBus();

export const useFileActionModal = () => {
  const showFileActionModal = (modalFileEvent: IModalFileEvent) => {
    if (modalFileEvent.files.length > 0) {
      const { translations } = useLocalizations();
      const t = translations;
      const { open, destroy } = useModal({
        defaultModelValue: false,
        keepAlive: false,
        component: ModalFileAction,
        slots: {
          submit: t.Ok,
        },
        attrs: {
          title: modalFileEvent.modalTitle,
          modalName: modalFileEvent.uuid,
          fileItem: modalFileEvent.files[0],
          action: modalFileEvent.action ?? FileAction.Rename,
          onConfirm(action: IConfirmFileActionViewModel) {
            confirmFileActionModal(action);
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

  const confirmFileActionModal = (confirmAction: IConfirmFileActionViewModel) => {
    if (confirmAction.action == FileAction.Rename) {
      const renameFile = confirmAction.file as IRenameFileLibraryItemDto;
      renameFile.newName = confirmAction.inputValue ?? confirmAction.file.name;
      emit("FileRenameReq", renameFile);
    } else if (confirmAction.action == FileAction.Move) {
      if (confirmAction.file) {
        const moveAssetsState = {
          files: [confirmAction.file],
          sourceFolder: confirmAction.file.directoryPath,
          targetFolder: Object.keys(confirmAction.inputValue ?? {})[0],
        };

        emit("FileListMove", moveAssetsState);
      }
    } else if (confirmAction.action == FileAction.Copy) {
      const copyAssetsState = {
        oldPath: confirmAction.file.filePath,
        newPath: Object.keys(confirmAction.inputValue ?? {})[0] + "/" + confirmAction.file.name,
      };

      emit("FileCopy", copyAssetsState);
    }
  };

  return { showFileActionModal };
};
