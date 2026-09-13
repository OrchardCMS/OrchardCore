using System.ComponentModel;
using OrchardCore.ContentManagement;

namespace OrchardCore.Menu.Models;

public class LinkMenuItemPart : ContentPart
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
}
