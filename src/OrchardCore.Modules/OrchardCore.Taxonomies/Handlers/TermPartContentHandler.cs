using System.Text.Json.Nodes;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Taxonomies.Handlers
{
    public class TermPartContentHandler : ContentHandlerBase
    {
        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<ContainedContentItemsAspect>(aspect =>
            {
                // Check this content item contains Terms.
                if ((context.ContentItem.Content as JsonObject)["Terms"] is JsonArray children)
                {
                    aspect.Accessors.Add((jsonObject) =>
                    {
                        return jsonObject["Terms"] as JsonArray;
                    });
                }

                return Task.CompletedTask;
            });
        }
    }
}
