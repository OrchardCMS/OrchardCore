using System;
using OrchardCore.AuditTrail.Models;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailQueryContext : QueryExecutionContext<AuditTrailEvent>
    {
        public AuditTrailQueryContext(IServiceProvider serviceProvider, IQuery<AuditTrailEvent> query) : base(query)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
