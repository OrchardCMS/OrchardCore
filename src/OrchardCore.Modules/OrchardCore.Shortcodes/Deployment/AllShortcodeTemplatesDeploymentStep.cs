using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep, IDeploymentStep
{
    public override string Name => "AllShortcodeTemplates";

    public static LocalizationSource Category => CommonLocalizationSources.Content;
}
