using System;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCreateContext : AuditTrailContext
    {
        public AuditTrailCreateContext(
            string category,
            string eventName,
            string correlationId,
            string userName,
            Dictionary<string, object> eventData)
            : base(category, eventName, correlationId, userName, eventData) { }

        public string ClientIpAddress { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public string Comment { get; set; } = String.Empty;
    }
}
