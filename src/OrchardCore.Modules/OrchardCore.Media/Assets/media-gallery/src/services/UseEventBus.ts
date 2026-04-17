import mitt from "mitt";

export type MittEvents = {
    IsUploading: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    PagerEvent: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirSelected: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    AfterDirSelected: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirDeleted: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirAdded: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileListMove: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileListMoved: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileCopy: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileCopied: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileRenamed: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirChildrenLoaded: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirAddReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirCreateReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirDelete: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    DirDeleteReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileSortChangeReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileSelectReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileRenameReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileDeleteReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FilesDeleteReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileDragReq: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    FileDeleted: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    SelectAll: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    ResetModalFolderAction: any; // eslint-disable-line @typescript-eslint/no-explicit-any
    UploadFileAdded: { name: string };
    UploadProgress: { name: string; percentage: number; bytesUploaded: number; bytesTotal: number };
    UploadSuccess: { name: string; resumed?: boolean };
    UploadError: { name: string; errorMessage: string };
    UploadPauseToggle: { name: string };
    UploadPaused: { name: string; paused: boolean };
};

const emitter = mitt<MittEvents>();

export function useEventBus() {
  return emitter;
}
