using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    public override string Name => "AllShortcodeTemplates";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Content;
}
