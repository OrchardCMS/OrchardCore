/**
 * Maps to OrchardCore's FileStoreEntryDto returned by the MediaGen2ApiController.
 */
export interface IFileLibraryItemDto {
  name: string;
  size?: number;
  directoryPath: string;
  filePath: string;
  lastModifiedUtc?: string;
  isDirectory: boolean;
  url?: string;
  mime?: string;
  hasChildren?: boolean;
}

/**
 * Used for folders/files hierarchy structure.
 */
export interface IHFileLibraryItemDto extends IFileLibraryItemDto {
  selected: boolean | undefined;
  children: IHFileLibraryItemDto[];
  hasChildren?: boolean;
}

/**
 * Inherited interface for renaming a file.
 */
export interface IRenameFileLibraryItemDto extends IFileLibraryItemDto {
  newName: string;
}

/**
 * All File Actions.
 */
export enum FileAction {
  Rename,
  Delete,
  Copy,
  Move,
  Download,
}

/**
 * All Folder Actions.
 */
export enum FolderAction {
  Create,
  Delete,
}

/**
 * Browser LocalStorage
 */
export interface ILocalStorageData {
  smallThumbs: boolean;
  selectedDirectory: IFileLibraryItemDto;
  gridView: boolean;
}

/**
 * ViewModel returned by a ModalFileActionConfirm component.
 */
export interface IConfirmFileActionViewModel {
  action: FileAction;
  file: IFileLibraryItemDto;
  inputValue?: string;
}

/**
 * ViewModel returned by a ModalFilePicker component.
 */
export interface IConfirmFilePickerViewModel {
  inputValue?: string;
}

/**
 * ViewModel returned by a ModalFileActionConfirm component.
 */
export interface IConfirmViewModel {
  action?: FileAction;
  files?: IFileLibraryItemDto[];
  targetFolder?: string;
}

/**
 * File modal event
 */
export interface IModalFileEvent {
  files: IRenameFileLibraryItemDto[];
  modalName: string;
  uuid: string;
  isEdit: boolean;
  modalTitle?: string;
  action?: FileAction;
  targetFolder?: string;
  showModal?: boolean;
}

/**
 * ViewModel returned by a ModalFolderActionConfirm component.
 */
export interface IConfirmFolderActionViewModel {
  action: FolderAction;
  folder: IFileLibraryItemDto;
  inputValue?: string;
}

export type TreeNode = {
  key: string;
  label: string;
  data: any; // eslint-disable-line @typescript-eslint/no-explicit-any
  type?: string;
  icon: string;
  children: TreeNode[];
  style?: any; // eslint-disable-line @typescript-eslint/no-explicit-any
  styleClass?: string;
  selectable?: boolean;
  leaf?: boolean;
  loading?: boolean;
  expandedIcon?: string;
  collapsedIcon?: string;
};

export interface IConfirmFileEntry {
  file: IRenameFileLibraryItemDto;
  inputValue?: string;
}

export interface IFileListMoveDto {
  files: IFileLibraryItemDto[];
  targetFolder: string;
  sourceFolder: string;
}

export interface IFileCopyDto {
  newPath: string;
  oldPath: string;
}

/**
 * Storage backend capabilities returned by the GetCapabilities endpoint.
 */
export interface IFileStoreCapabilities {
  hasHierarchicalNamespace: boolean;
  supportsAtomicMove: boolean;
}

/**
 * Paginated result from the GetFolders endpoint.
 */
export interface IPaginatedFoldersResult {
  items: IFileLibraryItemDto[];
  hasMore: boolean;
}

/**
 * Combined result from the GetDirectoryContent endpoint (folders + files in one request).
 */
export interface IDirectoryContentResult {
  folders: IFileLibraryItemDto[];
  files: IFileLibraryItemDto[];
}

/**
 * A node in the server-built directory tree returned by the GetDirectoryTree endpoint.
 */
export interface IDirectoryTreeNode {
  name: string;
  path: string;
  hasChildren: boolean;
  children: IDirectoryTreeNode[];
}
