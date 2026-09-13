using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class DeleteMediaEndpoint
{
    public static IEndpointRouteBuilder AddDeleteMediaEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyPost("api/media/DeleteMedia", HandleLegacyAsync)
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>();

        builder.MapManagementDelete("api/media/file", HandleAsync)
            .WithName("ApiDeleteMedia")
            .WithSummary("Deletes a media file.")
            .WithDescription("Deletes a media file when the caller can manage both media and the containing folder.")
            .WithCliCommand(new CliOperationMetadata(["media", "files"], "delete")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                RequiresConfirmation = true,
                Arguments =
                {
                    new CliArgumentMetadata("path", 0),
                },
            })
            .Produces<DeleteMediaResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        string path)
        => HandleLegacyResultAsync(httpContext, authorizationService, mediaFileStore, path);

    private static Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [AsParameters] DeleteMediaRequest request)
        => HandleManagementAsync(httpContext, authorizationService, mediaFileStore, request.Path);

    private static async Task<IResult> HandleLegacyResultAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        string path)
    {
        var result = await DeleteAsync(httpContext, authorizationService, mediaFileStore, path);

        return result ?? TypedResults.Ok();
    }

    private static async Task<IResult> HandleManagementAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        string path)
    {
        var result = await DeleteAsync(httpContext, authorizationService, mediaFileStore, path);

        return result ?? TypedResults.Ok(new DeleteMediaResultDto { Path = path });
    }

    private static async Task<IResult> DeleteAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        string path)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia)
            || !await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
        {
            return httpContext.ApiForbidProblem();
        }

        if (string.IsNullOrEmpty(path))
        {
            return httpContext.ApiNotFoundProblem();
        }

        if (!await mediaFileStore.TryDeleteFileAsync(path))
        {
            return httpContext.ApiNotFoundProblem();
        }

        return null;
    }
}
