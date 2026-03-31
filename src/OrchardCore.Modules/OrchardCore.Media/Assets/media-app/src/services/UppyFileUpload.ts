import { onMounted } from "vue";
import { useGlobals } from "./Globals";
import Uppy, { debugLogger, Meta, UppyOptions } from "@uppy/core";
import DropTarget from "@uppy/drop-target";
import XHRUpload from "@uppy/xhr-upload";
import Tus from "@uppy/tus";
import English from "@uppy/locales/lib/en_US";
import French from "@uppy/locales/lib/fr_FR";
import Italian from "@uppy/locales/lib/it_IT";
import Spanish from "@uppy/locales/lib/es_ES";

import "@uppy/drop-target/dist/style.css";
import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { usePermissions } from "./Permissions";
import { MinimalRequiredUppyFile } from "@uppy/utils/lib/UppyFile";
import { getTranslations } from "@bloom/helpers/localizations";
import { Restrictions } from "@uppy/core/lib/Restricter";
import { OptionalPluralizeLocale } from "@uppy/utils/lib/Translator";
import { useEventBus } from "./UseEventBus";

const { on, emit } = useEventBus();
const { selectedDirectory, fileItems, uploadFilesUrl, assetsStore, setAssetsStore, setFileItems } = useGlobals();
const permissionsService = usePermissions();
const t = getTranslations();
const culture = document.querySelector("html")?.getAttribute("lang");
let uppyLocale = English;

/* v8 ignore next 6 -- module-scope culture detection runs at import time before test mocks */
if (culture == "fr") {
  uppyLocale = French;
} else if (culture == "it") {
  uppyLocale = Italian;
} else if (culture == "es") {
  uppyLocale = Spanish;
}

const uppy = new Uppy({ locale: uppyLocale });

export interface IFileUploadModel {
  maxUploadChunkSize: number;
  maxFileSize: number;
  allowedExtensions: string;
  debugEnabled: boolean;
  tusEnabled: boolean;
  tusEndpointUrl: string;
  tusFileInfoUrl: string;
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

/**
 * Sets the Uppy file upload URL based on the current upload endpoint (XHR mode only).
 */
const setUppyUrl = (): string => {
  const result = uploadFilesUrl.value;

  const xhrUploadPlugin = uppy.getPlugin("XHRUpload");
  if (xhrUploadPlugin) {
    xhrUploadPlugin.setOptions({
      endpoint: result,
    });
  }

  return result;
};

/**
 * Sets up Uppy.js to handle file uploads.
 * When TUS is enabled, uses the @uppy/tus plugin for resumable uploads.
 * Otherwise, falls back to @uppy/xhr-upload (default behavior).
 */
export const useFileUpload = (model: IFileUploadModel): void => {
  if (model.debugEnabled || import.meta.env.DEV) {
    uppy.setOptions({ logger: debugLogger });
  }

  const restrictions: Record<string, unknown> = {};
  if (model.maxFileSize > 0) {
    restrictions.maxFileSize = model.maxFileSize;
  }
  if (model.allowedExtensions) {
    restrictions.allowedFileTypes = model.allowedExtensions.split(",");
  }
  if (Object.keys(restrictions).length > 0) {
    uppy.setOptions({ restrictions });
  }

  const isTus = model.tusEnabled && !!model.tusEndpointUrl;

  if (!isTus) {
    // XHR mode: update endpoint when directory changes
    on("AfterDirSelected", () => {
      setUppyUrl();
    });
  }

  // Track files that were resumed from a previous partial upload (TUS only).
  // tus-js-client sets file.tus.uploadUrl from localStorage when it finds a
  // previous incomplete upload for the same fingerprint. We capture this BEFORE
  // upload starts so we can report it after success.
  const resumedFileIds = new Set<string>();

  // Pause/resume handler for TUS uploads.
  const pausedFileNames = new Set<string>();
  if (isTus) {
    on("UploadPauseToggle", (data) => {
      const file = uppy.getFiles().find((f) => f.name === data.name);
      if (file) {
        const wasPaused = pausedFileNames.has(data.name);
        if (wasPaused) {
          pausedFileNames.delete(data.name);
          emit("UploadPaused", { name: data.name, paused: false });
        } else {
          pausedFileNames.add(data.name);
          emit("UploadPaused", { name: data.name, paused: true });
        }
        uppy.pauseResume(file.id);
      }
    });
  }

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

    // Register the upload plugin (TUS or XHR)
    if (isTus) {
      if (!uppy.getPlugin("Tus")) {
        uppy.use(Tus, {
          endpoint: model.tusEndpointUrl,
          retryDelays: [0, 1000, 3000, 5000],
          chunkSize: model.maxUploadChunkSize > 0
            ? model.maxUploadChunkSize
            : 5 * 1024 * 1024, // Default 5MB chunks
          removeFingerprintOnSuccess: true,
          onShouldRetry: () => true,
          // Include file name in the fingerprint so files with the same content
          // but different names don't collide in tus-js-client's localStorage.
          fingerprint: (file, options) => {
            return Promise.resolve(
              ["tus", file.name, file.type, file.size, options.endpoint].filter(Boolean).join("-"),
            );
          },
        });
      }

      // Detect resumed uploads: when tus-js-client finds a previous partial upload
      // in localStorage, it sets file.tus.uploadUrl before the upload begins.
      // We capture this in the "upload" event (fired before actual upload starts).
      uppy.on("upload", (data) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (data.fileIDs ?? []).forEach((fileId: string) => {
          const file = uppy.getFile(fileId);
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          if ((file as any)?.tus?.uploadUrl) {
            resumedFileIds.add(fileId);
          }
        });
      });
    } else {
      if (!uppy.getPlugin("XHRUpload")) {
        uppy.use(XHRUpload, {
          endpoint: setUppyUrl(),
          fieldName: "files",
          bundle: false,
          limit: 5,
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
      } else {
        setUppyUrl();
      }
    }

    if (!uppy.getPlugin("DropTarget")) {
      uppy.use(DropTarget, {
        target: document.body,
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        onDragLeave: (event: any) => {
          uppy.clear();
          event.stopPropagation();
        },
      });
    }

    uppy.on("files-added", async (files) => {
      const destinationPath = selectedDirectory.value.directoryPath;

      if (isTus) {
        // TUS mode: pass destination path and file name as per-file metadata
        // (TUS metadata is sent as headers, not query params)
        files.forEach((file) => {
          uppy.setFileMeta(file.id, {
            destinationPath: destinationPath,
            fileName: file.name,
          });
        });
      } else {
        // XHR mode: destination path is in the query string
        uppy.setMeta({ destinationPath: destinationPath });
      }

      /* v8 ignore next 8 -- canManage is always true; server enforces auth */
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
      const newItems: IFileLibraryItemDto[] = [];
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      files.forEach((file: any) => {
        const filePath = (uploadDirectory == "/" ? "" : uploadDirectory) + "/" + file.name;
        if (!assetsStore.value.find((x) => x.filePath == filePath)) {
          newItems.push({
            filePath: filePath,
            directoryPath: selectedDirectory.value.directoryPath,
            name: file.name,
            isDirectory: false,
          });
        }
        emit("UploadFileAdded", { name: file.name });
      });

      if (newItems.length > 0) {
        setAssetsStore([...assetsStore.value, ...newItems]);
      }

      // Upload immediately
      try {
        await uppy.upload();
        if (isTus) {
          // In TUS mode, only remove completed/failed files — paused files must
          // stay in Uppy's state so they can be resumed via pauseResume().
          uppy.getFiles().forEach((f) => {
            if (!pausedFileNames.has(f.name)) {
              uppy.removeFile(f.id);
            }
          });
        } else {
          uppy.clear();
        }
      } catch (error) {
        console.debug("upload error", error);
      }
    });

    // Per-file progress event gives us bytesUploaded/bytesTotal for speed calculation
    uppy.on("upload-progress", (file, progress) => {
      if (file) {
        emit("UploadProgress", {
          name: file.name ?? "",
          percentage: file.progress?.percentage ?? 0,
          bytesUploaded: progress.bytesUploaded ?? 0,
          bytesTotal: progress.bytesTotal ?? 0,
        });
      }
    });

    uppy.on("file-removed", () => {
      fileInput.value = "";
    });

    uppy.on("upload-error", (file, error, response) => {
      console.debug("upload-error", file, error, response);
      if (file) {
        // Try to extract a meaningful error from the server response body.
        // tusdotnet returns validation messages as plain text in the response body
        // (e.g., "File extension not allowed: .mkv").
        let errorMessage = error?.message ?? t.Error;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const responseBody = (response as any)?.body;
        if (typeof responseBody === "string" && responseBody.length > 0 && responseBody.length < 500) {
          errorMessage = responseBody;
        }
        emit("UploadError", {
          name: file.name ?? "",
          errorMessage,
        });
      }
    });

    if (isTus) {
      // TUS mode: fetch file metadata from the server after each successful upload
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      uppy.on("upload-success", async (file: any, response: any) => {
        if (!file || !response?.uploadURL) return;

        const uploadId = response.uploadURL.split("/").pop();
        const fileInfoUrl = `${model.tusFileInfoUrl}/${uploadId}`;

        try {
          const res = await fetch(fileInfoUrl);
          if (res.ok) {
            const serverFile = await res.json();
            const matchName = serverFile.name || file.name;
            const matchDir = serverFile.directoryPath || selectedDirectory.value.directoryPath;
            setAssetsStore(assetsStore.value.map(a =>
              !a.isDirectory && a.name === matchName && a.directoryPath === matchDir
                ? { ...a, filePath: serverFile.filePath, size: serverFile.size, lastModifiedUtc: serverFile.lastModifiedUtc, url: serverFile.url, mime: serverFile.mime }
                : a
            ));

            // Add the uploaded file to the file panel immediately.
            const uploadedItem: IFileLibraryItemDto = {
              name: serverFile.name || file.name,
              filePath: serverFile.filePath,
              directoryPath: serverFile.directoryPath || selectedDirectory.value.directoryPath,
              size: serverFile.size,
              lastModifiedUtc: serverFile.lastModifiedUtc,
              url: serverFile.url,
              mime: serverFile.mime,
              isDirectory: false,
            };
            if (!fileItems.value.some(f => f.filePath === uploadedItem.filePath)) {
              setFileItems([...fileItems.value, uploadedItem]);
            }
          }
        } catch (err) {
          console.debug("Failed to fetch TUS file info:", err);
        }

        // Check if this upload was resumed from a previous partial upload
        const isResumed = resumedFileIds.has(file.id);
        resumedFileIds.delete(file.id);
        emit("UploadSuccess", {
          name: file.name,
          resumed: isResumed,
        });
      });
    } else {
      // XHR mode: parse server response from the bundled upload
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      uppy.on("complete", (result: any) => {
        fileInput.value = "";

        if (result.successful) {
          // Update placeholders in assetsStore with real metadata from the server response.
          // The server returns { files: [{ Name, Size, DirectoryPath, FilePath, LastModifiedUtc, Url, Mime }, ...] }
          // Build a map of server-returned file metadata keyed by name+directoryPath.
          const updates = new Map<string, Record<string, unknown>>();
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          result.successful.forEach((file: any) => {
            const serverFiles = file.response?.body?.files;
            if (Array.isArray(serverFiles)) {
              // eslint-disable-next-line @typescript-eslint/no-explicit-any
              serverFiles.forEach((serverFile: any) => {
                updates.set(`${serverFile.name}|${serverFile.directoryPath}`, serverFile);
              });
            }
            emit("UploadSuccess", { name: file.name });
          });
          if (updates.size > 0) {
            setAssetsStore(assetsStore.value.map(a => {
              const key = `${a.name}|${a.directoryPath}`;
              const serverFile = updates.get(key);
              if (!a.isDirectory && serverFile) {
                return { ...a, filePath: serverFile.filePath as string, size: serverFile.size as number, lastModifiedUtc: serverFile.lastModifiedUtc as string, url: serverFile.url as string, mime: serverFile.mime as string };
              }
              return a;
            }));

            // Add the uploaded files to the file panel immediately.
            const newFileItems: IFileLibraryItemDto[] = [];
            updates.forEach((serverFile) => {
              const item: IFileLibraryItemDto = {
                name: serverFile.name as string,
                filePath: serverFile.filePath as string,
                directoryPath: serverFile.directoryPath as string,
                size: serverFile.size as number,
                lastModifiedUtc: serverFile.lastModifiedUtc as string,
                url: serverFile.url as string,
                mime: serverFile.mime as string,
                isDirectory: false,
              };
              newFileItems.push(item);
            });
            if (newFileItems.length > 0) {
              // Merge: replace existing items by filePath, add new ones.
              const existingPaths = new Set(fileItems.value.map(f => f.filePath));
              const merged = [
                ...fileItems.value,
                ...newFileItems.filter(f => !existingPaths.has(f.filePath)),
              ];
              setFileItems(merged);
            }
          }
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
    }
  });
};
