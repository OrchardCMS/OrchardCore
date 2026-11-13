using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Jobs.Indexes;
using OrchardCore.Jobs.Models;
using OrchardCore.Modules;
using YesSql;
using IIdGenerator = OrchardCore.Entities.IIdGenerator;
using ISession = YesSql.ISession;

namespace OrchardCore.Jobs.Services;

/// <summary>
/// Queues runs of jobs, finds them, and cancels them.
/// </summary>
public sealed class JobManager : IJobManager
{
    private readonly ISession _session;
    private readonly IIdGenerator _idGenerator;
    private readonly IClock _clock;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JobRunCoordinator _coordinator;
    private readonly JobOptions _jobOptions;
    private readonly string _nodeId;

    public JobManager(
        ISession session,
        IIdGenerator idGenerator,
        IClock clock,
        IHttpContextAccessor httpContextAccessor,
        JobRunCoordinator coordinator,
        IOptions<JobOptions> jobOptions,
        IOptions<NodeOptions> nodeOptions)
    {
        _session = session;
        _idGenerator = idGenerator;
        _clock = clock;
        _httpContextAccessor = httpContextAccessor;
        _coordinator = coordinator;
        _jobOptions = jobOptions.Value;
        _nodeId = nodeOptions.Value.Id;
    }

    /// <inheritdoc />
    public Task<JobRun> EnqueueAsync<TJob>(object parameters = null, Action<JobRequest> configure = null)
        where TJob : class, IJob
    {
        var descriptor = _jobOptions.Find<TJob>()
            ?? throw new InvalidOperationException($"The type '{typeof(TJob)}' is not registered as a job. Register it with 'services.AddJob<{typeof(TJob).Name}>(name)'.");

        return EnqueueAsync(descriptor.Name, parameters, configure);
    }

    /// <inheritdoc />
    public async Task<JobRun> EnqueueAsync(string jobName, object parameters = null, Action<JobRequest> configure = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(jobName);

        var descriptor = _jobOptions.Find(jobName)
            ?? throw new InvalidOperationException($"No job is registered with the name '{jobName}'.");

        var request = new JobRequest();
        configure?.Invoke(request);

        if (request.CancelInProgress && string.IsNullOrEmpty(request.ConcurrencyKey))
        {
            throw new InvalidOperationException($"A run that cancels the runs in progress needs a '{nameof(JobRequest.ConcurrencyKey)}', which tells the runs it replaces.");
        }

        var now = _clock.UtcNow;
        var dueUtc = request.DueUtc ?? now;
        var runParameters = ToJsonObject(parameters);

        if (!string.IsNullOrEmpty(request.DeduplicationKey))
        {
            var existing = await FindActiveAsync(descriptor.Name, request.DeduplicationKey);

            if (existing is not null)
            {
                if (!request.ReplaceQueued || existing.Status != JobRunStatus.Queued)
                {
                    return existing;
                }

                // Changed in a scope of its own, with a concurrency check, so a run that a server takes at the same
                // time isn't queued again by this change.
                var (replaced, _) = await JobRunStore.UpdateAsync(existing.RunId, run =>
                {
                    if (run.Status != JobRunStatus.Queued)
                    {
                        return false;
                    }

                    run.Parameters = runParameters;
                    run.Title = request.Title;
                    run.DueUtc = dueUtc;

                    return true;
                });

                Dispatch(existing.RunId, dueUtc);

                return replaced ?? existing;
            }
        }

        var run = new JobRun
        {
            RunId = _idGenerator.GenerateUniqueId(),
            JobName = descriptor.Name,
            Title = request.Title,
            Status = JobRunStatus.Queued,
            Trigger = request.Trigger ?? JobTriggers.Code,
            TriggeredBy = request.TriggeredBy ?? GetCurrentUserName(),
            CorrelationId = request.CorrelationId,
            DeduplicationKey = request.DeduplicationKey,
            ConcurrencyKey = request.ConcurrencyKey,
            CancelInProgress = request.CancelInProgress,
            ParentRunId = request.ParentRunId,
            ScheduleId = request.ScheduleId,
            RequiredNodeId = request.RequireCurrentNode ? _nodeId : null,
            Parameters = runParameters,
            CreatedUtc = now,
            DueUtc = dueUtc,
            MaxAttempts = Math.Max(1, request.MaxAttempts ?? descriptor.MaxAttempts),
        };

        request.Alter?.Invoke(run);

        await _session.SaveAsync(run, collection: JobRun.Collection);

        Dispatch(run.RunId, run.DueUtc);

        return run;
    }

    /// <inheritdoc />
    public async Task<JobRun> GetAsync(string runId)
    {
        if (string.IsNullOrEmpty(runId))
        {
            return null;
        }

        return await JobRunStore.QueryAsync(_session, runId);
    }

    /// <inheritdoc />
    public async Task<JobRun> FindActiveAsync(string jobName, string deduplicationKey)
    {
        if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(deduplicationKey))
        {
            return null;
        }

        var name = JobRunIndexProvider.Truncate(jobName, JobRunIndex.MaxJobNameLength);
        var key = JobRunIndexProvider.Truncate(deduplicationKey, JobRunIndex.MaxKeyLength);
        var queued = nameof(JobRunStatus.Queued);
        var running = nameof(JobRunStatus.Running);
        var waiting = nameof(JobRunStatus.Waiting);

        return await _session
            .Query<JobRun, JobRunIndex>(index => index.DeduplicationKey == key && index.JobName == name && (index.Status == queued || index.Status == running || index.Status == waiting), collection: JobRun.Collection)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<JobRun> FindLastAsync(string jobName, string deduplicationKey)
    {
        if (string.IsNullOrEmpty(jobName) || string.IsNullOrEmpty(deduplicationKey))
        {
            return null;
        }

        var name = JobRunIndexProvider.Truncate(jobName, JobRunIndex.MaxJobNameLength);
        var key = JobRunIndexProvider.Truncate(deduplicationKey, JobRunIndex.MaxKeyLength);

        return await _session
            .Query<JobRun, JobRunIndex>(index => index.DeduplicationKey == key && index.JobName == name, collection: JobRun.Collection)
            .OrderByDescending(index => index.CreatedUtc)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<JobRun>> ListChildrenAsync(string parentRunId)
    {
        if (string.IsNullOrEmpty(parentRunId))
        {
            return Task.FromResult<IReadOnlyList<JobRun>>([]);
        }

        return JobRunStore.QueryChildrenAsync(_session, parentRunId);
    }

    /// <inheritdoc />
    public Task<bool> CancelAsync(string runId, string canceledBy = null)
    {
        return _coordinator.CancelAsync(runId, canceledBy);
    }

    /// <inheritdoc />
    public async Task<JobRun> RerunAsync(string runId, string triggeredBy = null)
    {
        var run = await GetAsync(runId);

        if (run is null || !run.IsCompleted || _jobOptions.Find(run.JobName) is null)
        {
            return null;
        }

        return await EnqueueAsync(run.JobName, run.Parameters, request =>
        {
            request.Title = run.Title;
            request.Trigger = JobTriggers.Manual;
            request.TriggeredBy = triggeredBy;
            request.DeduplicationKey = run.DeduplicationKey;
            request.ConcurrencyKey = run.ConcurrencyKey;
            request.CancelInProgress = run.CancelInProgress;
            request.MaxAttempts = run.MaxAttempts;
            request.RequireCurrentNode = run.RequiredNodeId is not null;
        });
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(string runId)
    {
        var run = await GetAsync(runId);

        if (run is null || !run.IsCompleted)
        {
            return false;
        }

        // The runs it queued are deleted with it, as are the ones they queued.
        foreach (var descendant in await JobRunStore.QueryDescendantsAsync(_session, runId))
        {
            _session.Delete(descendant, collection: JobRun.Collection);
        }

        _session.Delete(run, collection: JobRun.Collection);

        return true;
    }

    // Starts the run on this server once the current scope committed it. Another server starts it otherwise.
    private static void Dispatch(string runId, DateTime dueUtc)
    {
        ShellScope.AddDeferredTask(scope =>
        {
            scope.ServiceProvider.GetRequiredService<JobRunDispatcher>().Dispatch(runId, dueUtc);

            return Task.CompletedTask;
        });
    }

    private string GetCurrentUserName()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // A run queued from the background, such as by another run, has no user of its own.
        if (httpContext is null || httpContext.Items.ContainsKey("IsBackground"))
        {
            return null;
        }

        return httpContext.User?.Identity?.Name;
    }

    private static JsonObject ToJsonObject(object parameters)
    {
        return parameters switch
        {
            null => null,
            JsonObject jsonObject => jsonObject.DeepClone().AsObject(),
            _ => JObject.FromObject(parameters, JOptions.Default),
        };
    }
}
