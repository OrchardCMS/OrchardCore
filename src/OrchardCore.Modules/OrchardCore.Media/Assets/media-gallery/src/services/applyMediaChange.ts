import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useGlobals } from "./Globals";
import { useFileLibraryManager } from "./FileLibraryManager";

/**
 * A media change broadcast by the server.
 *
 * `item` is the affected entry, shaped exactly like the entries `GetDirectoryContent` returns, so it can
 * be spliced straight into the list. It is absent when the server could not resolve it.
 */
export interface IMediaChangedMessage {
  action?: string;
  path?: string;
  newPath?: string;
  item?: IFileLibraryItemDto;
}

function directoryOf(path: string): string {
  const index = path.lastIndexOf("/");

  return index >= 0 ? path.substring(0, index) : "";
}

/**
 * Applies a media change to the store in place.
 *
 * Returns `true` when the change was fully applied, and `false` when the caller should fall back to
 * reloading the directory. Every event used to trigger that reload — one directory listing per connected
 * client — so handling the common file events here removes the bulk of that fan-out.
 *
 * Directory events still fall back, because they alter the folder tree rather than the file list.
 */
export function applyMediaChange(message: IMediaChangedMessage): boolean {
  const { selectedDirectory, fileItems, setFileItems } = useGlobals();
  const { invalidateFileCache } = useFileLibraryManager();

  const action = message?.action;

  if (!action) {
    return false;
  }

  const currentDirectory = selectedDirectory.value?.directoryPath ?? "";

  switch (action) {
    case "fileUploaded":
    case "fileCopied": {
      const item = message.item;

      // Without the entry we cannot render the file, so let the caller reload.
      if (!item) {
        return false;
      }

      invalidateFileCache(item.directoryPath ?? "");

      if ((item.directoryPath ?? "") !== currentDirectory) {
        // Not the folder on screen: nothing to draw, and the cache is already invalidated.
        return true;
      }

      const existing = fileItems.value.filter(f => f.filePath !== item.filePath);
      setFileItems([...existing, item]);

      return true;
    }

    case "fileDeleted": {
      const path = message.path;

      if (!path) {
        return false;
      }

      const directory = directoryOf(path);
      invalidateFileCache(directory);

      if (directory !== currentDirectory) {
        return true;
      }

      setFileItems(fileItems.value.filter(f => f.filePath !== path));

      return true;
    }

    case "fileMoved": {
      const oldPath = message.path;
      const item = message.item;

      if (!oldPath || !item) {
        return false;
      }

      const oldDirectory = directoryOf(oldPath);
      const newDirectory = item.directoryPath ?? "";

      invalidateFileCache(oldDirectory);
      invalidateFileCache(newDirectory);

      // A rename keeps the file in place, so remove the old entry and add the new one in one pass.
      let updated = fileItems.value;

      if (oldDirectory === currentDirectory) {
        updated = updated.filter(f => f.filePath !== oldPath);
      }

      if (newDirectory === currentDirectory) {
        updated = [...updated.filter(f => f.filePath !== item.filePath), item];
      }

      if (updated !== fileItems.value) {
        setFileItems(updated);
      }

      return true;
    }

    default:
      // directoryCreated, directoryDeleted and anything added later: reload rather than guess.
      return false;
  }
}
