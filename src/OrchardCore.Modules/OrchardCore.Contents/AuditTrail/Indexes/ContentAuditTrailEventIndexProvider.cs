using System;
using OrchardCore.AuditTrail.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql.Indexes;

namespace OrchardCore.Contents.AuditTrail.Indexes
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    public class ContentAuditTrailEventIndexProvider : IndexProvider<AuditTrailEvent>
    {
        public override void Describe(DescribeContext<AuditTrailEvent> context) =>
            context.For<ContentAuditTrailEventIndex>()
                .Map(auditTrailEvent =>
                {
                    var contentAuditTrailEvent = auditTrailEvent.As<ContentEvent>();

                    if (contentAuditTrailEvent == null || String.IsNullOrEmpty(contentAuditTrailEvent.ContentItemId))
                    {
                        return null;
                    }

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
