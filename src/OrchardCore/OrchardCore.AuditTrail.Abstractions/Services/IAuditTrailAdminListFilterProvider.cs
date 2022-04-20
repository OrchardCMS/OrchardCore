using OrchardCore.AuditTrail.Models;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailAdminListFilterProvider
    {
        void Build(QueryEngineBuilder<AuditTrailEvent> builder);
    }
}
