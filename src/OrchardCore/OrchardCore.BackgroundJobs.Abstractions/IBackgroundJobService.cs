using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Schedules;

namespace OrchardCore.BackgroundJobs
{
    public interface IBackgroundJobService
    {
        ValueTask<(IBackgroundJob BackgroundJob, IBackgroundJobSchedule Schedule, BackgroundJobState State)> GetByIdAsync(string id);
        ValueTask<(IBackgroundJob BackgroundJob, IBackgroundJobSchedule Schedule, BackgroundJobState State)> GetByCorrelationIdAsync<TJob>(string id) where TJob : class, IBackgroundJob, new();
        ValueTask<string> CreateJobAsync(IBackgroundJob backgroundJob, IBackgroundJobSchedule schedule = null);
        ValueTask<(bool Success, string BackgroundJobId)> TryRescheduleJobAsync(IBackgroundJob backgroundJob, IBackgroundJobSchedule schedule);
        ValueTask<bool> TryCancelAsync(string id);
        IScheduleBuilderFactory Schedule { get; }
    }

    public static class IBackgroundJobServiceExtensions
    {
        public static async ValueTask<string> CreateJobAsync(this IBackgroundJobService backgroundJobService, IBackgroundJob backgroundJob, IScheduleBuilder scheduleBuilder)
            => await backgroundJobService.CreateJobAsync(backgroundJob, await scheduleBuilder.BuildAsync());

        public static async ValueTask<string> CreateJobAsync(this IBackgroundJobService backgroundJobService, IBackgroundJob backgroundJob)
            => await backgroundJobService.CreateJobAsync(backgroundJob);

        public static async ValueTask<(bool Success, string BackgroundJobId)> TryRescheduleJobAsync(this IBackgroundJobService backgroundJobService, IBackgroundJob backgroundJob, IScheduleBuilder scheduleBuilder)
            => await backgroundJobService.TryRescheduleJobAsync(backgroundJob, await scheduleBuilder.BuildAsync());
    }
}
