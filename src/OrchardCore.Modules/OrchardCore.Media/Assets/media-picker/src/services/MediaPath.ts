import type { IFileLibraryItemDto } from "@bloom/media/interfaces";
import type { IMediaFieldPath } from "../interfaces/MediaFieldTypes";

export function normalizeMediaPath(path?: string | null): string | undefined {
  if (typeof path !== "string") {
    return undefined;
  }

  const normalized = path.trim();
  if (!normalized) {
    return undefined;
  }

  const lower = normalized.toLowerCase();
  if (lower === "undefined" || lower === "null") {
    return undefined;
  }

  return normalized;
}

export function isValidMediaPath(path?: string | null): boolean {
  return normalizeMediaPath(path) !== undefined;
}

export function sanitizeFieldPaths(paths: IMediaFieldPath[] | undefined | null): IMediaFieldPath[] {
  if (!paths || paths.length === 0) {
    return [];
  }

  return paths
    .map((entry) => {
      const path = normalizeMediaPath(entry?.path);
      if (!path) {
        return null;
      }

      return {
        ...entry,
        path,
      } as IMediaFieldPath;
    })
    .filter((entry): entry is IMediaFieldPath => entry !== null);
}

export function resolvePickerFilePath(file: IFileLibraryItemDto): string | undefined {
  const fromFilePath = normalizeMediaPath(file.filePath);
  if (fromFilePath) {
    return fromFilePath;
  }

  const name = normalizeMediaPath(file.name);
  if (!name) {
    return undefined;
  }

  const directoryPath = normalizeMediaPath(file.directoryPath) ?? "";
  if (!directoryPath) {
    return name;
  }

  return `${directoryPath}/${name}`;
}
