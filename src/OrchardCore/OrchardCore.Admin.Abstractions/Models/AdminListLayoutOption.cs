namespace OrchardCore.Admin.Models;

/// <summary>
/// One layout a user can switch a list to, offered by the <c>AdminListLayoutSelector</c> shape.
/// </summary>
public sealed class AdminListLayoutOption
{
    /// <summary>
    /// The name of the layout, e.g. <c>Grid</c>.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The address rendering the list with it: the current page, carrying the layout in its query string.
    /// </summary>
    public string Url { get; set; }
}
