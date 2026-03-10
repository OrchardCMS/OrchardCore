/**
 * Shared utility functions for media apps.
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
