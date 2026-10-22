declare global {
  var __VUE_PROD_DEVTOOLS__: boolean; // eslint-disable-line no-var
}

globalThis.__VUE_PROD_DEVTOOLS__ = false;

import { createApp, h } from "vue";
import { library } from "@fortawesome/fontawesome-svg-core";
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
import { fas } from "@fortawesome/free-solid-svg-icons";
import { far } from "@fortawesome/free-regular-svg-icons";
import { createVfm } from "vue-final-modal";
import PrimeVue from "primevue/config";
import MediaFieldBasic from "./components/MediaFieldBasic.vue";
import MediaFieldAttached from "./components/MediaFieldAttached.vue";
import MediaFieldGallery from "./components/MediaFieldGallery.vue";
import MediaPickerModal from "./components/MediaPickerModal.vue";
import { setTranslations } from "@bloom/helpers/localizations";
import observeAndInit from "@bloom/helpers/observeAndInit";
import type { IMediaFieldConfig, IMediaFieldPath } from "./interfaces/MediaFieldTypes";
import type { IAttachedFieldConfig } from "./components/MediaFieldAttached.vue";
import type { IMediaFieldItem } from "./interfaces/MediaFieldTypes";
import { sanitizeFieldPaths } from "./services/MediaPath";
import { configureMediaAuth } from "@media-gallery-auth";

import "vue-final-modal/style.css";
import "./assets/css/field.css";

/* add icons to the library (once) */
library.add(fas);
library.add(far);

/**
 * Reads data-* attributes from an HTML element and returns a typed config object.
 */
function readConfig(el: HTMLElement): IMediaFieldConfig {
  const dataset = el.dataset;
  let paths: IMediaFieldPath[] = [];
  try {
    paths = sanitizeFieldPaths(JSON.parse(dataset.paths || "[]"));
  } catch {
    paths = [];
  }

  return {
    paths,
    multiple: dataset.multiple === "true",
    allowMediaText: dataset.allowMediaText === "true",
    allowAnchors: dataset.allowAnchors === "true",
    allowedExtensions: dataset.allowedExtensions || "",
    maxFileSize: dataset.maxFileSize ? Number(dataset.maxFileSize) : undefined,
    mediaItemUrl: dataset.mediaItemUrl || "",
    mediaItemsUrl: dataset.mediaItemsUrl || "",
    mediaAppTranslations: dataset.mediaGalleryTranslations || dataset.mediaAppTranslations || "",
    basePath: dataset.basePath || "",
    uploadFilesUrl: dataset.uploadFilesUrl || "",
    tusEnabled: dataset.tusEnabled === "true",
    tusEndpointUrl: dataset.tusEndpointUrl || "",
    tusFileInfoUrl: dataset.tusFileInfoUrl || "",
    signalrEnabled: dataset.signalrEnabled === "true",
    disableThumbnails: dataset.disableThumbnails === "true",
  };
}

/**
 * Reads localization strings from the data-translations JSON attribute.
 * Falls back to English defaults if not found.
 */
function readTranslations(el: HTMLElement): Record<string, string> {
  const defaults: Record<string, string> = {
    noImages: "No images",
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
    smallThumbsTitle: "Small thumbnails",
    largeThumbsTitle: "Large thumbnails",
    dropFiles: "Drop files here",
    uploads: "Uploads",
    errors: "Errors",
    clearErrors: "Clear errors",
    selectMedia: "Select Media",
    loadingMediaBrowser: "Loading media browser...",
  };

  try {
    const json = el.dataset.translations;
    if (json) {
      const parsed = JSON.parse(json) as Record<string, string>;
      return { ...defaults, ...parsed };
    }
  } catch {
    // Fall back to defaults
  }

  return defaults;
}

/**
 * Reads attached field config (extends basic config with upload settings).
 */
function readAttachedConfig(el: HTMLElement): IAttachedFieldConfig {
  const base = readConfig(el);
  const dataset = el.dataset;
  return {
    ...base,
    uploadAction: dataset.uploadAction || "",
    tempUploadFolder: dataset.tempUploadFolder || "",
    maxUploadChunkSize: dataset.maxUploadChunkSize
      ? parseInt(dataset.maxUploadChunkSize, 10)
      : undefined,
    debugEnabled: dataset.debugEnabled === "true" || import.meta.env.DEV,
    tusEnabled: dataset.tusEnabled === "true",
    tusEndpointUrl: dataset.tusEndpointUrl || "",
    tusFileInfoUrl: dataset.tusFileInfoUrl || "",
  };
}

/**
 * Creates and configures a Vue 3 app instance with shared plugins.
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
function createFieldApp(rootComponent: any, rootProps: Record<string, unknown>) {
  const app = createApp({
    render: () => h(rootComponent, rootProps),
  });

  const vfm = createVfm();

  app.component("fa-icon", FontAwesomeIcon);

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  app.use(PrimeVue as any, {
    unstyled: true,
  });
  app.use(vfm);

  return app;
}

/**
 * Mount a basic media field (picker via modal).
 * Each call creates an independent Vue app scoped to the given element.
 */
export function mountMediaField(el: HTMLElement) {
  const config = readConfig(el);
  const inputName = el.dataset.inputName || el.id || "MediaField";
  const translations = readTranslations(el);

  setTranslations(translations);

  const app = createFieldApp(MediaFieldBasic, { config, inputName });
  app.mount(el);
  el.dataset.mediaFieldMounted = "true";

  return app;
}

/**
 * Mount an attached media field (direct upload via file input + drag-drop).
 * Each call creates an independent Vue app scoped to the given element.
 */
export function mountAttachedMediaField(el: HTMLElement) {
  const config = readAttachedConfig(el);
  const inputName = el.dataset.inputName || el.id || "MediaField";
  const translations = readTranslations(el);

  setTranslations(translations);

  const app = createFieldApp(MediaFieldAttached, { config, inputName });
  app.mount(el);
  el.dataset.mediaFieldMounted = "true";

  return app;
}

/**
 * Mount a gallery media field (grid/list views with inline actions).
 * Each call creates an independent Vue app scoped to the given element.
 */
export function mountGalleryMediaField(el: HTMLElement) {
  const config = readConfig(el);
  const inputName = el.dataset.inputName || el.id || "MediaField";
  const translations = readTranslations(el);

  setTranslations(translations);

  const app = createFieldApp(MediaFieldGallery, { config, inputName });
  app.mount(el);
  el.dataset.mediaFieldMounted = "true";

  return app;
}

/**
 * Opens the media picker modal programmatically and returns a promise that resolves
 * with the selected items. Used by trumbowyg and MDE editor plugins.
 */
export function openMediaPicker(
  mediaAppTranslations: string,
  basePath: string,
  uploadFilesUrl: string,
  allowedExtensions = "",
  allowMultiple = true,
  maxFileSize = Number(document.querySelector<HTMLElement>("[data-media-picker-config]")?.dataset.maxFileSize ?? 0),
): Promise<IMediaFieldItem[]> {
  return new Promise((resolve) => {
    const host = document.createElement("div");
    document.body.appendChild(host);

    let settled = false;

    // The editor plugins pass no display options; the site-wide DisableThumbnails option is
    // read from the picker config element the host page renders.
    const pickerConfigEl = document.querySelector<HTMLElement>("[data-media-picker-config]");
    const disableThumbnails = pickerConfigEl?.dataset.disableThumbnails === "true";

    const app = createFieldApp(MediaPickerModal, {
      mediaAppTranslations,
      basePath,
      uploadFilesUrl,
      allowedExtensions,
      maxFileSize,
      allowMultiple,
      disableThumbnails,
      autoOpen: true,
      onResolve: (items: IMediaFieldItem[]) => {
        if (settled) return;
        settled = true;
        app.unmount();
        host.remove();
        resolve(items);
      },
    });

    app.mount(host);
  });
}

/**
 * Auto-discover and mount all media field elements on the page.
 * Uses data-media-field-type attribute: "basic", "attached", or "gallery".
 */
const mountFns: Record<string, (el: HTMLElement) => void> = {
  basic: mountMediaField,
  attached: mountAttachedMediaField,
  gallery: mountGalleryMediaField,
};

// Configure silent OIDC auth for the media fields when the Media API is in bearer mode. The mode
// (and base path) are read from the first field element mounted on the page — it is a site-wide
// setting, so all fields share it. This must run before the fields mount and start calling the API.
// No-op in cookie mode. The configured state is shared with the media-gallery app the picker mounts.
let fieldAuthConfigured = false;

function configureFieldAuth(el: HTMLElement) {
  if (fieldAuthConfigured) return;
  fieldAuthConfigured = true;
  if (el.dataset.apiAuthScheme === "Bearer") {
    configureMediaAuth({ basePath: el.dataset.basePath || "/" });
  }
}

function autoMountField(el: HTMLElement) {
  // Skip already-mounted elements
  if (el.dataset.mediaFieldMounted) return;
  const type = el.dataset.mediaFieldType || "";
  const fn = mountFns[type];
  if (fn) {
    configureFieldAuth(el);
    fn(el);
  }
}

// Mount the fields already in the DOM, and keep mounting the ones added later, e.g. by the Flow and
// Bag part editors which fetch widget editors via AJAX. A module script is only evaluated once per
// page, so re-inserting its <script> tag along with such an editor doesn't run this code again.
observeAndInit("[data-media-field-type]", autoMountField);

// Register openMediaPicker on the config elements so that picker-api.ts (bundled
// into Parcel/Webpack builds for trumbowyg/MDE) can call it directly without
// needing to dynamically import this module by URL. Config elements can also be
// added later along with the editors fetched by the Flow and Bag part editors.
observeAndInit("[data-media-picker-config]", (configEl) => {
  (configEl as { __openMediaPicker?: typeof openMediaPicker }).__openMediaPicker = openMediaPicker;
});

// Also export for manual mounting and dev mode
/* v8 ignore next 5 -- DEV-only code, import.meta.env.DEV is a compile-time constant */
if (import.meta.env.DEV) {
  const devEl = document.getElementById("media-field-dev");
  if (devEl) {
    mountMediaField(devEl);
  }
}
