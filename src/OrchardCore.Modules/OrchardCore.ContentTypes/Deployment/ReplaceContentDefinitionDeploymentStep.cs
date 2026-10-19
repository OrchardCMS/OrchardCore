using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

public class ReplaceContentDefinitionDeploymentStep : DeploymentStep
{
    public ReplaceContentDefinitionDeploymentStep()
    {
        Name = "ReplaceContentDefinition";
        Category = CommonLocalizationSources.ContentManagement;
        Title = ContentTypesLocalizationSources.ReplaceContentDefinitions;
    }

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
