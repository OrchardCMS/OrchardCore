using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Templates.Deployment;

/// <summary>
/// Adds templates to a <see cref="DeploymentPlanResult"/>.
/// </summary>
public class AllAdminTemplatesDeploymentStep : DeploymentStep
{
    public override string Name => "AllAdminTemplates";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Development;

    [JsonIgnore]
    public override LocalizationSource Title => TemplatesLocalizationSources.AllAdminTemplates;

    public bool ExportAsFiles { get; set; }
}
