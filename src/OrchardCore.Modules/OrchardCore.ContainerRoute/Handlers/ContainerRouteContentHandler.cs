using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace OrchardCore.ContainerRoute.Handlers
{
    public class ContainerRouteContentHandler : ContentHandlerBase
    {
        private readonly IContainerRouteEntries _containerRouteEntries;
        public ContainerRouteContentHandler(IContainerRouteEntries containerRouteEntries)
        {
            _containerRouteEntries = containerRouteEntries;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<ContentItemMetadata>(metadata =>
            {
                if (_containerRouteEntries.TryGetContainerRouteEntryByContentItemId(context.ContentItem.ContentItemId, out var containerRouteEntry))
                {
                    metadata.DisplayRouteValues = new RouteValueDictionary {
                        { "Area", "OrchardCore.Contents" },
                        { "Controller", "Item" },
                        { "Action", "Display" },
                        { "ContentItemId", containerRouteEntry.ContainerContentItemId},
                        { "ContainedContentItemId", containerRouteEntry.ContainedContentItemId }
                    };
                }

                return Task.CompletedTask;
            });
        }
    }
}
