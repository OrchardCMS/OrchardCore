using System.Threading.Tasks;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailAdminListQueryService
    {
        /// <summary>
        /// Queries a page of audit trail events.
        /// </summary>
        /// <param name="page">The page number to get events from.</param>
        /// <param name="pageSize">The number of events to get.</param>
        /// <param name="options">The filter options.</param>
        Task<AuditTrailEventQueryResult> QueryAsync(int page, int pageSize, AuditTrailIndexOptions options);
    }
}
