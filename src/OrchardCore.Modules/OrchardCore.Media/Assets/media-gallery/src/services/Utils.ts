import { NotificationMessage, notify } from "@bloom/services/notifications/notifier";
import { SeverityLevel } from "@bloom/services/notifications/interfaces";
import { IFileLibraryItemDto } from "@bloom/media/interfaces";
import { useGlobals } from "./Globals";
import { getTranslations } from "@bloom/helpers/localizations";
import { useRuntimeConfig } from "./RuntimeConfig";

export { humanFileSize, getFileExtension, printDateTime, buildMediaUrl } from "@bloom/media/utils";

const t = getTranslations();
const { setIsDownloading, selectedFiles, setSelectedFiles, setSelectedAll } = useGlobals();

/**
 * Resolve a server-returned media URL (typically root-relative, e.g. "/media/foo.jpg") to an
 * absolute URL against the Orchard origin, for display/download. In embedded mode the Orchard origin
 * IS the current origin, so this yields the same resource the browser would resolve for the relative
 * URL (behavior unchanged); in standalone mode it prefixes the remote Orchard origin so media loads
 * cross-origin instead of 404ing against the app origin. The raw stored `url` is left untouched
 * everywhere else (selection comparison, picker return values) — only the display boundary resolves.
 */
export function resolveMediaUrl(url?: string): string {
  if (!url) return "";
  // Already absolute (http/https/protocol-relative) — leave as-is.
  if (/^(?:https?:)?\/\//i.test(url)) return url;
  const orchardOrigin = new URL(useRuntimeConfig().orchardBaseUrl).origin;
  // Same-origin (embedded): leave the URL exactly as the server returned it (byte-for-byte behavior).
  if (orchardOrigin === window.location.origin) return url;
  // Cross-origin (standalone): prefix the Orchard origin so media resolves against the CMS instead of
  // the app origin. Root-relative media URLs already carry any tenant path prefix, so prepend origin only.
  return url.startsWith("/") ? `${orchardOrigin}${url}` : `${orchardOrigin}/${url}`;
}

export const isFileSelected = (file: IFileLibraryItemDto): boolean => {
  if (!file.url) return false;
  return selectedFiles.value.some((el) => el.url?.toLowerCase() === file.url?.toLowerCase());
};

export async function downloadFile(file: IFileLibraryItemDto) {
  if (file && file.url) {
    const mediaUrl = resolveMediaUrl(file.url);
    try {
      const response = await fetch(mediaUrl, { method: "HEAD" });
      if (response.ok) {
        const aElement = document.createElement("a");
        aElement.setAttribute("download", file.name);
        aElement.href = mediaUrl;
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
      return fetch(resolveMediaUrl(file.url))
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
