using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Json;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Endpoints.Api;

public static class DeleteEndpoint
{
    public static IEndpointRouteBuilder AddDeleteContentEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("api/content/{contentItemId}", HandleAsync)
            .AllowAnonymous()
            .DisableAntiforgery();

        return builder;
    }

    [Authorize(AuthenticationSchemes = "Api")]
    private static async Task<IResult> HandleAsync(
        string contentItemId,
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        HttpContext httpContext,
        IOptions<DocumentJsonSerializerOptions> options)
    {
        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.AccessContentApi))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        var contentItem = await contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return TypedResults.NotFound();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.DeleteContent, contentItem))
        {
            return httpContext.ChallengeOrForbid("Api");
        }

        await contentManager.RemoveAsync(contentItem);

        return Results.Json(contentItem, options.Value.SerializerOptions);
    }
}
