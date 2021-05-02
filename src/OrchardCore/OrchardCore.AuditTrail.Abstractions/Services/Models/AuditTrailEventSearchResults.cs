using System.Collections.Generic;
using OrchardCore.AuditTrail.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailEventSearchResults
    {
        public IEnumerable<AuditTrailEvent> AuditTrailEvents { get; set; }
        public int TotalCount { get; set; }
    }
}
