using System;
using Orchard.Autoroute.Model;
using YesSql.Indexes;

namespace Orchard.ContentManagement.Records
{
    public class AutoroutePartIndex : MapIndex
    {
        public string ContentItemId { get; set; }
        public string Path { get; set; }
        public bool Published { get; set; }
    }

    public class AutoroutePartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<AutoroutePartIndex>()
                .Map(contentItem =>
                {
                    var path = contentItem.As<AutoroutePart>()?.Path;
                    if (!String.IsNullOrEmpty(path) && (contentItem.Published || contentItem.Latest))
                    {
                        return new AutoroutePartIndex
                        {
                            ContentItemId = contentItem.ContentItemId,
                            Path = path,
                            Published = contentItem.Published
                        };
                    }

                    return null;
                });
        }
    }
}