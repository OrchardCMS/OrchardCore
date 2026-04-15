import { IFileLibraryItemDto, IDirectoryTreeNode, IPaginatedFoldersResult, IDirectoryContentResult, IPermittedStorageResult } from "../interfaces";
import { MediaApiClient, FileStoreEntryDto, MoveMedias, DirectoryTreeNodeDto } from "@bloom/services/OpenApiClient";

export interface IFileDataService {
  getFileItem(path: string): Promise<IFileLibraryItemDto>;
  getFolders(path: string, skip?: number, take?: number): Promise<IPaginatedFoldersResult>;
  getMediaItems(path: string): Promise<IFileLibraryItemDto[]>;
  listAllItems(): Promise<IFileLibraryItemDto[]>;
  copyMedia(oldPath: string, newPath: string): Promise<IFileLibraryItemDto>;
  moveMedia(oldPath: string, newPath: string): Promise<void>;
  moveMediaList(mediaNames: string[], sourceFolder: string, targetFolder: string): Promise<void>;
  deleteMedia(path: string): Promise<void>;
  deleteMediaList(paths: string[]): Promise<void>;
  deleteFolder(path: string): Promise<void>;
  createFolder(path: string, name: string): Promise<IFileLibraryItemDto>;
  getDirectoryTree(): Promise<IDirectoryTreeNode>;
  getDirectoryContent(path: string): Promise<IDirectoryContentResult>;
  getPermittedStorage(): Promise<IPermittedStorageResult>;
}

function toFileLibraryItem(dto: FileStoreEntryDto): IFileLibraryItemDto {
  return {
    name: dto.name ?? "",
    size: dto.size,
    directoryPath: dto.directoryPath ?? "",
    filePath: dto.filePath ?? "",
    lastModifiedUtc: dto.lastModifiedUtc?.toISOString(),
    isDirectory: dto.isDirectory ?? false,
    url: dto.url,
    mime: dto.mime,
    hasChildren: dto.hasChildren ?? undefined,
  };
}

function toDirectoryTreeNode(dto: DirectoryTreeNodeDto): IDirectoryTreeNode {
  return {
    name: dto.name ?? "",
    path: dto.path ?? "",
    hasChildren: dto.hasChildren ?? (dto.children ?? []).length > 0,
    children: (dto.children ?? []).map(toDirectoryTreeNode),
  };
}

/**
 * Delegates to the NSwag-generated Client.
 */
export class FileDataService implements IFileDataService {
  private client: MediaApiClient;

  constructor(baseUrl: string = "") {
    // NSwag appends "/api/..." to baseUrl; a trailing "/" would produce
    // "//api/..." which the browser parses as protocol-relative.
    const normalized = baseUrl.endsWith("/") ? baseUrl.slice(0, -1) : baseUrl;
    this.client = new MediaApiClient(normalized);
  }

  async getFileItem(path: string): Promise<IFileLibraryItemDto> {
    const dto = await this.client.getMediaItem(path);
    return toFileLibraryItem(dto);
  }

  async getFolders(path: string, skip?: number, take?: number): Promise<IPaginatedFoldersResult> {
    const dto = await this.client.getFolders(path, skip, take);
    return {
      items: (dto.items ?? []).map(toFileLibraryItem),
      hasMore: dto.hasMore ?? false,
    };
  }

  async getMediaItems(path: string): Promise<IFileLibraryItemDto[]> {
    const dtos = await this.client.getMediaItems(path, undefined);
    return dtos.map(toFileLibraryItem);
  }

  async listAllItems(): Promise<IFileLibraryItemDto[]> {
    const dtos = await this.client.getAllMediaItems(undefined);
    return dtos.map(toFileLibraryItem);
  }

  async copyMedia(oldPath: string, newPath: string): Promise<IFileLibraryItemDto> {
    const dto = await this.client.copyMedia(oldPath, newPath);
    return toFileLibraryItem(dto);
  }

  async moveMedia(oldPath: string, newPath: string): Promise<void> {
    await this.client.moveMedia(oldPath, newPath);
  }

  async moveMediaList(mediaNames: string[], sourceFolder: string, targetFolder: string): Promise<void> {
    const body = new MoveMedias({ mediaNames, sourceFolder, targetFolder });
    await this.client.moveMediaList(body);
  }

  async deleteMedia(path: string): Promise<void> {
    await this.client.deleteMedia(path);
  }

  async deleteMediaList(paths: string[]): Promise<void> {
    await this.client.deleteMediaList(paths);
  }

  async deleteFolder(path: string): Promise<void> {
    await this.client.deleteFolder(path);
  }

  async createFolder(path: string, name: string): Promise<IFileLibraryItemDto> {
    const dto = await this.client.createFolder(path, name);
    return toFileLibraryItem(dto);
  }

  async getDirectoryTree(): Promise<IDirectoryTreeNode> {
    const dto = await this.client.getDirectoryTree();
    return toDirectoryTreeNode(dto);
  }

  async getDirectoryContent(path: string): Promise<IDirectoryContentResult> {
    const dto = await this.client.getDirectoryContent(path, undefined);
    return {
      folders: (dto.folders ?? []).map(toFileLibraryItem),
      files: (dto.files ?? []).map(toFileLibraryItem),
    };
  }

  async getPermittedStorage(): Promise<IPermittedStorageResult> {
    const dto = await this.client.getPermittedStorage();
    return {
      bytes: dto.bytes ?? null,
      text: dto.text ?? "",
    };
  }
}
