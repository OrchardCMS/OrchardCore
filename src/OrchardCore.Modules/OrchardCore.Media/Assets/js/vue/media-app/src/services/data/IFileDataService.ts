import { IFileLibraryItemDto } from "../../interfaces/interfaces";

export interface IFileDataService {
  getFileItem(path: string): Promise<IFileLibraryItemDto>;
  getFolders(path: string): Promise<IFileLibraryItemDto[]>;
  getMediaItems(path: string): Promise<IFileLibraryItemDto[]>;
  listAllItems(): Promise<IFileLibraryItemDto[]>;
  moveMedia(oldPath: string, newPath: string): Promise<void>;
  moveMediaList(mediaNames: string[], sourceFolder: string, targetFolder: string): Promise<void>;
  deleteMedia(path: string): Promise<void>;
  deleteMediaList(paths: string[]): Promise<void>;
  deleteFolder(path: string): Promise<void>;
  createFolder(path: string, name: string): Promise<IFileLibraryItemDto>;
}

/**
 * Calls the OrchardCore MediaGen2ApiController endpoints.
 */
export class FileDataService implements IFileDataService {
  private baseUrl: string;

  constructor(baseUrl: string = "/api/media-gen2") {
    this.baseUrl = baseUrl;
  }

  private async fetchJson<T>(url: string, options?: RequestInit): Promise<T> {
    const response = await fetch(url, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options?.headers,
      },
    });

    if (!response.ok) {
      const error = await response.json().catch(() => ({ title: "Error", detail: response.statusText }));
      throw error;
    }

    const text = await response.text();
    return text ? JSON.parse(text) : ({} as T);
  }

  async getFileItem(path: string): Promise<IFileLibraryItemDto> {
    return this.fetchJson<IFileLibraryItemDto>(`${this.baseUrl}/GetMediaItem?path=${encodeURIComponent(path)}`);
  }

  async getFolders(path: string): Promise<IFileLibraryItemDto[]> {
    return this.fetchJson<IFileLibraryItemDto[]>(`${this.baseUrl}/GetFolders?path=${encodeURIComponent(path)}`);
  }

  async getMediaItems(path: string): Promise<IFileLibraryItemDto[]> {
    return this.fetchJson<IFileLibraryItemDto[]>(`${this.baseUrl}/GetMediaItems?path=${encodeURIComponent(path)}`);
  }

  /**
   * Lists all items (folders and files) by recursively fetching folders and their media items.
   */
  async listAllItems(): Promise<IFileLibraryItemDto[]> {
    const allItems: IFileLibraryItemDto[] = [];
    await this.fetchItemsRecursive("", allItems);
    return allItems;
  }

  private async fetchItemsRecursive(path: string, allItems: IFileLibraryItemDto[]): Promise<void> {
    const [folders, files] = await Promise.all([
      this.getFolders(path),
      this.getMediaItems(path),
    ]);

    for (const folder of folders) {
      allItems.push(folder);
      await this.fetchItemsRecursive(folder.directoryPath, allItems);
    }

    for (const file of files) {
      allItems.push(file);
    }
  }

  async moveMedia(oldPath: string, newPath: string): Promise<void> {
    await this.fetchJson<void>(
      `${this.baseUrl}/MoveMedia?oldPath=${encodeURIComponent(oldPath)}&newPath=${encodeURIComponent(newPath)}`,
      { method: "POST" },
    );
  }

  async moveMediaList(mediaNames: string[], sourceFolder: string, targetFolder: string): Promise<void> {
    await this.fetchJson<void>(`${this.baseUrl}/MoveMediaList`, {
      method: "POST",
      body: JSON.stringify({ mediaNames, sourceFolder, targetFolder }),
    });
  }

  async deleteMedia(path: string): Promise<void> {
    await this.fetchJson<void>(`${this.baseUrl}/DeleteMedia?path=${encodeURIComponent(path)}`, { method: "POST" });
  }

  async deleteMediaList(paths: string[]): Promise<void> {
    await this.fetchJson<void>(`${this.baseUrl}/DeleteMediaList`, {
      method: "POST",
      body: JSON.stringify(paths),
    });
  }

  async deleteFolder(path: string): Promise<void> {
    await this.fetchJson<void>(`${this.baseUrl}/DeleteFolder?path=${encodeURIComponent(path)}`, { method: "POST" });
  }

  async createFolder(path: string, name: string): Promise<IFileLibraryItemDto> {
    return this.fetchJson<IFileLibraryItemDto>(
      `${this.baseUrl}/CreateFolder?path=${encodeURIComponent(path)}&name=${encodeURIComponent(name)}`,
      { method: "POST" },
    );
  }
}
