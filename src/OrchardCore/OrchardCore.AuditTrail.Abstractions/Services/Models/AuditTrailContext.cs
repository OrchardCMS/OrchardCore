using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailContext
    {
        public string EventName { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, object> EventData { get; set; }
        public string EventFilterKey { get; set; }
        public string EventFilterData { get; set; }

        public AuditTrailContext(
            string eventName,
            string userName,
            Dictionary<string, object> eventData,
            string eventFilterKey,
            string eventFilterData)
        {
            EventName = eventName;
            UserName = userName;
            EventData = eventData;
            EventFilterKey = eventFilterKey;
            EventFilterData = eventFilterData;
        }
    }
}
