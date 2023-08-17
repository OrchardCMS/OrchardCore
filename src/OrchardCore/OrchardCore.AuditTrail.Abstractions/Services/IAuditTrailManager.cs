using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services.Models;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Describes a service managing audit trail events.
    /// </summary>
    public interface IAuditTrailManager
    {
        /// <summary>
        /// Records an audit trail event.
        /// </summary>
        /// <param name="context">The <see cref="AuditTrailContext{TEvent}"/> used to create a new event.</param>
        Task RecordEventAsync<TEvent>(AuditTrailContext<TEvent> context) where TEvent : class, new();

        /// <summary>
        /// Gets a single audit trail event by ID.
        /// </summary>
        /// <param name="eventId">The event ID.</param>
        /// <returns>The <see cref="AuditTrailEvent"/>.</returns>
        Task<AuditTrailEvent> GetEventAsync(string eventId);

        /// <summary>
        /// Trims the audit trail by deleting all events older than the specified retention period.
        /// </summary>
        /// <returns>The number of the deleted events.</returns>
        Task<int> TrimEventsAsync(TimeSpan retentionPeriod);

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <param name="auditTrailEvent">The <see cref="AuditTrailEvent"/> to describe.</param>
        /// <returns>The <see cref="AuditTrailEventDescriptor"/>.</returns>
        AuditTrailEventDescriptor DescribeEvent(AuditTrailEvent auditTrailEvent);

        /// <summary>
        /// Describes all audit trail event categories.
        /// </summary>
        /// <returns>The list of <see cref="AuditTrailCategoryDescriptor"/>.</returns>
        IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories();

        /// <summary>
        /// Describes a single audit trail event category.
        /// </summary>
        /// <param name="name">The category name.</param>
        /// <returns>The <see cref="AuditTrailCategoryDescriptor"/>.</returns>
        AuditTrailCategoryDescriptor DescribeCategory(string name);
    }
}
