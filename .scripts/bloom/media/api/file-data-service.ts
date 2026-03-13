import { IFileLibraryItemDto, IFileStoreCapabilities, IDirectoryTreeNode } from "../interfaces";
import { Client, FileStoreEntryDto, MoveMedias, DirectoryTreeNodeDto } from "@bloom/services/OpenApiClient";

export interface IFileDataService {
  getFileItem(path: string): Promise<IFileLibraryItemDto>;
  getFolders(path: string): Promise<IFileLibraryItemDto[]>;
  getMediaItems(path: string): Promise<IFileLibraryItemDto[]>;
  listAllItems(): Promise<IFileLibraryItemDto[]>;
  copyMedia(oldPath: string, newPath: string): Promise<IFileLibraryItemDto>;
  moveMedia(oldPath: string, newPath: string): Promise<void>;
  moveMediaList(mediaNames: string[], sourceFolder: string, targetFolder: string): Promise<void>;
  deleteMedia(path: string): Promise<void>;
  deleteMediaList(paths: string[]): Promise<void>;
  deleteFolder(path: string): Promise<void>;
  createFolder(path: string, name: string): Promise<IFileLibraryItemDto>;
  getCapabilities(): Promise<IFileStoreCapabilities>;
  getDirectoryTree(): Promise<IDirectoryTreeNode>;
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
  };
}

function toDirectoryTreeNode(dto: DirectoryTreeNodeDto): IDirectoryTreeNode {
  return {
    name: dto.name ?? "",
    path: dto.path ?? "",
    children: (dto.children ?? []).map(toDirectoryTreeNode),
  };
}

/**
 * Delegates to the NSwag-generated Client.
 */
export class FileDataService implements IFileDataService {
  private client: Client;

  constructor(baseUrl: string = "") {
    this.client = new Client(baseUrl);
  }

  async getFileItem(path: string): Promise<IFileLibraryItemDto> {
    const dto = await this.client.getMediaItem(path);
    return toFileLibraryItem(dto);
  }

  async getFolders(path: string): Promise<IFileLibraryItemDto[]> {
    const dtos = await this.client.getFolders(path);
    return dtos.map(toFileLibraryItem);
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

  async getCapabilities(): Promise<IFileStoreCapabilities> {
    const dto = await this.client.getCapabilities();
    return {
      hasHierarchicalNamespace: dto.hasHierarchicalNamespace ?? false,
      supportsAtomicMove: dto.supportsAtomicMove ?? false,
    };
  }

  async getDirectoryTree(): Promise<IDirectoryTreeNode> {
    const dto = await this.client.getDirectoryTree();
    return toDirectoryTreeNode(dto);
  }
}
