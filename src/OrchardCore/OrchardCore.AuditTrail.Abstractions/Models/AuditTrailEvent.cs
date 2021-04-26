using System;
using OrchardCore.Entities;

namespace OrchardCore.AuditTrail.Models
{
    public class AuditTrailEvent : Entity
    {
        /// <summary>
        /// The ID of the event.
        /// </summary>
        public string AuditTrailEventId { get; set; }

        /// <summary>
        /// The date and time when the event occurred.
        /// </summary>
        public DateTime CreatedUtc { get; set; }

        /// <summary>
        /// The user name of the user who caused the event to occur.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Just the name of the event without the name of the provider.
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// The full name of the event that contains the name of the provider and the name of the event.
        /// </summary>
        public virtual string FullEventName { get; set; }

        /// <summary>
        /// The category the event belongs to.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The filter key of the event that can be used to query the event on.
        /// </summary>
        public string EventFilterKey { get; set; }

        /// <summary>
        /// The filter data of the event that is returned by the filter queries.
        /// </summary>
        public string EventFilterData { get; set; }

        /// <summary>
        /// The comment of the event.
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// The IP address of the user who caused the event to occur.
        /// </summary>
        public string ClientIpAddress { get; set; }
    }
}
