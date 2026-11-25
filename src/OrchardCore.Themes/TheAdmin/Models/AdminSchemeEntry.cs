namespace OrchardCore.Themes.TheAdmin.Models;

/// <summary>
/// A row of the list of the schemes: the scheme, and whether it is the site's default scheme.
/// </summary>
public sealed class AdminSchemeEntry
{
    /// <summary>
    /// Gets or sets the scheme.
    /// </summary>
    public AdminScheme Scheme { get; set; }

    /// <summary>
    /// Gets or sets whether the scheme is the site's default scheme, which can't be deleted.
    /// </summary>
    public bool IsDefault { get; set; }
}
