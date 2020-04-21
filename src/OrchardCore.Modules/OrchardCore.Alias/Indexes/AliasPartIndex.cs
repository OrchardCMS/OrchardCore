using System;
using OrchardCore.Alias.Models;
using OrchardCore.ContentManagement;
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

    public class AliasPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AliasPartIndex>()
                .Map(contentItem =>
                {
                    var alias = contentItem.As<AliasPart>()?.Alias;
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
