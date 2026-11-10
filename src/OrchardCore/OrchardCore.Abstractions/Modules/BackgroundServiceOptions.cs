namespace OrchardCore.Modules;

public class BackgroundServiceOptions
{
    /// <summary>
    /// The default value of <see cref="MaxRecentRuns" />.
    /// </summary>
    public const int DefaultMaxRecentRuns = 10;

    /// <summary>
    /// Gets or sets a value indicating whether the shell should be warmed up.
    /// </summary>
    public bool ShellWarmup { get; set; }

    /// <summary>
    /// Gets or sets the polling time for the background tasks, i.e. how much to wait between executing all the background tasks of a given tenant.
    /// The background service also waits for a task at most this time, or the <see cref="MinimumIdleTime" /> if it is longer, before running
    /// the next ones, while a task that takes longer keeps running.
    /// </summary>
    public TimeSpan PollingTime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the minimum idle time before the background tasks of a tenant are triggered, as well as between tasks similar to <see cref="PollingTime" />.
    /// </summary>
    public TimeSpan MinimumIdleTime { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the number of completed runs kept in memory for each background task, to be shown with its execution state.
    /// </summary>
    public int MaxRecentRuns { get; set; } = DefaultMaxRecentRuns;
}
