namespace OrchardCore.Deployment.Operations;

/// <summary>Host-owned retention of queued deployment operations.</summary>
public sealed class DeploymentOperationOptions
{
    /// <summary>
    /// Gets or sets how long an operation stays available after it ended, defaulting to seven days.
    /// Its request ID can be used again afterwards. A value of zero or less keeps operations forever.
    /// </summary>
    public TimeSpan Retention { get; set; } = TimeSpan.FromDays(7);
}
