using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.Handlers;

public class TaxonomyPartHandler : ContentPartHandler<TaxonomyPart>
{
    private readonly IContentItemIdGenerator _idGenerator;

    public TaxonomyPartHandler(IContentItemIdGenerator idGenerator) => _idGenerator = idGenerator;

    public override Task CloningAsync(CloneContentContext context, TaxonomyPart part)
    {
        if (context.CloneContentItem.TryGet<TaxonomyPart>(out var clonedTaxonomyPart))
        {
            foreach (var termContentItem in clonedTaxonomyPart.Terms)
            {
                termContentItem.ContentItemId = _idGenerator.GenerateUniqueId(termContentItem);
            }

            clonedTaxonomyPart.Apply();
        }

        return Task.CompletedTask;
    }

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
