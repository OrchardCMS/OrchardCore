using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

/// <summary>
/// Deletes selected content definitions to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class DeleteContentDefinitionDeploymentStep : DeploymentStep
{
    public DeleteContentDefinitionDeploymentStep()
    {
        Name = "DeleteContentDefinition";
        Category = LocalizedString.Create("Content Management");
    }

    public string[] ContentTypes { get; set; } = [];

    public string[] ContentParts { get; set; } = [];
}
