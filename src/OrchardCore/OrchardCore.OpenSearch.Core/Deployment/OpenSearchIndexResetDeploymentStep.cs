using OrchardCore.Deployment;

namespace OrchardCore.OpenSearch.Core.Deployment;

/// <summary>
/// Adds reset OpenSearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class OpenSearchIndexResetDeploymentStep : DeploymentStep
{
    public OpenSearchIndexResetDeploymentStep()
    {
        Name = "OpenSearchIndexReset";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
