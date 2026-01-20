namespace OrchardCore.HealthChecks;

/// <summary>
/// Represents options for health checks.
/// </summary>
public class HealthChecksOptions
{
    /// <summary>
    /// Gets or sets the health check URL. Default to "/health/live".
    /// </summary>
    public string Url { get; set; } = "/health/live";

    /// <summary>
    /// Gets or sets a value indicating whether to show the detailed information (name, status and description) for the checks dependency or not. Defaults to <see langword="false"/>.
    /// </summary>
    public bool ShowDetails { get; set; }
}
