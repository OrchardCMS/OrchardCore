using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexResetDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AzureAISearchIndexReset";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => AzureAILocalizationSources.ResetAzureAISearchIndices;

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
