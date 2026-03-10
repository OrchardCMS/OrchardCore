import { ref, type Ref } from "vue";

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
}

/**
 * Per-instance upload state and logic for attached media fields.
 * Uses native fetch (no jQuery fileupload dependency).
 */
export function useFieldUpload(config: IFieldUploadConfig) {
  const files: Ref<IUploadFileEntry[]> = ref([]);

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
   * Upload a list of File objects to the temp folder.
   * Returns an array of uploaded media item metadata from the server.
   */
  async function uploadFiles(
    fileList: File[]
  ): Promise<{ uploaded: UploadedFileResult[]; errors: string[] }> {
    const uploaded: UploadedFileResult[] = [];
    const errors: string[] = [];

    for (const file of fileList) {
      const uniqueName = generateUUID() + file.name;
      const entry: IUploadFileEntry = {
        name: file.name,
        percentage: 0,
        errorMessage: "",
        success: false,
      };
      files.value.push(entry);

      try {
        const formData = new FormData();
        formData.append("files", file, uniqueName);
        formData.append("path", config.tempUploadFolder);
        formData.append(
          "__RequestVerificationToken",
          getAntiForgeryToken()
        );

        const xhr = new XMLHttpRequest();
        const result = await new Promise<UploadedFileResult | null>(
          (resolve, reject) => {
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
          }
        );

        if (result) {
          entry.success = true;
          entry.percentage = 100;
          uploaded.push(result);
          // Auto-remove from list after delay
          setTimeout(() => {
            files.value = files.value.filter((f) => f !== entry);
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
