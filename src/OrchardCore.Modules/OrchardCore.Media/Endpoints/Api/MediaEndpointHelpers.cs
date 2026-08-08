using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
    /// <summary>
    /// Characters a folder name may not contain.
    /// </summary>
    /// <remarks>
    /// '%' is rejected because authorization resolves paths through <see cref="Uri.UnescapeDataString(string)"/>
    /// to neutralize percent-encoded traversal such as <c>%2e%2e</c>. A folder literally named
    /// <c>100%20off</c> would therefore be authorized as <c>100 off</c> — a different folder, with
    /// different permissions. Rather than drop the traversal defence, the ambiguity is removed at the
    /// source: a name that survives decoding unchanged cannot be misread.
    /// </remarks>
    public static readonly char[] InvalidFolderNameCharacters = ['\\', '/', '%'];

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

    public static async Task<DirectoryTreeNodeDto> ToDtoAsync(
        IAuthorizationService authorizationService,
        ClaimsPrincipal user,
        DirectoryTreeNode node,
        MediaPathResolutionCache pathCache = null)
    {
        var filteredChildren = new List<DirectoryTreeNodeDto>();

        if (node.Children != null)
        {
            foreach (var child in node.Children)
            {
                // The path came from the cached directory tree, which was built by enumerating the
                // store, so it needs no resolving.
                pathCache?.MarkExistingDirectory(child.Path);

                // Only include sub-folders the user is permitted to access.
                if (!await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)child.Path))
                {
                    continue;
                }

                filteredChildren.Add(await ToDtoAsync(authorizationService, user, child, pathCache));
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
        string path,
        MediaPathResolutionCache pathCache = null,
        MediaDirectoryTreeCache treeCache = null,
        bool everyFolderIsVisible = false)
    {
        if (treeCache is not null)
        {
            // The cached tree is permission-agnostic, so it can only answer where permissions cannot
            // change the answer. "No sub-directories at all" is one such case — there is nothing to be
            // denied — and it is the common one for leaf folders.
            var hasAny = await treeCache.TryGetHasChildrenAsync(path);

            if (hasAny == false)
            {
                return false;
            }

            // When the caller may see every folder, the tree's answer is exact.
            if (hasAny == true && everyFolderIsVisible)
            {
                return true;
            }
        }

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            // Enumerated by the store, so already canonical.
            pathCache?.MarkExistingDirectory(entry.Path);

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
        string path,
        MediaPathResolutionCache pathCache = null,
        MediaDirectoryTreeCache treeCache = null)
    {
        var folders = new List<FileStoreEntryDto>();

        await foreach (var entry in mediaFileStore.GetDirectoriesAsync(path))
        {
            // Enumerated by the store, so already canonical.
            pathCache?.MarkExistingDirectory(entry.Path);

            // Only include folders the user is permitted to access.
            if (!await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMediaFolder, (object)entry.Path))
            {
                continue;
            }

            folders.Add(CreateFolderResult(entry));
        }

        // Resolved once for the whole listing rather than per folder: a caller holding the global
        // permissions is not subject to per-folder restrictions, so the cached tree's answer is exact
        // and no folder has to be probed.
        var everyFolderIsVisible = await CanSeeEveryFolderAsync(authorizationService, user);

        // Check HasChildren concurrently, considering only accessible sub-folders.
        var hasChildrenTasks = folders.Select(async folder =>
        {
            folder.HasChildren = await HasSubDirectoriesAsync(
                mediaFileStore, authorizationService, user, folder.DirectoryPath, pathCache, treeCache, everyFolderIsVisible);
        });
        await Task.WhenAll(hasChildrenTasks);

        return folders;
    }

    /// <summary>
    /// Whether every media folder is visible to <paramref name="user"/>, making a permission-agnostic
    /// answer about sub-directories exact for them.
    /// </summary>
    private static async Task<bool> CanSeeEveryFolderAsync(IAuthorizationService authorizationService, ClaimsPrincipal user)
        => await authorizationService.AuthorizeAsync(user, MediaPermissions.ManageMedia)
        && await authorizationService.AuthorizeAsync(user, MediaPermissions.ViewMedia);

    /// <summary>
    /// Whether files stored directly in the media root may be listed for the current user.
    /// </summary>
    /// <remarks>
    /// Holding any first-level folder permission is enough to open the root and reach the folders below
    /// it, but the root's own files belong to no folder and are covered by <c>ViewRootMediaContent</c>
    /// alone. Without Secure Media that permission does not exist, so the root behaves like any folder.
    /// </remarks>
    public static async Task<bool> CanListRootFilesAsync(IAuthorizationService authorizationService, HttpContext httpContext)
        => !httpContext.RequestServices.IsSecureMediaEnabled()
        || await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ViewRootMedia);

    public static async Task<List<FileStoreEntryDto>> GetDirectoryFilesAsync(
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        MediaOptions mediaOptions,
        string path,
        string extensions)
    {
        var allowedExtensions = GetRequestedExtensions(mediaOptions, extensions, false);
        var files = new List<FileStoreEntryDto>();

        if (path.Length == 0 && !await CanListRootFilesAsync(authorizationService, httpContext))
        {
            return files;
        }

        await foreach (var entry in mediaFileStore.GetFilesAsync(path))
        {
            if (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(entry.Path)))
            {
                files.Add(CreateFileResult(entry, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
            }
        }

        return files;
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
