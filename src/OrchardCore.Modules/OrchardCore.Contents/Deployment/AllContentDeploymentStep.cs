using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllContentDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(AllContentDeploymentStep));
    private static readonly LocalizationSource s_title = new("All Content", typeof(AllContentDeploymentStep));

    public AllContentDeploymentStep()
    {
        Name = "AllContent";
        Category = s_category;
        Title = s_title;
    }

    public bool ExportAsSetupRecipe { get; set; }
}
