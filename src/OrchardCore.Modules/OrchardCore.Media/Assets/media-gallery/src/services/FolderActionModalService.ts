import { FolderAction, IConfirmFolderActionViewModel, IFileLibraryItemDto, IHFileLibraryItemDto } from "@bloom/media/interfaces";
import { useModal } from "vue-final-modal";
import ModalFolderAction from "../components/ModalFolderAction.vue";
import { getTranslations } from "@bloom/helpers/localizations";
import { useEventBus } from "./UseEventBus";

const { emit } = useEventBus();

export interface IFolderActionModalEvent {
  folder: IFileLibraryItemDto;
  action: FolderAction;
  uuid: string;
  modalTitle: string;
}

export const useFolderActionModal = () => {
  const t = getTranslations();

  const showFolderActionModal = (event: IFolderActionModalEvent) => {
    const isCreate = event.action === FolderAction.Create;
    const folderName = event.folder.name || event.folder.directoryPath;
    const folderPath = event.folder.directoryPath || "/";

    const folderInfoBlock = `
      <div class="file-wrapper">
        <div class="img-wrapper">
          <svg viewBox="0 0 16 16" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path d="M7.06065 3.06033C6.80732 2.80699 6.46732 2.66699 6.11398 2.66699H2.66732C1.93398 2.66699 1.34065 3.26699 1.34065 4.00033L1.33398 12.0003C1.33398 12.7337 1.93398 13.3337 2.66732 13.3337H13.334C14.0673 13.3337 14.6673 12.7337 14.6673 12.0003V5.33366C14.6673 4.60033 14.0673 4.00033 13.334 4.00033H8.00065L7.06065 3.06033Z" fill="#2c84d8" />
          </svg>
        </div>
        <div class="tw:font-bold tw:text-lg tw:ml-2 tw:min-w-0 tw:break-all">${folderName}</div>
      </div>`;

    const defaultSlot = isCreate
      ? `<p class="tw:font-bold tw:m-0">${folderPath}</p>`
      : `<p class="tw:m-0">${t.DeleteFolderMessage}</p>${folderInfoBlock}`;

    const { open, destroy } = useModal({
      defaultModelValue: false,
      keepAlive: false,
      component: ModalFolderAction,
      slots: {
        default: defaultSlot,
        submit: t.Ok,
      },
      attrs: {
        title: event.modalTitle,
        modalName: event.uuid,
        folder: event.folder,
        preSelectedAction: event.action,
        onConfirm(viewModel: IConfirmFolderActionViewModel) {
          confirmFolderActionModal(viewModel);
          destroy();
        },
        onClosed() {
          destroy();
        },
      },
    });

    open();
  };

  const confirmFolderActionModal = (confirmAction: IConfirmFolderActionViewModel) => {
    if (confirmAction.action === FolderAction.Create && confirmAction.inputValue) {
      // directoryPath carries the PARENT folder path — createDirectory uses it to
      // place the new folder as a child of the folder whose menu was clicked.
      emit("DirCreateReq", {
        name: confirmAction.inputValue,
        directoryPath: confirmAction.folder.directoryPath,
        isDirectory: true,
        filePath: "",
      } as IFileLibraryItemDto);
    } else if (confirmAction.action === FolderAction.Delete) {
      const { children, ...directory } = confirmAction.folder as IHFileLibraryItemDto;
      emit("DirDeleteReq", directory);
    }
  };

  return { showFolderActionModal };
};
