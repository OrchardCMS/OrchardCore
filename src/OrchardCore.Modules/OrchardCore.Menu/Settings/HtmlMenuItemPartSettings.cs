using System.ComponentModel;

namespace OrchardCore.Menu.Settings;

public class HtmlMenuItemPartSettings
{
    /// <summary>
    /// Whether to sanitize the rendered HTML.
    /// </summary>
    [DefaultValue(true)]
    public bool SanitizeHtml { get; set; } = true;
}
