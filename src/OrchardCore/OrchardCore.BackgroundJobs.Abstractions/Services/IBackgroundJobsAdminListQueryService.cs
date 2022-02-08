using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;

namespace OrchardCore.BackgroundJobs.Services
{
    public interface IBackgroundJobsAdminListQueryService
    {
        Task<IQuery<BackgroundJobExecution>> QueryAsync(BackgroundJobIndexOptions options, IUpdateModel updater);
    }
}
