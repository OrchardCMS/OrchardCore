using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Models;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Queues runs of published pipelines, finds them, and cancels them. Runs execute in the background: a queued run
/// starts once the request that queued it is done, or within a minute.
/// </summary>
public sealed class DataPipelineRunManager
{
    private readonly ISession _session;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly DataPipelineRunTracker _tracker;

    public DataPipelineRunManager(
        ISession session,
        IIdGenerator idGenerator,
        IClock clock,
        DataPipelineRunTracker tracker)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
        _tracker = tracker;
    }

    /// <summary>
    /// Queues a run of the published version of a pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline.</param>
    /// <param name="request">Who starts the run, and with what values.</param>
    /// <returns>The queued run.</returns>
    /// <exception cref="InvalidOperationException">The pipeline was never published, or is disabled.</exception>
    public async Task<DataPipelineRun> QueueAsync(DataPipeline pipeline, DataPipelineRunRequest request)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentNullException.ThrowIfNull(request);

        if (pipeline.Published is null)
        {
            throw new InvalidOperationException("The pipeline can't run because it was never published.");
        }

        if (!pipeline.IsEnabled)
        {
            throw new InvalidOperationException("The pipeline can't run because it is disabled.");
        }

        var run = new DataPipelineRun
        {
            RunId = _idGenerator.GenerateUniqueId(),
            PipelineId = pipeline.PipelineId,
            PipelineName = pipeline.Name,
            VersionId = pipeline.PublishedVersionId,
            VersionNumber = pipeline.PublishedVersionNumber,
            Definition = DataPipelineManager.Clone(pipeline.Published),
            Status = DataPipelineRunStatus.Queued,
            Trigger = request.Trigger,
            TriggeredBy = request.TriggeredBy?.Identity?.Name,
            RunAsUserId = pipeline.PublishedByUserId,
            CorrelationId = request.CorrelationId,
            Parameters = new Dictionary<string, string>(request.Parameters ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase),
            QueuedUtc = _clock.UtcNow,
        };

        request.Alter?.Invoke(run);

        await _session.SaveAsync(run);

        // Start the run once this scope committed it.
        var runId = run.RunId;
        ShellScope.AddDeferredTask(scope =>
        {
            scope.ServiceProvider.GetRequiredService<DataPipelineRunDispatcher>().Dispatch(runId);

            return Task.CompletedTask;
        });

        return run;
    }

    /// <summary>
    /// Finds a run.
    /// </summary>
    /// <param name="runId">The run.</param>
    /// <returns>The run, or <see langword="null"/>.</returns>
    public async Task<DataPipelineRun> GetAsync(string runId)
    {
        if (string.IsNullOrEmpty(runId))
        {
            return null;
        }

        return await _session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.RunId == runId).FirstOrDefaultAsync();
    }

    /// <summary>
    /// Lists the most recent runs of a pipeline, or of every pipeline.
    /// </summary>
    /// <param name="pipelineId">The pipeline, or <see langword="null"/> for every pipeline.</param>
    /// <param name="skip">The number of runs to skip.</param>
    /// <param name="take">The number of runs to return.</param>
    /// <returns>The runs, the most recent first, and the number of runs.</returns>
    public async Task<(IReadOnlyList<DataPipelineRun> Runs, int Count)> ListAsync(string pipelineId, int skip, int take)
    {
        var query = string.IsNullOrEmpty(pipelineId)
            ? _session.Query<DataPipelineRun, DataPipelineRunIndex>()
            : _session.Query<DataPipelineRun, DataPipelineRunIndex>(index => index.PipelineId == pipelineId);

        var count = await query.CountAsync();
        var runs = await query.OrderByDescending(index => index.QueuedUtc).Skip(skip).Take(take).ListAsync();

        return (runs.ToList(), count);
    }

    /// <summary>
    /// Cancels a run: a queued run is cancelled at once, and a running run stops at its next batch.
    /// </summary>
    /// <param name="run">The run.</param>
    /// <param name="user">The user who cancels.</param>
    /// <returns><see langword="true"/> when the run was not over yet.</returns>
    public async Task<bool> CancelAsync(DataPipelineRun run, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (run.IsCompleted)
        {
            return false;
        }

        run.Log.Add(new Steps.DataPipelineLogEntry
        {
            Level = Microsoft.Extensions.Logging.LogLevel.Warning,
            Message = $"Cancellation requested by {user?.Identity?.Name ?? "the system"}.",
        });

        if (run.Status == DataPipelineRunStatus.Queued)
        {
            run.Status = DataPipelineRunStatus.Cancelled;
            run.CompletedUtc = _clock.UtcNow;
            run.Error = "The run was cancelled before it started.";
        }
        else
        {
            run.CancellationRequested = true;
            _tracker.TryCancel(run.RunId);
        }

        await _session.SaveAsync(run);

        return true;
    }
}
