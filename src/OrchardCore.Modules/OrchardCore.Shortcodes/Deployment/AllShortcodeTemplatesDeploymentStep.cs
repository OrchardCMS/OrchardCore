using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content", typeof(AllShortcodeTemplatesDeploymentStep));

    public AllShortcodeTemplatesDeploymentStep()
    {
        Name = "AllShortcodeTemplates";
        Category = s_category;
    }
}
