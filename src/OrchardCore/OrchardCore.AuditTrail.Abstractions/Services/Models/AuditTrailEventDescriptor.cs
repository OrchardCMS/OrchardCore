using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Models;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class AuditTrailEventDescriptor
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public LocalizedString LocalizedCategory { get; set; }
        public LocalizedString Description { get; set; }
        public Action<AuditTrailEvent, Dictionary<string, object>> BuildEvent { get; set; }
        public bool IsEnabledByDefault { get; set; }
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Returns a default descriptor based on an event record in case the related provider is no longer registered.
        /// </summary>
        public static AuditTrailEventDescriptor Default(AuditTrailEvent @event)
        {
            return new AuditTrailEventDescriptor
            {
                Name = @event.Name,
                Category = @event.Category,
                LocalizedName = new LocalizedString(@event.Name, @event.Name),
                LocalizedCategory = new LocalizedString(@event.Category, @event.Category),
                BuildEvent = (@event, data) => { }
            };
        }
    }
}
