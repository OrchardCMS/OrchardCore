using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Services
{
    /// <summary>
    /// Central service for the Audit Trail feature.
    /// </summary>
    public interface IAuditTrailManager
    {
        /// <summary>
        /// Records an audit trail event.
        /// </summary>
        /// <typeparam name="T">The audit trail event provider type to determine the scope of the event name.</typeparam>
        /// <param name="auditTrailContext">The context used to create a new audit trail event.</param>
        Task AddAuditTrailEventAsync<TAuditTrailEventProvider>(AuditTrailContext auditTrailContext) where TAuditTrailEventProvider : IAuditTrailEventProvider;

        /// <summary>
        /// Gets a page of events from the audit trail.
        /// </summary>
        /// <param name="page">The page number to get events from.</param>
        /// <param name="pageSize">The number of events to get.</param>
        /// <param name="orderBy">The value to order by.</param>
        /// <param name="filters">Optional. An object to filter the records on.</param>
        /// <returns>A page of events.</returns>
        Task<AuditTrailEventSearchResults> GetAuditTrailEventsAsync(
            int page,
            int pageSize,
            Filters filters = null,
            AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending);

        /// <summary>
        /// Gets a single event from the audit trail by ID.
        /// </summary>
        /// <param name="id">The event ID.</param>
        /// <returns>A single event.</returns>
        Task<AuditTrailEvent> GetAuditTrailEventAsync(string eventFilterData);

        /// <summary>
        /// Trims the audit trail by deleting all events older than the specified retention period.
        /// </summary>
        /// <returns>The number of the deleted events.</returns>
        Task<int> TrimAsync(TimeSpan retentionPeriod);

        /// <summary>
        /// Describes all audit trail events provided by the system.
        /// </summary>
        /// <returns>A list of audit trail category descriptors.</returns>
        IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories();

        /// <summary>
        /// Describes all audit trail event providers.
        /// </summary>
        DescribeContext DescribeProviders();

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <param name="auditTrailEvent">The audit trail event for which to find its descriptor.</param>
        /// <returns>A single audit trail event descriptor.</returns>
        AuditTrailEventDescriptor DescribeEvent(AuditTrailEvent auditTrailEvent);
    }
}
