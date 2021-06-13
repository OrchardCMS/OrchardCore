using System.Collections.Generic;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.Entities;
using OrchardCore.Users.AuditTrail.Models;

namespace OrchardCore.Users.AuditTrail.Providers
{
    public static class UserAuditTrailEventProviderHelper
    {
        public static void BuildEvent(AuditTrailEvent auditTrailEvent, Dictionary<string, object> eventData)
        {
            auditTrailEvent.Put(new AuditTrailUserEvent
            {
                Name = auditTrailEvent.Name,
                UserName = eventData.Get<string>("UserName"),
                UserId = eventData.Get<string>("UserId")
            });
        }
    }
}
