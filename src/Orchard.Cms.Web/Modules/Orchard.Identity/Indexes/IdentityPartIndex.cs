using System;
using Orchard.ContentManagement;
using Orchard.Identity.Models;
using YesSql.Core.Indexes;

namespace Orchard.Identity.Indexes
{
    public class IdentityPartIndex : MapIndex
    {
        public string Identifier { get; set; }
    }

    public class IdentityPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<IdentityPartIndex>()
                .Map(contentItem =>
                {
                    var identifier = contentItem.As<IdentityPart>()?.Identifier;
                    if (!String.IsNullOrEmpty(identifier))
                    {
                        return new IdentityPartIndex
                        {
                            Identifier = identifier
                        };
                    }

                    return null;
                });
        }
    }
}