using System.ComponentModel;

namespace OrchardCore.Html.Settings;

public class HtmlBodyPartSettings
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
