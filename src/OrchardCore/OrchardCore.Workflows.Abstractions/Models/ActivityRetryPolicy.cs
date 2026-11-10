namespace OrchardCore.Workflows.Models;

/// <summary>
/// How a task is retried when it faults, and what its failure does once the attempts are spent. It's kept in the
/// activity's properties, like its <see cref="ActivityMetadata"/>.
/// </summary>
public sealed class ActivityRetryPolicy
{
    /// <summary>
    /// The most retries a policy can have.
    /// </summary>
    public const int MaxRetriesLimit = 10;

    /// <summary>
    /// The name of the outcome a task follows when its attempts are spent and <see cref="OnFailure"/> is
    /// <see cref="ActivityFailureBehavior.FollowFailedOutcome"/>.
    /// </summary>
    public const string FailedOutcome = "Failed";

    /// <summary>
    /// The longest delay between two attempts, which a doubling delay doesn't exceed.
    /// </summary>
    public static readonly TimeSpan MaxDelay = TimeSpan.FromDays(1);

    /// <summary>
    /// How many times the task is retried when it faults, from 0 (it isn't) to <see cref="MaxRetriesLimit"/>.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// The delay before the first retry, in seconds. With none, the task is retried at once, in the same run.
    /// </summary>
    public int DelaySeconds { get; set; }

    /// <summary>
    /// How the delay grows from one retry to the next.
    /// </summary>
    public ActivityRetryBackoff Backoff { get; set; }

    /// <summary>
    /// What the failure does once the attempts are spent.
    /// </summary>
    public ActivityFailureBehavior OnFailure { get; set; }

    /// <summary>
    /// Whether the task follows its <see cref="FailedOutcome"/> once the attempts are spent.
    /// </summary>
    public bool FollowsFailedOutcome => OnFailure == ActivityFailureBehavior.FollowFailedOutcome;

    /// <summary>
    /// Whether the policy changes anything: it retries the task or follows the failed outcome.
    /// </summary>
    public bool IsActive => MaxRetries > 0 || FollowsFailedOutcome;

    /// <summary>
    /// The delay before a retry.
    /// </summary>
    /// <param name="retry">The retry, from 1.</param>
    public TimeSpan GetDelay(int retry)
    {
        if (DelaySeconds <= 0)
        {
            return TimeSpan.Zero;
        }

        var seconds = (double)DelaySeconds;

        if (Backoff == ActivityRetryBackoff.Exponential)
        {
            seconds *= Math.Pow(2, Math.Max(0, retry - 1));
        }

        return seconds >= MaxDelay.TotalSeconds ? MaxDelay : TimeSpan.FromSeconds(seconds);
    }
}

/// <summary>
/// How the delay of an <see cref="ActivityRetryPolicy"/> grows from one retry to the next.
/// </summary>
public enum ActivityRetryBackoff
{
    /// <summary>
    /// Each retry waits the same delay.
    /// </summary>
    Fixed,

    /// <summary>
    /// Each retry waits twice as long as the one before, up to <see cref="ActivityRetryPolicy.MaxDelay"/>.
    /// </summary>
    Exponential,
}

/// <summary>
/// What the failure of a task does once the attempts of its <see cref="ActivityRetryPolicy"/> are spent.
/// </summary>
public enum ActivityFailureBehavior
{
    /// <summary>
    /// The instance is faulted at the task, and can be retried from there.
    /// </summary>
    FaultWorkflow,

    /// <summary>
    /// The task produces its <see cref="ActivityRetryPolicy.FailedOutcome"/>, and the instance goes on with it.
    /// </summary>
    FollowFailedOutcome,
}
