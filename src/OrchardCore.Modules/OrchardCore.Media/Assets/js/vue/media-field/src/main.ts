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
import Aura from "@primevue/themes/aura";
import MediaFieldBasic from "./components/MediaFieldBasic.vue";
import MediaFieldAttached from "./components/MediaFieldAttached.vue";
import MediaFieldGallery from "./components/MediaFieldGallery.vue";
import { useLocalizations } from "@bloom/helpers/localizations";
import type { IMediaFieldConfig, IMediaFieldPath } from "./interfaces/MediaFieldTypes";
import type { IAttachedFieldConfig } from "./components/MediaFieldAttached.vue";

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
    paths = JSON.parse(dataset.paths || "[]");
  } catch {
    paths = [];
  }

  return {
    paths,
    multiple: dataset.multiple === "true",
    allowMediaText: dataset.allowMediaText === "true",
    allowAnchors: dataset.allowAnchors === "true",
    allowedExtensions: dataset.allowedExtensions || "",
    mediaItemUrl: dataset.mediaItemUrl || "",
    mediaAppTranslations: dataset.mediaAppTranslations || "",
    basePath: dataset.basePath || "",
    uploadFilesUrl: dataset.uploadFilesUrl || "",
    tusEnabled: dataset.tusEnabled === "true",
    tusEndpointUrl: dataset.tusEndpointUrl || "",
    tusFileInfoUrl: dataset.tusFileInfoUrl || "",
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
    theme: {
      preset: Aura,
      options: {
        prefix: "p",
        darkModeSelector: '[data-bs-theme="dark"]',
        cssLayer: {
          name: "primevue",
          order: "theme, base, primevue, utilities",
        },
      },
    },
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

  // Set translations via composable
  const { setTranslations } = useLocalizations();
  setTranslations(translations);

  const app = createFieldApp(MediaFieldBasic, { config, inputName });
  app.mount(el);

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

  const { setTranslations } = useLocalizations();
  setTranslations(translations);

  const app = createFieldApp(MediaFieldAttached, { config, inputName });
  app.mount(el);

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

  const { setTranslations } = useLocalizations();
  setTranslations(translations);

  const app = createFieldApp(MediaFieldGallery, { config, inputName });
  app.mount(el);

  return app;
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

function autoMount() {
  document.querySelectorAll<HTMLElement>("[data-media-field-type]").forEach((el) => {
    // Skip already-mounted elements
    if (el.dataset.mediaFieldMounted) return;
    const type = el.dataset.mediaFieldType || "";
    const fn = mountFns[type];
    if (fn) {
      fn(el);
      el.dataset.mediaFieldMounted = "true";
    }
  });
}

// Auto-mount when the module loads (DOM is ready for module scripts)
autoMount();

// Also export for manual mounting and dev mode
/* v8 ignore next 5 -- DEV-only code, import.meta.env.DEV is a compile-time constant */
if (import.meta.env.DEV) {
  const devEl = document.getElementById("media-field-dev");
  if (devEl) {
    mountMediaField(devEl);
  }
}
