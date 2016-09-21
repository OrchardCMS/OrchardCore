using Microsoft.AspNetCore.Routing;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Lists.Models;

namespace Orchard.Lists.Drivers
{
    public class ListPartDriver : ContentPartDriver<ListPart>
    {
        protected override void GetContentItemMetadata(ContentItemMetadataContext context, ListPart part)
        {
            context.Metadata.AdminRouteValues = new RouteValueDictionary
            {
                {"Area", "Orchard.Contents"},
                {"Controller", "Admin"},
                {"Action", "Display"},
                {"Id", context.ContentItem.ContentItemId}
            };
        }
    }
}