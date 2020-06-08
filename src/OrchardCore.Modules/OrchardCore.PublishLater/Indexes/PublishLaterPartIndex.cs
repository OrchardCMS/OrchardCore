using System;
using OrchardCore.ContentManagement;
using OrchardCore.PublishLater.Models;
using YesSql.Indexes;

namespace OrchardCore.PublishLater.Indexes
{
    public class PublishLaterPartIndex : MapIndex
    {
        public DateTime? ScheduledPublishUtc { get; set; }
    }

    public class PublishLaterPartIndexProvider : IndexProvider<ContentItem>
    {
        public override void Describe(DescribeContext<ContentItem> context)
        {
            context.For<PublishLaterPartIndex>()
                .Map(contentItem =>
                {
                    var publishLaterPart = contentItem.As<PublishLaterPart>();
                    if (publishLaterPart == null || !publishLaterPart.ScheduledPublishUtc.HasValue)
                    {
                        return null;
                    }

                    // Remove index for items that are already published or not the latest version
                    if (contentItem.Published || !contentItem.Latest)
                    {
                        return null;
                    }

                    return new PublishLaterPartIndex
                    {
                        ScheduledPublishUtc = publishLaterPart.ScheduledPublishUtc,
                    };
                });
        }
    }
}
