using OrchardCore.Deployment;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStep : DeploymentStep
{
    public AllSitemapsDeploymentStep()
    {
        Name = "AllSitemaps";
    }
}
