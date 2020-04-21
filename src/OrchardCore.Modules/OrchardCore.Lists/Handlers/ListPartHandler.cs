using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.Drivers
{
    public class ListPartHandler : ContentPartHandler<ListPart>
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context, ListPart part)
        {
            return context.ForAsync<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.AdminRouteValues = new RouteValueDictionary
                {
                    {"Area", "OrchardCore.Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Display"},
                    {"ContentItemId", context.ContentItem.ContentItemId}
                };

                return Task.CompletedTask;
            });
        }
    }
}
