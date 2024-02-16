using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Indexing
{
    public class TaxonomyFieldIndexHandler : ContentFieldIndexHandler<TaxonomyField>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public TaxonomyFieldIndexHandler(
            IServiceProvider serviceProvider,
            IOptions<JsonSerializerOptions> jsonSerializerOptions)
        {
            _serviceProvider = serviceProvider;
            _jsonSerializerOptions = jsonSerializerOptions.Value;
        }

        public override async Task BuildIndexAsync(TaxonomyField field, BuildFieldIndexContext context)
        {
            // TODO: Also add the parents of each term, probably as a separate field

            var options = context.Settings.ToOptions();
            options |= DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;

            // Directly selected term ids are added to the default field name
            foreach (var contentItemId in field.TermContentItemIds)
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key + IndexingConstants.IdsKey, contentItemId, options);
                }
            }

            // Inherited term ids are added to a distinct field, prefixed with "Inherited"
            var contentManager = _serviceProvider.GetRequiredService<IContentManager>();
            var taxonomy = await contentManager.GetAsync(field.TaxonomyContentItemId);

            var inheritedContentItems = new List<ContentItem>();
            foreach (var contentItemId in field.TermContentItemIds)
            {
                TaxonomyOrchardHelperExtensions.FindTermHierarchy((JsonArray)taxonomy.Content.TaxonomyPart.Terms, contentItemId, inheritedContentItems, _jsonSerializerOptions);
            }

            foreach (var key in context.Keys)
            {
                foreach (var contentItem in inheritedContentItems)
                {
                    context.DocumentIndex.Set(key + IndexingConstants.InheritedKey, contentItem.ContentItemId, options);
                }
            }
        }
    }
}
