import type { IMediaFieldConfig, IMediaFieldItem, IMediaFieldPath } from "../interfaces/MediaFieldTypes";
import { normalizeMediaPath } from "./MediaPath";

type IMediaFieldLookupConfig = Pick<IMediaFieldConfig, "mediaItemUrl" | "mediaItemsUrl">;

function createErrorItem(path: IMediaFieldPath, index: number, errorType: "transient" | "not-found"): IMediaFieldItem {
  return {
    name: path.path,
    mime: "",
    mediaPath: path.path,
    errorType,
    mediaText: path.mediaText,
    anchor: path.anchor,
    attachedFileName: path.attachedFileName,
    vuekey: path.path + index,
  };
}

function createLoadedItem(data: Record<string, unknown>, path: IMediaFieldPath, index: number): IMediaFieldItem {
  const resolvedPath = normalizeMediaPath(data.mediaPath ?? data.filePath) ?? path.path;
  const name = typeof data.name === "string" ? data.name : path.path;

  return {
    ...data,
    mediaPath: resolvedPath,
    mediaText: path.mediaText,
    anchor: path.anchor,
    attachedFileName: path.attachedFileName,
    vuekey: name + index,
  } as IMediaFieldItem;
}

async function loadMediaItemsIndividually(paths: IMediaFieldPath[], config: IMediaFieldLookupConfig): Promise<IMediaFieldItem[]> {
  return Promise.all(
    paths.map(async (path, index) => {
      try {
        const url = `${config.mediaItemUrl}?path=${encodeURIComponent(path.path)}`;
        const response = await fetch(url);
        if (!response.ok) {
          return createErrorItem(path, index, response.status === 404 ? "not-found" : "transient");
        }

        const data = await response.json() as Record<string, unknown>;
        return createLoadedItem(data, path, index);
      } catch {
        return createErrorItem(path, index, "transient");
      }
    })
  );
}

async function loadMediaItemsBatch(paths: IMediaFieldPath[], config: IMediaFieldLookupConfig): Promise<IMediaFieldItem[] | null> {
  if (!config.mediaItemsUrl) {
    return null;
  }

  try {
    const searchParams = new URLSearchParams();
    for (const path of paths) {
      searchParams.append("paths", path.path);
    }

    const response = await fetch(`${config.mediaItemsUrl}?${searchParams.toString()}`);
    if (!response.ok) {
      return null;
    }

    const data = await response.json() as Record<string, unknown>[];
    if (!Array.isArray(data)) {
      return null;
    }

    const mediaItemsByPath = new Map<string, Record<string, unknown>>();
    for (const item of data) {
      const mediaPath = normalizeMediaPath(item.mediaPath ?? item.filePath);
      if (mediaPath) {
        mediaItemsByPath.set(mediaPath, item);
      }
    }

    return paths.map((path, index) => {
      const normalizedPath = normalizeMediaPath(path.path) ?? path.path;
      const mediaItem = mediaItemsByPath.get(normalizedPath);
      return mediaItem
        ? createLoadedItem(mediaItem, path, index)
        : createErrorItem(path, index, "not-found");
    });
  } catch {
    return null;
  }
}

export async function loadInitialMediaItems(paths: IMediaFieldPath[], config: IMediaFieldLookupConfig): Promise<IMediaFieldItem[]> {
  const batchLoadedItems = await loadMediaItemsBatch(paths, config);
  if (batchLoadedItems) {
    return batchLoadedItems;
  }

  return loadMediaItemsIndividually(paths, config);
}