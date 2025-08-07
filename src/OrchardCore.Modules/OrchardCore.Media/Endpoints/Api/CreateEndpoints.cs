using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Api;
using OrchardCore.Json;
using OrchardCore.Media.Services;
using OrchardCore.Modules;

namespace OrchardCore.Media.Endpoints.Api;

public class CreateEndpoints : IEndpoint
{
    private static readonly char[] _extensionSeparator = [' ', ','];
    private static readonly char[] _invalidFolderNameCharacters = ['\\', '/'];

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/media/folder/{path}", CreateFolderAsync);
        endpoints.MapPost("api/media/{path}", UploadMediaAsync);

        return endpoints;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> CreateFolderAsync(string path,
        string name,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        IMediaNameNormalizerService mediaNameNormalizerService,
        HttpContext httpContext,
        IStringLocalizer<CreateEndpoints> S)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        name = mediaNameNormalizerService.NormalizeFolderName(name);

        if (_invalidFolderNameCharacters.Any(invalidChar => name.Contains(invalidChar)))
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = S["Cannot create folder because the folder name contains invalid characters"]
            });
        }

        var newPath = mediaFileStore.Combine(path, name);
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(newPath);
        if (mediaFolder != null)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = S["Cannot create folder because a file already exists with the same name"]
            });
        }

        var existingFile = await mediaFileStore.GetFileInfoAsync(newPath);
        if (existingFile is not null)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = S["Cannot create folder because a file already exists with the same name"]
            });
        }

        await mediaFileStore.TryCreateDirectoryAsync(newPath);

        mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(newPath);

        return TypedResults.Ok(Results.Json(mediaFolder));
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> UploadMediaAsync(string path,
        string extensions,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        IFileVersionProvider fileVersionProvider,
        IChunkFileUploadService chunkFileUploadService,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IContentTypeProvider contentTypeProvider,
        IServiceProvider serviceProvider,
        IOptions<MediaOptions> mediaOptions,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions,
        HttpContext httpContext,
        IStringLocalizer<CreateEndpoints> S)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || (httpContext.IsSecureMediaEnabled() && !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ViewMedia, (object)(path ?? string.Empty))))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var allowedExtensions = GetRequestedExtensions(extensions, true);
        var filesOnError = new List<string>();

        await chunkFileUploadService.ProcessRequestAsync(
            httpContext.Request,
            (_, _, _) => Task.FromResult<IActionResult>(new OkObjectResult(new { })),
            async (files) =>
            {
                if (string.IsNullOrEmpty(path))
                {
                    path = string.Empty;
                }

                var result = new List<object>();
                foreach (var file in files)
                {
                    var extension = Path.GetExtension(file.FileName);
                    if (!allowedExtensions.Contains(extension))
                    {
                        filesOnError.Add(file.FileName);

                        result.Add(new
                        {
                            name = file.FileName,
                            size = file.Length,
                            folder = path,
                            error = S["This file extension is not allowed: {0}", extension].ToString()
                        });

                        continue;
                    }

                    var fileName = mediaNameNormalizerService.NormalizeFileName(file.FileName);

                    Stream stream = null;
                    try
                    {
                        var mediaFilePath = mediaFileStore.Combine(path, fileName);
                        stream = file.OpenReadStream();

                        mediaFilePath = await mediaFileStore.CreateFileFromStreamAsync(mediaFilePath, stream);

                        var mediaFile = await mediaFileStore.GetFileInfoAsync(mediaFilePath);

                        try
                        {
                            stream.Position = 0;
                        }
                        catch (ObjectDisposedException)
                        {
                            stream = null;
                        }

                        await PreCacheRemoteMedia(mediaFile, stream);

                        contentTypeProvider.TryGetContentType(mediaFile.Name, out var contentType);

                        result.Add(new
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
                    catch
                    {
                        filesOnError.Add(fileName);
                    }
                    finally
                    {
                        stream?.Dispose();
                    }
                }

                return new OkObjectResult(new { files = result.ToArray() });
            });

        if (filesOnError.Count > 0)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Some files could not be uploaded.",
                Detail = $"The following files could not be uploaded: {string.Join(", ", filesOnError)}"
            });
        }
        else
        {
            return TypedResults.Ok();
        }

        async Task PreCacheRemoteMedia(IFileStoreEntry mediaFile, Stream stream = null)
        {
            var mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();
            if (mediaFileStoreCache == null)
            {
                return;
            }

            Stream localStream = null;

            if (stream == null)
            {
                stream = localStream = await mediaFileStore.GetFileStreamAsync(mediaFile);
            }

            try
            {
                await mediaFileStoreCache.SetCacheAsync(stream, mediaFile, httpContext.RequestAborted);
            }
            finally
            {
                localStream?.Dispose();
            }
        }

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
}
