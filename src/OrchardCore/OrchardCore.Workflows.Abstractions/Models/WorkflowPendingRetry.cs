namespace OrchardCore.Workflows.Models;

/// <summary>
/// The next attempt of a task that faulted and is retried later by its <see cref="ActivityRetryPolicy"/>: the
/// instance is faulted until then.
/// </summary>
public sealed class WorkflowPendingRetry
{
    /// <summary>
    /// The <see cref="ActivityRecord.ActivityId"/> of the task.
    /// </summary>
    public string ActivityId { get; set; }

    /// <summary>
    /// How many attempts failed so far, the first one included.
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// How many retries the policy allows, when the attempt failed.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// When the next attempt is due.
    /// </summary>
    public DateTime DueUtc { get; set; }
}
