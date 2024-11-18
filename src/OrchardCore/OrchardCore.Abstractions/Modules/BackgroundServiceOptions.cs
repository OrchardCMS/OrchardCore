namespace OrchardCore.Modules;

public class BackgroundServiceOptions
{
    public bool ShellWarmup { get; set; }

    /// <summary>
    /// Gets or sets the time in seconds between each background task execution.
    /// </summary>
    public int PollingTime { get; set; } = 60;

    /// <summary>
    /// Gets or sets the minimum time in seconds that the background task should be idle before being considered for execution.
    /// </summary>
    public int MinimumIdelTime { get; set; } = 10;
}
