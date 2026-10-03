using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Contents.Deployment;

/// <summary>
/// Adds all content items to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllContentDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<AllContentDeploymentStep>("Content Management");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<AllContentDeploymentStep>("All Content");

    public AllContentDeploymentStep()
    {
        Name = "AllContent";
        Category = s_category;
        Title = s_title;
    }

    public bool ExportAsSetupRecipe { get; set; }
}
