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
using OrchardCore.Media.Controllers;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

public static class CopyMediaEndpoint
{
    public static IEndpointRouteBuilder AddCopyMediaEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/CopyMedia", HandleAsync)
            .WithName("ApiCopyMedia")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<FileStoreEntryDto>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IContentTypeProvider contentTypeProvider,
        IFileVersionProvider fileVersionProvider,
        IOptions<MediaOptions> options,
        IStringLocalizer<MediaApiController> localizer,
        string oldPath,
        string newPath)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)oldPath)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)newPath))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(oldPath) || string.IsNullOrEmpty(newPath))
        {
            return httpContext.ApiNotFoundProblem();
        }

        if (await mediaFileStore.GetFileInfoAsync(oldPath) == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        var newExtension = Path.GetExtension(newPath);

        if (!options.Value.AllowedFileExtensions.Contains(newExtension, System.StringComparer.OrdinalIgnoreCase))
        {
            return httpContext.ApiValidationProblem(detail: localizer["This file extension is not allowed: {0}", newExtension]);
        }

        if (await mediaFileStore.GetFileInfoAsync(newPath) != null)
        {
            return httpContext.ApiValidationProblem(detail: localizer["Cannot copy media because a file already exists with the same name"]);
        }

        await mediaFileStore.CopyFileAsync(oldPath, newPath);

        var copiedFile = await mediaFileStore.GetFileInfoAsync(newPath);

        return TypedResults.Ok(MediaEndpointHelpers.CreateFileResult(copiedFile, httpContext, contentTypeProvider, fileVersionProvider, mediaFileStore));
    }
}
