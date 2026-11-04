namespace OrchardCore.Deployment.Artifacts;

/// <summary>Host-owned limits and retention for private deployment artifacts.</summary>
public sealed class DeploymentArtifactOptions
{
    /// <summary>Gets or sets the maximum stored artifact size, defaulting to 500 MiB.</summary>
    public long MaxBytes { get; set; } = 500 * 1024 * 1024;
    /// <summary>Gets or sets artifact lifetime, defaulting to 24 hours.</summary>
    public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(24);
}
