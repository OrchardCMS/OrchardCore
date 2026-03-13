import { ref, toRaw, type Ref } from "vue";
import Uppy from "@uppy/core";
import Tus from "@uppy/tus";

export interface IUploadFileEntry {
  name: string;
  percentage: number;
  errorMessage: string;
  success: boolean;
}

export interface IFieldUploadConfig {
  uploadAction: string;
  tempUploadFolder: string;
  maxUploadChunkSize?: number;
  tusEnabled?: boolean;
  tusEndpointUrl?: string;
  tusFileInfoUrl?: string;
}

/**
 * Per-instance upload state and logic for attached media fields.
 * Supports both traditional XHR uploads and TUS resumable uploads.
 */
export function useFieldUpload(config: IFieldUploadConfig) {
  const files: Ref<IUploadFileEntry[]> = ref([]);
  const isTus = !!config.tusEnabled && !!config.tusEndpointUrl;

  function getAntiForgeryToken(): string {
    const input = document.querySelector<HTMLInputElement>(
      'input[name="__RequestVerificationToken"]'
    );
    return input?.value || "";
  }

  function generateUUID(): string {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0;
      const v = c === "x" ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }

  /**
   * Upload a single file via TUS protocol.
   * Creates a per-file Uppy instance, uploads, then fetches metadata from the server.
   */
  async function uploadFileTus(
    file: File,
    entry: IUploadFileEntry
  ): Promise<UploadedFileResult | null> {
    const uppy = new Uppy({ autoProceed: false });
    uppy.use(Tus, {
      endpoint: config.tusEndpointUrl!,
      retryDelays: [0, 1000, 3000, 5000],
      chunkSize:
        config.maxUploadChunkSize && config.maxUploadChunkSize > 0
          ? config.maxUploadChunkSize
          : 5 * 1024 * 1024,
      removeFingerprintOnSuccess: true,
    });

    uppy.addFile({
      source: "media-field",
      name: file.name,
      type: file.type,
      data: file,
      meta: {
        destinationPath: config.tempUploadFolder,
        fileName: file.name,
      },
    });

    return new Promise<UploadedFileResult | null>((resolve, reject) => {
      uppy.on("progress", (progress) => {
        const uppyFile = uppy.getFiles()[0];
        entry.percentage = uppyFile?.progress?.percentage ?? progress;
      });

      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      uppy.on("upload-success", async (uppyFile: any, response: any) => {
        if (!response?.uploadURL) {
          reject(new Error("No upload URL in TUS response"));
          return;
        }

        const uploadId = response.uploadURL.split("/").pop();
        const fileInfoUrl = `${config.tusFileInfoUrl}/${uploadId}`;

        try {
          const res = await fetch(fileInfoUrl);
          if (res.ok) {
            const serverFile = await res.json();
            resolve({
              ...serverFile,
              isNew: true,
              attachedFileName: file.name,
            });
          } else {
            reject(new Error(`TUS file info request failed: ${res.status}`));
          }
        } catch (err) {
          reject(err);
        } finally {
          uppy.destroy();
        }
      });

      uppy.on("upload-error", (_file, error) => {
        uppy.destroy();
        reject(error || new Error("TUS upload failed"));
      });

      uppy.upload();
    });
  }

  /**
   * Upload a single file via XHR (legacy approach).
   */
  async function uploadFileXhr(
    file: File,
    entry: IUploadFileEntry
  ): Promise<UploadedFileResult | null> {
    const uniqueName = generateUUID() + file.name;
    const formData = new FormData();
    formData.append("files", file, uniqueName);
    formData.append("path", config.tempUploadFolder);
    formData.append("__RequestVerificationToken", getAntiForgeryToken());

    const xhr = new XMLHttpRequest();
    return new Promise<UploadedFileResult | null>((resolve, reject) => {
      xhr.upload.addEventListener("progress", (e) => {
        if (e.lengthComputable) {
          entry.percentage = Math.round((e.loaded / e.total) * 100);
        }
      });

      xhr.addEventListener("load", () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          try {
            const resp = JSON.parse(xhr.responseText);
            if (resp.files && resp.files.length > 0) {
              const serverFile = resp.files[0];
              if (serverFile.error) {
                reject(new Error(serverFile.error));
              } else {
                resolve({
                  ...serverFile,
                  isNew: true,
                  attachedFileName: file.name,
                });
              }
            } else {
              reject(new Error("No files in response"));
            }
          } catch {
            reject(new Error("Invalid response"));
          }
        } else {
          reject(new Error(`Upload failed: ${xhr.status}`));
        }
      });

      xhr.addEventListener("error", () => {
        reject(new Error("Network error"));
      });

      xhr.open("POST", config.uploadAction);
      xhr.send(formData);
    });
  }

  /**
   * Upload a list of File objects.
   * Uses TUS when enabled, otherwise falls back to XHR.
   */
  async function uploadFiles(
    fileList: File[]
  ): Promise<{ uploaded: UploadedFileResult[]; errors: string[] }> {
    const uploaded: UploadedFileResult[] = [];
    const errors: string[] = [];

    for (const file of fileList) {
      const entry: IUploadFileEntry = {
        name: file.name,
        percentage: 0,
        errorMessage: "",
        success: false,
      };
      files.value.push(entry);

      try {
        const result = isTus
          ? await uploadFileTus(file, entry)
          : await uploadFileXhr(file, entry);

        if (result) {
          entry.success = true;
          entry.percentage = 100;
          uploaded.push(result);
          setTimeout(() => {
            files.value = files.value.filter((f) => toRaw(f) !== entry);
          }, 3000);
        }
      } catch (err: unknown) {
        const message = err instanceof Error ? err.message : "Upload failed";
        entry.errorMessage = message;
        errors.push(`${file.name}: ${message}`);
      }
    }

    return { uploaded, errors };
  }

  function clearErrors() {
    files.value = files.value.filter((f) => f.errorMessage === "");
  }

  function dismiss(entry: IUploadFileEntry) {
    files.value = files.value.filter((f) => f !== entry);
  }

  function dismissAll() {
    files.value = [];
  }

  return {
    files,
    uploadFiles,
    clearErrors,
    dismiss,
    dismissAll,
  };
}

export interface UploadedFileResult {
  name: string;
  size: number;
  url: string;
  mediaPath: string;
  mime: string;
  isNew: boolean;
  attachedFileName: string;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  [key: string]: any;
}
