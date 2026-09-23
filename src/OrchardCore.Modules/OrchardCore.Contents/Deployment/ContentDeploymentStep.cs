using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDeploymentStep : DeploymentStep
{
    public ContentDeploymentStep()
    {
        Name = "ContentDeploymentStep";
        Category = LocalizedString.Create("Content Management");
    }

    public string[] ContentTypes { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
