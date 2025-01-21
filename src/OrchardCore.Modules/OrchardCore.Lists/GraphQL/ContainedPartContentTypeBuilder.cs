using GraphQL.Types;
using OrchardCore.ContentManagement.GraphQL.Queries.Types;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Lists.Models;

namespace OrchardCore.Lists.GraphQL;

internal sealed class ContainedPartContentTypeBuilder : IContentTypeBuilder
{
    public void Build(ISchema schema, FieldType contentQuery, ContentTypeDefinition contentTypeDefinition, ContentItemType contentItemType)
    {
        foreach (var listPart in contentTypeDefinition.Parts.Where(p => p.PartDefinition.Name.Equals(nameof(ListPart), StringComparison.OrdinalIgnoreCase)))
        {
            var settings = listPart?.GetSettings<ListPartSettings>();
            if (settings == null)
            {
                continue;
            }

            if (contentItemType.Metadata.TryGetValue("ContainedContentTypes", out var containedContentTypes) &&
                containedContentTypes is IEnumerable<string> existingContainedContentTypes)
            {
                contentItemType.Metadata[nameof(ListPartSettings.ContainedContentTypes)] = existingContainedContentTypes.Concat(settings.ContainedContentTypes).Distinct().ToArray();
            }
            else
            {
                contentItemType.Metadata[nameof(ListPartSettings.ContainedContentTypes)] = settings.ContainedContentTypes;
            }
        }
    }

    public void Clear()
    {
    }
}
