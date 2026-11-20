using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs;

/// <summary>
/// Queues runs of jobs, finds them, and cancels them. A run is executed in the background by a single server,
/// whatever the number of servers: by the server that queued it once its request is done, or by any server within a
/// minute.
/// </summary>
public interface IJobManager
{
    /// <summary>
    /// Queues a run of a job. The run is saved with the current database session, so it is only queued if the
    /// current request or scope completes.
    /// </summary>
    /// <param name="jobName">The name of the job, as it is registered.</param>
    /// <param name="parameters">The parameters of the run, serialized to JSON, or <see langword="null"/>.</param>
    /// <param name="configure">Changes how the run is queued, if needed: its due date, its deduplication key.</param>
    /// <returns>The queued run, or the run that already does this work when the request is deduplicated.</returns>
    /// <exception cref="InvalidOperationException">No job is registered with this name.</exception>
    Task<JobRun> EnqueueAsync(string jobName, object parameters = null, Action<JobRequest> configure = null);

    /// <summary>
    /// Queues a run of a job, found by its type.
    /// </summary>
    /// <typeparam name="TJob">The type that executes the runs, as it is registered.</typeparam>
    /// <param name="parameters">The parameters of the run, serialized to JSON, or <see langword="null"/>.</param>
    /// <param name="configure">Changes how the run is queued, if needed: its due date, its deduplication key.</param>
    /// <returns>The queued run, or the run that already does this work when the request is deduplicated.</returns>
    /// <exception cref="InvalidOperationException">The type is not registered as a job.</exception>
    Task<JobRun> EnqueueAsync<TJob>(object parameters = null, Action<JobRequest> configure = null)
        where TJob : class, IJob;

    /// <summary>
    /// Finds a run.
    /// </summary>
    /// <param name="runId">The identifier of the run.</param>
    /// <returns>The run, or <see langword="null"/>.</returns>
    Task<JobRun> GetAsync(string runId);

    /// <summary>
    /// Finds the run of a job with a deduplication key that is queued, running or waiting.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="deduplicationKey">The deduplication key.</param>
    /// <returns>The run, or <see langword="null"/> when no such run is active.</returns>
    Task<JobRun> FindActiveAsync(string jobName, string deduplicationKey);

    /// <summary>
    /// Finds the most recent run of a job with a deduplication key, whether it ended or not. Use it to tell how the
    /// last attempt at some work went, such as to wait before queuing a run again after a failure.
    /// </summary>
    /// <param name="jobName">The name of the job.</param>
    /// <param name="deduplicationKey">The deduplication key.</param>
    /// <returns>The run, or <see langword="null"/> when no run was queued with this key, or it was deleted.</returns>
    Task<JobRun> FindLastAsync(string jobName, string deduplicationKey);

    /// <summary>
    /// Lists the child runs of a run, the oldest first.
    /// </summary>
    /// <param name="parentRunId">The identifier of the parent run.</param>
    /// <returns>The child runs.</returns>
    Task<IReadOnlyList<JobRun>> ListChildrenAsync(string parentRunId);

    /// <summary>
    /// Cancels a run and its child runs. A run that didn't start is cancelled at once; a running run stops at the
    /// next heartbeat of its server, whichever server that is.
    /// </summary>
    /// <param name="runId">The identifier of the run.</param>
    /// <param name="canceledBy">The name of the user who cancels, if any.</param>
    /// <returns><see langword="true"/> when the run exists and was not over yet.</returns>
    Task<bool> CancelAsync(string runId, string canceledBy = null);

    /// <summary>
    /// Queues a new run with the job, the title and the parameters of a run that ended.
    /// </summary>
    /// <param name="runId">The identifier of the run to run again.</param>
    /// <param name="triggeredBy">The name of the user who asks for it, if any.</param>
    /// <returns>The new run, or <see langword="null"/> when the run doesn't exist, didn't end, or its job is not
    /// registered anymore.</returns>
    Task<JobRun> RerunAsync(string runId, string triggeredBy = null);

    /// <summary>
    /// Deletes a run that ended, and its child runs.
    /// </summary>
    /// <param name="runId">The identifier of the run.</param>
    /// <returns><see langword="true"/> when the run exists and was deleted.</returns>
    Task<bool> DeleteAsync(string runId);
}
