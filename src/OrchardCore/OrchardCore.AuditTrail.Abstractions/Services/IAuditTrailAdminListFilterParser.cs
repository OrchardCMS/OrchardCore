using OrchardCore.AuditTrail.Models;
using YesSql.Filters.Query;

namespace OrchardCore.AuditTrail.Services
{
    public interface IAuditTrailAdminListFilterParser : IQueryParser<AuditTrailEvent>
    {
    }
}
