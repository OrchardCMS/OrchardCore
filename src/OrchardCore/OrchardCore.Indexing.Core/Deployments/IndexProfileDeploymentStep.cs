using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class IndexProfileDeploymentStep : DeploymentStep
{
    public override string Name => CreateOrUpdateIndexProfileStep.StepKey;

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Indexing;

    [JsonIgnore]
    public override LocalizationSource Title => IndexingLocalizationSources.IndexProfiles;

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
