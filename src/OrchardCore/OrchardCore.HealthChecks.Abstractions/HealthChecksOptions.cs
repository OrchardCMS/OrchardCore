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
}
