import { ref } from "vue";
import { v4 as uuidv4 } from "uuid";
import { IFileLibraryItemDto, FileAction, IModalFileEvent } from "@bloom/media/interfaces";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { useConfirmModal } from "../services/ConfirmModalService";
import { useLocalizations } from "./useLocalizations";

export function useFolderDragDrop() {
  const { translations: t } = useLocalizations();
  const { showConfirmModal } = useConfirmModal();

  const isHovered = ref(false);

  const handleDragOver = () => {
    isHovered.value = true;
  };

  const handleDragLeave = () => {
    isHovered.value = false;
  };

  const moveFileToFolder = (folder: IFileLibraryItemDto, e: DragEvent): void => {
    isHovered.value = false;

    const filesData = e.dataTransfer?.getData("files") ?? "";
    const files = JSON.parse(filesData);

    if (files.length < 1) {
      return;
    }

    let sourceFolder = e.dataTransfer?.getData("sourceFolder") ?? "root";
    let targetFolder = folder.directoryPath;

    if (targetFolder === "" || targetFolder === "/") {
      targetFolder = "root";
    }

    if (sourceFolder === targetFolder) {
      notify(new NotificationMessage({ summary: t.ErrorMovingFile, detail: t.SameFolderMessage, severity: SeverityLevel.Error }));
      return;
    }

    const modal = { files, modalName: "move", uuid: uuidv4(), isEdit: true, modalTitle: t.MoveFileTitle, action: FileAction.Move, targetFolder } as IModalFileEvent;
    showConfirmModal(modal);
  };

  return { isHovered, handleDragOver, handleDragLeave, moveFileToFolder };
}
