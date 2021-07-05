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
                        EventId = auditTrailEvent.EventId,
                        Category = auditTrailEvent.Category,
                        Name = auditTrailEvent.Name,
                        CorrelationId = auditTrailEvent.CorrelationId,
                        UserId = auditTrailEvent.UserId,
                        NormalizedUserName = auditTrailEvent.NormalizedUserName,
                        CreatedUtc = auditTrailEvent.CreatedUtc
                    };
                });
    }
}
