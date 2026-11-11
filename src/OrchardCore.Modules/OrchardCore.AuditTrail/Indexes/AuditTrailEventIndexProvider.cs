using OrchardCore.AuditTrail.Models;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail.Indexes;

public class AuditTrailEventIndexProvider : IndexProvider<AuditTrailEvent>
{
    public AuditTrailEventIndexProvider() => CollectionName = AuditTrailEvent.Collection;

    // An event is a document with a single record here, so a query that only joins indexes with a single record per document isn't
    // grouped by document, and its count reads this table without joining the document table.
    public override void Describe(DescribeContext<AuditTrailEvent> context) =>
        context.For<AuditTrailEventIndex>()
            .ExclusiveToDocumentType()
            .SingleRecordPerDocument()
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
                    CreatedUtc = auditTrailEvent.CreatedUtc,
                };
            });
}
