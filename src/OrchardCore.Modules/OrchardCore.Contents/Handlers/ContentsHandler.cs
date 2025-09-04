using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Environment.Cache;

namespace OrchardCore.Contents.Handlers;

public class ContentsHandler : ContentHandlerBase
{
    private readonly ITagCache _tagCache;

    public ContentsHandler(ITagCache tagCache)
    {
        _tagCache = tagCache;
    }

    public override Task PublishedAsync(PublishContentContext context)
    {
        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }

    public override Task RemovedAsync(RemoveContentContext context)
    {
        if (context.NoActiveVersionLeft)
        {
            return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
        }

        return Task.CompletedTask;
    }

    public override Task UnpublishedAsync(PublishContentContext context)
    {
        return _tagCache.RemoveTagAsync($"contentitemid:{context.ContentItem.ContentItemId}");
    }

    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<ContentItemMetadata>(metadata =>
        {
            metadata.CreateRouteValues ??= new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Create" },
                { "Id", context.ContentItem.ContentType },
            };

            metadata.EditorRouteValues ??= new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Edit" },
                { "ContentItemId", context.ContentItem.ContentItemId },
            };

            metadata.AdminRouteValues ??= new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Edit" },
                { "ContentItemId", context.ContentItem.ContentItemId },
            };

            metadata.DisplayRouteValues ??= new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Item" },
                { "Action", "Display" },
                { "ContentItemId", context.ContentItem.ContentItemId },
            };

            metadata.RemoveRouteValues ??= new RouteValueDictionary
            {
                { "Area", "OrchardCore.Contents" },
                { "Controller", "Admin" },
                { "Action", "Remove" },
                { "ContentItemId", context.ContentItem.ContentItemId },
            };

            return Task.CompletedTask;
        });
    }
}
