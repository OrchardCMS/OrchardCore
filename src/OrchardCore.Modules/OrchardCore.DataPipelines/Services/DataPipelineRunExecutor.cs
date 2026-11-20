using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.DataPipelines.Steps;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Jobs;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Executes a queued run, as its job run does: runs its definition with the access of the user who published the
/// pipeline, records its progress every few seconds, and records how it ended. The job run is what makes a single
/// server execute the run, and one run of a pipeline execute at a time.
/// </summary>
public sealed class DataPipelineRunExecutor
{
    private static readonly TimeSpan _progressInterval = TimeSpan.FromSeconds(3);

    private readonly DataPipelineExecutor _executor;
    private readonly DataPipelineUserResolver _userResolver;
    private readonly IClock _clock;
    private readonly ShellSettings _shellSettings;
    private readonly IOptions<ShellOptions> _shellOptions;
    private readonly IServiceProvider _services;
    private readonly ILogger _logger;

    public DataPipelineRunExecutor(
        DataPipelineExecutor executor,
        DataPipelineUserResolver userResolver,
        IClock clock,
        ShellSettings shellSettings,
        IOptions<ShellOptions> shellOptions,
        IServiceProvider services,
        ILogger<DataPipelineRunExecutor> logger)
    {
        _executor = executor;
        _userResolver = userResolver;
        _clock = clock;
        _shellSettings = shellSettings;
        _shellOptions = shellOptions;
        _services = services;
        _logger = logger;
    }

    /// <summary>
    /// Executes a queued run. It does nothing when the run is not queued anymore, such as when it was cancelled.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <param name="jobContext">The context of the job run that executes the run.</param>
    /// <param name="cancellationToken">A token that is cancelled when the job run is cancelled, or its server stops.</param>
    /// <returns>A task that completes when the run ends.</returns>
    /// <exception cref="InvalidOperationException">The run failed, which fails its job run.</exception>
    /// <exception cref="OperationCanceledException">The run was cancelled, or its server stops.</exception>
    public async Task ExecuteAsync(string runId, JobExecutionContext jobContext, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(jobContext);

        var queued = await LoadAsync(runId);

        if (queued is null || queued.Status != DataPipelineRunStatus.Queued)
        {
            jobContext.LogInformation("The data pipeline run is not queued anymore.");

            return;
        }

        var started = false;

        await UpdateAsync(runId, run =>
        {
            if (run.Status != DataPipelineRunStatus.Queued)
            {
                return false;
            }

            run.Status = DataPipelineRunStatus.Running;
            run.StartedUtc = _clock.UtcNow;
            run.HeartbeatUtc = run.StartedUtc;
            run.Steps = run.Definition.Steps
                .Select(step => new DataPipelineStepRun { StepId = step.StepId, Type = step.Type, Title = step.Title })
                .ToList();
            started = true;

            return true;
        });

        if (!started)
        {
            return;
        }

        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var completed = await RunAsync(queued, cancellation, cancellationToken);

        // The job run ends as the run did, so the runs of the jobs tell the same as the runs of the pipelines.
        if (completed?.Status == DataPipelineRunStatus.Cancelled)
        {
            throw new OperationCanceledException(completed.Error, cancellation.Token);
        }

        if (completed?.Status != DataPipelineRunStatus.Succeeded)
        {
            throw new InvalidOperationException(completed?.Error ?? "The data pipeline run failed.");
        }

        jobContext.SetResult(new { Deliveries = completed.Deliveries.Count });
    }

    private async Task<DataPipelineRun> RunAsync(DataPipelineRun queued, CancellationTokenSource cancellation, CancellationToken stopping)
    {
        var runId = queued.RunId;
        var observer = new RunObserver(_clock);
        var directory = Path.Combine(_shellOptions.Value.ShellsApplicationDataPath, _shellOptions.Value.ShellsContainerName, _shellSettings.Name, "DataPipelines", "Runs", runId);
        DataPipelineExecutionResult result = null;
        DataPipelineRun completed = null;

        try
        {
            var user = await _userResolver.GetPrincipalAsync(queued.RunAsUserId);

            if (user is null)
            {
                result = new DataPipelineExecutionResult
                {
                    Status = DataPipelineRunStatus.Failed,
                    Error = "The user who published the pipeline doesn't exist anymore, or is disabled. Publish the pipeline again to run it.",
                };
            }
            else
            {
                result = await RunDefinitionAsync(queued, user, directory, observer, cancellation);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The data pipeline run '{RunId}' failed.", runId);

            result = new DataPipelineExecutionResult
            {
                Status = cancellation.IsCancellationRequested ? DataPipelineRunStatus.Cancelled : DataPipelineRunStatus.Failed,
                Error = ex.Message,
            };
        }
        finally
        {
            DeleteDirectory(directory);

            await UpdateAsync(runId, run =>
            {
                observer.Apply(run);

                run.Status = result?.Status ?? DataPipelineRunStatus.Failed;
                run.Error = result?.Error;
                run.FailedStepId = result?.FailedStepId;
                run.CompletedUtc = _clock.UtcNow;
                run.HeartbeatUtc = run.CompletedUtc;
                run.Deliveries = result?.Deliveries?.ToList() ?? [];

                if (run.Status == DataPipelineRunStatus.Cancelled && stopping.IsCancellationRequested && !run.CancellationRequested)
                {
                    run.Error = "The run was stopped, such as when the site shut down.";
                }

                foreach (var issue in result?.Issues ?? [])
                {
                    AddLog(run, new DataPipelineLogEntry
                    {
                        Level = issue.Severity == DataPipelineIssueSeverity.Error ? LogLevel.Error : LogLevel.Warning,
                        StepId = issue.StepId,
                        Message = issue.Message,
                    });
                }

                if (run.Status == DataPipelineRunStatus.Failed && !string.IsNullOrEmpty(run.Error))
                {
                    AddLog(run, new DataPipelineLogEntry { Level = LogLevel.Error, StepId = run.FailedStepId, Message = run.Error });
                }

                completed = run;

                return true;
            });

            if (completed is not null)
            {
                await NotifyAsync(completed);
            }
        }

        return completed;
    }

    // Runs the definition of the run, and records its progress every few seconds while it runs.
    private async Task<DataPipelineExecutionResult> RunDefinitionAsync(DataPipelineRun queued, System.Security.Claims.ClaimsPrincipal user, string directory, RunObserver observer, CancellationTokenSource cancellation)
    {
        var runId = queued.RunId;

        Directory.CreateDirectory(directory);

        var context = new DataPipelineRunContext
        {
            RunId = runId,
            PipelineId = queued.PipelineId,
            PipelineName = queued.PipelineName,
            VersionNumber = queued.VersionNumber,
            User = user,
            Services = _services,
            Parameters = queued.Parameters.ToDictionary(pair => pair.Key, pair => (object)pair.Value, StringComparer.OrdinalIgnoreCase),
            StartedUtc = _clock.UtcNow,
            TemporaryDirectory = directory,
            CancellationToken = cancellation.Token,
            PreviewRowLimit = DataPipelineFormulas.GetOptions(_services).PreviewRowLimit,

            // Steps run at the same time, so each one gets its own scope, and its own database session.
            StepScope = (step, work) => ShellScope.UsingChildScopeAsync(scope => work(scope.ServiceProvider)),
            State = await LoadStateAsync(queued.PipelineId),
        };

        var execution = _executor.ExecuteAsync(queued.Definition, context, observer);

        using var delayCancellation = new CancellationTokenSource();

        while (await Task.WhenAny(execution, Task.Delay(_progressInterval, delayCancellation.Token)) != execution)
        {
            await UpdateAsync(runId, run =>
            {
                observer.Apply(run);
                run.HeartbeatUtc = _clock.UtcNow;

                // Asked from the pipeline pages; the cancellation of the job run cancels the token as well.
                if (run.CancellationRequested && !cancellation.IsCancellationRequested)
                {
                    cancellation.Cancel();
                }

                return true;
            });
        }

        delayCancellation.Cancel();

        var result = await execution;

        if (result.Status == DataPipelineRunStatus.Succeeded)
        {
            await CommitAsync(queued.PipelineId, context, result, cancellation.Token);
        }

        return result;
    }

    // Runs the actions the steps asked for once the run succeeded, then saves the state of the pipeline. When one
    // fails, the run fails and the state isn't saved, so the next run reads again what this one couldn't confirm.
    private async Task CommitAsync(string pipelineId, DataPipelineRunContext context, DataPipelineExecutionResult result, CancellationToken cancellationToken)
    {
        try
        {
            foreach (var action in context.SucceededActions)
            {
                await ShellScope.UsingChildScopeAsync(scope => action(scope.ServiceProvider, cancellationToken));
            }

            if (context.State.IsChanged)
            {
                await ShellScope.UsingChildScopeAsync(scope => scope.ServiceProvider.GetRequiredService<DataPipelineStateManager>().SaveAsync(pipelineId, context.State, context.RunId));
            }
        }
        catch (Exception ex) when (!ex.IsFatal())
        {
            _logger.LogError(ex, "The data pipeline run '{RunId}' delivered its data, but couldn't confirm it.", context.RunId);

            result.Status = DataPipelineRunStatus.Failed;
            result.Error = $"The run delivered its data, but couldn't confirm it, so the next run starts from the same point: {ex.Message}";
        }
    }

    private static async Task<DataPipelineState> LoadStateAsync(string pipelineId)
    {
        DataPipelineState state = null;

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            state = await scope.ServiceProvider.GetRequiredService<DataPipelineStateManager>().GetAsync(pipelineId);
        });

        return state;
    }

    private static Task NotifyAsync(DataPipelineRun run)
    {
        return ShellScope.UsingChildScopeAsync(scope => DataPipelineRunNotifications.NotifyAsync(scope.ServiceProvider, run));
    }

    private static async Task<DataPipelineRun> LoadAsync(string runId)
    {
        DataPipelineRun run = null;

        await ShellScope.UsingChildScopeAsync(async scope =>
        {
            run = await scope.ServiceProvider.GetRequiredService<ISession>()
                .Query<DataPipelineRun, DataPipelineRunIndex>(index => index.RunId == runId)
                .FirstOrDefaultAsync();
        });

        return run;
    }

    /// <summary>
    /// Changes a run in its own scope, so the change is saved at once, whatever the run's steps do.
    /// </summary>
    private static Task UpdateAsync(string runId, Func<DataPipelineRun, bool> change)
    {
        return ShellScope.UsingChildScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();
            var run = await session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.RunId == runId).FirstOrDefaultAsync();

            if (run is not null && change(run))
            {
                await session.SaveAsync(run);
            }
        });
    }

    private static void AddLog(DataPipelineRun run, DataPipelineLogEntry entry)
    {
        if (run.Log.Count < DataPipelineRun.MaxLogEntries)
        {
            run.Log.Add(entry);
        }
    }

    private void DeleteDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogWarning(ex, "The temporary folder '{Directory}' of a data pipeline run couldn't be deleted.", directory);
        }
    }

    /// <summary>
    /// Collects the progress of the steps while the engine runs them.
    /// </summary>
    private sealed class RunObserver : IDataPipelineRunObserver
    {
        private readonly IClock _clock;
        private readonly ConcurrentDictionary<string, StepState> _steps = new(StringComparer.Ordinal);
        private readonly ConcurrentQueue<DataPipelineLogEntry> _log = new();
        private int _logCount;

        public RunObserver(IClock clock)
        {
            _clock = clock;
        }

        public void StepStarted(DataPipelineStep step, DataPipelineStepMetrics metrics)
        {
            _steps[step.StepId] = new StepState { Metrics = metrics, Status = DataPipelineStepStatus.Running, StartedUtc = _clock.UtcNow };
        }

        public void StepCompleted(DataPipelineStep step, DataPipelineStepStatus status, Exception error)
        {
            if (_steps.TryGetValue(step.StepId, out var state))
            {
                state.Status = status;
                state.Error = error?.Message;
                state.CompletedUtc = _clock.UtcNow;
            }
        }

        public void Log(DataPipelineLogEntry entry)
        {
            if (Interlocked.Increment(ref _logCount) <= DataPipelineRun.MaxLogEntries)
            {
                _log.Enqueue(entry);
            }
        }

        public void Apply(DataPipelineRun run)
        {
            foreach (var stepRun in run.Steps)
            {
                if (!_steps.TryGetValue(stepRun.StepId, out var state))
                {
                    continue;
                }

                stepRun.Status = state.Status;
                stepRun.Error = state.Error;
                stepRun.StartedUtc = state.StartedUtc;
                stepRun.CompletedUtc = state.CompletedUtc;
                stepRun.RowsIn = state.Metrics.RowsIn;
                stepRun.RowsOut = state.Metrics.RowsOut;
                stepRun.FilesIn = state.Metrics.FilesIn;
                stepRun.FilesOut = state.Metrics.FilesOut;
                stepRun.Warnings = state.Metrics.Warnings;
            }

            while (_log.TryDequeue(out var entry))
            {
                AddLog(run, entry);
            }
        }

        private sealed class StepState
        {
            public DataPipelineStepMetrics Metrics { get; init; }

            public DataPipelineStepStatus Status { get; set; }

            public string Error { get; set; }

            public DateTime? StartedUtc { get; init; }

            public DateTime? CompletedUtc { get; set; }
        }
    }
}
