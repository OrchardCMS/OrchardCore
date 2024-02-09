using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Endpoints.Item;

public static class DisplayEndpoint
{
    public static IEndpointRouteBuilder AddDisplayContentEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("ep-contents/contentitems/{contentItemId}", GetAsync);

        return builder;
    }

    private static async Task<IResult> GetAsync(
        string contentItemId,
        string jsonPath,
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        IAuthorizationService authorizationService,
        HttpContext httpContext)
    {
        var contentItem = await contentManager.GetAsync(contentItemId, jsonPath);

        if (contentItem == null)
        {
            return TypedResults.NotFound();
        }

        if (!await authorizationService.AuthorizeAsync(httpContext.User, CommonPermissions.ViewContent, contentItem))
        {
            return TypedResults.Forbid();
        }

        var model = await contentItemDisplayManager.BuildDisplayAsync(contentItem, updateModelAccessor.ModelUpdater);

        // ~/OrchardCore.Contents/Views/Item/Display.cshtml
        return new EndpointView<IShape>("/Display.cshtml", model);
    }
}
