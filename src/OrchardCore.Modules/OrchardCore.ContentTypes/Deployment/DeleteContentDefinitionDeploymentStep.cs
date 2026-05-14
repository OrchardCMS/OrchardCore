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
    }

    public DeleteContentDefinitionDeploymentStep(IStringLocalizer<DeleteContentDefinitionDeploymentStep> S)
        : this()
    {
        Category = S["Content Management"];
    }

    public string[] ContentTypes { get; set; } = [];

    public string[] ContentParts { get; set; } = [];
}
