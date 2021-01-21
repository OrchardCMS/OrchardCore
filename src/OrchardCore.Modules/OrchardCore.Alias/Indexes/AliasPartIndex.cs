using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.Alias.Indexes
{
    public class AliasPartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string Alias { get; set; }
        public bool Latest { get; set; }
        public bool Published { get; set; }
    }

    public class AliasPartIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private IContentDefinitionManager _contentDefinitionManager;
        private HashSet<string> _ignoredTypes;

        public AliasPartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AliasPartIndex>()
                .Map(contentItem =>
                {
                    var aliasPart = contentItem.As<AliasPart>();

                    if (aliasPart == null)
                    {
                        return null;
                    }

                    // Can we safely ignore this content item?
                    if (_ignoredTypes != null && _ignoredTypes.Contains(contentItem.ContentType))
                    {
                        contentItem.Remove<AliasPart>();
                        return null;
                    }

                    // Lazy initialization because of ISession cyclic dependency
                    _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                    // Search for AliasPart
                    var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                    // Validate that the content definition contains an AliasPart.
                    // This prevents indexing parts that have been removed from the type definition, but are still present in the elements.
                    if (contentTypeDefinition == null || !contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(AliasPart)))
                    {
                        _ignoredTypes ??= new HashSet<string>();
                        _ignoredTypes.Add(contentItem.ContentType);
                        contentItem.Remove<AliasPart>();

                        return null;
                    }

                    var alias = aliasPart.Alias;

                    if (!String.IsNullOrEmpty(alias) && (contentItem.Published || contentItem.Latest))
                    {
                        return new AliasPartIndex
                        {
                            Alias = alias.ToLowerInvariant(),
                            ContentItemId = contentItem.ContentItemId,
                            Latest = contentItem.Latest,
                            Published = contentItem.Published
                        };
                    }

                    return null;
                });
        }
    }
}
