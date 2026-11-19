using OrchardCore.Data.Documents;

namespace OrchardCore.Themes.Schemes.Models;

/// <summary>
/// The schemes of every theme that supports them. A theme's default schemes (see
/// <see cref="ThemeSchemeOptions.Defaults"/>) are seeded the first time they are read; admins edit, delete and add
/// schemes from there. Kept in one document behind Orchard Core's cached
/// <c>IDocumentManager</c>: read on every page of a theme with schemes, written only when a scheme is saved or deleted.
/// </summary>
public sealed class ThemeSchemesDocument : Document
{
    /// <summary>
    /// Gets or sets the schemes, by theme id, then by scheme id.
    /// </summary>
    public Dictionary<string, Dictionary<string, ThemeScheme>> Themes { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the site's default scheme of each theme, by theme id. A theme with no entry, or whose entry names
    /// a scheme that is not there, uses its own default scheme (see <see cref="ThemeSchemeOptions.DefaultSchemeId"/>).
    /// </summary>
    public Dictionary<string, string> Defaults { get; set; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets the ids of the themes whose default schemes were seeded. A theme is seeded once: deleting all of
    /// its schemes never brings the defaults back.
    /// </summary>
    public HashSet<string> Seeded { get; set; } = new(StringComparer.Ordinal);
}
