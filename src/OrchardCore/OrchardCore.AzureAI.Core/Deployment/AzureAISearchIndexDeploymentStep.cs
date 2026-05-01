using Microsoft.Extensions.Localization;
using OrchardCore.AzureAI.Recipes;
using OrchardCore.Deployment;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexDeploymentStep : DeploymentStep
{
    public AzureAISearchIndexDeploymentStep()
    {
        Name = AzureAISearchIndexSettingsStep.Name;
    }

    public AzureAISearchIndexDeploymentStep(IStringLocalizer<AzureAISearchIndexDeploymentStep> S)
        : this()
    {
        Category = S["Search"];
    }

    public bool IncludeAll { get; set; } = true;

    public string[] IndexNames { get; set; }
}
