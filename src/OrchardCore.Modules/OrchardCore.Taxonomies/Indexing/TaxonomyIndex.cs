using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using OrchardCore.Taxonomies.Fields;
using YesSql.Indexes;

namespace OrchardCore.Taxonomies.Indexing
{
    public class TaxonomyIndex : MapIndex
    {
        public string TaxonomyContentItemId { get; set; }
        public string ContentItemId { get; set; }
        public string ContentType { get; set; }
        public string ContentPart { get; set; }
        public string ContentField { get; set; }
        public string TermContentItemId { get; set; }
    }

    public class TaxonomyIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();
        private IContentDefinitionManager _contentDefinitionManager;

        public TaxonomyIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<TaxonomyIndex>()
                .Map(contentItem =>
                {
                    if (!contentItem.IsPublished())
                    {
                        return null;
                    }

                    // Can we safely ignore this content item?
                    if (_ignoredTypes.Contains(contentItem.ContentType))
                    {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager = _contentDefinitionManager ?? _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for Taxonomy fields
                    var fieldDefinitions = _contentDefinitionManager
                        .GetTypeDefinition(contentItem.ContentType)
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
                        var jPart = (JObject)contentItem.Content[fieldDefinition.PartDefinition.Name];

                        if (jPart == null)
                        {
                            continue;
                        }

                        var jField = (JObject)jPart[fieldDefinition.Name];

                        if (jField == null)
                        {
                            continue;
                        }

                        var field = jField.ToObject<TaxonomyField>();

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
                            });
                        }
                    }

                    return results;
                });
        }
    }
}
