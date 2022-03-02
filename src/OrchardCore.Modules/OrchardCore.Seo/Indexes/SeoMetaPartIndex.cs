using OrchardCore.ContentManagement;
using OrchardCore.Seo.Models;
using YesSql.Indexes;

namespace OrchardCore.Seo.Indexes;

public class SeoMetaPartIndex : MapIndex
{
    public string PageTitle { get; set; }
}

public class SeoMetaPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context)
    {
        context.For<SeoMetaPartIndex>()
            .When(contentItem => contentItem.Has<SeoMetaPart>())
            .Map(contentItem =>
            {
                var containedPart = contentItem.As<SeoMetaPart>();
                if (containedPart == null)
                {
                    return null;
                }

                return new SeoMetaPartIndex
                {
                    PageTitle = containedPart.PageTitle
                };
            });
    }
}
