using System.ComponentModel;

namespace OrchardCore.Html.Settings;

public class HtmlMenuItemPartSettings
{
    /// <summary>
    /// Whether to sanitize the html input.
    /// </summary>
    [DefaultValue(true)]
    public bool SanitizeHtml { get; set; } = true;
}
