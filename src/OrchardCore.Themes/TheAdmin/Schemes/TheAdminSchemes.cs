using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// The schemes of TheAdmin: its default one (Gray, which the stylesheet falls back to), and how its stylesheet names
/// things. The schemes it starts with (Gray, Blue, Ocean...) are imported once by a migration, from
/// Migrations/admin-schemes.recipe.json, and are ordinary records from then on. Its tokens are the generated
/// ones under the <c>ta</c> prefix, a few of them renamed (see <see cref="TokenNames"/>), and the ones of
/// <see cref="TheAdminSchemeTokens"/>, set on <c>&lt;html data-ta-scheme="..."&gt;</c>.
/// </summary>
public static class TheAdminSchemes
{
    /// <summary>
    /// The scheme used when the site names no default: also the look of the stylesheet's own fallback, so the admin
    /// keeps its colors even with no scheme at all.
    /// </summary>
    public const string DefaultId = "gray";

    /// <summary>
    /// The attribute of the page that holds the current scheme, which the theme's stylesheet selects on.
    /// </summary>
    public const string AttributeName = "data-ta-scheme";

    /// <summary>
    /// The prefix of the theme's tokens.
    /// </summary>
    public const string TokenPrefix = "ta";

    /// <summary>
    /// The selector of the dark mode, which the dark rule of a scheme puts before the scheme's own:
    /// <c>[data-bs-theme="dark"][data-ta-scheme="{id}"]</c>, both on the page's <c>&lt;html&gt;</c> element.
    /// </summary>
    public const string DarkModeSelector = "[data-bs-theme=\"dark\"]";

    /// <summary>
    /// The generated tokens TheAdmin's Sass names otherwise, by generic name (e.g. <c>deep</c>, without the prefix).
    /// </summary>
    public static IReadOnlyDictionary<string, string> TokenNames { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["deep"] = "navy",
        ["accent"] = "sky",
        ["page"] = "page-bg",
        ["focus"] = "focus-color",
        ["row-hover"] = "row-hover-bg",
    };

    /// <summary>
    /// The font of a scheme that names none.
    /// </summary>
    public const string DefaultFontFamily = "lato";

    private const string FallbackStack = AdminSchemeTypography.FallbackStack;

    /// <summary>
    /// The font the theme ships, which the editor of schemes suggests, with the system font. A scheme may name any other
    /// font family too, which the users' devices must have installed.
    /// </summary>
    public static IReadOnlyList<AdminSchemeFont> Fonts { get; } =
    [
        new(DefaultFontFamily, "Lato", "\"Lato\", " + FallbackStack),
    ];

    /// <summary>
    /// The CSS font stack of a font a scheme names; the stack of <see cref="DefaultFontFamily"/> when the scheme names
    /// none, or names one that is not valid.
    /// </summary>
    public static string GetFontStack(string fontFamily)
        => AdminSchemeTypography.GetFontStack(fontFamily, Fonts)
            ?? AdminSchemeTypography.GetFontStack(DefaultFontFamily, Fonts)
            ?? AdminSchemeTypography.System.Stack;

    /// <summary>
    /// Gray, the default scheme, as the migration's recipe imports it (Migrations/admin-schemes.recipe.json): the scheme
    /// used when there is no stored scheme at all, for example when the schemes were removed outside the admin. It is
    /// never stored. A test keeps it in step with the recipe.
    /// </summary>
    public static AdminScheme CreateFallback()
        => new()
        {
            Id = DefaultId, Name = "Gray", Position = 0,
            Primary = "#0d6efd", Deep = "#212529", Ink = "#212529", Accent = "#6ea8fe",
            LightMenu = true, Hue = 210, Saturation = 0.12, Page = "#ffffff", Toolbar = "#495057", Tint = "#495057", TintDark = "#adb5bd",
            FontFamily = AdminSchemeTypography.SystemFont,
        };
}
