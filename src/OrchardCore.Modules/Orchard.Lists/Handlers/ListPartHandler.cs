using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartHandler : ContentPartHandler<ListPart>
    {
        public override void GetContentItemAspect(ContentItemAspectContext context, ListPart part)
        {
            context.For<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.AdminRouteValues = new RouteValueDictionary
                {
                    {"Area", "Orchard.Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Display"},
                    {"ContentItemId", context.ContentItem.ContentItemId}
                };
            });
        }
    }
}