using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep
{
    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Development;

    [JsonIgnore]
    public override LocalizationSource Title => TemplatesLocalizationSources.AllAdminTemplates;

    public AllAdminTemplatesDeploymentStep()
    {
        Name = "AllAdminTemplates";
    }
    public bool ExportAsFiles { get; set; }
}
