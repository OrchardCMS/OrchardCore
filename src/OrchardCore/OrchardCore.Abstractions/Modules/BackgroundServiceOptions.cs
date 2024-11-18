namespace OrchardCore.Modules;

public class BackgroundServiceOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether the shell should be warmed up.
    /// </summary>
    public bool ShellWarmup { get; set; }

    /// <summary>
    /// Gets or sets the polling time for the background task.
    /// </summary>
    public TimeSpan PollingTime { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or set the minimum idle time before the background task is triggered.
    /// </summary>
    public TimeSpan MinimumIdelTime { get; set; } = TimeSpan.FromSeconds(10);
}
