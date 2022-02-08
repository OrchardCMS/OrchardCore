using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundJobs.Events;
using OrchardCore.BackgroundJobs.Models;
using OrchardCore.BackgroundTasks;
using OrchardCore.Environment.Shell;
using OrchardCore.Locking.Distributed;
using OrchardCore.Modules;

namespace OrchardCore.BackgroundJobs.Services
{
    [BackgroundTask(Schedule = "* * * * *", Description = "Schedule Jobs.")]
    public class ScheduleJobsBackgroundTask : IBackgroundTask
    {
        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var logger = serviceProvider.GetService<ILogger<ScheduleJobsBackgroundTask>>();
            var backgroundJobEntries = serviceProvider.GetRequiredService<IBackgroundJobEntries>();
            var entries = await backgroundJobEntries.GetEntriesAsync();
            if (!entries.Any())
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("No background job entries found");
                }

                return;
            }

            var executableEntries = entries.Where(x => x.Status != BackgroundJobStatus.Executing);
            if (!executableEntries.Any())
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("No executable background job entries found");
                }

                return;
            }

            var scheduleHandler = serviceProvider.GetRequiredService<IBackgroundJobScheduleHandler>();

            List<(BackgroundJobEntry Entry, long Priority)> jobsToRun = null;

            foreach (var entry in executableEntries)
            {
                var canRun = scheduleHandler.CanRun(entry);
                if (canRun.CanRun)
                {
                    jobsToRun ??= new List<(BackgroundJobEntry Entry, long Priority)>();
                    jobsToRun.Add((entry, canRun.Priority));
                }
            }

            if (jobsToRun is null)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("No background job entries that can run found");
                }
                return;
            }

            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("{Count} runnable backgound job entries found", entries.Count());
            }

            var shellShettings = serviceProvider.GetRequiredService<ShellSettings>();
            var queue = serviceProvider.GetRequiredService<IBackgroundJobQueue>();
            var backgroundJobStore = serviceProvider.GetRequiredService<IBackgroundJobStore>();
            var clock = serviceProvider.GetRequiredService<IClock>();
            var eventHandlers = serviceProvider.GetServices<IBackgroundJobEvent>();
            var distributedLock = serviceProvider.GetRequiredService<IDistributedLock>();

            var backgroundJobIds = jobsToRun.Select(x => x.Entry.BackgroundJobId).ToArray();
            var backgroundJobExecutions = (await backgroundJobStore.GetJobsByIdAsync(backgroundJobIds)).ToDictionary(x => x.BackgroundJob.BackgroundJobId);
            var executingNames = entries.Where(x => x.Status == BackgroundJobStatus.Executing).Select(x => x.Name).ToList();

            var jobContexts = jobsToRun.Select(job =>
            {
                if (backgroundJobExecutions.TryGetValue(job.Entry.BackgroundJobId, out var backgroundJobExecution))
                {
                    return (BackgroundJobDocument: backgroundJobExecution, ScheduleContext: new BackgroundJobScheduleContext(backgroundJobExecution, job.Priority, executingNames));
                }
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning("Background job '{BackgroundJobId}' not found", job.Entry.BackgroundJobId);
                }

                return (BackgroundJobDocument: null, ScheduleContext: null);
            })
            .Where(x => x.BackgroundJobDocument is not null && x.ScheduleContext is not null);

            foreach (var context in jobContexts)
            {
                await eventHandlers.InvokeValueAsync((handler, context) => handler.SchedulingAsync(context.ScheduleContext), context, logger);
            }

            var prioritizedJobContexts = jobContexts.Where(x => x.ScheduleContext.CanRun).OrderByDescending(x => x.ScheduleContext.Priority);

            foreach (var prioritizedContext in prioritizedJobContexts)
            {
                var backgroundJobExecution = prioritizedContext.BackgroundJobDocument;
                var scheduleContext = prioritizedContext.ScheduleContext;

                await eventHandlers.InvokeValueAsync((handler, context) => handler.ScheduledAsync(context), scheduleContext, logger);

                if (!scheduleContext.CanRun)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Background job '{Name}', id '{BackgroundJobId}' not scheduled", backgroundJobExecution.BackgroundJob.Name, backgroundJobExecution.BackgroundJob.BackgroundJobId);
                    }

                    continue;
                }

                (var locker, var locked) = await distributedLock.TryAcquireBackgroundJobLockAsync(backgroundJobExecution.BackgroundJob.BackgroundJobId);
                if (!locked)
                {
                    logger.LogDebug("Could not aquire scheduling lock on Background job '{Name}', id '{BackgroundJobId}'", backgroundJobExecution.BackgroundJob.Name, backgroundJobExecution.BackgroundJob.BackgroundJobId);
                    continue;
                }

                await using (var acquiredLock = locker)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Queuing background job '{Name}', id '{BackgroundJobId}'", backgroundJobExecution.BackgroundJob.Name, backgroundJobExecution.BackgroundJob.BackgroundJobId);
                    }

                    var ready = new TaskCompletionSource();

                    await queue.QueueBackgroundJobAsync(new BackgroundJobQueueItem(shellShettings.Name, backgroundJobExecution.BackgroundJob, ready.Task));

                    try
                    {
                        executingNames.Add(backgroundJobExecution.BackgroundJob.Name);
                        backgroundJobExecution.State.UpdateState(BackgroundJobStatus.Executing, clock.UtcNow);
                        await backgroundJobStore.UpdateJobAsync(backgroundJobExecution);
                        ready.SetResult();
                    }
                    catch (Exception ex)
                    {
                        ready.SetException(ex);
                    }
                }
            }
        }
    }
}
