using System;
using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailCreateContext : AuditTrailContext
    {
        public AuditTrailCreateContext(
            string name,
            string category,
            string correlationId,
            string userId,
            string userName,
            Dictionary<string, object> data)
            : base(name, category, correlationId, userId, userName, data) { }

        public string ClientIpAddress { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }
}
