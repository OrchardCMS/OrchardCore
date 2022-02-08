using System;
using OrchardCore.BackgroundJobs.Models;
using YesSql;
using YesSql.Filters.Query.Services;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobQueryContext : QueryExecutionContext<BackgroundJobExecution>
    {
        public BackgroundJobQueryContext(IServiceProvider serviceProvider, IQuery<BackgroundJobExecution> query) : base(query)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }
    }
}
