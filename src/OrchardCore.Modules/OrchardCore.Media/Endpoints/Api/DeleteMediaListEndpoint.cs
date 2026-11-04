using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Media.ViewModels;
using OrchardCore.RemoteManagement;

namespace OrchardCore.Media.Endpoints.Api;

public static class DeleteMediaListEndpoint
{
    public static IEndpointRouteBuilder AddDeleteMediaListEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapLegacyPost("api/media/DeleteMediaList", HandleLegacyAsync)
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>();

        builder.MapManagementPost("api/media/files:delete", HandleAsync)
            .WithName("ApiDeleteMediaList")
            .WithSummary("Deletes multiple media files.")
            .WithDescription("Deletes a batch of media files after verifying that the caller can manage each requested folder. Paths that don't exist are reported in the result.")
            .WithCliCommand(new CliOperationMetadata(["media", "files"], "delete-batch")
            {
                Capability = MediaApiEndpointConventions.CapabilityName,
                ResourceName = "Media file",
                RequiresConfirmation = true,
                InputMode = CliInputMode.Json,
            })
            .Accepts<DeleteMediaListRequest>("application/json")
            .Produces<DeleteMediaListResultDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    private static Task<IResult> HandleLegacyAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        List<string> paths)
        => HandleLegacyResultAsync(httpContext, authorizationService, mediaFileStore, paths);

    private static Task<IResult> HandleAsync(
        HttpContext httpContext,
        [FromServices] IAuthorizationService authorizationService,
        [FromServices] IMediaFileStore mediaFileStore,
        [FromBody] DeleteMediaListRequest request)
        => HandleManagementAsync(httpContext, authorizationService, mediaFileStore, request?.Paths);

    private static async Task<IResult> HandleLegacyResultAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        List<string> paths)
    {
        if (await AuthorizeAsync(httpContext, authorizationService, paths) is { } problem)
        {
            return problem;
        }

        foreach (var path in paths)
        {
            if (!await mediaFileStore.TryDeleteFileAsync(path))
            {
                return httpContext.ApiNotFoundProblem();
            }
        }

        return TypedResults.Ok();
    }

    private static async Task<IResult> HandleManagementAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IMediaFileStore mediaFileStore,
        List<string> paths)
    {
        if (await AuthorizeAsync(httpContext, authorizationService, paths) is { } problem)
        {
            return problem;
        }

        return TypedResults.Ok(await DeleteAllAsync(mediaFileStore, paths));
    }

    internal static async Task<DeleteMediaListResultDto> DeleteAllAsync(IMediaFileStore mediaFileStore, IEnumerable<string> paths)
    {
        var deleted = new List<string>();
        var missing = new List<string>();

        foreach (var path in paths)
        {
            (await mediaFileStore.TryDeleteFileAsync(path) ? deleted : missing).Add(path);
        }

        return new DeleteMediaListResultDto { Paths = deleted, MissingPaths = missing };
    }

    private static async Task<IResult> AuthorizeAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        List<string> paths)
    {
        if (paths == null)
        {
            return httpContext.ApiNotFoundProblem();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        foreach (var path in paths)
        {
            if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMediaFolder, (object)path))
            {
                return httpContext.ApiForbidProblem();
            }
        }

        return null;
    }
}
