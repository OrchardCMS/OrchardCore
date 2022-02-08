using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCrontab;
using OrchardCore.BackgroundJobs.Events;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundJobs.Services;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Handlers
{
    public class BackgroundJobEventHandler : BackgroundJobEventBase
    {
        private readonly BackgroundJobOptions _backgroundJobOptions;
        private readonly IBackgroundJobIdGenerator _backgroundJobIdGenerator;
        private readonly IBackgroundJobEntries _entries;
        private readonly IClock _clock;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private IBackgroundJobStore _backgroundJobStore;
        private IBackgroundJobService _backgroundJobService;

        public BackgroundJobEventHandler(
            IOptions<BackgroundJobOptions> options,
            IBackgroundJobIdGenerator backgroundJobIdGenerator,
            IBackgroundJobEntries entries,
            IClock clock,
            IServiceProvider serviceProvider,
            ILogger<BackgroundJobEventHandler> logger)
        {
            _backgroundJobOptions = options.Value;
            _backgroundJobIdGenerator = backgroundJobIdGenerator;
            _entries = entries;
            _clock = clock;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public override ValueTask CreatingAsync(BackgroundJobCreateContext context)
        {
            context.BackgroundJobExecution.BackgroundJob.BackgroundJobId = _backgroundJobIdGenerator.GenerateUniqueId(context.BackgroundJobExecution.BackgroundJob);
            context.BackgroundJobExecution.State.UpdateState(BackgroundJobStatus.Scheduled, _clock.UtcNow);

            // The first background job id is always used to link jobs together.
            if (context.BackgroundJobExecution.Schedule is IBackgroundJobRepeatSchedule && String.IsNullOrEmpty(context.BackgroundJobExecution.BackgroundJob.RepeatCorrelationId))
            {
                context.BackgroundJobExecution.BackgroundJob.RepeatCorrelationId = context.BackgroundJobExecution.BackgroundJob.BackgroundJobId;
            }

            return default;
        }

        public async override ValueTask CreatedAsync(BackgroundJobCreateContext context)
        {
            var execution = context.BackgroundJobExecution;
            var backgroundJob = execution.BackgroundJob;
            await _entries.AddOrUpdateEntryAsync(new BackgroundJobEntry(backgroundJob.BackgroundJobId, backgroundJob.Name, execution.State.CurrentStatus, execution.Schedule.ExecutionUtc));
        }

        public override async ValueTask UpdatedAsync(BackgroundJobUpdateContext context)
        {
            var execution = context.BackgroundJobExecution;
            var backgroundJob = execution.BackgroundJob;
            if (execution.State.CurrentStatus < BackgroundJobStatus.Executed)
            {
                await _entries.AddOrUpdateEntryAsync(new BackgroundJobEntry(backgroundJob.BackgroundJobId, backgroundJob.Name, execution.State.CurrentStatus, execution.Schedule.ExecutionUtc));
            }
            else
            {
                await _entries.RemoveEntryAsync(backgroundJob.BackgroundJobId);
            }
        }

        public override ValueTask ExecutingAsync(BackgroundJobExecutionContext context)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Executing job '{Name}' instance '{BackgroundJobId}'", context.BackgroundJobExecution.BackgroundJob.Name, context.BackgroundJobExecution.BackgroundJob.BackgroundJobId);
            }

            return default;
        }

        public override ValueTask ScheduledAsync(BackgroundJobScheduleContext context)
        {
            var name = context.BackgroundJobExecution.BackgroundJob.Name;
            var option = _backgroundJobOptions.BackgroundJobs[name];
            if (option?.ConcurrentJobsLimit > 0)
            {
                var executingCount = context.ExecutingNames.Count(x => x.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (executingCount >= option.ConcurrentJobsLimit)
                {
                    context.CanRun = false;
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Concurrency count '{Concurrency}' limit reached for job '{Name}' id '{BackgroundJobId}'", option.ConcurrentJobsLimit, context.BackgroundJobExecution.BackgroundJob.Name, context.BackgroundJobExecution.BackgroundJob.BackgroundJobId);
                    }
                }
            }

            return default;
        }

        public override async ValueTask ExecutedAsync(BackgroundJobExecutionContext context)
        {
            _backgroundJobStore ??= _serviceProvider.GetRequiredService<IBackgroundJobStore>();

            context.BackgroundJobExecution.State.UpdateState(BackgroundJobStatus.Executed, _clock.UtcNow);

            await _backgroundJobStore.UpdateJobAsync(context.BackgroundJobExecution);
            // TODO consider abstracting this to an IRepeatSchedule, with a handler for it.
            if (context.BackgroundJobExecution.Schedule is RepeatCrontabSchedule crontabSchedule)
            {
                _backgroundJobService ??= _serviceProvider.GetRequiredService<IBackgroundJobService>();

                var schedule = CrontabSchedule.Parse(crontabSchedule.RepeatCrontab);
                var whenUtc = schedule.GetNextOccurrence(_clock.UtcNow);

                var nextExecution = new BackgroundJobExecution
                {
                    BackgroundJob = context.BackgroundJobExecution.BackgroundJob,
                    Schedule = new RepeatCrontabSchedule()
                    {
                        Initial = new DateTimeSchedule { ExecutionUtc = whenUtc },
                        RepeatCrontab = crontabSchedule.RepeatCrontab
                    },
                    State = new BackgroundJobState()
                };

                await _backgroundJobStore.CreateJobAsync(nextExecution);
            }
        }

        public override ValueTask DeletedAsync(BackgroundJobDeleteContext context)
           => _entries.RemoveEntryAsync(context.BackgroundJobExecution.BackgroundJob.BackgroundJobId);

        public override async ValueTask FailedAsync(BackgroundJobFailureContext context)
        {
            var retryCount = context.BackgroundJobExecution.State.CurrentRetryCount;
            var retryAttempts = _backgroundJobOptions.DefaultRetryAttempts;

            var option = _backgroundJobOptions.BackgroundJobs[context.BackgroundJobExecution.BackgroundJob.Name];
            if (option?.RetryAttempts is not null)
            {
                retryAttempts = option.RetryAttempts.Value;
            }

            if (retryCount < retryAttempts)
            {
                context.BackgroundJobExecution.State.CurrentRetryCount++;

                var retryInterval = _backgroundJobOptions.DefaultRetryInterval;
                if (option?.RetryInterval is not null)
                {
                    retryInterval = option.RetryInterval.Value;
                }

                context.BackgroundJobExecution.State.UpdateState(BackgroundJobStatus.Retrying, _clock.UtcNow, context.Exception);

                _backgroundJobService ??= _serviceProvider.GetRequiredService<IBackgroundJobService>();

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Retrying background job '{Name}', id '{BackgroundJobId}', retry count '{RetryCount}'", context.BackgroundJobExecution.BackgroundJob.Name, context.BackgroundJobExecution.BackgroundJob.BackgroundJobId, context.BackgroundJobExecution.State.CurrentRetryCount);
                }

                IBackgroundJobSchedule schedule = new DateTimeSchedule()
                {
                    ExecutionUtc = _clock.UtcNow.Add(retryInterval),
                };

                // TODO consider abstracting this.
                if (context.BackgroundJobExecution.Schedule is RepeatCrontabSchedule crontabSchedule)
                {
                    schedule = new RepeatCrontabSchedule()
                    {
                        Initial = schedule,
                        RepeatCrontab = crontabSchedule.RepeatCrontab
                    };
                }

                await _backgroundJobService.TryRescheduleJobAsync(
                    context.BackgroundJobExecution.BackgroundJob,
                    schedule
                );
            }
            else
            {
                context.BackgroundJobExecution.State.UpdateState(BackgroundJobStatus.Failed, _clock.UtcNow, context.Exception);

                _backgroundJobStore ??= _serviceProvider.GetRequiredService<IBackgroundJobStore>();
                await _backgroundJobStore.UpdateJobAsync(context.BackgroundJobExecution);

                _logger.LogError("Background job '{Name}', id '{BackgroundJobId}' exceeded retry count", context.BackgroundJobExecution.BackgroundJob.Name, context.BackgroundJobExecution.BackgroundJob.BackgroundJobId);
            }
        }
    }
}
