using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Web;

namespace OrchardCore.Settings.TagHelpers;

/// <summary>
/// Renders a badge indicating the configuration source for a property.
/// </summary>
[HtmlTargetElement("config-badge", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ConfigurationSourceBadgeTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the configuration source.
    /// </summary>
    [HtmlAttributeName("source")]
    public ConfigurationSource Source { get; set; }

    /// <summary>
    /// Gets or sets the size of the badge. Defaults to "sm".
    /// </summary>
    [HtmlAttributeName("size")]
    public string Size { get; set; } = "sm";

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";

        var (cssClass, text, title) = Source switch
        {
            ConfigurationSource.ConfigurationFile => ("badge text-bg-info", "Config File", "Value is set from appsettings.json"),
            ConfigurationSource.Database => ("badge text-bg-secondary", "Database", "Value is set from Admin UI"),
            _ => ("badge text-bg-light", "Default", "Using default value"),
        };

        output.Attributes.SetAttribute("class", cssClass);
        output.Attributes.SetAttribute("title", title);
        output.Attributes.SetAttribute("data-bs-toggle", "tooltip");

        output.Content.SetContent(text);
    }
}

/// <summary>
/// Renders comprehensive information about a configuration property including badge and effective value.
/// </summary>
[HtmlTargetElement("config-property-info", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ConfigurationPropertyInfoTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    [HtmlAttributeName("property-name")]
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the configuration metadata.
    /// </summary>
    [HtmlAttributeName("metadata")]
    public SettingsConfigurationMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets whether to show the effective value.
    /// </summary>
    [HtmlAttributeName("show-effective-value")]
    public bool ShowEffectiveValue { get; set; } = true;

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(PropertyName) || Metadata == null)
        {
            output.SuppressOutput();
            return;
        }

        var propertyMetadata = Metadata.GetPropertyMetadata(PropertyName);
        if (propertyMetadata == null)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";
        output.Attributes.SetAttribute("class", "config-property-info d-inline-flex align-items-center gap-2");

        // Badge
        var badgeHtml = CreateBadge(propertyMetadata.Source);
        output.Content.AppendHtml(badgeHtml);

        // Effective value if shown and different from form value
        if (ShowEffectiveValue && propertyMetadata.IsOverriddenByFile)
        {
            var effectiveValueHtml = CreateEffectiveValueDisplay(propertyMetadata);
            output.Content.AppendHtml(effectiveValueHtml);
        }
    }

    private static string CreateBadge(ConfigurationSource source)
    {
        var (cssClass, text, title) = source switch
        {
            ConfigurationSource.ConfigurationFile => ("badge text-bg-info", "Config File", "Value is set from appsettings.json"),
            ConfigurationSource.Database => ("badge text-bg-secondary", "Database", "Value is set from Admin UI"),
            _ => ("badge text-bg-light", "Default", "Using default value"),
        };

        return $"<span class=\"{cssClass}\" title=\"{title}\" data-bs-toggle=\"tooltip\">{text}</span>";
    }

    private static string CreateEffectiveValueDisplay(PropertyConfigurationMetadata metadata)
    {
        var effectiveValue = metadata.IsSensitive
            ? metadata.GetMaskedValue()
            : metadata.EffectiveValue?.ToString() ?? "(empty)";

        return $"<small class=\"text-muted\" title=\"Effective value from config file\">→ {HttpUtility.HtmlEncode(effectiveValue)}</small>";
    }
}

/// <summary>
/// Disables or marks an input as read-only based on configuration metadata.
/// </summary>
[HtmlTargetElement("input", Attributes = "config-property")]
[HtmlTargetElement("select", Attributes = "config-property")]
[HtmlTargetElement("textarea", Attributes = "config-property")]
public class ConfigurationPropertyInputTagHelper : TagHelper
{
    /// <summary>
    /// Gets or sets the property name for configuration checking.
    /// </summary>
    [HtmlAttributeName("config-property")]
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the configuration metadata.
    /// </summary>
    [HtmlAttributeName("config-metadata")]
    public SettingsConfigurationMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets whether the form is in read-only mode.
    /// </summary>
    [HtmlAttributeName("config-readonly")]
    public bool IsReadOnly { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var shouldDisable = IsReadOnly;

        if (!shouldDisable && !string.IsNullOrEmpty(PropertyName) && Metadata != null)
        {
            var propertyMetadata = Metadata.GetPropertyMetadata(PropertyName);
            shouldDisable = propertyMetadata != null && !propertyMetadata.CanConfigureViaUI;
        }

        if (shouldDisable)
        {
            output.Attributes.SetAttribute("disabled", "disabled");
            output.Attributes.SetAttribute("readonly", "readonly");
        }
    }
}
