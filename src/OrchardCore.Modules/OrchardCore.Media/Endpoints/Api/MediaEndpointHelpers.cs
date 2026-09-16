using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

/// <summary>
/// Shared helpers used by the media API endpoints (and the controller actions that
/// have not been converted to minimal-API endpoints yet). Dependencies are passed in
/// explicitly so the same logic can be reused from either an endpoint handler or a controller.
/// </summary>
internal static class MediaEndpointHelpers
{
    public const int DefaultTake = 50;
    public const int MaximumTake = 200;

    public static readonly char[] InvalidFolderNameCharacters = ['\\', '/'];

    private static readonly char[] s_extensionSeparator = [' ', ','];

    private static readonly HashSet<string> s_emptySet = [];

    public static bool IsBaseName(string name)
        => !string.IsNullOrWhiteSpace(name)
            && name is not "." and not ".."
            && !name.Contains('/')
            && !name.Contains('\\');

    public static Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult ValidatePaging(int skip, int take)
    {
        if (skip < 0 || take < 1)
        {
            return TypedResults.Problem(
                detail: "Skip must be zero or greater and take must be greater than zero.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return take > MaximumTake
            ? TypedResults.Problem(
                detail: $"Take cannot exceed {MaximumTake}.",
                statusCode: StatusCodes.Status400BadRequest)
            : null;
    }

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
            Url = GetFileUrl(mediaFile.Path, httpContext, fileVersionProvider, mediaFileStore),
            Mime = contentType ?? "application/octet-stream",
        };
    }

    public static string GetFileUrl(string path, HttpContext httpContext, IFileVersionProvider fileVersionProvider, IMediaFileStore mediaFileStore)
    {
        var url = fileVersionProvider.AddFileVersionToPath(httpContext.Request.PathBase, mediaFileStore.MapPathToPublicUrl(path));
        if (httpContext.GetEndpoint()?.Metadata.GetMetadata<CliOperationMetadata>() is null
            || Uri.TryCreate(url, UriKind.Absolute, out var absoluteUrl) && absoluteUrl.Scheme is "http" or "https")
        {
            return url;
        }

        // The media store already supplies the tenant/media prefix and escapes file names.
        // Resolve relative mappings without replacing a configured CDN origin or version query.
        var request = httpContext.Request;
        var tenantUrl = UriHelper.BuildAbsolute(request.Scheme, request.Host, request.PathBase.Add("/"));
        return new Uri(new Uri(tenantUrl), url).AbsoluteUri;
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
        MediaOptions mediaOptions,
        bool canUploadRestrictedMedia,
        string path,
        string extensions)
    {
        var filterByExtensions = !string.IsNullOrWhiteSpace(extensions);
        var allowedExtensions = GetRequestedExtensions(
            mediaOptions,
            extensions,
            canUploadRestrictedMedia);
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
        IMediaFileStore mediaFileStore,
        IMediaFileStoreCache mediaFileStoreCache,
        HttpContext httpContext,
        ILogger logger)
    {
        if (mediaFileStoreCache == null)
        {
            return;
        }

        // Pre-caching is an optimization; a cache failure must not fail the upload or move
        // that already succeeded in the remote store.
        try
        {
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error pre-caching remote media with path {Path}.", mediaFile.Path);
        }
    }

    public static HashSet<string> GetRequestedExtensions(
        MediaOptions mediaOptions,
        string extensions,
        bool canUploadRestrictedMedia)
    {
        if (!string.IsNullOrWhiteSpace(extensions))
        {
            return extensions
                .Split(s_extensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(extension => mediaOptions.IsFileExtensionAllowed(extension, canUploadRestrictedMedia))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        return s_emptySet;
    }

    public static HashSet<string> GetRequestedExtensions(string extensions)
        => string.IsNullOrWhiteSpace(extensions)
            ? s_emptySet
            : extensions
                .Split(s_extensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
}
