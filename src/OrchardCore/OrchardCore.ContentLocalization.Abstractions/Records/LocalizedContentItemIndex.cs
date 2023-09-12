using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContentLocalization.Records
{
    public class LocalizedContentItemIndex : MapIndex
    {
        public long DocumentId { get; set; }
        public string ContentItemId { get; set; }
        public string LocalizationSet { get; set; }
        public string Culture { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
    }

    public class LocalizedContentItemIndexProvider : ContentHandlerBase, IIndexProvider, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<ContentItem> _itemRemoved = new();
        private readonly HashSet<string> _partRemoved = new();
        private IContentDefinitionManager _contentDefinitionManager;

        public LocalizedContentItemIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            if (context.NoActiveVersionLeft)
            {
                var part = context.ContentItem.As<LocalizationPart>();

                if (part != null)
                {
                    _itemRemoved.Add(context.ContentItem);
                }
            }

            return Task.CompletedTask;
        }

        public override Task PublishedAsync(PublishContentContext context)
        {
            var part = context.ContentItem.As<LocalizationPart>();

            // Validate that the content definition contains this part, this prevents indexing parts
            // that have been removed from the type definition, but are still present in the elements.            
            if (part != null)
            {
                // Lazy initialization because of ISession cyclic dependency.
                _contentDefinitionManager ??= _serviceProvider.GetRequiredService<IContentDefinitionManager>();

                // Search for this part.
                var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(context.ContentItem.ContentType);
                if (!contentTypeDefinition.Parts.Any(ctpd => ctpd.Name == nameof(LocalizationPart)))
                {
                    context.ContentItem.Remove<LocalizationPart>();
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
            context.For<LocalizedContentItemIndex>()
                .When(contentItem => contentItem.Has<LocalizationPart>() || _partRemoved.Contains(contentItem.ContentItemId))
                .Map(contentItem =>
                {
                    // If the content item was removed, a record is still added.
                    if (!contentItem.Published && !contentItem.Latest && !_itemRemoved.Contains(contentItem))
                    {
                        return null;
                    }

                    // If the part was removed from the type definition, a record is still added.
                    var partRemoved = _partRemoved.Contains(contentItem.ContentItemId);

                    var part = contentItem.As<LocalizationPart>();
                    if (!partRemoved && (part == null || String.IsNullOrEmpty(part.LocalizationSet) || part.Culture == null))
                    {
                        return null;
                    }

                    // If the part was removed, a record is still added but with a null Culture.
                    return new LocalizedContentItemIndex
                    {
                        Culture = !partRemoved ? part.Culture.ToLowerInvariant() : null,
                        LocalizationSet = !partRemoved ? part.LocalizationSet : null,
                        ContentItemId = contentItem.ContentItemId,
                        Published = contentItem.Published,
                        Latest = contentItem.Latest
                    };
                });
        }
    }
}
