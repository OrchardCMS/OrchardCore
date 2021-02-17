using System;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCreateContext : AuditTrailContext
    {
        public string Comment { get; set; } = string.Empty;
        public DateTime? CreatedUtc { get; set; }
        public string ClientIpAddress { get; set; }

        public AuditTrailCreateContext(
            string eventName,
            string userName,
            Dictionary<string, object> eventData,
            string eventFilterKey,
            string eventFilterData)
            : base(eventName, userName, eventData, eventFilterKey, eventFilterData) { }
    }
}
