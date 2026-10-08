using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

public class ReplaceContentDefinitionDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = LocalizationSource.Create<ReplaceContentDefinitionDeploymentStep>("Content Management");
    private static readonly LocalizationSource s_title = LocalizationSource.Create<ReplaceContentDefinitionDeploymentStep>("Replace Content Definitions");

    public ReplaceContentDefinitionDeploymentStep()
    {
        Name = "ReplaceContentDefinition";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
