using System;
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

                    // Also check if the related content item was removed, if so it is still indexed.
                    if (!contentItem.Published && !contentItem.Latest && !part.ContentItemRemoved)
                    {
                        return null;
                    }

                    if (String.IsNullOrEmpty(part.LocalizationSet) || part.Culture == null)
                    {
                        return null;
                    }

                    // Don't persist this property as true.
                    part.ContentItemRemoved = false;

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
