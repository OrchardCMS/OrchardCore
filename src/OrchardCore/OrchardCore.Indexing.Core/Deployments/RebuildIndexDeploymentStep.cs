using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => RebuildIndexStep.Key;

    public static LocalizationSource Category => IndexingLocalizationSources.Indexing;

    public static LocalizationSource Title => IndexingLocalizationSources.RebuildIndexes;

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
