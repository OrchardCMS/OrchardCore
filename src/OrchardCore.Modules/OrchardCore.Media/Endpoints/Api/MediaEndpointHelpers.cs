using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

/// <summary>
/// Shared helpers used by the media API endpoints (and the controller actions that
/// have not been converted to minimal-API endpoints yet). Dependencies are passed in
/// explicitly so the same logic can be reused from either an endpoint handler or a controller.
/// </summary>
internal static class MediaEndpointHelpers
{
    public static readonly char[] InvalidFolderNameCharacters = ['\\', '/'];

    private static readonly char[] _extensionSeparator = [' ', ','];

    private static readonly HashSet<string> _emptySet = [];

    public static FileStoreEntryDto CreateFileResult(
        IFileStoreEntry mediaFile,
        HttpContext httpContext,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IMediaFileStore mediaFileStore)
    {
        contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        return new FileStoreEntryDto
        {
            Name = mediaFile.Name,
            Size = mediaFile.Length,
            DirectoryPath = mediaFile.DirectoryPath,
            FilePath = mediaFile.Path,
            LastModifiedUtc = mediaFile.LastModifiedUtc,
            IsDirectory = false,
            Url = fileVersionProvider.AddFileVersionToPath(httpContext.Request.PathBase, mediaFileStore.MapPathToPublicUrl(mediaFile.Path)),
            Mime = contentType ?? "application/octet-stream",
        };
    }

    public static FileStoreEntryDto CreateFolderResult(IFileStoreEntry folder)
    {
        return new FileStoreEntryDto
        {
            Name = folder.Name,
            Size = folder.Length,
            DirectoryPath = folder.Path,
            LastModifiedUtc = folder.LastModifiedUtc,
            IsDirectory = true,
        };
    }

    public static DirectoryTreeNodeDto ToDto(DirectoryTreeNode node)
    {
        return new DirectoryTreeNodeDto
        {
            Name = node.Name,
            Path = node.Path,
            HasChildren = node.HasChildren,
            Children = node.Children?.ConvertAll(ToDto) ?? [],
        };
    }

    public static async Task<bool> HasSubDirectoriesAsync(IMediaFileStore mediaFileStore, string path)
    {
        await foreach (var _ in mediaFileStore.GetDirectoriesAsync(path))
        {
            return true;
        }

        return false;
    }

    public static async Task<List<FileStoreEntryDto>> GetDirectoryFoldersAsync(IMediaFileStore mediaFileStore, string path)
    {
        var folders = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            folders.Add(CreateFolderResult(entry));
        }

        // Check HasChildren concurrently.
        var hasChildrenTasks = folders.Select(async folder =>
        {
            folder.HasChildren = await HasSubDirectoriesAsync(mediaFileStore, folder.DirectoryPath);
        });
        await Task.WhenAll(hasChildrenTasks);

        return folders;
    }

    public static async Task<List<FileStoreEntryDto>> GetDirectoryFilesAsync(
        IMediaFileStore mediaFileStore,
        HttpContext httpContext,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        MediaOptions mediaOptions,
        string path,
        string extensions)
    {
        var allowedExtensions = GetRequestedExtensions(mediaOptions, extensions, false);
        var files = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetFilesAsync(path))
        {
            if (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                files.Add(CreateFileResult(entry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }

        return files;
    }

    public static async Task CollectAllItemsRecursiveAsync(
        IMediaFileStore mediaFileStore,
        HttpContext httpContext,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        string path,
        HashSet<string> allowedExtensions,
        List<FileStoreEntryDto> allItems)
    {
        var subFolders = new List<IFileStoreEntry>();

        await foreach (var entry in mediaFileStore.GetDirectoryContentAsync(path))
        {
            if (entry.IsDirectory)
            {
                allItems.Add(CreateFolderResult(entry));
                subFolders.Add(entry);
            }
            else if (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                allItems.Add(CreateFileResult(entry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }

        foreach (var folder in subFolders)
        {
            await CollectAllItemsRecursiveAsync(mediaFileStore, httpContext, contentTypeProvider, fileVersionProvider, folder.Path, allowedExtensions, allItems);
        }
    }

    public static bool IsSpecialFolder(MediaOptions mediaOptions, AttachedMediaFieldFileService attachedMediaFieldFileService, string path)
        => string.Equals(path, mediaOptions.AssetsUsersFolder, StringComparison.OrdinalIgnoreCase)
        || string.Equals(path, attachedMediaFieldFileService.MediaFieldsFolder, StringComparison.OrdinalIgnoreCase);

    public static async Task PreCacheRemoteMediaAsync(
        IFileStoreEntry mediaFile,
        IServiceProvider serviceProvider,
        IMediaFileStore mediaFileStore,
        HttpContext httpContext)
    {
        var mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();
        if (mediaFileStoreCache == null)
        {
            return;
        }

        var stream = await mediaFileStore.GetFileStreamAsync(mediaFile);
        try
        {
            await mediaFileStoreCache.SetCacheAsync(stream, mediaFile, httpContext.RequestAborted);
        }
        finally
        {
            stream?.Dispose();
        }
    }

    public static HashSet<string> GetRequestedExtensions(MediaOptions mediaOptions, string exts, bool fallback)
    {
        if (!string.IsNullOrWhiteSpace(exts))
        {
            var extensions = exts.Split(_extensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var requestedExtensions = mediaOptions.AllowedFileExtensions
                .Intersect(extensions)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (requestedExtensions.Count > 0)
            {
                return requestedExtensions;
            }
        }

        if (fallback)
        {
            return mediaOptions.AllowedFileExtensions
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return _emptySet;
    }
}
