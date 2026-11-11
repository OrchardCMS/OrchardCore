namespace OrchardCore.Jobs;

/// <summary>
/// Configures how the runs of jobs execute. Read from the <c>OrchardCore:Jobs</c> configuration section, for all
/// tenants or per tenant.
/// </summary>
public sealed class JobExecutionOptions
{
    /// <summary>
    /// Gets or sets the number of runs of the tenant a server executes at the same time. The other runs wait,
    /// queued, for one to end, or for another server.
    /// </summary>
    public int MaxConcurrentRuns { get; set; } = 4;

    /// <summary>
    /// Gets or sets the number of runs a server executes at the same time for all its tenants, or 0 for no limit.
    /// The value of the first tenant that executes a run applies, so set it in the application configuration.
    /// </summary>
    public int MaxConcurrentRunsPerServer { get; set; }

    /// <summary>
    /// Gets or sets how often, in seconds, a server reports that a run is still executing, and saves its progress
    /// and its log. A cancellation requested from another server is seen at that time.
    /// </summary>
    public int HeartbeatSeconds { get; set; } = 10;

    /// <summary>
    /// Gets or sets the number of minutes after which a running run that stopped reporting is considered abandoned
    /// by its server: it is queued again when it has attempts left, and fails otherwise.
    /// </summary>
    public int AbandonedAfterMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets the number of days the runs that ended are kept; older runs are deleted. 0 keeps them.
    /// </summary>
    public int RunRetentionDays { get; set; } = 30;

    internal TimeSpan HeartbeatInterval => TimeSpan.FromSeconds(Math.Max(1, HeartbeatSeconds));

    internal TimeSpan AbandonedAfter => TimeSpan.FromMinutes(Math.Max(1, AbandonedAfterMinutes));
}
