using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Sitemaps.Deployment;

public sealed class AllSitemapsDeploymentStep : DeploymentStep
{
    public override string Name => "AllSitemaps";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.ContentManagement;
}
