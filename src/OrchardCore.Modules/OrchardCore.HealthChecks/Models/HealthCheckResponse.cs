namespace OrchardCore.HealthChecks.Models;

/// <summary>
/// Represents a health check response to be displayed in the JSON result when the health checks endpoint hit.
/// </summary>
public class HealthCheckResponse
{
    /// <summary>
    /// Gets or sets the overall health checks status.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the overall health checks duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the health check entries.
    /// </summary>
    public IEnumerable<HealthCheckEntry> HealthChecks { get; set; }
}
