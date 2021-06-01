using System.Collections.Generic;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailContext
    {
        public AuditTrailContext(
            string name,
            string category,
            string correlationId,
            string userId,
            string userName,
            Dictionary<string, object> data)
        {
            Name = name;
            Category = category;
            CorrelationId = correlationId;
            UserId = userId;
            UserName = userName;
            Data = data;
        }

        public string Name { get; set; }
        public string Category { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
