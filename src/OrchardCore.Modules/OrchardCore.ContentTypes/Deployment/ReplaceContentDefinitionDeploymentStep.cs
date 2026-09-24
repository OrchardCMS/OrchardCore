using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

public class ReplaceContentDefinitionDeploymentStep : DeploymentStep
{
    public ReplaceContentDefinitionDeploymentStep()
    {
        Name = "ReplaceContentDefinition";
        Category = LocalizedString.Create("Content Management");
        Title = LocalizedString.Create("Replace Content Definitions");
    }

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
