using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.FileStorage;
using OrchardCore.Media.Controllers;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class CreateFolderEndpoint
{
    public static IEndpointRouteBuilder AddCreateFolderEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/CreateFolder", HandleAsync)
            .WithName("ApiCreateFolder")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<FileStoreEntryDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IMediaNameNormalizerService mediaNameNormalizerService,
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiController> localizer,
        string path,
        string name)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = string.Empty;
        }

        name = mediaNameNormalizerService.NormalizeFolderName(name);

        if (MediaEndpointHelpers.InvalidFolderNameCharacters.Any(invalidChar => name.Contains(invalidChar)))
        {
            return httpContext.ApiValidationProblem(detail: localizer["Cannot create folder because the folder name contains invalid characters"]);
        }

        if (MediaEndpointHelpers.IsSpecialFolder(options.Value, attachedMediaFieldFileService, path))
        {
            return httpContext.ApiValidationProblem(detail: localizer["Cannot create folder inside a system folder"]);
        }

        var newPath = mediaFileStore.Combine(path, name);

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return httpContext.ApiForbidProblem();
        }

        var mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(newPath);
        if (mediaFolder != null)
        {
            return httpContext.ApiValidationProblem(detail: localizer["Cannot create folder because a folder already exists with the same name"]);
        }

        var existingFile = await mediaFileStore.GetFileInfoAsync(newPath);
        if (existingFile != null)
        {
            return httpContext.ApiValidationProblem(detail: localizer["Cannot create folder because a file already exists with the same name"]);
        }

        await mediaFileStore.TryCreateDirectoryAsync(newPath);
        directoryTreeCache.Invalidate();

        mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(newPath);

        return TypedResults.Ok(MediaEndpointHelpers.CreateFolderResult(mediaFolder));
    }
}
