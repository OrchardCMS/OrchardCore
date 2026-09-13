using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class CopyMediaEndpoint
{
    public static IEndpointRouteBuilder AddCopyMediaEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyPost("api/media/CopyMedia", HandleLegacyAsync)
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>();

        builder.MapManagementPost("api/media/files:copy", HandleAsync)
            .WithName("ApiCopyMedia")
            .WithSummary("Copies a media file.")
            .WithDescription("Copies a media file to a new media path after validating both folder permissions and the destination file extension.")
            .WithCliCommand(new CliOperationMetadata(["media", "files"], "copy")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                InputMode = CliInputMode.Options,
                TableColumns =
                {
                    new CliTableColumnMetadata("file.name", "Name"),
                    new CliTableColumnMetadata("file.filePath", "Path"),
                    new CliTableColumnMetadata("file.url", "URL"),
                    new CliTableColumnMetadata("oldPath", "Source"),
                },
            })
            .Accepts<CopyMediaRequest>("application/json")
            .Produces<CopyMediaResultDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        string oldPath,
        string newPath)
        => HandleLegacyResultAsync(httpContext, authorizationService, mediaFileStore, contentTypeProvider, fileVersionProvider, options, localizer, oldPath, newPath);

    private static Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IContentTypeProvider contentTypeProvider,
        [FromServices] IFileVersionProvider fileVersionProvider,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        [FromBody] CopyMediaRequest request)
        => HandleManagementAsync(httpContext, authorizationService, mediaFileStore, contentTypeProvider, fileVersionProvider, options, localizer, request.OldPath, request.NewPath);

    private static async Task<IResult> HandleLegacyResultAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IOptions<MediaOptions> options,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string oldPath,
        string newPath)
    {
        var (result, copiedFile) = await CopyAsync(httpContext, authorizationService, mediaFileStore, options, localizer, oldPath, newPath);

        return result ?? TypedResults.Ok(MediaEndpointHelpers.CreateFileResult(copiedFile, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
    }

    private static async Task<IResult> HandleManagementAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IOptions<MediaOptions> options,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string oldPath,
        string newPath)
    {
        var (result, copiedFile) = await CopyAsync(httpContext, authorizationService, mediaFileStore, options, localizer, oldPath, newPath);

        return result ?? TypedResults.Ok(new CopyMediaResultDto
        {
            OldPath = oldPath,
            NewPath = newPath,
            File = MediaEndpointHelpers.CreateFileResult(copiedFile, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore),
        });
    }

    private static async Task<(IResult Result, IFileStoreEntry File)> CopyAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IOptions<MediaOptions> options,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string oldPath,
        string newPath)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)oldPath)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return (httpContext.ApiForbidProblem(), null);
        }

        if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath))
        {
            return (httpContext.ApiNotFoundProblem(), null);
        }

        if (await mediaFileStore.GetFileInfoAsync(oldPath) == null)
        {
            return (httpContext.ApiNotFoundProblem(), null);
        }

        var newExtension = Path.GetExtension(newPath);
        var canUploadRestrictedMedia = await authorizationService.AuthorizeAsync(
            httpContext.User,
            MediaPermissions.UploadRestrictedMedia);

        if (!options.Value.IsFileExtensionAllowed(newExtension, canUploadRestrictedMedia))
        {
            return (httpContext.ApiValidationProblem(detail: localizer["This file extension is not allowed: {0}", newExtension]), null);
        }

        if (await mediaFileStore.GetFileInfoAsync(newPath) != null)
        {
            return (httpContext.ApiValidationProblem(detail: localizer["Cannot copy media because a file already exists with the same name"]), null);
        }

        await mediaFileStore.CopyFileAsync(oldPath, newPath);

        var copiedFile = await mediaFileStore.GetFileInfoAsync(newPath);

        return (null, copiedFile);
    }
}
