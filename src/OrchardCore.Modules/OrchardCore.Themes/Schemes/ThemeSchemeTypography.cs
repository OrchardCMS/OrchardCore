using System.Globalization;
using System.Text;

namespace OrchardCore.Themes.Schemes;

/// <summary>
/// The typography and shape settings a scheme can carry: how a font is turned into a CSS font stack, and the range and
/// the steps of its font, radius and density scales. The editor, the stylesheet and the validation of schemes all read
/// them here. Which of them a theme supports, and the fonts it ships, are in its <see cref="ThemeSchemeOptions"/>.
/// </summary>
public static class ThemeSchemeTypography
{
    /// <summary>
    /// The key of the system font: the font of the visitor's operating system. Every theme offers it.
    /// </summary>
    public const string SystemFont = "system";

    /// <summary>
    /// The longest font a scheme may name, with every family of its list.
    /// </summary>
    public const int MaxFontFamilyLength = 200;

    public const double MinFontScale = 0.875;

    public const double MaxFontScale = 1.125;

    public const double DefaultFontScale = 1;

    public const double MinRadiusScale = 0;

    public const double MaxRadiusScale = 1.5;

    public const double DefaultRadiusScale = 1;

    public const double MinDensity = 0.75;

    public const double MaxDensity = 1.25;

    public const double DefaultDensity = 1;

    /// <summary>
    /// The families written after a font, so the text falls back to the system's font when the font is missing.
    /// </summary>
    public const string FallbackStack = "system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", Arial, sans-serif";

    // The generic families and system keywords of CSS, written without quotes.
    private static readonly HashSet<string> s_genericFamilies = new(StringComparer.OrdinalIgnoreCase)
    {
        "serif", "sans-serif", "monospace", "cursive", "fantasy", "system-ui", "ui-serif", "ui-sans-serif", "ui-monospace",
        "ui-rounded", "emoji", "math", "fangsong", "-apple-system", "BlinkMacSystemFont",
    };

    /// <summary>
    /// The system font, which every theme offers: the font of the visitor's operating system.
    /// </summary>
    public static ThemeSchemeFont System { get; } = new(
        SystemFont,
        "System font",
        "system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", \"Noto Sans\", \"Liberation Sans\", Arial, sans-serif");

    /// <summary>
    /// The font scales the editor offers: a base size of 14 to 18 pixels.
    /// </summary>
    public static IReadOnlyList<double> FontScales { get; } = [0.875, 0.9375, 1, 1.0625, 1.125];

    /// <summary>
    /// The radius scales the editor offers: square, subtle, default and round corners.
    /// </summary>
    public static IReadOnlyList<double> RadiusScales { get; } = [0, 0.5, 1, 1.5];

    /// <summary>
    /// The densities the editor offers: compact, default and comfortable.
    /// </summary>
    public static IReadOnlyList<double> Densities { get; } = [0.75, 1, 1.25];

    /// <summary>
    /// The font of <paramref name="fonts"/>, or the system font, whose key or name is <paramref name="value"/>, ignoring
    /// case, or <see langword="null"/> when there is none.
    /// </summary>
    public static ThemeSchemeFont FindFont(string value, IEnumerable<ThemeSchemeFont> fonts = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        value = value.Trim();

        foreach (var font in (fonts ?? []).Append(System))
        {
            if (string.Equals(font.Key, value, StringComparison.OrdinalIgnoreCase)
                || string.Equals(font.Name, value, StringComparison.OrdinalIgnoreCase))
            {
                return font;
            }
        }

        return null;
    }

    /// <summary>
    /// The CSS font stack of a font a scheme names: the stack of the theme's font (or of the system font) with that key or
    /// name, or else the font families it lists, separated by commas, followed by <see cref="FallbackStack"/>. Returns
    /// <see langword="null"/> when the value is empty or is not a list of font family names.
    /// </summary>
    public static string GetFontStack(string value, IEnumerable<ThemeSchemeFont> fonts = null)
    {
        if (FindFont(value, fonts) is { } font)
        {
            return font.Stack;
        }

        if (!TryParseFamilies(value, out var families))
        {
            return null;
        }

        var stack = new StringBuilder();

        foreach (var family in families)
        {
            if (stack.Length > 0)
            {
                stack.Append(", ");
            }

            if (s_genericFamilies.Contains(family))
            {
                stack.Append(family);
            }
            else
            {
                stack.Append('"').Append(family).Append('"');
            }
        }

        // A list that does not end with a generic family falls back to the system's font.
        if (!s_genericFamilies.Contains(families[^1]))
        {
            stack.Append(", ").Append(FallbackStack);
        }

        return stack.ToString();
    }

    /// <summary>
    /// The name to show for a font a scheme names: the name of the theme's font (or of the system font) with that key or
    /// name, or else the value itself.
    /// </summary>
    public static string GetDisplayName(string value, IEnumerable<ThemeSchemeFont> fonts = null)
        => FindFont(value, fonts)?.Name ?? value?.Trim();

    /// <summary>
    /// Whether <paramref name="value"/> is empty (the default) or a font: the key or name of a font, or a list of font
    /// family names separated by commas (e.g. <c>Georgia, serif</c>). A name may be quoted, and holds only letters,
    /// digits, spaces, dashes, underscores and dots, so it can never break out of the stylesheet.
    /// </summary>
    public static bool IsValidFontFamily(string value)
        => string.IsNullOrWhiteSpace(value) || TryParseFamilies(value, out _);

    public static bool IsValidFontScale(double? value)
        => value is null || value is >= MinFontScale and <= MaxFontScale;

    public static bool IsValidRadiusScale(double? value)
        => value is null || value is >= MinRadiusScale and <= MaxRadiusScale;

    public static bool IsValidDensity(double? value)
        => value is null || value is >= MinDensity and <= MaxDensity;

    /// <summary>
    /// A number as CSS writes it: invariant culture, no trailing zeros.
    /// </summary>
    public static string Format(double value)
        => value.ToString("0.####", CultureInfo.InvariantCulture);

    private static bool TryParseFamilies(string value, out List<string> families)
    {
        families = null;

        if (string.IsNullOrWhiteSpace(value) || value.Length > MaxFontFamilyLength)
        {
            return false;
        }

        var parsed = new List<string>();

        foreach (var part in value.Split(','))
        {
            var family = part.Trim();

            // A name may be in single or double quotes.
            if (family.Length >= 2 && (family[0] is '"' or '\'') && family[^1] == family[0])
            {
                family = family[1..^1].Trim();
            }

            if (family.Length == 0)
            {
                return false;
            }

            foreach (var c in family)
            {
                if (!char.IsLetterOrDigit(c) && c is not (' ' or '-' or '_' or '.'))
                {
                    return false;
                }
            }

            parsed.Add(family);
        }

        families = parsed;

        return true;
    }
}

/// <summary>
/// A font a theme ships, or the system font: offered by the editor of schemes, and written as its stack when a scheme
/// names it by its key or its name.
/// </summary>
/// <param name="Key">The key a scheme may store, e.g. <c>lato</c>.</param>
/// <param name="Name">The name shown in the editor, which a scheme may store too, e.g. <c>Lato</c>.</param>
/// <param name="Stack">The CSS font stack, e.g. <c>"Lato", system-ui, sans-serif</c>.</param>
public sealed record ThemeSchemeFont(string Key, string Name, string Stack);
