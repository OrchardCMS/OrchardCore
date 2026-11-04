using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models;

public class HtmlMenuItemPart : ContentPart
{
    /// <summary>
    /// The url of the link to create.
    /// </summary>
    [Description("The URL opened by the menu item.")]
    public string Url { get; set; }

    /// <summary>
    /// The target of the link to create.
    /// </summary>
    [Description("The browsing context in which to open the URL, such as _blank.")]
    public string Target { get; set; }

    /// <summary>
    /// The raw html to display for this link.
    /// </summary>
    [Description("The HTML markup displayed for the menu item.")]
    public string Html { get; set; }
}
