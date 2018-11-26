using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Models;
using OrchardCore.ContentManagement.Routable;
using OrchardCore.Data;
using OrchardCore.Taxonomies.Models;
using YesSql.Indexes;

namespace OrchardCore.Taxonomies.Indexing
{
    public class TaxonomyPartIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private IContentManager _contentManager;

        public TaxonomyPartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<RoutableIndex>()
                .Map(async contentItem =>
                {
                    if (!contentItem.Has<TaxonomyPart>())
                    {
                        return null;
                    }

                    var taxonomyPart = contentItem.As<TaxonomyPart>();

                    _contentManager = _contentManager ?? _serviceProvider.GetRequiredService<IContentManager>();

                    var allTerms = new List<JObject>();
                    TaxonomyOrchardHelperExtensions.FindAllTerms(taxonomyPart.Content.Terms as JArray, allTerms);

                    var allRoutable = new List<RoutableIndex>();

                    foreach(var term in allTerms)
                    {
                        var termContentItem = term.ToObject<ContentItem>();
                        var routableAspect = await _contentManager.PopulateAspectAsync<RoutableAspect>(termContentItem);

                        if (!String.IsNullOrEmpty(routableAspect.Path))
                        {
                            term.TryGetJsonPath(out var jsonPath);

                            allRoutable.Add(new RoutableIndex
                            {
                                ContentItemId = termContentItem.ContentItemId,
                                RootContentItemId = taxonomyPart.ContentItem.ContentItemId,
                                Published = taxonomyPart.IsPublished(),
                                Path = routableAspect.Path,
                                JsonPath = jsonPath
                            });
                        }
                    }

                    return allRoutable;
                });
        }
    }
}
