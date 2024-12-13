using System.Text.Json.Nodes;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.Handlers;

public class TaxonomyPartHandler : ContentPartHandler<TaxonomyPart>
{
    public override Task GetContentItemAspectAsync(ContentItemAspectContext context, TaxonomyPart part)
    {
        return context.ForAsync<ContainedContentItemsAspect>(aspect =>
        {
            aspect.Accessors.Add((jsonObject) =>
            {
                return jsonObject["TaxonomyPart"]["Terms"] as JsonArray;
            });

            return Task.CompletedTask;
        });
    }
}
