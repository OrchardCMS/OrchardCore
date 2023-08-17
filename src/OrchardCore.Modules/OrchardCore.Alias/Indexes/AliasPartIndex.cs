using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
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

    public class AliasPartIndexProvider : ContentHandlerBase, IIndexProvider, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _partRemoved = new();
        private IContentDefinitionManager _contentDefinitionManager;

        public AliasPartIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task UpdatedAsync(UpdateContentContext context)
        {
            var part = context.ContentItem.As<AliasPart>();

            // Validate that the content definition contains this part, this prevents indexing parts
            // that have been removed from the type definition, but are still present in the elements.            
            if (part != null)
            {
                // Lazy initialization because of ISession cyclic dependency.
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for this part.
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(AliasPart)))
                {
                    context.ContentItem.Remove<AliasPart>();
                    _partRemoved.Add(context.ContentItem.ContentItemId);
                }
            }

            return Task.CompletedTask;
        }

        public string CollectionName { get; set; }
        public Type ForType() => typeof(ContentItem);
        public void Describe(IDescriptor context) => Describe((DescribeContext<ContentItem>)context);

        public void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AliasPartIndex>()
                .When(contentItem => contentItem.Has<AliasPart>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // Remove index records of soft deleted items.
                    if (!contentItem.Published && !contentItem.Latest)
                    {
                        return null;
                    }

                    var part = contentItem.As<AliasPart>();
                    if (part == null || String.IsNullOrEmpty(part.Alias))
                    {
                        return null;
                    }

                    return new AliasPartIndex
                    {
                        Alias = part.Alias.ToLowerInvariant(),
                        ContentItemId = contentItem.ContentItemId,
                        Latest = contentItem.Latest,
                        Published = contentItem.Published
                    };
                });
        }
    }
}
