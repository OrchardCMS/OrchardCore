using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Services;

namespace OrchardCore.Media.Endpoints.Api;

public static class DeleteFolderEndpoint
{
    public static IEndpointRouteBuilder AddDeleteFolderEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/DeleteFolder", HandleAsync)
            .WithName("ApiDeleteFolder")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces(StatusCodes.Status200OK)
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
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(path))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Cannot delete root media folder"]);
        }

        if (MediaEndpointHelpers.IsSpecialFolder(options.Value, attachedMediaFieldFileService, path))
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Cannot delete a system folder"]);
        }

        var mediaFolder = await mediaFileStore.GetDirectoryInfoAsync(path);
        if (mediaFolder != null && !mediaFolder.IsDirectory)
        {
            return httpContext.ApiBadRequestProblem(detail: localizer["Cannot delete path because it is not a directory"]);
        }

        if (await mediaFileStore.TryDeleteDirectoryAsync(path) == false)
        {
            return httpContext.ApiNotFoundProblem();
        }

        directoryTreeCache.Invalidate();

        return TypedResults.Ok();
    }
}
