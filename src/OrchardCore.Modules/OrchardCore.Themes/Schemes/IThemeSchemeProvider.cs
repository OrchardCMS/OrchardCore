using System.Text;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// Makes a theme support schemes: the schemes it seeds, its default one, and how a scheme is written to CSS. Register
/// it with <see cref="ThemeSchemesServiceCollectionExtensions.AddThemeSchemes{TProvider}"/>, and add
/// <c>OrchardCore.Themes.Schemes</c> to the dependencies of the theme's manifest.
/// </summary>
/// <remarks>
/// A provider is registered as a singleton: keep it stateless.
/// </remarks>
public interface IThemeSchemeProvider
{
    /// <summary>
    /// Gets the id of the theme, the id of its extension (e.g. <c>TheAdmin</c>).
    /// </summary>
    string ThemeId { get; }

    /// <summary>
    /// Gets the id of the scheme used when the site names no default for the theme, or names one that was deleted.
    /// </summary>
    string DefaultSchemeId { get; }

    /// <summary>
    /// Gets the attribute of the page's <c>&lt;html&gt;</c> element that holds the id of the current scheme, and that
    /// the rules <see cref="AppendCss"/> writes select on, e.g. <c>data-ta-scheme</c>.
    /// </summary>
    string SchemeAttributeName { get; }

    /// <summary>
    /// Creates the schemes the theme seeds the first time its schemes are read. They are ordinary records from then on:
    /// admins edit or delete them, and add their own.
    /// </summary>
    IReadOnlyList<ThemeScheme> CreateDefaults();

    /// <summary>
    /// Appends the rules of the scheme <paramref name="schemeId"/> to <paramref name="css"/>, selecting on
    /// <c>[{SchemeAttributeName}="{schemeId}"]</c>. The id is a valid CSS identifier (lowercase letters, digits and
    /// dashes). A scheme the theme can't use, e.g. one with an invalid color, appends nothing.
    /// </summary>
    void AppendCss(StringBuilder css, string schemeId, ThemeScheme scheme);
}
