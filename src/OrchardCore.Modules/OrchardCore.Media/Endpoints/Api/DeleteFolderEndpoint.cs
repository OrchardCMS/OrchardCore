using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Media.Services;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class DeleteFolderEndpoint
{
    public static IEndpointRouteBuilder AddDeleteFolderEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyPost("api/media/DeleteFolder", HandleLegacyAsync)
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>();

        builder.MapManagementDelete("api/media/folder", HandleAsync)
            .WithName("ApiDeleteFolder")
            .WithSummary("Deletes a media folder.")
            .WithDescription("Deletes a media folder when it is not the root or a protected system folder and the caller is allowed to manage it.")
            .WithCliCommand(new CliOperationMetadata(["media", "folders"], "delete")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                RequiresConfirmation = true,
                Arguments =
                {
                    new CliArgumentMetadata("path", 0),
                },
            })
            .Produces<DeleteFolderResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] AttachedMediaFieldFileService attachedMediaFieldFileService,
        [FromServices] MediaDirectoryTreeCache directoryTreeCache,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        string path)
        => HandleLegacyResultAsync(httpContext, authorizationService, mediaFileStore, options, attachedMediaFieldFileService, directoryTreeCache, localizer, path);

    private static Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromServices] IOptions<MediaOptions> options,
        [FromServices] AttachedMediaFieldFileService attachedMediaFieldFileService,
        [FromServices] MediaDirectoryTreeCache directoryTreeCache,
        [FromServices] IStringLocalizer<MediaApiEndpoints> localizer,
        [AsParameters] DeleteFolderRequest request)
        => HandleManagementAsync(httpContext, authorizationService, mediaFileStore, options, attachedMediaFieldFileService, directoryTreeCache, localizer, request.Path);

    private static async Task<IResult> HandleLegacyResultAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path)
    {
        var result = await DeleteAsync(httpContext, authorizationService, mediaFileStore, options, attachedMediaFieldFileService, directoryTreeCache, localizer, path);

        return result ?? TypedResults.Ok();
    }

    private static async Task<IResult> HandleManagementAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        IOptions<MediaOptions> options,
        AttachedMediaFieldFileService attachedMediaFieldFileService,
        MediaDirectoryTreeCache directoryTreeCache,
        IStringLocalizer<MediaApiEndpoints> localizer,
        string path)
    {
        var result = await DeleteAsync(httpContext, authorizationService, mediaFileStore, options, attachedMediaFieldFileService, directoryTreeCache, localizer, path);

        return result ?? TypedResults.Ok(new DeleteFolderResultDto { Path = path });
    }

    private static async Task<IResult> DeleteAsync(
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

        return null;
    }
}
