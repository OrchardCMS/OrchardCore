using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Contents.Indexing;
using OrchardCore.Indexing;
using OrchardCore.Taxonomies.Fields;

namespace OrchardCore.Taxonomies.Indexing
{
    public class TaxonomyFieldIndexHandler : ContentFieldIndexHandler<TaxonomyField>
    {
        private readonly IServiceProvider _serviceProvider;

        public TaxonomyFieldIndexHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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
                TaxonomyOrchardHelperExtensions.FindTermHierarchy(taxonomy.Content.TaxonomyPart.Terms as JArray, contentItemId, inheritedContentItems);
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
