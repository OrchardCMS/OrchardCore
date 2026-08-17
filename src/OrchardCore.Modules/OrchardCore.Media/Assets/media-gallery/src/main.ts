declare global {
  var __VUE_PROD_DEVTOOLS__: boolean; // eslint-disable-line no-var
}

globalThis.__VUE_PROD_DEVTOOLS__ = false;

import { createApp, h, watch } from "vue";
import AppComponent from "./App.vue";
import { registerNotificationBus } from "@bloom/services/notifications/notifier";
import { configureMediaApp } from "./appSetup";
import { useGlobals } from "./services/Globals";
import { useEventBus } from "./services/UseEventBus";
import { handleSilentRenewCallback } from "./services/media-gallery-auth";
import type { IFileLibraryItemDto } from "@bloom/media/interfaces";

/**
 * Auto-mount the full media app on the admin Media Library page.
 * Only mounts if #media-gallery exists in the DOM.
 */
// When this bundle is loaded inside the hidden OIDC silent-renew iframe
// (media-gallery-oidc-silent.html sets this marker), complete the handshake and do not
// mount the app.
const isOidcSilentRenew = !!(window as unknown as { __mediaGalleryOidcSilent?: boolean }).__mediaGalleryOidcSilent;
if (isOidcSilentRenew) {
  void handleSilentRenewCallback();
}

const mediaAppEl = isOidcSilentRenew ? null : document.getElementById("media-gallery");
if (mediaAppEl) {
  const app = createApp({ name: "media-library" });
  app.component("media-gallery", AppComponent);
  configureMediaApp(app);
  registerNotificationBus();
  app.mount("#media-gallery");
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

export interface IMediaPickerConfig {
  translations: string;
  basePath: string;
  uploadFilesUrl: string;
  allowedExtensions?: string;
  allowMultiple?: boolean;
  maxUploadChunkSize?: number;
}

export function mountMediaAppAsPicker(
  container: HTMLElement,
  config: IMediaPickerConfig & { onSelectionChange?: (count: number) => void }
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


