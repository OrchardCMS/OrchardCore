using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStep : DeploymentStep
{
    public AllSitemapsDeploymentStep()
    {
        Name = "AllSitemaps";
    }

    public AllSitemapsDeploymentStep(IStringLocalizer<AllSitemapsDeploymentStep> S)
        : this()
    {
        Category = S["Content Management"];
    }
}
