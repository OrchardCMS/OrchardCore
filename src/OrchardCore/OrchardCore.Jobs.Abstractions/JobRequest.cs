using OrchardCore.Jobs.Models;

namespace OrchardCore.Jobs;

/// <summary>
/// How a run is queued: when it is due, what deduplicates it, and what it is linked to.
/// </summary>
public sealed class JobRequest
{
    /// <summary>
    /// Gets or sets what the run is about, shown next to the title of the job.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets when the run may start, in UTC. When <see langword="null"/>, the run starts as soon as a server
    /// is available.
    /// </summary>
    public DateTime? DueUtc { get; set; }

    /// <summary>
    /// Gets or sets the key that prevents the same work from being queued twice. While a run of the job with this
    /// key is queued, running or waiting, no other run is queued: that run is returned instead.
    /// </summary>
    public string DeduplicationKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a run with the same <see cref="DeduplicationKey"/> that didn't start
    /// yet gets the parameters, the title and the due date of this request. Use it to reschedule a run.
    /// </summary>
    public bool ReplaceQueued { get; set; }

    /// <summary>
    /// Gets or sets the key of the runs that execute one at a time, on any server. When <see langword="null"/>,
    /// the runs of the job execute at the same time.
    /// </summary>
    public string ConcurrencyKey { get; set; }

    /// <summary>
    /// Gets or sets an identifier that links the run to what queues it, such as a workflow instance.
    /// </summary>
    public string CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets what starts the run. It defaults to <see cref="JobTriggers.Code"/>.
    /// </summary>
    public string Trigger { get; set; } = JobTriggers.Code;

    /// <summary>
    /// Gets or sets the name of the user who starts the run. It defaults to the user of the current request.
    /// </summary>
    public string TriggeredBy { get; set; }

    /// <summary>
    /// Gets or sets the number of attempts the run may make. When <see langword="null"/>, the job decides.
    /// </summary>
    public int? MaxAttempts { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether only the server that queues the run may execute it. Use it for a
    /// run that needs something only this server has, such as a file it just received. Such a run fails when this
    /// server stops before executing it.
    /// </summary>
    public bool RequireCurrentNode { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the run that queues this run as one of its child runs.
    /// <see cref="JobExecutionContext.EnqueueChildAsync"/> sets it.
    /// </summary>
    public string ParentRunId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the schedule that creates the run.
    /// </summary>
    public string ScheduleId { get; set; }

    /// <summary>
    /// Gets or sets a delegate that changes the run before it is saved, such as to set its properties.
    /// </summary>
    public Action<JobRun> Alter { get; set; }
}
