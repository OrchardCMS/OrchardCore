using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.Jobs.Services;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Jobs.BackgroundTasks;

/// <summary>
/// Every minute, on every server: queues the runs of the schedules that are due, starts the queued runs that are
/// due, takes care of the runs whose server stopped, and deletes the old runs. It never executes a run itself, so a
/// long run doesn't hold up the other background tasks.
/// </summary>
/// <remarks>
/// This is a background task, and not a job, because every server has to do it: each server offers to execute the
/// runs. It takes no lock, as what it changes is saved with a concurrency check: when two servers do the same thing
/// at once, one of them is ignored.
/// </remarks>
[BackgroundTask(
    Title = "Jobs",
    Schedule = "* * * * *",
    Description = "Queues the runs of the job schedules that are due, starts the queued job runs, and cleans up the old ones.")]
public sealed class JobsBackgroundTask : IBackgroundTask
{
    private const int BatchSize = 50;

    // A run this server claims to execute, but doesn't, was left by an earlier process with the same identifier.
    private static readonly TimeSpan _orphanedAfter = TimeSpan.FromMinutes(1);

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var session = serviceProvider.GetRequiredService<ISession>();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var dispatcher = serviceProvider.GetRequiredService<JobRunDispatcher>();
        var tracker = serviceProvider.GetRequiredService<JobRunTracker>();
        var coordinator = serviceProvider.GetRequiredService<JobRunCoordinator>();
        var scheduleManager = serviceProvider.GetRequiredService<JobScheduleManager>();
        var state = serviceProvider.GetRequiredService<JobsState>();
        var options = serviceProvider.GetRequiredService<IOptions<JobExecutionOptions>>().Value;
        var nodeId = serviceProvider.GetRequiredService<IOptions<NodeOptions>>().Value.Id;
        var logger = serviceProvider.GetRequiredService<ILogger<JobsBackgroundTask>>();

        // The schedules the jobs declare are created once per start of the tenant; the admin creates them as well.
        if (!state.DeclaredSchedulesEnsured)
        {
            await scheduleManager.EnsureDeclaredAsync();
            await session.SaveChangesAsync(cancellationToken);
            state.DeclaredSchedulesEnsured = true;
        }

        // A schedule that follows the time zone of the site moves with it, before its occurrences are looked at.
        await scheduleManager.RefreshNextOccurrencesAsync();
        await scheduleManager.QueueDueAsync();

        var now = clock.UtcNow;
        var queued = nameof(JobRunStatus.Queued);
        var running = nameof(JobRunStatus.Running);
        var waiting = nameof(JobRunStatus.Waiting);

        // Runs normally start when the request that queued them ends; start the ones that are due since, or soon.
        var dueBefore = now.AddMinutes(1);
        var due = await session.QueryIndex<JobRunIndex>(index => index.Status == queued && index.DueUtc <= dueBefore, collection: JobRun.Collection)
            .OrderBy(index => index.DueUtc)
            .Take(BatchSize)
            .ListAsync(cancellationToken);

        var pinnedBefore = now - (options.AbandonedAfter * 3);

        foreach (var run in due)
        {
            if (run.RequiredNodeId is null || run.RequiredNodeId == nodeId)
            {
                dispatcher.Dispatch(run.RunId, run.DueUtc);
            }
            else if (run.DueUtc < pinnedBefore)
            {
                // Only its server may execute it, and it didn't: it stopped.
                await FailPinnedAsync(run.RunId);
            }
        }

        // A run that stopped reporting was abandoned by its server, such as when it restarted.
        // A run misses several heartbeats before it is considered abandoned, whatever their interval: a run can
        // still execute in the previous context of a tenant that was reloaded, which this context doesn't track.
        var missedHeartbeats = options.HeartbeatInterval * 3;
        var staleBefore = now - (options.AbandonedAfter > missedHeartbeats ? options.AbandonedAfter : missedHeartbeats);
        var orphanedBefore = now - (_orphanedAfter > missedHeartbeats ? _orphanedAfter : missedHeartbeats);
        var stale = await session.QueryIndex<JobRunIndex>(index => index.Status == running && (index.HeartbeatUtc < staleBefore || (index.NodeId == nodeId && index.HeartbeatUtc < orphanedBefore)), collection: JobRun.Collection)
            .Take(BatchSize)
            .ListAsync(cancellationToken);

        foreach (var run in stale)
        {
            if (tracker.IsRunning(run.RunId))
            {
                continue;
            }

            await AbandonAsync(run.RunId);
        }

        // A run completes with its last child run; check the ones whose child runs ended at the same time.
        var waitingRuns = await session.QueryIndex<JobRunIndex>(index => index.Status == waiting, collection: JobRun.Collection)
            .Take(BatchSize)
            .ListAsync(cancellationToken);

        foreach (var run in waitingRuns)
        {
            await coordinator.TryCompleteParentAsync(run.RunId);
        }

        if (options.RunRetentionDays > 0)
        {
            var completedBefore = now.AddDays(-options.RunRetentionDays);
            var old = await session.Query<JobRun, JobRunIndex>(index => index.CompletedUtc != null && index.CompletedUtc < completedBefore, collection: JobRun.Collection)
                .Take(100)
                .ListAsync(cancellationToken);

            foreach (var run in old)
            {
                session.Delete(run, collection: JobRun.Collection);
            }
        }

        async Task AbandonAsync(string runId)
        {
            var requeued = false;

            var (run, outcome) = await JobRunStore.UpdateAsync(runId, run =>
            {
                // Its server reported in between, or recorded its end. The run is checked as it is read here, as
                // the dates of the index don't have the precision of the dates of the document.
                if (run.Status != JobRunStatus.Running
                    || !(run.HeartbeatUtc < staleBefore || (run.NodeId == nodeId && run.HeartbeatUtc < orphanedBefore)))
                {
                    return false;
                }

                const string message = "The server that was executing the run stopped.";

                if (run.CancellationRequested)
                {
                    // Its cancellation was requested, which its server didn't record: it isn't tried again.
                    run.Status = JobRunStatus.Canceled;
                    run.CompletedUtc = now;
                    run.Error = run.Superseded ? JobRunCoordinator.SupersededError : "The run was canceled.";
                    requeued = false;
                }
                else if (run.Attempt < run.MaxAttempts)
                {
                    run.Status = JobRunStatus.Queued;
                    run.DueUtc = now;
                    run.NodeId = null;
                    JobRunStore.AddLog(run, now, LogLevel.Warning, $"Attempt {run.Attempt} of {run.MaxAttempts} failed: {message}");
                    requeued = true;
                }
                else
                {
                    run.Status = JobRunStatus.Failed;
                    run.CompletedUtc = now;
                    run.Error = message;
                    JobRunStore.AddLog(run, now, LogLevel.Error, message);
                    requeued = false;
                }

                return true;
            });

            if (outcome != JobRunUpdateOutcome.Saved)
            {
                return;
            }

            logger.LogWarning("The job run '{RunId}' stopped reporting and was {Outcome}.", runId, requeued ? "queued again" : run.Status == JobRunStatus.Canceled ? "canceled" : "marked as failed");

            if (requeued)
            {
                dispatcher.Dispatch(runId);
            }
            else
            {
                await coordinator.OnCompletedAsync(run);
            }
        }

        async Task FailPinnedAsync(string runId)
        {
            const string message = "The server that had to execute the run stopped before executing it.";

            var (run, outcome) = await JobRunStore.UpdateAsync(runId, run =>
            {
                if (run.Status != JobRunStatus.Queued || run.RequiredNodeId is null || run.RequiredNodeId == nodeId || run.DueUtc >= pinnedBefore)
                {
                    return false;
                }

                run.Status = JobRunStatus.Failed;
                run.CompletedUtc = now;
                run.Error = message;
                JobRunStore.AddLog(run, now, LogLevel.Error, message);

                return true;
            });

            if (outcome == JobRunUpdateOutcome.Saved)
            {
                logger.LogWarning("The job run '{RunId}' failed: {Message}", runId, message);
                await coordinator.OnCompletedAsync(run);
            }
        }
    }
}
