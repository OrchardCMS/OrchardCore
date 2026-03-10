import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { IFileLibraryItemDto } from "../interfaces/interfaces";
import { useGlobals } from "./Globals";
import { useLocalizations } from "./Localizations";

const { translations } = useLocalizations();
const t = translations.value;
const { setIsDownloading, selectedFiles, setSelectedFiles, setSelectedAll } = useGlobals();

export function humanFileSize(bytes: number | null | undefined, si: boolean = false, dp: number = 1): string {
  if (bytes === null || bytes === undefined) {
    throw new Error("humanFileSize: bytes is null or undefined");
  }

  const thresh = si ? 1000 : 1024;

  if (Math.abs(bytes) < thresh) {
    return bytes + " B";
  }

  const units = si ? ["kB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"] : ["KiB", "MiB", "GiB", "TiB", "PiB", "EiB", "ZiB", "YiB"];
  let u = -1;
  const r = 10 ** dp;

  do {
    bytes /= thresh;
    ++u;
  } while (Math.round(Math.abs(bytes) * r) / r >= thresh && u < units.length - 1);

  return bytes.toFixed(dp) + " " + units[u];
}

export function getFileExtension(fileName?: string) {
  if (fileName && fileName.includes(".")) {
    return fileName.split(".").pop();
  }

  return "";
}

export function printDateTime(datemillis: string | number | Date | null | undefined): string {
  if (datemillis != "" && datemillis != null && datemillis != undefined) {
    const d = new Date(datemillis);
    return d.toLocaleString();
  } else {
    return "";
  }
}

export function downloadFile(file: IFileLibraryItemDto) {
  if (file && file.url) {
    const xhr = new XMLHttpRequest();
    xhr.open("HEAD", file.url, false);

    xhr.onload = function () {
      if (xhr.status === 200) {
        const aElement = document.createElement("a");
        if (aElement) {
          aElement.setAttribute("download", file.name);
          aElement.href = file.url ?? "";
          aElement.setAttribute("target", "_blank");
          aElement.click();
        }
      } else {
        notify(new NotificationMessage({ summary: t.Error, detail: t.FailedDownload, severity: SeverityLevel.Error }));
      }
    };

    xhr.onerror = function () {
      notify(new NotificationMessage({ summary: t.Error, detail: t.FailedDownload, severity: SeverityLevel.Error }));
    };

    xhr.send();
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
