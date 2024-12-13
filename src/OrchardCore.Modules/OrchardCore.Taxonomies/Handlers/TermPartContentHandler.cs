using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Taxonomies.Handlers;

public class TermPartContentHandler : ContentHandlerBase
{
    public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
    {
        return context.ForAsync<ContainedContentItemsAspect>(aspect =>
        {
            // Check this content item contains Terms.
            if (((JsonNode)context.ContentItem.Content)["Terms"] is JsonArray)
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
