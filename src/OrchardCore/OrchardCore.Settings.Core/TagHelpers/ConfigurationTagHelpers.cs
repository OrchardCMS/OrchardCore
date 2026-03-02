using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;
using System.Text;
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
        output.TagMode = TagMode.StartTagAndEndTag;

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
        output.TagMode = TagMode.StartTagAndEndTag;
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
            : metadata.GetDisplayValue() ?? "(empty)";

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

/// <summary>
/// Renders an alert banner when configuration file settings are active.
/// </summary>
[HtmlTargetElement("config-alert", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ConfigurationAlertTagHelper : TagHelper
{
    private readonly IStringLocalizer<ConfigurationAlertTagHelper> _localizer;

    public ConfigurationAlertTagHelper(IStringLocalizer<ConfigurationAlertTagHelper> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Gets or sets the configuration metadata.
    /// </summary>
    [HtmlAttributeName("metadata")]
    public SettingsConfigurationMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets whether to show the list of overridden properties.
    /// </summary>
    [HtmlAttributeName("show-overridden-properties")]
    public bool ShowOverriddenProperties { get; set; } = true;

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Check if there are any file overrides (either via flag or by checking properties)
        var hasFileOverrides = Metadata != null &&
            (Metadata.IsConfiguredFromFile || Metadata.GetOverriddenProperties().Any());

        if (!hasFileOverrides)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "alert alert-info");
        output.Attributes.SetAttribute("role", "alert");

        var sb = new StringBuilder();

        // Header
        sb.Append("<h5 class=\"alert-heading\">");
        sb.Append("<i class=\"fa-solid fa-file-code me-2\"></i>");
        sb.Append(HttpUtility.HtmlEncode(_localizer["Configuration File Active"]));
        sb.Append("</h5>");

        // Description
        sb.Append("<p class=\"mb-2\">");
        sb.Append(HttpUtility.HtmlEncode(_localizer["Some settings are configured via appsettings.json and may override values entered here."]));
        sb.Append("</p>");

        if (Metadata.DisableUIConfiguration)
        {
            sb.Append("<p class=\"mb-0 fw-bold\">");
            sb.Append(HttpUtility.HtmlEncode(_localizer["UI configuration is disabled. All settings are managed through configuration files."]));
            sb.Append("</p>");
        }
        else
        {
            sb.Append("<p class=\"mb-2 small\">");
            sb.AppendFormat(
                _localizer["Properties with the {0} badge are configured from files."].Value,
                "<span class=\"badge text-bg-info\">Config File</span>");
            sb.Append("</p>");

            if (ShowOverriddenProperties)
            {
                var overriddenProperties = Metadata.GetOverriddenProperties().ToList();
                if (overriddenProperties.Count > 0)
                {
                    sb.Append("<hr />");
                    sb.Append("<p class=\"mb-2 small fw-bold\">");
                    sb.Append(HttpUtility.HtmlEncode(_localizer["Overridden properties:"]));
                    sb.Append("</p>");
                    sb.Append("<ul class=\"mb-0 small\">");

                    foreach (var prop in overriddenProperties)
                    {
                        sb.Append("<li>");
                        sb.Append(HttpUtility.HtmlEncode(prop.DisplayName));

                        if (prop.IsSensitive)
                        {
                            sb.Append("<span class=\"text-muted\">: ");
                            sb.Append(HttpUtility.HtmlEncode(prop.GetMaskedValue()));
                            sb.Append("</span>");
                        }
                        else if (prop.EffectiveValue != null)
                        {
                            sb.Append("<span class=\"text-muted\">: ");
                            sb.Append(HttpUtility.HtmlEncode(prop.GetDisplayValue()));
                            sb.Append("</span>");
                        }

                        sb.Append("</li>");
                    }

                    sb.Append("</ul>");
                }
            }
        }

        output.Content.SetHtmlContent(sb.ToString());
    }
}

/// <summary>
/// Renders a warning when a property value is overridden by configuration file.
/// </summary>
[HtmlTargetElement("config-override-warning", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ConfigurationOverrideWarningTagHelper : TagHelper
{
    private readonly IStringLocalizer<ConfigurationOverrideWarningTagHelper> _localizer;

    public ConfigurationOverrideWarningTagHelper(IStringLocalizer<ConfigurationOverrideWarningTagHelper> localizer)
    {
        _localizer = localizer;
    }

    /// <summary>
    /// Gets or sets the property metadata.
    /// </summary>
    [HtmlAttributeName("property")]
    public PropertyConfigurationMetadata Property { get; set; }

    /// <inheritdoc/>
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        // Don't show warning if:
        // - No property metadata
        // - Property is not overridden by file
        // - Property uses Merge strategy (values are combined, not overridden)
        if (Property == null || !Property.IsOverriddenByFile || Property.MergeStrategy == PropertyMergeStrategy.Merge)
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "alert alert-warning py-2 mb-2");
        output.Attributes.SetAttribute("role", "alert");

        var sb = new StringBuilder();
        sb.Append("<small>");
        sb.Append("<i class=\"fa-solid fa-exclamation-triangle me-1\"></i>");
        sb.Append(HttpUtility.HtmlEncode(_localizer["This value is overridden by configuration file."]));

        if (Property.IsSensitive)
        {
            sb.Append(" <span class=\"fw-bold\">");
            sb.AppendFormat(
                HttpUtility.HtmlEncode(_localizer["Effective value: {0}"].Value),
                HttpUtility.HtmlEncode(Property.GetMaskedValue()));
            sb.Append("</span>");
        }
        else if (Property.EffectiveValue != null)
        {
            sb.Append(" <span class=\"fw-bold\">");
            sb.AppendFormat(
                HttpUtility.HtmlEncode(_localizer["Effective value: {0}"].Value),
                HttpUtility.HtmlEncode(Property.GetDisplayValue()));
            sb.Append("</span>");
        }

        sb.Append("</small>");

        output.Content.SetHtmlContent(sb.ToString());
    }
}
