using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllContentDeploymentStep : DeploymentStep
{
    public AllContentDeploymentStep()
    {
        Name = "AllContent";
        Category = CommonLocalizationSources.ContentManagement;
        Title = ContentsLocalizationSources.AllContent;
    }

    public bool ExportAsSetupRecipe { get; set; }
}
