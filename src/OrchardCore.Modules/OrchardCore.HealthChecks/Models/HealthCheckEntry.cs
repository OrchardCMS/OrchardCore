namespace OrchardCore.HealthChecks.Models;

/// <summary>
/// Represents a health check entry.
/// </summary>
public class HealthCheckEntry
{
    /// <summary>
    /// Gets or sets the health check name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the health check status.
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    /// Gets or sets the health check description.
    /// </summary>
    public string Description { get; set; }
}
