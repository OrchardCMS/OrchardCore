using System.Collections.Generic;
using OrchardCore.AuditTrail.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailEventQueryResult
    {
        public IEnumerable<AuditTrailEvent> Events { get; set; }
        public int TotalCount { get; set; }
    }
}
