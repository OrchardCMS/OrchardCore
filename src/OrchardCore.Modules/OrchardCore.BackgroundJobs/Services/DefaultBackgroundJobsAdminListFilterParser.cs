using OrchardCore.BackgroundJobs.Models;
using YesSql.Filters.Query;

namespace OrchardCore.BackgroundJobs.Services
{
    public class DefaultBackgroundJobsAdminListFilterParser : IBackgroundJobsAdminListFilterParser
    {
        private readonly IQueryParser<BackgroundJobExecution> _parser;

        public DefaultBackgroundJobsAdminListFilterParser(IQueryParser<BackgroundJobExecution> parser)
        {
            _parser = parser;
        }

        public QueryFilterResult<BackgroundJobExecution> Parse(string text)
            => _parser.Parse(text);
    }
}
