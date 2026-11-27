using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => CreateOrUpdateIndexProfileStep.StepKey;

    public static LocalizationSource Category => IndexingLocalizationSources.Indexing;

    public static LocalizationSource Title => IndexingLocalizationSources.IndexProfiles;

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
