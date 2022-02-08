using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs.Indexes;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.BackgroundJobs.Services
{
    public class DefaultBackgroundJobsAdminListQueryService : IBackgroundJobsAdminListQueryService
    {
        private readonly ISession _session;
        private readonly IServiceProvider _serviceProvider;

        public DefaultBackgroundJobsAdminListQueryService(
            ISession session,
            IServiceProvider serviceProvider,
            ILogger<DefaultBackgroundJobsAdminListQueryService> logger)
        {
            _session = session;
            _serviceProvider = serviceProvider;
        }

        public async Task<IQuery<BackgroundJobExecution>> QueryAsync(BackgroundJobIndexOptions options, IUpdateModel updater)
        {
            // Because admin filters can add a different index to the query this must be added as a Query<BackgroundJobDocument>()
            var query = _session.Query<BackgroundJobExecution>();

            // This query is designed to operate with a background job name.
            query.With<BackgroundJobIndex>(x => x.Name == options.BackgroundJobName);

            // TODO move to filter.
            query.With<BackgroundJobIndex>().OrderByDescending(x => x.CreatedUtc);

            query = await options.FilterResult.ExecuteAsync(new BackgroundJobQueryContext(_serviceProvider, query));

            return query;
        }
    }
}
