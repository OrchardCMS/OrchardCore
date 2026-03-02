using System.ComponentModel;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.ContentFields.Settings;

public class HtmlFieldSettings : FieldSettings
{
    /// <summary>
    /// Whether to sanitize the Html input.
    /// </summary>
    [DefaultValue(true)]
    public bool SanitizeHtml { get; set; } = true;

    /// <summary>
    /// Whether Liquid templating is enabled.
    /// </summary>
    public bool RenderLiquid { get; set; }
}
