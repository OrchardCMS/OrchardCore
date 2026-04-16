declare global {
  var __VUE_PROD_DEVTOOLS__: boolean; // eslint-disable-line no-var
}

globalThis.__VUE_PROD_DEVTOOLS__ = false;

import { createApp, h, watch, type App as VueApp } from "vue";
import AppComponent from "./App.vue";
import router from './router';
/* import the fontawesome core */
import { library } from "@fortawesome/fontawesome-svg-core";
/* import font awesome icon component */
import { FontAwesomeIcon } from "@fortawesome/vue-fontawesome";
/* import specific icons */
import { fas } from "@fortawesome/free-solid-svg-icons";
import { far } from "@fortawesome/free-regular-svg-icons";
import { createVfm } from "vue-final-modal";
import { registerNotificationBus } from "@bloom/services/notifications/notifier";
import PrimeVue from 'primevue/config';
import Aura from '@primevue/themes/aura';
import Menu from 'primevue/menu';
import TreeSelect from 'primevue/treeselect';
import { useGlobals } from "./services/Globals";
import { useEventBus } from "./services/UseEventBus";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";

/* add icons to the library (once) */
library.add(fas);
library.add(far);

/**
 * Configures a Vue app instance with shared plugins for the media app.
 */
function configureMediaApp(app: VueApp) {
  const vfm = createVfm();

  app.component("fa-icon", FontAwesomeIcon);
  app.component('p-menu', Menu);
  app.component('p-treeselect', TreeSelect);

  app.use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        prefix: 'p',
        darkModeSelector: '[data-bs-theme="dark"]',
        cssLayer: {
          name: 'primevue',
          order: 'theme, base, primevue, utilities',
        },
      },
    },
  });
  app.use(vfm);
  app.use(router);
}

/**
 * Auto-mount the full media app on the admin Media Library page.
 * Only mounts if #media-app exists in the DOM.
 */
const mediaAppEl = document.getElementById("media-app");
if (mediaAppEl) {
  const app = createApp({ name: "media-library" });
  app.component("media-app", AppComponent);
  configureMediaApp(app);
  registerNotificationBus();
  app.mount("#media-app");
}

/**
 * Mount the media app as a picker inside a given container element.
 * Returns a handle to read selected files and unmount when done.
 */
export interface IMediaPickerHandle {
  getSelectedFiles(): IFileLibraryItemDto[];
  clearSelection(): void;
  unmount(): void;
}

export function mountMediaAppAsPicker(
  container: HTMLElement,
  config: {
    translations: string;
    basePath: string;
    uploadFilesUrl: string;
    allowedExtensions?: string;
    allowMultiple?: boolean;
    maxUploadChunkSize?: number;
    onSelectionChange?: (count: number) => void;
  }
): IMediaPickerHandle {
  const parseBoolean = (value: unknown, defaultValue: boolean): boolean => {
    if (typeof value === "boolean") {
      return value;
    }

    if (typeof value === "string") {
      const normalized = value.trim().toLowerCase();
      if (normalized === "true") {
        return true;
      }
      if (normalized === "false") {
        return false;
      }
    }

    return defaultValue;
  };

  const pickerAllowsMultiple = parseBoolean(config.allowMultiple, true);

  // Reset module-level state before mounting a new picker instance.
  // Clear the event bus to prevent duplicate handlers from previous mount cycles,
  // since the mitt emitter is a module-level singleton that persists across unmounts.
  const globals = useGlobals();
  const emitter = useEventBus();
  emitter.all.clear();
  globals.setSelectedFiles([]);
  globals.setSelectedAll(false);

  const app = createApp({
    name: "media-picker",
    render: () =>
      h(AppComponent, {
        translations: config.translations,
        basePath: config.basePath,
        uploadFilesUrl: config.uploadFilesUrl,
        allowedExtensions: config.allowedExtensions ?? "",
        allowMultipleSelection: pickerAllowsMultiple,
        maxUploadChunkSize: config.maxUploadChunkSize ?? 0,
      }),
  });

  configureMediaApp(app);
  registerNotificationBus();
  app.mount(container);

  // Watch selectedFiles for changes and notify via callback.
  // Must use deep:true because the array is mutated in-place via push/splice.
  const stopSelectionEnforcement = watch(
    globals.selectedFiles,
    (files) => {
      if (!pickerAllowsMultiple && files.length > 1) {
        globals.setSelectedFiles([files[files.length - 1]]);
      }
    },
    { deep: true },
  );

  const stopWatch = config.onSelectionChange
    ? watch(
        globals.selectedFiles,
        (files) => config.onSelectionChange!(files.length),
        { deep: true },
      )
    : null;

  return {
    getSelectedFiles(): IFileLibraryItemDto[] {
      return [...globals.selectedFiles.value];
    },
    clearSelection() {
      globals.setSelectedFiles([]);
      globals.setSelectedAll(false);
    },
    unmount() {
      if (stopWatch) stopWatch();
      stopSelectionEnforcement();
      app.unmount();
    },
  };
}
