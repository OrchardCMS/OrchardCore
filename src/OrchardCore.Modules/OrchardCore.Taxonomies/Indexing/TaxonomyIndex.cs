using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using OrchardCore.Taxonomies.Fields;
using YesSql.Indexes;

namespace OrchardCore.Taxonomies.Indexing;

public class TaxonomyIndex : MapIndex
{
    public string TaxonomyContentItemId { get; set; }
    public string ContentItemId { get; set; }
    public string ContentType { get; set; }
    public string ContentPart { get; set; }
    public string ContentField { get; set; }
    public string TermContentItemId { get; set; }
    public bool Published { get; set; }
    public bool Latest { get; set; }
}

public class TaxonomyIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly HashSet<string> _ignoredTypes = [];
    private IContentDefinitionManager _contentDefinitionManager;

    public TaxonomyIndexProvider(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<TaxonomyIndex>()
            .Map(async contentItem =>
            {
                // Remove index records of soft deleted items.
                if (!contentItem.Published && !contentItem.Latest)
                {
                    return null;
                }

                // Can we safely ignore this content item?
                if (_ignoredTypes.Contains(contentItem.ContentType))
                {
                    return null;
                }

                // Lazy initialization because of ISession cyclic dependency
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for Taxonomy fields
                var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

                // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
                if (contentTypeDefinition == null)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                var fieldDefinitions = contentTypeDefinition
                    .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)))
                    .ToArray();

                // This type doesn't have any TaxonomyField, ignore it
                if (fieldDefinitions.Length == 0)
                {
                    _ignoredTypes.Add(contentItem.ContentType);
                    return null;
                }

                var results = new List<TaxonomyIndex>();

                // Get all field values
                foreach (var fieldDefinition in fieldDefinitions)
                {
                    var jPart = contentItem.Content[fieldDefinition.PartDefinition.Name];
                    if (jPart is null)
                    {
                        continue;
                    }

                    var jField = jPart[fieldDefinition.Name];
                    if (jField is null)
                    {
                        continue;
                    }

                    var field = ((JsonObject)jField).ToObject<TaxonomyField>();

                    foreach (var termContentItemId in field.TermContentItemIds)
                    {
                        results.Add(new TaxonomyIndex
                        {
                            TaxonomyContentItemId = field.TaxonomyContentItemId,
                            ContentItemId = contentItem.ContentItemId,
                            ContentType = contentItem.ContentType,
                            ContentPart = fieldDefinition.PartDefinition.Name,
                            ContentField = fieldDefinition.Name,
                            TermContentItemId = termContentItemId,
                            Published = contentItem.Published,
                            Latest = contentItem.Latest
                        });
                    }
                }

                return results;
            });
    }
}
