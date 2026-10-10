using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Services;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.DataPipelines.BackgroundTasks;

/// <summary>
/// Starts the queued runs that didn't start yet, fails the runs that stopped reporting their progress, such as when
/// the site restarted, deletes the runs older than <see cref="DataPipelineOptions.RunRetentionDays"/>, and deletes the
/// files shared through links that stopped working. It never
/// executes a run itself, so a long run doesn't hold up the other background tasks.
/// </summary>
[BackgroundTask(
    Title = "Data pipeline runs",
    Schedule = "* * * * *",
    Description = "Starts queued data pipeline runs and cleans up old runs.",
    LockTimeout = 3_000,
    LockExpiration = 30_000)]
public sealed class DataPipelineRunsBackgroundTask : IBackgroundTask
{
    private static readonly TimeSpan _queueDelay = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan _staleAfter = TimeSpan.FromMinutes(10);

    public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var session = serviceProvider.GetRequiredService<ISession>();
        var clock = serviceProvider.GetRequiredService<IClock>();
        var dispatcher = serviceProvider.GetRequiredService<DataPipelineRunDispatcher>();
        var tracker = serviceProvider.GetRequiredService<DataPipelineRunTracker>();
        var options = serviceProvider.GetRequiredService<IOptions<DataPipelineOptions>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<DataPipelineRunsBackgroundTask>>();
        var now = clock.UtcNow;

        // Runs normally start when the request that queued them ends; start the ones that were missed.
        var queuedBefore = now - _queueDelay;
        var queuedStatus = nameof(DataPipelineRunStatus.Queued);
        var queued = await session.QueryIndex<DataPipelineRunIndex>(index => index.Status == queuedStatus && index.QueuedUtc < queuedBefore)
            .OrderBy(index => index.QueuedUtc)
            .Take(20)
            .ListAsync(cancellationToken);

        foreach (var run in queued)
        {
            dispatcher.Dispatch(run.RunId);
        }

        // A run that stopped reporting its progress stopped, such as when the site restarted.
        var staleBefore = now - _staleAfter;
        var runningStatus = nameof(DataPipelineRunStatus.Running);
        var stale = await session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.Status == runningStatus && index.HeartbeatUtc < staleBefore)
            .Take(20)
            .ListAsync(cancellationToken);

        foreach (var run in stale)
        {
            if (tracker.IsRunning(run.RunId))
            {
                continue;
            }

            run.Status = DataPipelineRunStatus.Failed;
            run.CompletedUtc = now;
            run.Error = "The run stopped unexpectedly, such as when the site restarted.";
            await session.SaveAsync(run, cancellationToken: cancellationToken);
            DataPipelineRunNotifications.NotifyAfterCommit(run);

            logger.LogWarning("The data pipeline run '{RunId}' stopped reporting its progress and was marked as failed.", run.RunId);
        }

        // The content of a shared file is kept a day after its link stops working, then deleted.
        var sharedFileManager = serviceProvider.GetRequiredService<DataPipelineSharedFileManager>();

        foreach (var sharedFile in await sharedFileManager.ListExpiredAsync(now.AddDays(-1), 50))
        {
            sharedFileManager.Delete(sharedFile);
        }

        if (options.RunRetentionDays > 0)
        {
            var completedBefore = now.AddDays(-options.RunRetentionDays);
            var old = await session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.CompletedUtc != null && index.CompletedUtc < completedBefore)
                .Take(100)
                .ListAsync(cancellationToken);

            foreach (var run in old)
            {
                session.Delete(run);
            }
        }
    }
}
