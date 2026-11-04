using System.Text.Json.Serialization;
using OrchardCore.Deployment;
using OrchardCore.Localization;

namespace OrchardCore.Shortcodes.Deployment;

public class AllShortcodeTemplatesDeploymentStep : DeploymentStep
{
    public override string Name => "AllShortcodeTemplates";

    [JsonIgnore]
    public override LocalizationSource Category => CommonLocalizationSources.Content;

    /// <summary>
    /// Gets or sets the exact names of the shortcode templates to export. All of them are exported when it is <see langword="null"/> or empty.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[] Names { get; set; }
}
