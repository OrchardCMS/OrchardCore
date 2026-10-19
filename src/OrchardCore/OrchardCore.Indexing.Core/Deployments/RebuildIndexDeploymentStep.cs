using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class RebuildIndexDeploymentStep : DeploymentStep
{
    public override string Name => RebuildIndexStep.Key;

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Indexing;

    [JsonIgnore]
    public override LocalizationSource Title => IndexingLocalizationSources.RebuildIndexes;

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
