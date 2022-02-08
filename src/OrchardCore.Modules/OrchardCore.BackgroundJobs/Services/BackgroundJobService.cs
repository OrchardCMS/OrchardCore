using System;
using System.Threading.Tasks;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobStore _backgroundJobStore;
        
        private readonly IClock _clock;

        public BackgroundJobService(
            IBackgroundJobStore backgroundJobStore,
            IScheduleBuilderFactory _scheduleBuilderFactory,
            IClock clock)
        {
            _backgroundJobStore = backgroundJobStore;
            Schedule = _scheduleBuilderFactory;
            _clock = clock;
        }

        public IScheduleBuilderFactory Schedule { get; }

        public async ValueTask<string> CreateJobAsync(IBackgroundJob backgroundJob, IBackgroundJobSchedule schedule = null)
        {
            if (schedule is null)
            {
                schedule = new DateTimeSchedule()
                {
                    ExecutionUtc = _clock.UtcNow
                };
            }

            var document = new BackgroundJobExecution
            {
                BackgroundJob = backgroundJob,
                Schedule = schedule,
                State = new BackgroundJobState()
            };

            await _backgroundJobStore.CreateJobAsync(document);

            return document.BackgroundJob.BackgroundJobId;
        }

        public async ValueTask<(IBackgroundJob BackgroundJob, IBackgroundJobSchedule Schedule, BackgroundJobState State)> GetByCorrelationIdAsync<TJob>(string id) where TJob : class, IBackgroundJob, new()
        {
            var job = await _backgroundJobStore.GetJobByCorrelationIdAsync(id);
            if (job is null || job.BackgroundJob is not TJob)
            {
                return (null, null, null);
            }

            return (job.BackgroundJob as TJob, job.Schedule, job.State);
        }

        // TODO extension.
        public async ValueTask<(TJob BackgroundJob, IBackgroundJobSchedule Schedule, BackgroundJobState State)> GetByIdAsync<TJob>(string id) where TJob : class, IBackgroundJob, new()
        {
            var job = await this.GetByIdAsync(id);
            return (job.BackgroundJob as TJob, job.Schedule, job.State);
        }

        public async ValueTask<(IBackgroundJob BackgroundJob, IBackgroundJobSchedule Schedule, BackgroundJobState State)> GetByIdAsync(string id)
        {
            var job = await _backgroundJobStore.GetJobByIdAsync(id);
            if (job is null)
            {
                return (null, null, null);
            }

            return (job.BackgroundJob, job.Schedule, job.State);
        }

        // this could be an extension method.
        public async ValueTask<(bool Success, string BackgroundJobId)> TryRescheduleJobAsync(IBackgroundJob backgroundJob, IBackgroundJobSchedule schedule)
        {
            var existingExecution = await _backgroundJobStore.GetJobByIdAsync(backgroundJob.BackgroundJobId);

            if (existingExecution is null)
            {
                throw new NullReferenceException($"Background job '{backgroundJob.BackgroundJobId}' not found");
            }

            // When rescheduling check if we need to cancel the existing job.
            if (existingExecution.State.CurrentStatus == BackgroundJobStatus.Scheduled || existingExecution.State.CurrentStatus == BackgroundJobStatus.Queued)
            {
                if (!await TryCancelAsync(backgroundJob.BackgroundJobId))
                {
                    return (false, String.Empty);
                }
            }

            if (existingExecution.State.CurrentStatus == BackgroundJobStatus.Executing)
            {
                return (false, String.Empty);
            }

            var nextExecution = new BackgroundJobExecution
            {
                BackgroundJob = backgroundJob,
                Schedule = schedule,
                State = new BackgroundJobState()
            };

            await _backgroundJobStore.CreateJobAsync(nextExecution);

            return (true, nextExecution.BackgroundJob.BackgroundJobId);
        }

        public async ValueTask<bool> TryCancelAsync(string id)
        {
            var document = await _backgroundJobStore.GetJobByIdAsync(id);
            if (document is null)
            {
                return false;
            }

            // TODO cancellation token?

            if (document.State.CurrentStatus != BackgroundJobStatus.Scheduled)
            {
                return false;
            }

            document.State.UpdateState(BackgroundJobStatus.Cancelled, _clock.UtcNow);
            await _backgroundJobStore.UpdateJobAsync(document);

            return true;
        }
    }
}
