using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailContext
    {
        public AuditTrailContext(
            string category,
            string eventName,
            string correlationId,
            string userName,
            Dictionary<string, object> eventData)
        {
            Category = category;
            EventName = eventName;
            CorrelationId = correlationId;
            UserName = userName;
            EventData = eventData;
        }

        public string Category { get; set; }
        public string EventName { get; set; }
        public string CorrelationId { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, object> EventData { get; set; }
    }
}
