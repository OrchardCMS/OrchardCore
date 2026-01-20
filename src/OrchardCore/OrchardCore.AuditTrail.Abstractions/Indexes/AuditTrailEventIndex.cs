using System;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail.Indexes
{
    /// <summary>
    /// Used to index the generic Audit Trail Events.
    /// </summary>
    public class AuditTrailEventIndex : MapIndex
    {
        public string EventId { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
        public string NormalizedUserName { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
