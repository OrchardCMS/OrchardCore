using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    public AllShortcodeTemplatesDeploymentStep()
    {
        Name = "AllShortcodeTemplates";
    }

    public AllShortcodeTemplatesDeploymentStep(IStringLocalizer<AllShortcodeTemplatesDeploymentStep> S)
        : this()
    {
        Category = S["Content"];
    }
}
