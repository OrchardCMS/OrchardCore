using Microsoft.Extensions.Localization;
using OrchardCore.Deployment;

namespace OrchardCore.ContentTypes.Deployment;

/// <summary>
/// Adds selected content definitions to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDefinitionDeploymentStep : DeploymentStep
{
    public ContentDefinitionDeploymentStep()
    {
        Name = "ContentDefinition";
    }

    public ContentDefinitionDeploymentStep(IStringLocalizer<ContentDefinitionDeploymentStep> S)
        : this()
    {
        Category = S["Content Management"];
    }

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
