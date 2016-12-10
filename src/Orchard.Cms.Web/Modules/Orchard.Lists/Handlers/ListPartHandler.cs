using Microsoft.AspNetCore.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Lists.Models;

namespace Orchard.Lists.Drivers
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