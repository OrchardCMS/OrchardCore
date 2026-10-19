using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    public AllShortcodeTemplatesDeploymentStep()
    {
        Name = "AllShortcodeTemplates";
        Category = CommonLocalizationSources.Content;
    }
}
