using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Media.ViewModels;

namespace OrchardCore.Media.Endpoints.Api;

/// <summary>
/// Provides the tenant's upload limits to authenticated media clients.
/// </summary>
public static class GetUploadOptionsEndpoint
{
    /// <summary>
    /// Maps the Media API upload-options endpoint.
    /// </summary>
    public static IEndpointRouteBuilder AddGetUploadOptionsEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("api/media/GetUploadOptions", HandleAsync)
            .WithName("ApiGetUploadOptions")
            .WithTags("MediaApi")
            .DisableAntiforgery()
            .Produces<MediaUploadOptionsDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        return builder;
    }

    [Authorize(Policy = MediaApiConstants.AuthorizationPolicyName)]
    private static async Task<IResult> HandleAsync(
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        IOptions<MediaOptions> options)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, MediaPermissions.ManageMedia))
        {
            return httpContext.ApiForbidProblem();
        }

        httpContext.Response.Headers.CacheControl = "no-store";

        return TypedResults.Ok(new MediaUploadOptionsDto { MaxFileSize = options.Value.MaxFileSize });
    }
}
