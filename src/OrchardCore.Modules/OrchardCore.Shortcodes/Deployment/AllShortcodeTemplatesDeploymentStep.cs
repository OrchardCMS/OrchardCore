using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllShortcodeTemplatesDeploymentStep>("Content");

    public AllShortcodeTemplatesDeploymentStep()
    {
        Name = "AllShortcodeTemplates";
        Category = s_category;
    }
}
