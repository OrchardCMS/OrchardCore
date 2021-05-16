using System;
using OrchardCore.Entities;

namespace OrchardCore.AuditTrail.Models
{
    public class AuditTrailEvent : Entity
    {
        /// <summary>
        /// The name of the collection that is used for this type.
        /// </summary>
        public const string Collection = "Audit";

        /// <summary>
        /// The ID of the event.
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// The category the event belongs to.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Just the name of the event without the name of the provider.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// The full name of the event that contains the name of the provider and the name of the event.
        /// </summary>
        public virtual string FullEventName { get; set; }

        /// <summary>
        /// The correlation id used to associate an event to e.g. a content item.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// The user name of the user who caused the event to occur.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The IP address of the user who caused the event to occur.
        /// </summary>
        public string ClientIpAddress { get; set; }

        /// <summary>
        /// The date and time when the event occurred.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// The comment of the event.
        /// </summary>
        public string Comment { get; set; }
    }
}
