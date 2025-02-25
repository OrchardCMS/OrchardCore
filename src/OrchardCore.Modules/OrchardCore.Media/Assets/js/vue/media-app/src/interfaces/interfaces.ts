import { IFileStoreEntry } from "../services/MediaApiClient";

/**
 * Used to match folders with specific actions.
 * Use "*"" as allowedFolder name for common actions.
 */
export interface IFileActionElem {
  id: number;
  displayName: string;
  allowedFolder: string;
}

/**
 * All File Actions.
 */
export enum FileAction {
  Rename,
  Delete,
  Download,
}

/**
 * All Folder Actions.
 */
export enum FolderAction {
  Create,
  //Rename, TODO: this needs to be implemented
  Delete,
}

/**
 * ViewModel returned by a ModalFileActionConfirm component.
 */
export interface IConfirmFileActionViewModel {
  actionEntries: IConfirmFileActionEntry[]
}

export interface IConfirmFileActionEntry {
  action?: FileAction;
  file: IFileStoreEntry;
  inputValue?: string;
}

/**
 * ViewModel returned by a ModalFolderActionConfirm component.
 */
export interface IConfirmFolderActionViewModel {
  action: FolderAction;
  folder: IFileStoreEntry;
  inputValue?: string;
}

/**
 * File modal event
 */
export interface IModalFileEvent {
  files: IFileStoreEntry[];
  action: string;
  uuid: string;
}

/**
 * Represent a notification severity level
 */
export enum SeverityLevel {
    Success = "Success",
    Info = "Info",
    Warn = "Warn",
    Error = "Error"  
  }
  