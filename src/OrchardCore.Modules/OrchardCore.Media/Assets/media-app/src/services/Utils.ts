import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useGlobals } from "./Globals";
import { useLocalizations } from "@bloom/helpers/localizations";

export { humanFileSize, getFileExtension, printDateTime } from "@bloom/media/utils";

const { translations } = useLocalizations();
const t = translations;
const { setIsDownloading, selectedFiles, setSelectedFiles, setSelectedAll } = useGlobals();

export const isFileSelected = (file: IFileLibraryItemDto): boolean => {
  if (!file.url) return false;
  return selectedFiles.value.some((el) => el.url?.toLowerCase() === file.url?.toLowerCase());
};

export async function downloadFile(file: IFileLibraryItemDto) {
  if (file && file.url) {
    try {
      const response = await fetch(file.url, { method: "HEAD" });
      if (response.ok) {
        const aElement = document.createElement("a");
        aElement.setAttribute("download", file.name);
        aElement.href = file.url;
        aElement.setAttribute("target", "_blank");
        aElement.click();
      } else {
        notify(new NotificationMessage({ summary: t.Error, detail: t.FailedDownload, severity: SeverityLevel.Error }));
      }
    } catch {
      notify(new NotificationMessage({ summary: t.Error, detail: t.FailedDownload, severity: SeverityLevel.Error }));
    }
  }
}

export const downloadSelectedFiles = async () => {
  setIsDownloading(true);
  const promises = selectedFiles.value.map((file) => {
    if (file.url) {
      return fetch(file.url)
        .then((response) => response.blob())
        .then((blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement("a");
          a.href = url;
          a.download = file.name || "download";
          document.body.appendChild(a);
          a.click();
          a.remove();
          window.URL.revokeObjectURL(url);
        });
    }
  });

  await Promise.all(promises).then(() => {
    setIsDownloading(false);
    setSelectedFiles([]);
    setSelectedAll(false);
  });
};
