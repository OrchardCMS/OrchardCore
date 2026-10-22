namespace OrchardCore.Admin.Models;

/// <summary>
/// Presentation options for one admin list layout. Declaring options does not select or force that layout.
/// </summary>
public sealed class AdminListLayoutOptions
{
    /// <summary>
    /// Additional CSS classes for the element containing the rows, merged with <c>RowsAttributes["class"]</c>.
    /// </summary>
    public string RowsCssClass { get; set; }

    /// <summary>
    /// CSS classes for each row. For List, replaces <c>ItemCssClass</c> and its default classes.
    /// For Grid, adds classes alongside the required grid row classes.
    /// </summary>
    public string ItemCssClass { get; set; }
}
