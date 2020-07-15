using OrchardCore.AuditTrail.Models;
using OrchardCore.Entities;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail.Indexes
{
    public class ContentAuditTrailEventIndexProvider : IndexProvider<AuditTrailEvent>
    {
        public override void Describe(DescribeContext<AuditTrailEvent> context) =>
            context.For<ContentAuditTrailEventIndex>()
                .Map(auditTrailEvent =>
                {
                    var contentAuditTrailEvent = auditTrailEvent.As<ContentEvent>();

                    if (contentAuditTrailEvent == null || string.IsNullOrEmpty(contentAuditTrailEvent.ContentItemId))
                        return null;

                    return new ContentAuditTrailEventIndex
                    {
                        ContentItemId = contentAuditTrailEvent.ContentItemId,
                        ContentType = contentAuditTrailEvent.ContentType,
                        Published = contentAuditTrailEvent.Published,
                        EventName = contentAuditTrailEvent.EventName,
                        VersionNumber = contentAuditTrailEvent.VersionNumber
                    };
                });
    }
}
