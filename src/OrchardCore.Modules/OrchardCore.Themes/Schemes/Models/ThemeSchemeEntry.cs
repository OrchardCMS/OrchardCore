namespace OrchardCore.Themes.Schemes.Models;

/// <summary>
/// A row of the list of the schemes of a theme: the scheme, its theme, and whether it is the site's default scheme
/// of the theme.
/// </summary>
public sealed class ThemeSchemeEntry
{
    /// <summary>
    /// Gets or sets the id of the theme.
    /// </summary>
    public string ThemeId { get; set; }

    /// <summary>
    /// Gets or sets the scheme.
    /// </summary>
    public ThemeScheme Scheme { get; set; }

    /// <summary>
    /// Gets or sets whether the scheme is the site's default scheme of the theme, which can't be deleted.
    /// </summary>
    public bool IsDefault { get; set; }
}
