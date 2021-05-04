using OrchardCore.AuditTrail.Models;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail.Indexes
{
    public class AuditTrailEventIndexProvider : IndexProvider<AuditTrailEvent>
    {
        public AuditTrailEventIndexProvider() => CollectionName = AuditTrailEvent.Collection;

        public override void Describe(DescribeContext<AuditTrailEvent> context) =>
            context.For<AuditTrailEventIndex>()
                .Map(auditTrailEvent =>
                {
                    return new AuditTrailEventIndex
                    {
                        AuditTrailEventId = auditTrailEvent.AuditTrailEventId,
                        Category = auditTrailEvent.Category,
                        CreatedUtc = auditTrailEvent.CreatedUtc,
                        EventFilterData = auditTrailEvent.EventFilterData,
                        EventFilterKey = auditTrailEvent.EventFilterKey,
                        EventName = auditTrailEvent.EventName,
                        UserName = auditTrailEvent.UserName
                    };
                });
    }
}
