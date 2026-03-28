using OrchardCore.Deployment;

namespace OrchardCore.Search.OpenSearch.Core.Deployment;

/// <summary>
/// Adds layers to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class OpenSearchIndexDeploymentStep : DeploymentStep
{
    public OpenSearchIndexDeploymentStep()
    {
        Name = "OpenSearchIndexSettings";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
