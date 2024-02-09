using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Endpoints.Api;

public static class DeleteEndpoint
{
    public static IEndpointRouteBuilder AddDeleteContentApiEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("api/ep-content/{contentItemId}", ActionAsync);

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> ActionAsync(string contentItemId,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, Permissions.AccessContentApi))
        {
            return Results.Forbid();
        }

        var contentItem = await contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return Results.NotFound();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.DeleteContent, contentItem))
        {
            return Results.Forbid();
        }

        await contentManager.RemoveAsync(contentItem);

        return Results.Ok(contentItem);
    }
}
