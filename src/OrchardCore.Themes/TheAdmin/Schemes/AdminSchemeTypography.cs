using System.Globalization;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// The typography and shape settings a scheme can carry: the font families it may use, and the range and the steps of
/// its font, radius and density scales. The editor, the stylesheet and the validation of schemes all read them here.
/// </summary>
public static class AdminSchemeTypography
{
    public const string Lato = "lato";

    public const string Inter = "inter";

    public const string SourceSans3 = "source-sans-3";

    public const string SystemFont = "system";

    /// <summary>
    /// The body font of a scheme that names none.
    /// </summary>
    public const string DefaultFontFamily = Lato;

    public const double MinFontScale = 0.875;

    public const double MaxFontScale = 1.125;

    public const double DefaultFontScale = 1;

    public const double MinRadiusScale = 0;

    public const double MaxRadiusScale = 1.5;

    public const double DefaultRadiusScale = 1;

    public const double MinDensity = 0.75;

    public const double MaxDensity = 1.25;

    public const double DefaultDensity = 1;

    private const string FallbackStack = "system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", Arial, sans-serif";

    /// <summary>
    /// The font families a scheme may use, in the order of the editor.
    /// </summary>
    public static IReadOnlyList<AdminSchemeFont> Fonts { get; } =
    [
        new(Lato, "Lato", "\"Lato\", " + FallbackStack),
        new(Inter, "Inter", "\"Inter\", " + FallbackStack),
        new(SourceSans3, "Source Sans 3", "\"Source Sans 3\", " + FallbackStack),
        new(SystemFont, "System font", "system-ui, -apple-system, \"Segoe UI\", Roboto, \"Helvetica Neue\", \"Noto Sans\", \"Liberation Sans\", Arial, sans-serif"),
    ];

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
    /// The font family with the given key, or <see langword="null"/> when there is none.
    /// </summary>
    public static AdminSchemeFont FindFont(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }

        foreach (var font in Fonts)
        {
            if (string.Equals(font.Key, key, StringComparison.Ordinal))
            {
                return font;
            }
        }

        return null;
    }

    /// <summary>
    /// Whether <paramref name="key"/> is empty (the default) or the key of a font family.
    /// </summary>
    public static bool IsValidFontFamily(string key)
        => string.IsNullOrEmpty(key) || FindFont(key) is not null;

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
}

/// <summary>
/// A font family a scheme may use.
/// </summary>
/// <param name="Key">The key stored in the scheme.</param>
/// <param name="Name">The name shown in the editor.</param>
/// <param name="Stack">The CSS font stack.</param>
public sealed record AdminSchemeFont(string Key, string Name, string Stack);
