using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Providers;
using OrchardCore.AuditTrail.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        /// <typeparam name="TAuditTrailEventProvider">The provider type to determine the event scope.</typeparam>
        /// <param name="auditTrailContext">The context used to create a new audit trail event.</param>
        Task RecordAuditTrailEventAsync<TAuditTrailEventProvider>(AuditTrailContext auditTrailContext)
            where TAuditTrailEventProvider : IAuditTrailEventProvider;

        /// <summary>
        /// Gets a page of audit trail events.
        /// </summary>
        /// <param name="page">The page number to get events from.</param>
        /// <param name="pageSize">The number of events to get.</param>
        /// <param name="filters">An optional <see cref="Filters"/>.</param>
        /// <param name="orderBy">an optional <see cref="AuditTrailOrderBy"/>.</param>
        /// <returns>The <see cref="AuditTrailEventSearchResults"/>.</returns>
        Task<AuditTrailEventSearchResults> GetAuditTrailEventsAsync(
            int page,
            int pageSize,
            Filters filters = null,
            AuditTrailOrderBy orderBy = AuditTrailOrderBy.DateDescending);

        /// <summary>
        /// Gets a single audit trail event by ID.
        /// </summary>
        /// <param name="auditTrailEventId">The event ID.</param>
        /// <returns>The <see cref="AuditTrailEvent"/>.</returns>
        Task<AuditTrailEvent> GetAuditTrailEventAsync(string auditTrailEventId);

        /// <summary>
        /// Trims the audit trail by deleting all events older than the specified retention period.
        /// </summary>
        /// <returns>The number of the deleted events.</returns>
        Task<int> TrimAsync(TimeSpan retentionPeriod);

        /// <summary>
        /// Describes all audit trail event categories.
        /// </summary>
        /// <returns>The list of <see cref="AuditTrailCategoryDescriptor"/>.</returns>
        IEnumerable<AuditTrailCategoryDescriptor> DescribeCategories();

        /// <summary>
        /// Describes all audit trail event providers.
        /// </summary>
        /// <returns>The <see cref="DescribeContext"/>.</returns>
        DescribeContext DescribeProviders();

        /// <summary>
        /// Describes a single audit trail event.
        /// </summary>
        /// <param name="auditTrailEvent">The <see cref="AuditTrailEvent"/> to describe.</param>
        /// <returns>The <see cref="AuditTrailEventDescriptor"/>.</returns>
        AuditTrailEventDescriptor DescribeEvent(AuditTrailEvent auditTrailEvent);
    }
}
