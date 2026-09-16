namespace OrchardCore.Deployment.Core.Services;

/// <summary>Host-owned bounds for deployment package staging.</summary>
public sealed class DeploymentPackageOptions
{
    /// <summary>Gets or sets the maximum uploaded package bytes.</summary>
    public long MaxUploadBytes { get; set; } = 100 * 1024 * 1024;
    /// <summary>Gets or sets the maximum total expanded bytes.</summary>
    public long MaxExpandedBytes { get; set; } = 500 * 1024 * 1024;
    /// <summary>Gets or sets the maximum number of archive entries.</summary>
    public int MaxEntries { get; set; } = 10000;
}
