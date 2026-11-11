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
/// Executes a queued run on this server: takes it, so no other server executes it, runs its job, reports every few
/// seconds that it is still executing, and records how it ended.
/// </summary>
public sealed class JobRunExecutor
{
    private readonly JobRunTracker _tracker;
    private readonly JobRunCoordinator _coordinator;
    private readonly JobOptions _jobOptions;
    private readonly JobExecutionOptions _executionOptions;
    private readonly string _nodeId;
    private readonly IClock _clock;
    private readonly ILogger _logger;

    public JobRunExecutor(
        JobRunTracker tracker,
        JobRunCoordinator coordinator,
        IOptions<JobOptions> jobOptions,
        IOptions<JobExecutionOptions> executionOptions,
        IOptions<NodeOptions> nodeOptions,
        IClock clock,
        ILogger<JobRunExecutor> logger)
    {
        _tracker = tracker;
        _coordinator = coordinator;
        _jobOptions = jobOptions.Value;
        _executionOptions = executionOptions.Value;
        _nodeId = nodeOptions.Value.Id;
        _clock = clock;
        _logger = logger;
    }

    /// <summary>
    /// Executes a queued run. It does nothing when the run is not queued or not due, when another server takes it at
    /// the same time, or when a run with the same concurrency key is running: the run then waits for the background
    /// task to try again.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <returns>When this server should try to execute the run again, such as for its next attempt, or
    /// <see langword="null"/>.</returns>
    public async Task<DateTime?> ExecuteAsync(string runId)
    {
        var (run, retryUtc) = await ClaimAsync(runId);

        if (run is null)
        {
            return retryUtc;
        }

        var descriptor = _jobOptions.Find(run.JobName);

        using var cancellation = _tracker.Start(runId);

        try
        {
            return await RunAsync(run, descriptor, cancellation);
        }
        finally
        {
            _tracker.Complete(runId);
        }
    }

    // Takes the run: changes it from queued to running on this server. The change is only saved if nobody changed
    // the run in between, so when several servers try at once, a single one succeeds.
    private async Task<(JobRun Run, DateTime? RetryUtc)> ClaimAsync(string runId)
    {
        var now = _clock.UtcNow;
        var running = nameof(JobRunStatus.Running);

        var (run, outcome) = await JobRunStore.TryUpdateAsync(runId, async (session, run) =>
        {
            if (run.Status != JobRunStatus.Queued
                || run.DueUtc > now.AddSeconds(1)
                || (run.RequiredNodeId is not null && run.RequiredNodeId != _nodeId)
                || _jobOptions.Find(run.JobName) is null)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(run.ConcurrencyKey) && await IsKeyRunningAsync(session, run))
            {
                return false;
            }

            run.Status = JobRunStatus.Running;
            run.StartedUtc = now;
            run.HeartbeatUtc = now;
            run.CompletedUtc = null;
            run.NodeId = _nodeId;
            run.Attempt++;
            run.DurationMilliseconds = null;
            run.Progress = null;
            run.ProgressMessage = null;

            return true;
        });

        if (outcome != JobRunUpdateOutcome.Saved)
        {
            return (null, null);
        }

        if (string.IsNullOrEmpty(run.ConcurrencyKey))
        {
            return (run, null);
        }

        // Two servers may each have taken a run with the same key at the same time, as they are different documents.
        // Each one then checks again once its own run is saved as running: at least one of them sees the other, and
        // gives its run back. When both do, they try again a little later, at different times.
        var conflict = false;

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            conflict = await IsKeyRunningAsync(scope.ServiceProvider.GetRequiredService<ISession>(), run);
        });

        if (!conflict)
        {
            return (run, null);
        }

        var (_, released) = await JobRunStore.UpdateAsync(runId, taken =>
        {
            if (taken.Status != JobRunStatus.Running || taken.NodeId != _nodeId)
            {
                return false;
            }

            taken.Status = JobRunStatus.Queued;
            taken.StartedUtc = null;
            taken.HeartbeatUtc = null;
            taken.NodeId = null;
            taken.Attempt--;

            return true;
        });

        return (null, released == JobRunUpdateOutcome.Saved ? _clock.UtcNow.AddSeconds(Random.Shared.Next(2, 15)) : null);

        async Task<bool> IsKeyRunningAsync(ISession session, JobRun candidate)
        {
            var key = JobRunIndexProvider.Truncate(candidate.ConcurrencyKey, JobRunIndex.MaxKeyLength);
            var candidateId = candidate.RunId;

            return await session.QueryIndex<JobRunIndex>(index => index.ConcurrencyKey == key && index.Status == running && index.RunId != candidateId, collection: JobRun.Collection)
                .FirstOrDefaultAsync() is not null;
        }
    }

    private async Task<DateTime?> RunAsync(JobRun run, JobDescriptor descriptor, CancellationTokenSource cancellation)
    {
        var context = new DefaultJobExecutionContext(run, _clock);
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Exception error = null;

        try
        {
            // The job starts on another thread, so a job that blocks before its first await doesn't stop the heartbeat.
            var execution = Task.Run(() => ExecuteJobAsync(descriptor, context, cancellation.Token));

            using var delayCancellation = new CancellationTokenSource();

            while (await Task.WhenAny(execution, Task.Delay(_executionOptions.HeartbeatInterval, delayCancellation.Token)) != execution)
            {
                await HeartbeatAsync(run.RunId, context, cancellation);
            }

            await delayCancellation.CancelAsync();
            await execution;
        }
        catch (Exception ex)
        {
            error = ex;
        }

        return await CompleteAsync(run, descriptor, context, cancellation, error, stopwatch.ElapsedMilliseconds);
    }

    // Runs the job in a scope of its own, so what it saved is committed, and its deferred tasks are done, before the
    // run is recorded as succeeded. A commit that fails then fails the run.
    private static Task ExecuteJobAsync(JobDescriptor descriptor, DefaultJobExecutionContext context, CancellationToken cancellationToken)
    {
        return ShellScope.UsingChildScopeAsync(async scope =>
        {
            context.SetServiceProvider(scope.ServiceProvider);

            var job = (IJob)ActivatorUtilities.CreateInstance(scope.ServiceProvider, descriptor.JobType);

            try
            {
                await job.ExecuteAsync(context, cancellationToken);
            }
            finally
            {
                if (job is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (job is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        });
    }

    // Tells the other servers this run still executes, saves what the job reported, and reads what was asked from
    // another server: a cancellation, or the run being taken away because this server looked stopped.
    private async Task HeartbeatAsync(string runId, DefaultJobExecutionContext context, CancellationTokenSource cancellation)
    {
        try
        {
            var cancel = false;

            var (_, outcome) = await JobRunStore.UpdateAsync(runId, run =>
            {
                if (run.Status != JobRunStatus.Running || run.NodeId != _nodeId)
                {
                    return false;
                }

                context.Apply(run);
                run.HeartbeatUtc = _clock.UtcNow;
                cancel = run.CancellationRequested;

                return true;
            });

            if (outcome is JobRunUpdateOutcome.NotFound or JobRunUpdateOutcome.Refused)
            {
                _logger.LogWarning("The job run '{RunId}' doesn't belong to this server anymore, and is stopped.", runId);
                cancel = true;
            }

            if (cancel && !cancellation.IsCancellationRequested)
            {
                await cancellation.CancelAsync();
            }
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            // The next heartbeat tries again; the run is only considered abandoned after several minutes.
            _logger.LogWarning(ex, "The progress of the job run '{RunId}' couldn't be saved.", runId);
        }
    }

    private async Task<DateTime?> CompleteAsync(JobRun started, JobDescriptor descriptor, DefaultJobExecutionContext context, CancellationTokenSource cancellation, Exception error, long durationMilliseconds)
    {
        var runId = started.RunId;
        var now = _clock.UtcNow;
        var stopping = _tracker.Stopping.IsCancellationRequested;
        var canceled = cancellation.IsCancellationRequested;
        var waitsForChildren = false;
        var failedChildren = 0;
        var childrenCount = 0;

        if (error is null && context.HasChildren)
        {
            var children = await JobRunStore.LoadChildrenAsync(runId);
            childrenCount = children.Count;
            waitsForChildren = children.Any(child => !child.IsCompleted);
            failedChildren = children.Count(child => child.IsCompleted && child.Status != JobRunStatus.Succeeded);
        }

        if (error is not null && !(canceled && error is OperationCanceledException))
        {
            _logger.LogError(error, "The run '{RunId}' of the job '{JobName}' failed on its attempt {Attempt} of {MaxAttempts}.", runId, started.JobName, started.Attempt, started.MaxAttempts);
        }

        DateTime? retryUtc = null;

        var (run, outcome) = await JobRunStore.UpdateAsync(runId, run =>
        {
            // Another server took the run over, as this one looked stopped: that server records how the run ends.
            if (run.Status != JobRunStatus.Running || run.NodeId != _nodeId)
            {
                return false;
            }

            retryUtc = null;
            context.Apply(run);
            run.HeartbeatUtc = now;
            run.DurationMilliseconds = durationMilliseconds;

            if (error is null)
            {
                run.Result = context.Result;
                run.Error = null;
                run.ErrorDetails = null;

                if (waitsForChildren)
                {
                    run.Status = JobRunStatus.Waiting;
                    run.ProgressMessage = null;
                }
                else if (failedChildren > 0)
                {
                    run.Status = JobRunStatus.Failed;
                    run.CompletedUtc = now;
                    run.Error = $"{failedChildren} of its {childrenCount} child runs didn't succeed.";
                    JobRunStore.AddLog(run, now, LogLevel.Error, run.Error);
                }
                else
                {
                    run.Status = JobRunStatus.Succeeded;
                    run.CompletedUtc = now;
                    run.Progress = 100;
                }

                return true;
            }

            if (canceled && run.CancellationRequested)
            {
                run.Status = JobRunStatus.Canceled;
                run.CompletedUtc = now;
                run.Error = "The run was canceled.";

                return true;
            }

            var interrupted = canceled && stopping;
            var message = interrupted ? "The server stopped while it was executing the run." : error.Message;

            if (run.Attempt < run.MaxAttempts)
            {
                // A run that was interrupted starts again at once, on any server; a run that failed waits first.
                var delay = interrupted ? TimeSpan.Zero : descriptor.RetryDelay * Math.Pow(2, run.Attempt - 1);

                retryUtc = now + delay;
                run.Status = JobRunStatus.Queued;
                run.DueUtc = retryUtc.Value;
                run.NodeId = null;
                JobRunStore.AddLog(run, now, LogLevel.Warning, $"Attempt {run.Attempt} of {run.MaxAttempts} failed: {message}");

                return true;
            }

            run.Status = JobRunStatus.Failed;
            run.CompletedUtc = now;
            run.Error = message;
            run.ErrorDetails = interrupted ? null : error.ToString();
            JobRunStore.AddLog(run, now, LogLevel.Error, message);

            return true;
        });

        if (outcome != JobRunUpdateOutcome.Saved)
        {
            _logger.LogWarning("The end of the job run '{RunId}' couldn't be recorded by this server: {Outcome}.", runId, outcome);

            return null;
        }

        if (run.IsCompleted)
        {
            await _coordinator.OnCompletedAsync(run);
        }
        else if (run.Status == JobRunStatus.Waiting)
        {
            // A child run that ended while this run was still running didn't complete it.
            await _coordinator.TryCompleteParentAsync(runId);
        }
        else if (!stopping)
        {
            return retryUtc;
        }

        return null;
    }
}
