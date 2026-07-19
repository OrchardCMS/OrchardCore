using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Media.Endpoints.Api;

public static class DeleteMediaListEndpoint
{
    public static IEndpointRouteBuilder AddDeleteMediaListEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("api/media/DeleteMediaList", HandleAsync)
            .WithName("ApiDeleteMediaList")
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

        foreach (var path in paths)
        {
            if (!await mediaFileStore.TryDeleteFileAsync(path))
            {
                return httpContext.ApiNotFoundProblem();
            }
        }

        return TypedResults.Ok();
    }
}
