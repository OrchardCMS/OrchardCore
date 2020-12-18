using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Autoroute.Handlers
{
    public class AutorouteContentHandler : ContentHandlerBase
    {
        private readonly IAutorouteEntries _autorouteEntries;

        public AutorouteContentHandler(IAutorouteEntries autorouteEntries)
        {
            _autorouteEntries = autorouteEntries;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<ContentItemMetadata>(async metadata =>
            {
                // When a content item is contained we provide different route values when generating urls.
                (var found, var entry) = await _autorouteEntries.TryGetEntryByContentItemIdAsync(context.ContentItem.ContentItemId);

                if (found && !string.IsNullOrEmpty(entry.ContainedContentItemId))
                {
                    metadata.DisplayRouteValues = new RouteValueDictionary {
                        { "Area", "OrchardCore.Contents" },
                        { "Controller", "Item" },
                        { "Action", "Display" },
                        { "ContentItemId", entry.ContentItemId},
                        { "ContainedContentItemId", entry.ContainedContentItemId }
                    };
                }
            });
        }
    }
}
