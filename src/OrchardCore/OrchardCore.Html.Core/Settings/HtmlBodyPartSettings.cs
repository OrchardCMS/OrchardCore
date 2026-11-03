using System.ComponentModel;

namespace OrchardCore.Html.Settings;

public class HtmlBodyPartSettings
{
    /// <summary>
    /// Whether to sanitize the Html input.
    /// </summary>
    [DefaultValue(true)]
    public bool SanitizeHtml { get; set; } = true;

}
