using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    private static readonly char[] s_extensionSeparator = [' ', ','];

    private static readonly HashSet<string> s_emptySet = [];

    public static string GetFileName(IMediaFileStore mediaFileStore, string path)
        => Path.GetFileName(mediaFileStore.NormalizePath(path));

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

    public static async Task<DirectoryTreeNodeDto> ToDtoAsync(
        IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        DirectoryTreeNode node)
    {
        var filteredChildren = new List<DirectoryTreeNodeDto>();

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                // Only include sub-folders the user is permitted to access.
                if (!await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)child.Path))
                {
                    continue;
                }

                filteredChildren.Add(await ToDtoAsync(authorizationService, user, child));
            }
        }

        return new DirectoryTreeNodeDto
        {
            Name = node.Name,
            Path = node.Path,
            // HasChildren reflects only accessible children.
            HasChildren = filteredChildren.Count > 0,
            Children = filteredChildren,
        };
    }

    public static async Task<bool> HasSubDirectoriesAsync(
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        string path)
    {
        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            if (await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)entry.Path))
            {
                return true;
            }
        }

        return false;
    }

    public static async Task<List<FileStoreEntryDto>> GetDirectoryFoldersAsync(
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        string path)
    {
        var folders = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            // Only include folders the user is permitted to access.
            if (!await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)entry.Path))
            {
                continue;
            }

            folders.Add(CreateFolderResult(entry));
        }

        // Check HasChildren concurrently, considering only accessible sub-folders.
        var hasChildrenTasks = folders.Select(async folder =>
        {
            folder.HasChildren = await HasSubDirectoriesAsync(mediaFileStore, authorizationService, user, folder.DirectoryPath);
        });
        await Task.WhenAll(hasChildrenTasks);

        return folders;
    }

    public static async Task<List<FileStoreEntryDto>> GetDirectoryFilesAsync(
        IMediaFileStore mediaFileStore,
        HttpContext httpContext,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        HashSet<string> allowedFileExtensions,
        string path,
        string extensions)
    {
        var filterByExtensions = !string.IsNullOrWhiteSpace(extensions);
        var allowedExtensions = GetRequestedExtensions(allowedFileExtensions, extensions, false);
        var files = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetFilesAsync(path))
        {
            if (!filterByExtensions || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                files.Add(CreateFileResult(entry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }

        return files;
    }

    public static async Task CollectAllItemsRecursiveAsync(
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        string path,
        HashSet<string> allowedExtensions,
        bool filterByExtensions,
        List<FileStoreEntryDto> allItems)
    {
        var subFolders = new List<IFileStoreEntry>();

        await foreach (var entry in mediaFileStore.GetDirectoryContentAsync(path))
        {
            if (entry.IsDirectory)
            {
                // Only include and recurse into folders the user is permitted to access.
                if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)entry.Path))
                {
                    continue;
                }

                allItems.Add(CreateFolderResult(entry));
                subFolders.Add(entry);
            }
            else if (!filterByExtensions || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                allItems.Add(CreateFileResult(entry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }

        foreach (var folder in subFolders)
        {
            await CollectAllItemsRecursiveAsync(mediaFileStore, authorizationService, httpContext, contentTypeProvider, fileVersionProvider, folder.Path, allowedExtensions, filterByExtensions, allItems);
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

    public static HashSet<string> GetRequestedExtensions(HashSet<string> allowedFileExtensions, string exts, bool fallback)
    {
        if (!string.IsNullOrWhiteSpace(exts))
        {
            var extensions = exts.Split(s_extensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            return allowedFileExtensions
                .Intersect(extensions, StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        if (fallback)
        {
            return allowedFileExtensions
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return s_emptySet;
    }
}
