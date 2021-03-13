using OrchardCore.AuditTrail.Models;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailEventSearchResults
    {
        public IEnumerable<AuditTrailEvent> AuditTrailEvents { get; set; }
        public int TotalCount { get; set; }
    }
}
