using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// Does what follows the end of a run, whichever way it ended: tells the handlers, completes its parent run once
/// all its child runs ended, starts the next run that waited for it, and deletes it when its job doesn't keep
/// succeeded runs.
/// </summary>
public sealed class JobRunCoordinator
{
    private readonly JobRunDispatcher _dispatcher;
    private readonly JobOptions _jobOptions;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public JobRunCoordinator(
        JobRunDispatcher dispatcher,
        IOptions<JobOptions> jobOptions,
        IClock clock,
        ILogger<JobRunCoordinator> logger)
    {
        _dispatcher = dispatcher;
        _jobOptions = jobOptions.Value;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Does what follows the end of a run. Call it once the end of the run is committed.
    /// </summary>
    /// <param name="run">The run that ended.</param>
    /// <returns>A task that completes when the handlers were called and the parent run was checked.</returns>
    public async Task OnCompletedAsync(JobRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        await ShellScope.UsingChildScopeAsync(scope =>
        {
            var handlers = scope.ServiceProvider.GetServices<IJobRunHandler>();

            return handlers.InvokeAsync((handler, completed) => handler.CompletedAsync(completed), run, _logger);
        });

        if (!string.IsNullOrEmpty(run.ParentRunId))
        {
            await TryCompleteParentAsync(run.ParentRunId);
        }
        else if (run.Status == JobRunStatus.Succeeded && _jobOptions.Find(run.JobName) is { KeepSucceededRuns: false })
        {
            // A child run is kept with its parent, which is what tells how the whole work went.
            await DeleteWithChildrenAsync(run.RunId);
        }

        if (!string.IsNullOrEmpty(run.ConcurrencyKey))
        {
            await DispatchNextAsync(run.ConcurrencyKey);
        }
    }

    /// <summary>
    /// Completes a run that waits for its child runs, once they all ended: it succeeds when they all succeeded.
    /// </summary>
    /// <param name="parentRunId">The run that waits for its child runs.</param>
    /// <returns>A task that completes when the run was checked.</returns>
    public async Task TryCompleteParentAsync(string parentRunId)
    {
        var children = await JobRunStore.LoadChildrenAsync(parentRunId);

        if (children.Any(child => !child.IsCompleted))
        {
            return;
        }

        var failed = children.Count(child => child.Status != JobRunStatus.Succeeded);
        var now = _clock.UtcNow;

        var (parent, outcome) = await JobRunStore.UpdateAsync(parentRunId, run =>
        {
            if (run.Status != JobRunStatus.Waiting)
            {
                return false;
            }

            run.CompletedUtc = now;
            run.HeartbeatUtc = now;

            if (failed == 0)
            {
                run.Status = JobRunStatus.Succeeded;
                run.Progress = 100;
            }
            else
            {
                run.Status = JobRunStatus.Failed;
                run.Error = $"{failed} of its {children.Count} child runs didn't succeed.";
                JobRunStore.AddLog(run, now, LogLevel.Error, run.Error);
            }

            return true;
        });

        if (outcome == JobRunUpdateOutcome.Saved)
        {
            await OnCompletedAsync(parent);
        }
    }

    // Starts the oldest run that waited for a run with the same concurrency key to end.
    private async Task DispatchNextAsync(string concurrencyKey)
    {
        var key = JobRunIndexProvider.Truncate(concurrencyKey, JobRunIndex.MaxKeyLength);
        var queued = nameof(JobRunStatus.Queued);
        var now = _clock.UtcNow;
        JobRunIndex next = null;

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            next = await scope.ServiceProvider.GetRequiredService<ISession>()
                .QueryIndex<JobRunIndex>(index => index.ConcurrencyKey == key && index.Status == queued && index.DueUtc <= now, collection: JobRun.Collection)
                .OrderBy(index => index.DueUtc)
                .FirstOrDefaultAsync();
        });

        if (next is not null)
        {
            _dispatcher.Dispatch(next.RunId);
        }
    }

    private static Task DeleteWithChildrenAsync(string runId)
    {
        return ShellScope.UsingChildScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            foreach (var child in await JobRunStore.QueryChildrenAsync(session, runId))
            {
                session.Delete(child, collection: JobRun.Collection);
            }

            var run = await JobRunStore.QueryAsync(session, runId);

            if (run is not null)
            {
                session.Delete(run, collection: JobRun.Collection);
            }
        });
    }
}
