using OrchardCore.Themes.Schemes;
using OrchardCore.Themes.Schemes.Models;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// The schemes of TheAdmin: the ones it seeds the first time its schemes are read (Blue, Gray, Ocean...), which are
/// ordinary records from then on, its default one, and how its stylesheet names things. Its tokens are the generated
/// ones under the <c>ta</c> prefix, a few of them renamed (see <see cref="TokenNames"/>), and the ones of
/// <see cref="TheAdminSchemeTokens"/>, set on <c>&lt;html data-ta-scheme="..."&gt;</c>.
/// </summary>
public static class TheAdminSchemes
{
    /// <summary>
    /// The scheme used when the site names no default: also the look of the stylesheet's own fallback, so the admin
    /// keeps its colors even with no scheme at all.
    /// </summary>
    public const string DefaultId = "blue";

    /// <summary>
    /// The attribute of the page that holds the current scheme, which the theme's stylesheet selects on.
    /// </summary>
    public const string AttributeName = "data-ta-scheme";

    /// <summary>
    /// The prefix of the theme's tokens.
    /// </summary>
    public const string TokenPrefix = "ta";

    /// <summary>
    /// The generated tokens TheAdmin's Sass names otherwise, by generic name: copied to
    /// <see cref="ThemeSchemeOptions.TokenNames"/>.
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
    /// The schemes to seed, in the order of the pickers.
    /// </summary>
    public static IReadOnlyList<ThemeScheme> CreateDefaults()
        =>
        [
            new() { Id = DefaultId, Name = "Blue", Position = 0, Primary = "#2a81bb", Deep = "#144c71", Ink = "#081b26", Accent = "#5aa4ef", Hue = 209, Saturation = 1 },

            // The look of the former TheAdmin: Bootstrap's blue on a white page, a light gray menu and the system font.
            new()
            {
                Id = "gray", Name = "Gray", Position = 1,
                Primary = "#0d6efd", Deep = "#212529", Ink = "#212529", Accent = "#6ea8fe",
                LightMenu = true, Hue = 210, Saturation = 0.12, Page = "#ffffff", Toolbar = "#495057", Tint = "#495057", TintDark = "#adb5bd",
                FontFamily = ThemeSchemeTypography.SystemFont,
            },
            new() { Id = "ocean", Name = "Ocean", Position = 2, Primary = "#12868b", Deep = "#0d4a52", Ink = "#062227", Accent = "#4fd1c5", Hue = 187, Saturation = 0.9 },
            new() { Id = "forest", Name = "Forest", Position = 3, Primary = "#2f855a", Deep = "#1c4a36", Ink = "#0b2018", Accent = "#68d391", Hue = 150, Saturation = 0.7 },
            new() { Id = "plum", Name = "Plum", Position = 4, Primary = "#7b4fc0", Deep = "#3d2a66", Ink = "#170f2b", Accent = "#b794f4", Hue = 262, Saturation = 0.7 },
            new() { Id = "sunset", Name = "Sunset", Position = 5, Primary = "#c2551f", Deep = "#5c2a1c", Ink = "#24100b", Accent = "#f6ad55", Hue = 18, Saturation = 0.6 },
            new() { Id = "graphite", Name = "Graphite", Position = 6, Primary = "#4b6584", Deep = "#2d3540", Ink = "#121519", Accent = "#94a8bf", Hue = 213, Saturation = 0.2 },

            // Gold on charcoal; the gold is too light for white text, so its buttons and current page take the charcoal.
            new()
            {
                Id = "gold", Name = "Gold", Position = 7,
                Primary = "#eaa52c", Deep = "#1e1e1e", Ink = "#09080b", Accent = "#eaa52c", Hue = 240, Saturation = 0.08, OnPrimary = "#1e1e1e",
            },
        ];
}
