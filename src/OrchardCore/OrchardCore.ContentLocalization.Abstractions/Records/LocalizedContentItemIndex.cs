using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
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

    public class LocalizedContentItemIndexProvider : IndexProvider<ContentItem>
    {
        private readonly IServiceProvider _serviceProvider;

        private IContentManagerSession _contentManagerSession;

        public LocalizedContentItemIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<LocalizedContentItemIndex>()
                .Map(contentItem =>
                {
                    var part = contentItem.As<LocalizationPart>();

                    if (part == null)
                    {
                        return null;
                    }

                    _contentManagerSession ??= _serviceProvider.GetRequiredService<IContentManagerSession>();
                    var isRemovedContentItem = _contentManagerSession.IsRemovedContentItem(contentItem);

                    // If the related content item was removed, a record is still added.
                    if (!contentItem.Published && !contentItem.Latest && !isRemovedContentItem)
                    {
                        return null;
                    }

                    if (String.IsNullOrEmpty(part.LocalizationSet) || part.Culture == null)
                    {
                        return null;
                    }

                    return new LocalizedContentItemIndex
                    {
                        Culture = part.Culture.ToLowerInvariant(),
                        LocalizationSet = part.LocalizationSet,
                        ContentItemId = contentItem.ContentItemId,
                        Published = contentItem.Published
                    };
                });
        }
    }
}
