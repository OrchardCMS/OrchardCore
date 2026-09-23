using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllContentDeploymentStep : DeploymentStep
{
    public AllContentDeploymentStep()
    {
        Name = "AllContent";
        Category = LocalizedString.Create("Content Management");
    }

    public bool ExportAsSetupRecipe { get; set; }
}
