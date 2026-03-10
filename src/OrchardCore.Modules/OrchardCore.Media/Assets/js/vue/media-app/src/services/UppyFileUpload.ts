import { onMounted } from "vue";
import { useGlobals } from "./Globals";
import Uppy, { debugLogger, Meta, UppyOptions } from "@uppy/core";
import DropTarget from "@uppy/drop-target";
import XHRUpload from "@uppy/xhr-upload";
import English from "@uppy/locales/lib/en_US";
import French from "@uppy/locales/lib/fr_FR";
import Italian from "@uppy/locales/lib/it_IT";
import Spanish from "@uppy/locales/lib/es_ES";

import "@uppy/drop-target/dist/style.css";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { usePermissions } from "./Permissions";
import { MinimalRequiredUppyFile } from "@uppy/utils/lib/UppyFile";
import { useLocalizations } from "@bloom/helpers/localizations";
import { Restrictions } from "@uppy/core/lib/Restricter";
import { OptionalPluralizeLocale } from "@uppy/utils/lib/Translator";
import { useEventBus } from "./UseEventBus";

const { on, emit } = useEventBus();
const { selectedDirectory, fileItems, uploadFilesUrl, assetsStore, setAssetsStore } = useGlobals();
const permissionsService = usePermissions();
const { translations } = useLocalizations();
const t = translations;
const culture = document.querySelector("html")?.getAttribute("lang");
let uppyLocale = English;

if (culture == "fr") {
  uppyLocale = French;
} else if (culture == "it") {
  uppyLocale = Italian;
} else if (culture == "es") {
  uppyLocale = Spanish;
}

const uppy = new Uppy({ logger: debugLogger, locale: uppyLocale });

interface IFileUploadModel {
  maxUploadChunkSize: number;
}

export const updateUploadOptions = (
  options: Partial<
    Omit<UppyOptions<Meta, Record<string, never>>, "meta" | "restrictions" | "locale"> & {
      locale: OptionalPluralizeLocale;
      meta: Partial<Meta>;
      restrictions: Partial<Restrictions>;
    }
  >,
) => {
  uppy.setOptions(options);
};

on("AfterDirSelected", () => {
  setUppyUrl();
});

/**
 * Sets the Uppy file upload URL based on the current upload endpoint.
 */
const setUppyUrl = (): string => {
  const result = uploadFilesUrl.value;

  const xhrUploadPlugin = uppy.getPlugin("XHRUpload");
  if (xhrUploadPlugin) {
    xhrUploadPlugin.setOptions({
      endpoint: result,
      fieldName: "files",
      bundle: true,
    });
  }

  return result;
};

/**
 * Sets up Uppy.js to handle file uploads.
 */
export const useFileUpload = (model: IFileUploadModel): void => {
  onMounted(() => {
    const fileInput = <HTMLInputElement>document.querySelector("#fileupload");

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    uppy.on("restriction-failed", (_file: any, error: { message: any }) => {
      notify(
        new NotificationMessage({
          summary: t.ValidationError,
          detail: error.message,
          severity: SeverityLevel.Warn,
        }),
      );
      return false;
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    fileInput?.addEventListener("change", (event: any) => {
      uppy.clear();
      const files = Array.from(event.target?.files);
      const addedFiles: MinimalRequiredUppyFile<Meta, Record<string, never>>[] = [];

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      files.forEach((file: any) => {
        addedFiles.push({
          source: "file input",
          name: file.name,
          type: file.type,
          data: file,
        });
      });

      try {
        uppy.addFiles(addedFiles);
      } catch (err: any) { // eslint-disable-line @typescript-eslint/no-explicit-any
        if (err?.isRestriction) {
          console.debug("Restriction error:", err);
        } else {
          console.debug("error:", err);
        }
      }
    });

    uppy.use(XHRUpload, {
      endpoint: setUppyUrl(),
      fieldName: "files",
      bundle: true,
      shouldRetry: () => false,
      onAfterResponse(xhr) {
        const statuses = [400, 401, 403, 500];

        if (statuses.includes(xhr.status)) {
          const jsonResponse = JSON.parse(xhr.response);
          notify(
            new NotificationMessage({
              summary: jsonResponse.title ?? t.Error,
              detail: jsonResponse.detail ?? xhr.response,
              severity: SeverityLevel.Warn,
            }),
          );
          fileInput.value = "";
        }
      },
    });

    uppy.use(DropTarget, {
      target: document.body,
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      onDragLeave: (event: any) => {
        uppy.clear();
        event.stopPropagation();
      },
    });

    uppy.on("files-added", async (files) => {
      uppy.setMeta({ destinationPath: selectedDirectory.value.directoryPath });

      if (!permissionsService.canManage.value) {
        notify(
          new NotificationMessage({
            summary: t.Unauthorized,
            detail: t.UnauthorizedFolder,
            severity: SeverityLevel.Warn,
          }),
        );
        return;
      }

      // Check if files already exist
      let hasExisting = false;
      const uploadDirectory = selectedDirectory.value.directoryPath;

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      files.forEach((file: any) => {
        const filePath = uploadDirectory === "/" ? `/${file.name}` : `${uploadDirectory}/${file.name}`;
        if (fileItems.value.find((x) => x.filePath == filePath)) {
          hasExisting = true;
        }
      });

      if (hasExisting) {
        notify(
          new NotificationMessage({
            summary: t.ValidationError,
            detail: t.ValidationErrorUploadFileExist ?? "A file with this name already exists.",
            severity: SeverityLevel.Warn,
          }),
        );
        return;
      }

      // Add uploading placeholders to the store and notify toast
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      files.forEach((file: any) => {
        const filePath = (uploadDirectory == "/" ? "" : uploadDirectory) + "/" + file.name;
        if (!assetsStore.value.find((x) => x.filePath == filePath)) {
          assetsStore.value.push({
            filePath: filePath,
            directoryPath: selectedDirectory.value.directoryPath,
            name: file.name,
            isDirectory: false,
          });
        }
        emit("UploadFileAdded", { name: file.name });
      });

      setAssetsStore(assetsStore.value);

      // Upload immediately
      try {
        await uppy.upload();
        uppy.clear();
      } catch (error) {
        console.debug("upload error", error);
      }
    });

    uppy.on("progress", (progress) => {
      // Uppy's global "progress" event reports 0-100 for all files combined
      const uppyFiles = uppy.getFiles();
      uppyFiles.forEach((file) => {
        emit("UploadProgress", {
          name: file.name ?? "",
          percentage: file.progress?.percentage ?? progress,
        });
      });
    });

    uppy.on("file-removed", () => {
      fileInput.value = "";
    });

    uppy.on("upload-error", (file, error, response) => {
      console.debug("upload-error", file, error, response);
      if (file) {
        emit("UploadError", {
          name: file.name ?? "",
          errorMessage: error?.message ?? t.Error,
        });
      }
    });

    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    uppy.on("complete", (result: any) => {
      fileInput.value = "";

      if (result.successful) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        result.successful.forEach((file: any) => {
          emit("UploadSuccess", { name: file.name });
        });
      }

      if (result.failed) {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        result.failed.forEach((file: any) => {
          emit("UploadError", {
            name: file.name,
            errorMessage: file.error ?? t.Error,
          });
        });
      }
    });
  });
};
