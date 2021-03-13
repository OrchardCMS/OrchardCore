using System;
using YesSql.Indexes;

namespace OrchardCore.AuditTrail.Indexes
{
    /// <summary>
    /// Used to index the generic Audit Trail Events.
    /// </summary>
    public class AuditTrailEventIndex : MapIndex
    {
        public string AuditTrailEventId { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string UserName { get; set; }
        public string EventName { get; set; }
        public string Category { get; set; }
        public string EventFilterKey { get; set; }
        public string EventFilterData { get; set; }
    }
}
