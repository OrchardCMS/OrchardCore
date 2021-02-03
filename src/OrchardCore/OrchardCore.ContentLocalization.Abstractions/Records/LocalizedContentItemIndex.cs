using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContentLocalization.Records
{
    public class LocalizedContentItemIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string LocalizationSet { get; set; }
        public string Culture { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }
    }

    public class LocalizedContentItemIndexProvider : ContentHandlerBase, IIndexProvider, IScopedIndexProvider
    {
        private readonly HashSet<ContentItem> _removed = new HashSet<ContentItem>();

        public LocalizedContentItemIndexProvider()
        {
        }

        public override Task RemovedAsync(RemoveContentContext context)
        {
            if (context.NoActiveVersionLeft)
            {
                var part = context.ContentItem.As<LocalizationPart>();

                if (part != null)
                {
                    _removed.Add(context.ContentItem);
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
                .When(contentItem => contentItem.Has<LocalizationPart>())
                .Map(contentItem =>
                {
                    var part = contentItem.As<LocalizationPart>();
                    if (part == null || String.IsNullOrEmpty(part.LocalizationSet) || part.Culture == null)
                    {
                        return null;
                    }

                    // If the related content item was removed, a record is still added.
                    if (!contentItem.Published && !contentItem.Latest && !_removed.Contains(contentItem))
                    {
                        return null;
                    }

                    return new LocalizedContentItemIndex
                    {
                        Culture = part.Culture.ToLowerInvariant(),
                        LocalizationSet = part.LocalizationSet,
                        ContentItemId = contentItem.ContentItemId,
                        Published = contentItem.Published,
                        Latest = contentItem.Latest
                    };
                });
        }
    }
}
