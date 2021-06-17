using System;

namespace OrchardCore.AuditTrail.Services.Models
{
    public abstract class AuditTrailCreateContext : AuditTrailContext
    {
        public AuditTrailCreateContext(
            string name,
            string category,
            string correlationId,
            string userId,
            string userName)
            : base(name, category, correlationId, userId, userName) { }

        public string ClientIpAddress { get; set; }
        public DateTime? CreatedUtc { get; set; }
    }

    public class AuditTrailCreateContext<TEvent> : AuditTrailCreateContext where TEvent : class, new()
    {
        public AuditTrailCreateContext(
            string name,
            string category,
            string correlationId,
            string userId,
            string userName,
            TEvent auditTrailEventItem)
        : base(name, category, correlationId, userId, userName)
        {
            AuditTrailEventItem = auditTrailEventItem;
        }

        public TEvent AuditTrailEventItem { get; set; }
    }
}
