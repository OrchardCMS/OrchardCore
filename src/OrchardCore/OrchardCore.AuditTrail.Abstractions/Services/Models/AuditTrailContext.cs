using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailContext
    {
        public AuditTrailContext(
            string category,
            string @event,
            string correlationId,
            string userName,
            Dictionary<string, object> data)
        {
            Category = category;
            Event = @event;
            CorrelationId = correlationId;
            UserName = userName;
            Data = data;
        }

        public string Category { get; set; }
        public string Event { get; set; }
        public string CorrelationId { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
