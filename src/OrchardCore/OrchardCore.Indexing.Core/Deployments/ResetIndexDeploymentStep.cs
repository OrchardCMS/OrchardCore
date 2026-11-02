using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Indexing.Core.Recipes;
using OrchardCore.Localization;

namespace OrchardCore.Indexing.Core.Deployments;

public sealed class ResetIndexDeploymentStep : DeploymentStep
{
    public override string Name => ResetIndexStep.Key;

    [JsonIgnore]
    public override LocalizationSource Category => IndexingLocalizationSources.Indexing;

    [JsonIgnore]
    public override LocalizationSource Title => IndexingLocalizationSources.ResetIndexes;

    public bool IncludeAll { get; set; }

    public string[] IndexNames { get; set; }
}
