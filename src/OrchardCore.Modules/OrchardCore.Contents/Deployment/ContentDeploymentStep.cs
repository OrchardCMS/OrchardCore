using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(ContentDeploymentStep));
    private static readonly LocalizationSource s_title = new("Content Types", typeof(ContentDeploymentStep));

    public ContentDeploymentStep()
    {
        Name = "ContentDeploymentStep";
        Category = s_category;
        Title = s_title;
    }

    public string[] ContentTypes { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
