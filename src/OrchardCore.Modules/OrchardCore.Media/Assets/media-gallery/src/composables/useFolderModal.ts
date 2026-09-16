import { nextTick } from "vue";
import { useVfm } from "vue-final-modal";
import { IFileLibraryItemDto, IConfirmFolderActionViewModel, FolderAction, IHFileLibraryItemDto } from "@bloom/media/interfaces";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { useEventBus } from "../services/UseEventBus";
import { getTranslations } from "@bloom/helpers/localizations";
import { usePermissions } from "../services/Permissions";

export function useFolderModal() {
  const t = getTranslations();
  const { canManageFolder } = usePermissions();
  const { emit } = useEventBus();

  const getFolderModalName = (action: string, folder: IFileLibraryItemDto): string => {
    return action + "-folder-" + (folder.directoryPath || folder.name);
  };

  const openFolderModal = async (action: string, folder: IFileLibraryItemDto): Promise<void> => {
    if (canManageFolder(folder.directoryPath)) {
      const uVfm = useVfm();
      const modalName = getFolderModalName(action, folder);
      uVfm.open(modalName);

      await nextTick();

      emit("ResetModalFolderAction", null);
    }
    /* v8 ignore next 3 -- canManageFolder always returns true; server enforces auth */
    else {
      notify(new NotificationMessage({ summary: t.Unauthorized, detail: t.UnauthorizedFolder, severity: SeverityLevel.Warn }));
    }
  };

  const confirmFolderModal = (
    modalName: string,
    confirmAction: IConfirmFolderActionViewModel,
    onCreateFolder: (folder: IFileLibraryItemDto) => void,
    onDeleteFolder: (folder: IFileLibraryItemDto) => void,
  ) => {
    const uVfm = useVfm();
    uVfm.close(getFolderModalName(modalName, confirmAction.folder));

    if (confirmAction.action === FolderAction.Create && confirmAction.inputValue) {
      // directoryPath carries the PARENT folder path — createDirectory uses it to
      // place the new folder as a child of the folder whose menu was clicked,
      // not the currently selected folder.
      onCreateFolder({ name: confirmAction.inputValue, directoryPath: confirmAction.folder.directoryPath, isDirectory: true, filePath: "" } as IFileLibraryItemDto);
    } else if (confirmAction.action === FolderAction.Delete) {
      onDeleteFolder(confirmAction.folder);
    }
  };

  return { getFolderModalName, openFolderModal, confirmFolderModal };
}
