using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.DataLocalization.Deployment;

public class AllDataTranslationsDeploymentStep : DeploymentStep
{
    public override string Name => "AllDataTranslations";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Internationalization;
}
