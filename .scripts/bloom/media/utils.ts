/**
 * Shared utility functions for media apps.
 */

/**
 * Formats a byte count into a human-readable string (e.g. "1.5 MiB" or "1.5 MB").
 * @param bytes - The number of bytes to format.
 * @param si - If true, uses SI units (kB, MB) with 1000 threshold. If false (default), uses binary units (KiB, MiB) with 1024 threshold.
 * @param dp - Number of decimal places (default 1).
 */
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

/**
 * Appends ImageSharp resize query parameters to a media URL.
 * Used for generating thumbnail URLs in the media gallery and media fields.
 * Both width and height are optional — pass at least one for resizing.
 */
export function buildMediaUrl(url: string, width?: number, height?: number): string {
  const params: string[] = [];
  if (width) params.push(`width=${width}`);
  if (height) params.push(`height=${height}`);
  if (params.length === 0 || !url) return url ?? "";
  const sep = url.indexOf("?") === -1 ? "?" : "&";
  return `${url}${sep}${params.join("&")}`;
}

/**
 * Extracts the file extension from a file name (without the dot).
 * @param fileName - The file name to extract the extension from.
 * @returns The extension (e.g. "jpg") or an empty string if none found.
 */
export function getFileExtension(fileName?: string) {
  if (fileName && fileName.includes(".")) {
    return fileName.split(".").pop();
  }

  return "";
}

/**
 * Formats a date value into a locale-specific date/time string.
 * @param datemillis - A date value (string, number, or Date object).
 * @returns The formatted date/time string or an empty string if the input is empty/null.
 */
export function printDateTime(datemillis: string | number | Date | null | undefined): string {
  if (datemillis != "" && datemillis != null && datemillis != undefined) {
    const d = new Date(datemillis);
    return d.toLocaleString();
  } else {
    return "";
  }
}
