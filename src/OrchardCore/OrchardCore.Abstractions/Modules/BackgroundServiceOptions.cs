namespace OrchardCore.Modules;

public class BackgroundServiceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the shell should be warmed up.
    /// </summary>
    public bool ShellWarmup { get; set; }

    /// <summary>
    /// Gets or sets the polling time for the background tasks, i.e. how much to wait between executing all the background tasks of a given tenant.
    /// </summary>
    public TimeSpan PollingTime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the minimum idle time before the background tasks of a tenant are triggered, as well as between tasks similar to <see cref="PollingTime" />.
    /// </summary>
    public TimeSpan MinimumIdleTime { get; set; } = TimeSpan.FromSeconds(10);
}
