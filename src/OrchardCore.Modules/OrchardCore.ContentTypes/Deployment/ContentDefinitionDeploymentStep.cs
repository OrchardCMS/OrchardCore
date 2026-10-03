using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Deployment;

/// <summary>
/// Adds selected content definitions to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class ContentDefinitionDeploymentStep : DeploymentStep
{
    private static readonly LocalizationSource s_category = new("Content Management", typeof(ContentDefinitionDeploymentStep));
    private static readonly LocalizationSource s_title = new("Update Content Definitions", typeof(ContentDefinitionDeploymentStep));

    public ContentDefinitionDeploymentStep()
    {
        Name = "ContentDefinition";
        Category = s_category;
        Title = s_title;
    }

    public bool IncludeAll { get; set; }

    public string[] ContentTypes { get; set; }

    public string[] ContentParts { get; set; }
}
