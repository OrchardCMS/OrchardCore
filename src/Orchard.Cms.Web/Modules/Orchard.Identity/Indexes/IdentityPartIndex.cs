using System;
using Orchard.ContentManagement;
using Orchard.Identity.Models;
using YesSql.Core.Indexes;

namespace Orchard.Identity.Indexes
{
    public class IdentityPartIndex : MapIndex
    {
        public string Identifier { get; set; }
        public string ContentItemId { get; set; }
        public int Number { get; set; }
        public bool Published { get; set; }
        public bool Latest { get; set; }

    }

    public class IdentityPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<IdentityPartIndex>()
                .Map(contentItem =>
                {
                    var identifier = contentItem.As<IdentityPart>()?.Identity;
                    if (!String.IsNullOrEmpty(identifier))
                    {
                        return new IdentityPartIndex
                        {
                            Identifier = identifier,
                            Latest = contentItem.Latest,
                            Number = contentItem.Number,
                            Published = contentItem.Published,
                            ContentItemId = contentItem.ContentItemId,

                        };
                    }

                    return null;
                });
        }
    }
}