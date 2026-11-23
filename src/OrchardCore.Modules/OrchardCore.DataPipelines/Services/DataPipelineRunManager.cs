using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DataPipelines.Indexes;
using OrchardCore.DataPipelines.Jobs;
using OrchardCore.DataPipelines.Models;
using OrchardCore.Entities;
using OrchardCore.Jobs;
using OrchardCore.Jobs.Models;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using YesSql;
using YesSql.Services;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;

namespace OrchardCore.DataPipelines.Services;

/// <summary>
/// Queues runs of published pipelines, finds them, and cancels them. Runs execute in the background, each as a job
/// run that a single server takes: a queued run starts once the request that queued it is done, or within a minute.
/// </summary>
public sealed class DataPipelineRunManager
{
    private readonly ISession _session;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IJobManager _jobManager;
    private readonly DataPipelineManager _pipelineManager;

    public DataPipelineRunManager(
        ISession session,
        IIdGenerator idGenerator,
        IClock clock,
        IJobManager jobManager,
        DataPipelineManager pipelineManager)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
        _jobManager = jobManager;
        _pipelineManager = pipelineManager;
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

        // The job run is saved with the run, so it only starts once this scope committed both.
        await EnqueueJobAsync(_jobManager, run);
        await _session.SaveAsync(run);

        return run;
    }

    /// <summary>
    /// Tells whether a run of a pipeline is queued or running.
    /// </summary>
    /// <param name="pipelineId">The identifier of the pipeline.</param>
    /// <returns><see langword="true"/> when a run is queued or running.</returns>
    public async Task<bool> HasActiveRunAsync(string pipelineId)
    {
        var queued = nameof(DataPipelineRunStatus.Queued);
        var running = nameof(DataPipelineRunStatus.Running);

        return await _session.QueryIndex<DataPipelineRunIndex>(index => index.PipelineId == pipelineId && (index.Status == queued || index.Status == running))
            .FirstOrDefaultAsync() is not null;
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
    public Task<(IReadOnlyList<DataPipelineRun> Runs, int Count)> ListAsync(string pipelineId, int skip, int take)
    {
        return ListAsync(new DataPipelineRunFilter { PipelineId = pipelineId }, skip, take);
    }

    /// <summary>
    /// Lists the most recent runs that match a filter.
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <param name="skip">The number of runs to skip.</param>
    /// <param name="take">The number of runs to return.</param>
    /// <returns>The runs, the most recent first, and the number of runs that match.</returns>
    public async Task<(IReadOnlyList<DataPipelineRun> Runs, int Count)> ListAsync(DataPipelineRunFilter filter, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(filter);

        var query = _session.Query<DataPipelineRun, DataPipelineRunIndex>();

        if (!string.IsNullOrEmpty(filter.PipelineId))
        {
            query = query.Where(index => index.PipelineId == filter.PipelineId);
        }

        if (filter.Status is { } status)
        {
            var statusName = status.ToString();
            query = query.Where(index => index.Status == statusName);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pipelineIds = await _pipelineManager.FindIdsAsync(filter.Search);

            if (pipelineIds.Count == 0)
            {
                return ([], 0);
            }

            query = query.Where(index => index.PipelineId.IsIn(pipelineIds));
        }

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

        var canceledBy = user?.Identity?.Name;

        if (run.Status == DataPipelineRunStatus.Queued)
        {
            // Its job run didn't start: cancelling it ends this run, and tells the handlers.
            if (!string.IsNullOrEmpty(run.JobRunId) && await _jobManager.CancelAsync(run.JobRunId, canceledBy))
            {
                return true;
            }

            // No job executes the run, such as a run queued before an upgrade.
            run.Status = DataPipelineRunStatus.Cancelled;
            run.CompletedUtc = _clock.UtcNow;
            run.Error = "The run was cancelled before it started.";
            await _session.SaveAsync(run);
            DataPipelineRunNotifications.NotifyAfterCommit(run);

            return true;
        }

        run.Log.Add(new Steps.DataPipelineLogEntry
        {
            Level = Microsoft.Extensions.Logging.LogLevel.Warning,
            Message = $"Cancellation requested by {canceledBy ?? "the system"}.",
        });

        run.CancellationRequested = true;
        await _session.SaveAsync(run);

        // The server that executes the run stops it, at once when it is this one, within a few seconds otherwise.
        // Its job run is only cancelled once this scope committed the run: the run that stops records how it ended,
        // which this scope would otherwise overwrite with the run as it was read here.
        if (!string.IsNullOrEmpty(run.JobRunId))
        {
            var jobRunId = run.JobRunId;

            ShellScope.AddDeferredTask(scope => scope.ServiceProvider.GetRequiredService<IJobManager>().CancelAsync(jobRunId, canceledBy));
        }

        return true;
    }

    /// <summary>
    /// Queues the job run that executes a run, and links the run to it. One run of a pipeline executes at a time.
    /// </summary>
    internal static async Task EnqueueJobAsync(IJobManager jobManager, DataPipelineRun run)
    {
        var jobRun = await jobManager.EnqueueAsync<DataPipelineRunJob>(new DataPipelineRunJobParameters { RunId = run.RunId }, request =>
        {
            request.Title = run.PipelineName;
            request.Trigger = run.Trigger ?? JobTriggers.Code;
            request.TriggeredBy = run.TriggeredBy;
            request.CorrelationId = run.RunId;
            request.ConcurrencyKey = DataPipelineRunJob.GetConcurrencyKey(run.PipelineId);
        });

        run.JobRunId = jobRun.RunId;
    }
}
