using OrchardCore.BackgroundJobs.Models;
using YesSql.Filters.Query;

namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobsAdminListFilterParser : IQueryParser<BackgroundJobExecution>
    {
    }
}
