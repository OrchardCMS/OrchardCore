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
    }

    public class LocalizedContentItemIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<LocalizedContentItemIndex>()
                .Map(contentItem =>
                {
                    if (!contentItem.Latest)
                    {
                        return null;
                    }

                    var localizationPart = contentItem.As<LocalizationPart>();

                    if (string.IsNullOrEmpty(localizationPart?.LocalizationSet) || localizationPart?.Culture == null)
                    {
                        return null;
                    }

                    return new LocalizedContentItemIndex
                    {
                        Culture = localizationPart.Culture.ToLowerInvariant(),
                        LocalizationSet = localizationPart.LocalizationSet,
                        ContentItemId = contentItem.ContentItemId,
                        Published = contentItem.Published
                    };
                });
        }
    }
}
