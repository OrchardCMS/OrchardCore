using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    public AllShortcodeTemplatesDeploymentStep()
    {
        Name = "AllShortcodeTemplates";
        Category = LocalizedString.Create("Content");
    }
}
