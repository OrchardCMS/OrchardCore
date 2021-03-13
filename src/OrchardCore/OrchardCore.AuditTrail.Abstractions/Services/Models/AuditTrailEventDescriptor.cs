using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailEventDescriptor
    {
        public LocalizedString LocalizedName { get; set; }
        public LocalizedString Description { get; set; }
        public string EventName { get; set; }
        public string FullEventName { get; set; }
        public AuditTrailCategoryDescriptor CategoryDescriptor { get; set; }
        public Action<AuditTrailEvent, Dictionary<string, object>> BuildAuditTrailEvent { get; set; }
        public bool IsEnabledByDefault { get; set; }
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Returns a basic descriptor based on an event record.
        /// This is useful in cases where event records were previously stored by providers that are no longer enabled.
        /// </summary>
        public static AuditTrailEventDescriptor Basic(AuditTrailEvent auditTrailEvent) =>
            new AuditTrailEventDescriptor
            {
                LocalizedName = new LocalizedString(auditTrailEvent.EventName, auditTrailEvent.EventName),
                FullEventName = auditTrailEvent.FullEventName,
                EventName = auditTrailEvent.EventName,
                CategoryDescriptor = new AuditTrailCategoryDescriptor
                {
                    Category = auditTrailEvent.Category,
                    Events = Enumerable.Empty<AuditTrailEventDescriptor>(),
                    LocalizedName = new LocalizedString(auditTrailEvent.Category, auditTrailEvent.Category)
                }
            };
    }
}
