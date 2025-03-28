using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Api;
using OrchardCore.Json;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Media.Endpoints.Api;

public class GetEndpoints : IEndpoint
{
    private static readonly char[] _extensionSeparator = [' ', ','];

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("api/media/folders/{path}", GetMediaFoldersAsync);
        endpoints.MapGet("api/media/list/{path}", GetMediaItemsAsync);
        endpoints.MapGet("api/media/{path}", GetMediaItemAsync);

        return endpoints;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> GetMediaFoldersAsync(string path,
        IUserAssetFolderNameProvider userAssetFolderNameProvider,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        IOptions<MediaOptions> mediaOptions,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
                || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia, (object)path))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        if (await mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return TypedResults.NotFound();
        }

        var folderPath = mediaFileStore.Combine(mediaOptions.Value.AssetsUsersFolder, userAssetFolderNameProvider.GetUserAssetFolderName(httpContext.User));
        if (await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageOwnMedia)
            && await mediaFileStore.GetDirectoryInfoAsync(folderPath) is null)
        {
            await mediaFileStore.TryCreateDirectoryAsync(folderPath);
        }

        var allowed = new List<IFileStoreEntry>();
        await foreach (var e in mediaFileStore.GetDirectoryContentAsync(path))
        {
            if (e.IsDirectory && await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)e.Path))
            {
                allowed.Add(e);
            }
        }

        return TypedResults.Ok(allowed.Select(folder =>
        {
            var isSpecial = IsSpecialFolder(folder.Path);

            return Results.Json(new MediaFolderViewModel
            {
                Name = folder.Name,
                Path = folder.Path,
                DirectoryPath = folder.DirectoryPath,
                IsDirectory = true,
                LastModifiedUtc = folder.LastModifiedUtc,
                Length = folder.Length,
                CanCreateFolder = !isSpecial,
                CanDeleteFolder = !isSpecial
            }, documentJsonSerializerOptions.Value.SerializerOptions);
        }));

        bool IsSpecialFolder(string path) => string.Equals(path, mediaOptions.Value.AssetsUsersFolder, StringComparison.OrdinalIgnoreCase)
            || string.Equals(path, attachedMediaFieldFileService.MediaFieldsFolder, StringComparison.OrdinalIgnoreCase);
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> GetMediaItemsAsync(string path,
        string extensions,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IOptions<MediaOptions> mediaOptions,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }


        if (await mediaFileStore.GetDirectoryInfoAsync(path) == null)
        {
            return TypedResults.NotFound();
        }

        var allowedExtensions = GetRequestedExtensions(extensions, false);

        var allowed = new List<object>();

        await foreach (var mediaFile in mediaFileStore.GetDirectoryContentAsync(path))
        {
            if (!mediaFile.IsDirectory
                && (allowedExtensions.Count == 0 || allowedExtensions.Contains(Path.GetExtension(mediaFile.Path)))
                && await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)mediaFile.Path))
            {
                contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

                allowed.Add(new
                {
                    name = mediaFile.Name,
                    size = mediaFile.Length,
                    lastModify = mediaFile.LastModifiedUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
                    folder = mediaFile.DirectoryPath,
                    url = fileVersionProvider.AddFileVersionToPath(httpContext.Request.PathBase, mediaFileStore.MapPathToPublicUrl(mediaFile.Path)),
                    mediaPath = mediaFile.Path,
                    mime = contentType ?? "application/octet-stream",
                    mediaText = string.Empty,
                    anchor = new { x = 0.5f, y = 0.5f },
                    attachedFileName = string.Empty
                });
            }
        }

        return TypedResults.Ok(allowed);

        HashSet<string> GetRequestedExtensions(string exts, bool fallback)
        {
            if (!string.IsNullOrWhiteSpace(exts))
            {
                var extensions = exts.Split(_extensionSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var requestedExtensions = mediaOptions.Value.AllowedFileExtensions
                    .Intersect(extensions)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (requestedExtensions.Count > 0)
                {
                    return requestedExtensions;
                }
            }

            if (fallback)
            {
                return mediaOptions.Value.AllowedFileExtensions.ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            return [];
        }
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> GetMediaItemAsync(string path,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || (httpContext.IsSecureMediaEnabled() && !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ViewMedia, (object)(path ?? string.Empty))))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(path))
        {
            return TypedResults.NotFound();
        }

        var mediaFile = await mediaFileStore.GetFileInfoAsync(path);

        if (mediaFile is null)
        {
            return TypedResults.NotFound();
        }

        contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

        return TypedResults.Ok(new
        {
            name = mediaFile.Name,
            size = mediaFile.Length,
            lastModify = mediaFile.LastModifiedUtc.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds,
            folder = mediaFile.DirectoryPath,
            url = fileVersionProvider.AddFileVersionToPath(httpContext.Request.PathBase, mediaFileStore.MapPathToPublicUrl(mediaFile.Path)),
            mediaPath = mediaFile.Path,
            mime = contentType ?? "application/octet-stream",
            mediaText = string.Empty,
            anchor = new { x = 0.5f, y = 0.5f },
            attachedFileName = string.Empty
        });
    }
}
