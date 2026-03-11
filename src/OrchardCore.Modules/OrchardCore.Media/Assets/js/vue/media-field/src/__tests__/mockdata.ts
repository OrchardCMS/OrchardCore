import type { IMediaFieldItem, IMediaFieldConfig, IMediaFieldPath } from "../interfaces/MediaFieldTypes";
import type { IUploadFileEntry } from "../services/FieldUploadService";

export const mockTranslations: Record<string, string> = {
  noImages: "No Files",
  addMedia: "Add media",
  removeMedia: "Remove",
  mediaText: "Media text",
  editMediaText: "Edit media text",
  anchor: "Anchor",
  editAnchor: "Edit anchor",
  resetAnchor: "Reset",
  ok: "OK",
  cancel: "Cancel",
  mediaNotFound: "Media not found",
  mediaTemporarilyUnavailable: "Temporarily unavailable",
  smallThumbsTitle: "Small Thumbs",
  largeThumbsTitle: "Large Thumbs",
  dropFiles: "Drop files here",
  uploads: "Uploads",
  errors: "Errors",
  clearErrors: "Clear errors",
  selectMedia: "Select Media",
  loadingMediaBrowser: "Loading media browser...",
};

export function makeMediaItem(overrides: Partial<IMediaFieldItem> = {}): IMediaFieldItem {
  return {
    name: "test.jpg",
    mime: "image/jpeg",
    mediaPath: "test/test.jpg",
    url: "/media/test/test.jpg",
    size: 1024,
    vuekey: "test.jpg0",
    ...overrides,
  };
}

export function makeConfig(overrides: Partial<IMediaFieldConfig> = {}): IMediaFieldConfig {
  return {
    paths: [],
    multiple: false,
    allowMediaText: false,
    allowAnchors: false,
    allowedExtensions: "",
    mediaItemUrl: "/api/media",
    mediaAppTranslations: "",
    basePath: "/",
    uploadFilesUrl: "/api/upload",
    ...overrides,
  };
}

export function makePath(overrides: Partial<IMediaFieldPath> = {}): IMediaFieldPath {
  return {
    path: "test/test.jpg",
    ...overrides,
  };
}

export function makeUploadEntry(overrides: Partial<IUploadFileEntry> = {}): IUploadFileEntry {
  return {
    name: "photo.jpg",
    percentage: 0,
    errorMessage: "",
    success: false,
    ...overrides,
  };
}
