/**
 * Represents a media item in a media field editor.
 * Extended from the server response with field-specific metadata.
 */
export interface IMediaFieldItem {
  name: string;
  mime: string;
  mediaPath: string;
  url?: string;
  size?: number;
  mediaText?: string;
  anchor?: { x: number; y: number };
  /** Error type when the item can't be loaded */
  errorType?: "transient" | "not-found";
  /** True if the item was just uploaded (name has a GUID prefix) */
  isNew?: boolean;
  /** Marked for removal (filtered from display) */
  isRemoved?: boolean;
  /** Original file name before GUID prefix was added (attached field) */
  attachedFileName?: string;
  /** Unique key for Vue's v-for */
  vuekey?: string;
}

/**
 * Serialized path entry stored in the hidden input.
 */
export interface IMediaFieldPath {
  path: string;
  mediaText?: string;
  anchor?: { x: number; y: number };
  /** Original file name (attached field) */
  attachedFileName?: string;
}

/**
 * Configuration read from data-* attributes on the mount element.
 */
export interface IMediaFieldConfig {
  paths: IMediaFieldPath[];
  multiple: boolean;
  allowMediaText: boolean;
  allowAnchors: boolean;
  allowedExtensions: string;
  mediaItemUrl: string;
  /** URL to the media-app ES module (media2.js) for the picker modal */
  mediaAppUrl?: string;
  /** JSON-serialized translations for the media-app picker */
  mediaAppTranslations?: string;
  /** Base path for media URLs (e.g., "/") */
  basePath?: string;
  /** Upload endpoint URL for the media-app picker */
  uploadFilesUrl?: string;
}
