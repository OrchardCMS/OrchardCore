using OrchardCore.AuditTrail.Models;
using YesSql;

namespace OrchardCore.AuditTrail.Services.Models
{
    public class QueryFilterContext
    {
        public IQuery<AuditTrailEvent> Query { get; }
        public Filters Filters { get; }

        public QueryFilterContext(IQuery<AuditTrailEvent> query, Filters filters)
        {
            Query = query;
            Filters = filters;
        }
    }
}
