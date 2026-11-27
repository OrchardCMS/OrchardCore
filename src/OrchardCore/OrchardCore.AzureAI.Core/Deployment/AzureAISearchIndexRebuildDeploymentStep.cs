using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.AzureAI.Deployment;

public class AzureAISearchIndexRebuildDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AzureAISearchIndexRebuild";

    public static LocalizationSource Category => CommonLocalizationSources.Search;

    public static LocalizationSource Title => AzureAILocalizationSources.RebuildAzureAISearchIndices;

    public bool IncludeAll { get; set; }

    public string[] Indices { get; set; }
}
