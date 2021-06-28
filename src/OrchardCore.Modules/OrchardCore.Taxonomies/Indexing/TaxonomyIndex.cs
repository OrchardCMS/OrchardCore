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

        public int Order { get; set; }

        public string ContainedContentItemId { get; set; }
        public string JsonPath { get; set; }

        public DateTime? CreatedUtc { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
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
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    var results = DescribeContentItem(contentItem);

                    if (contentItem.ContentType == "Taxonomy")
                    {
                        DescribeTermItems(results, contentItem, contentItem.Content.TaxonomyPart.Terms as JArray);
                    }

                    return results;
                });
        }

        // Process the Terms of the Taxonomy
        private void DescribeTermItems(List<TaxonomyIndex> results, ContentItem taxonomyContentItem, JArray termsArray)
        {
            foreach (JObject term in termsArray)
            {
                // Add this term to the index
                results.AddRange(DescribeContentItem(term.ToObject<ContentItem>(), taxonomyContentItem, term.Path));

                if (term.GetValue("Terms") is JArray children)
                {
                    // Process child terms
                    DescribeTermItems(results, taxonomyContentItem, children);
                }
            }
        }

        // Create index entries for a Taxonomy or Term
        // If we are processing a Taxonomy, it will be in "item" and "taxonomy" and "jsonPath" will be null.
        // If we are processing a Term, it will be in "item" and the Taxonomy the Term belongs to will be in "taxonomy", and "jsonPath" will be the Path of the categoried Term.
        private List<TaxonomyIndex> DescribeContentItem(ContentItem item, ContentItem taxonomy = null, string jsonPath = null)
        {
            var results = new List<TaxonomyIndex>();

            // Can we safely ignore this content item?
            if (_ignoredTypes.Contains(item.ContentType))
            {
                return results;
            }

            // Search for Taxonomy fields
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(item.ContentType);

            // This can occur when content items become orphaned, particularly layer widgets when a layer is removed, before its widgets have been unpublished.
            if (contentTypeDefinition == null)
            {
                _ignoredTypes.Add(item.ContentType);
                return results;
            }

            var fieldDefinitions = contentTypeDefinition
                .Parts.SelectMany(x => x.PartDefinition.Fields.Where(f => f.FieldDefinition.Name == nameof(TaxonomyField)))
                .ToArray();

            // This type doesn't have any TaxonomyField, ignore it
            if (fieldDefinitions.Length == 0)
            {
                _ignoredTypes.Add(item.ContentType);
                return results;
            }

            // Get all field values
            foreach (var fieldDefinition in fieldDefinitions)
            {
                var jPart = (JObject)item.Content[fieldDefinition.PartDefinition.Name];

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
                        // Taxonomy info
                        TaxonomyContentItemId = field.TaxonomyContentItemId,
                        TermContentItemId = termContentItemId,
                        JsonPath = taxonomy == null ? null : jsonPath,

                        // Categorized content item info
                        ContentItemId = taxonomy == null ? item.ContentItemId : taxonomy.ContentItemId,
                        ContainedContentItemId = taxonomy == null ? null : item.ContentItemId,
                        CreatedUtc = item.CreatedUtc,
                        ContentType = item.ContentType,
                        ContentPart = fieldDefinition.PartDefinition.Name,
                        ContentField = fieldDefinition.Name,
                        Order = field.TermContentItemOrder.GetValueOrDefault(termContentItemId, 0),
                        Published = item.Published,
                        Latest = item.Latest
                    });
                }
            }

            return results;
        }
    }
}
