import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useFileLibraryManager, type MediaChange } from "./FileLibraryManager";

/**
 * A media change broadcast by the server.
 *
 * `item` is the affected entry, shaped exactly like the entries `GetDirectoryContent` returns, so it can
 * be applied to the store directly. It is absent when the server could not resolve it.
 */
export interface IMediaChangedMessage {
  action?: string;
  path?: string;
  newPath?: string;
  item?: IFileLibraryItemDto;
}

function nameOf(path: string): string {
  const index = path.lastIndexOf("/");

  return index >= 0 ? path.substring(index + 1) : path;
}

function directoryOf(path: string): string {
  const index = path.lastIndexOf("/");

  return index >= 0 ? path.substring(0, index) : "";
}

/**
 * Translates a broadcast media event into a {@link MediaChange}.
 *
 * Returns null when the event cannot be described — the caller then reloads the directory rather than
 * guessing. Everything about *how* the store changes lives in `applyChange`; this only maps the wire
 * format onto it, so a locally performed copy and a copy performed by someone else converge on the same
 * code.
 */
export function toMediaChange(message: IMediaChangedMessage): MediaChange | null {
  const action = message?.action;
  const path = message?.path;

  if (!action || !path) {
    return null;
  }

  switch (action) {
    case "fileUploaded":
    case "fileCopied":
      return message.item ? { kind: "fileAdded", item: message.item } : null;

    case "fileDeleted":
      return { kind: "fileRemoved", filePath: path };

    case "fileMoved":
      return message.item ? { kind: "fileMoved", oldPath: path, item: message.item } : null;

    case "directoryCreated":
      // Directories carry no item — the path is enough to describe one.
      return {
        kind: "directoryAdded",
        parentPath: directoryOf(path),
        item: {
          name: nameOf(path),
          directoryPath: path,
          filePath: "",
          isDirectory: true,
        },
      };

    case "directoryDeleted":
      return { kind: "directoryRemoved", directoryPath: path };

    default:
      // Something this client is too old to understand.
      return null;
  }
}

/**
 * Applies a broadcast media change to the store.
 *
 * Returns `true` when it was applied, and `false` when the caller should reload the directory instead.
 * Every event used to trigger that reload — one directory listing per connected client — so describing
 * the change removes the bulk of that fan-out.
 */
export function applyMediaChange(message: IMediaChangedMessage): boolean {
  const change = toMediaChange(message);

  if (change === null) {
    return false;
  }

  useFileLibraryManager().applyChange(change);

  return true;
}
