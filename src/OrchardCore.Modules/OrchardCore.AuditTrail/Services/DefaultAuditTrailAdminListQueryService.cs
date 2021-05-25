using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.ViewModels;
using YesSql;

namespace OrchardCore.AuditTrail.Services
{
    public class DefaultAuditTrailAdminListQueryService : IAuditTrailAdminListQueryService
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public DefaultAuditTrailAdminListQueryService(
            ISession session,
            IServiceProvider serviceProvider,
            ILogger<DefaultAuditTrailAdminListQueryService> logger)
        {
            _session = session;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<IQuery<AuditTrailEvent>> QueryAsync(AuditTrailIndexOptions options, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<AuditTrailEvent>()
            var query = _session.Query<AuditTrailEvent>(collection: AuditTrailEvent.Collection);

            query = await options.FilterResult.ExecuteAsync(new AuditTrailQueryContext(_serviceProvider, query));

            return query;
        }
    }
}
