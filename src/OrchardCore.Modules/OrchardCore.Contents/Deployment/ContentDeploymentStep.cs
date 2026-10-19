using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items of a specific type to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDeploymentStep : DeploymentStep
{
    public ContentDeploymentStep()
    {
        Name = "ContentDeploymentStep";
        Category = CommonLocalizationSources.ContentManagement;
        Title = ContentsLocalizationSources.ContentTypes;
    }

    public string[] ContentTypes { get; set; }
    public bool ExportAsSetupRecipe { get; set; }
}
