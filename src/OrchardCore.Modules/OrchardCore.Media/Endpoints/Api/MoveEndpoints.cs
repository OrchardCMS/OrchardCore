using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Api;
using OrchardCore.Json;
using OrchardCore.Modules;

namespace OrchardCore.Media.Endpoints.Api;

public class MoveEndpoints : IEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("api/media/folder/{sourceFolder}/{targetFolder}", MoveMediaListAsync);
        endpoints.MapPost("api/media/{oldPath}/{newPath}", MoveMediaAsync);

        return endpoints;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> MoveMediaListAsync(string[] mediaNames,
        string sourceFolder,
        string targetFolder,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)sourceFolder)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)targetFolder))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (mediaNames is null || mediaNames.Length < 1 || string.IsNullOrEmpty(sourceFolder) || string.IsNullOrEmpty(targetFolder))
        {
            return TypedResults.NotFound();
        }

        sourceFolder = sourceFolder == "root"
            ? string.Empty
            : sourceFolder;
        targetFolder = targetFolder == "root"
            ? string.Empty
            : targetFolder;

        var filesOnError = new List<string>();
        foreach (var name in mediaNames)
        {
            var sourcePath = mediaFileStore.Combine(sourceFolder, name);
            var targetPath = mediaFileStore.Combine(targetFolder, name);
            try
            {
                await mediaFileStore.MoveFileAsync(sourcePath, targetPath);
            }
            catch (FileStoreException)
            {
                filesOnError.Add(sourcePath);
            }
        }

        if (filesOnError.Count > 0)
        {
            return TypedResults.BadRequest(new ProblemDetails
            {
                Title = "Some files could not be moved.",
                Detail = $"The following files could not be moved: {string.Join(", ", filesOnError)}"
            });
        }

        return TypedResults.Ok();
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> MoveMediaAsync(string oldPath,
        string newPath,
        IMediaFileStore mediaFileStore,
        IAuthorizationService authorizationService,
        IFileVersionProvider fileVersionProvider,
        IServiceProvider serviceProvider,
        IOptions<MediaOptions> mediaOptions,
        IOptions<DocumentJsonSerializerOptions> documentJsonSerializerOptions,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.AccessMediaApi)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)oldPath)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath))
        {
            return TypedResults.NotFound();
        }

        if (await mediaFileStore.GetFileInfoAsync(oldPath) == null)
        {
            return TypedResults.NotFound();
        }

        var newExtension = Path.GetExtension(newPath);

        if (!mediaOptions.Value.AllowedFileExtensions.Contains(newExtension, StringComparer.OrdinalIgnoreCase))
        {
            return TypedResults.BadRequest();
        }

        if (await mediaFileStore.GetFileInfoAsync(newPath) != null)
        {
            return TypedResults.BadRequest();
        }

        await mediaFileStore.MoveFileAsync(oldPath, newPath);

        var newFileInfo = await mediaFileStore.GetFileInfoAsync(newPath);

        await PreCacheRemoteMedia(newFileInfo);

        return TypedResults.Ok(Results.Json(
            new
            {
                newUrl = fileVersionProvider.AddFileVersionToPath(httpContext.Request.PathBase, mediaFileStore.MapPathToPublicUrl(newPath))
            },
            documentJsonSerializerOptions.Value.SerializerOptions));

        async Task PreCacheRemoteMedia(IFileStoreEntry mediaFile, Stream stream = null)
        {
            var mediaFileStoreCache = serviceProvider.GetService<IMediaFileStoreCache>();
            if (mediaFileStoreCache is null)
            {
                return;
            }

            Stream localStream = null;

            if (stream is null)
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
    }
}
