using OrchardCore.Deployment;

namespace OrchardCore.OpenSearch.Core.Deployment;

/// <summary>
/// Adds rebuild OpenSearch index task to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public sealed class OpenSearchIndexRebuildDeploymentStep : DeploymentStep
{
    public OpenSearchIndexRebuildDeploymentStep()
    {
        Name = "OpenSearchIndexRebuild";
    }

    public bool IncludeAll { get; set; } = true;

    public string[] Indices { get; set; }
}
