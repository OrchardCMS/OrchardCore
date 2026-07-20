using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Media.Endpoints.Api;

public static class DeleteMediaEndpoint
{
    public static IEndpointRouteBuilder AddDeleteMediaEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/DeleteMedia", HandleAsync)
            .WithName("ApiDeleteMedia")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .AddEndpointFilter<MediaApiAntiforgeryEndpointFilter>()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return builder;
    }

    [Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
    private static async Task<IResult> HandleAsync(
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

        return TypedResults.Ok();
    }
}
