using System.Globalization;

namespace OrchardCore.Themes.TheAdmin.Schemes;

/// <summary>
/// An sRGB color with unrounded channels (0 to 255), and the color functions the admin derives the tokens of its
/// schemes with. They are the C# counterparts of the usual Sass functions (color.mix, hsl, luminance, contrast and an
/// ensure-contrast loop), so a scheme added at runtime can get the same tokens as one built into a theme's stylesheet.
/// </summary>
public readonly record struct AdminColor(double R, double G, double B)
{
    public static readonly AdminColor White = new(255, 255, 255);

    public static readonly AdminColor Black = new(0, 0, 0);

    /// <summary>
    /// Parses a '#rgb' or '#rrggbb' color.
    /// </summary>
    public static bool TryParse(string value, out AdminColor color)
    {
        color = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var hex = value.Trim().TrimStart('#');

        if (hex.Length == 3)
        {
            hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
        }

        if (hex.Length != 6 || !int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            return false;
        }

        color = new AdminColor((rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);

        return true;
    }

    public static AdminColor Parse(string value)
        => TryParse(value, out var color) ? color : throw new FormatException($"'{value}' is not a '#rrggbb' color.");

    /// <summary>
    /// The color as '#rrggbb', channels rounded.
    /// </summary>
    public string ToHex()
        => string.Create(CultureInfo.InvariantCulture, $"#{Channel(R):x2}{Channel(G):x2}{Channel(B):x2}");

    /// <summary>
    /// The rounded channels as 'r, g, b', for '-rgb' tokens.
    /// </summary>
    public string ToTriplet()
        => string.Create(CultureInfo.InvariantCulture, $"{Channel(R)}, {Channel(G)}, {Channel(B)}");

    /// <summary>
    /// The color with an alpha, as 'rgba(r, g, b, a)'.
    /// </summary>
    public string ToRgba(double alpha)
        => string.Create(CultureInfo.InvariantCulture, $"rgba({Channel(R)}, {Channel(G)}, {Channel(B)}, {alpha})");

    public override string ToString() => ToHex();

    /// <summary>
    /// Sass' 'color.mix($this, $other, $weight)': this color at <paramref name="weight"/> (0 to 1), the other at the rest.
    /// </summary>
    public AdminColor Mix(AdminColor other, double weight)
        => new(
            (R * weight) + (other.R * (1 - weight)),
            (G * weight) + (other.G * (1 - weight)),
            (B * weight) + (other.B * (1 - weight)));

    /// <summary>
    /// Sass' 'hsl($hue, $saturation, $lightness)' in sRGB, saturation and lightness from 0 to 1.
    /// </summary>
    public static AdminColor FromHsl(double hue, double saturation, double lightness)
    {
        saturation = Math.Clamp(saturation, 0, 1);
        lightness = Math.Clamp(lightness, 0, 1);
        hue = ((hue % 360) + 360) % 360;

        var chroma = (1 - Math.Abs((2 * lightness) - 1)) * saturation;
        var x = chroma * (1 - Math.Abs(((hue / 60) % 2) - 1));
        var m = lightness - (chroma / 2);

        var (r, g, b) = hue switch
        {
            < 60 => (chroma, x, 0d),
            < 120 => (x, chroma, 0d),
            < 180 => (0d, chroma, x),
            < 240 => (0d, x, chroma),
            < 300 => (x, 0d, chroma),
            _ => (chroma, 0d, x),
        };

        return new AdminColor((r + m) * 255, (g + m) * 255, (b + m) * 255);
    }

    /// <summary>
    /// The hue (0 to 360) and saturation (0 to 1) of the color, in HSL.
    /// </summary>
    public (double Hue, double Saturation) ToHueSaturation()
    {
        var r = R / 255;
        var g = G / 255;
        var b = B / 255;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;
        var lightness = (max + min) / 2;

        if (delta == 0)
        {
            return (0, 0);
        }

        var saturation = delta / (1 - Math.Abs((2 * lightness) - 1));
        double hue;

        if (max == r)
        {
            hue = 60 * (((g - b) / delta) % 6);
        }
        else if (max == g)
        {
            hue = 60 * (((b - r) / delta) + 2);
        }
        else
        {
            hue = 60 * (((r - g) / delta) + 4);
        }

        return ((hue + 360) % 360, saturation);
    }

    /// <summary>
    /// The relative luminance of WCAG 2.
    /// </summary>
    public double Luminance
        => (0.2126 * ChannelLuminance(R)) + (0.7152 * ChannelLuminance(G)) + (0.0722 * ChannelLuminance(B));

    /// <summary>
    /// The contrast ratio of WCAG 2 between two colors.
    /// </summary>
    public double ContrastWith(AdminColor other)
    {
        var a = Luminance;
        var b = other.Luminance;

        return (Math.Max(a, b) + 0.05) / (Math.Min(a, b) + 0.05);
    }

    /// <summary>
    /// The lowest contrast of this color against every background.
    /// </summary>
    public double MinContrast(IEnumerable<AdminColor> backgrounds)
    {
        var lowest = 21d;

        foreach (var background in backgrounds)
        {
            lowest = Math.Min(lowest, ContrastWith(background));
        }

        return lowest;
    }

    /// <summary>
    /// Sass' 'ensure-contrast': mixes this color with <paramref name="target"/>, 2% at a time, until it reaches
    /// <paramref name="minimum"/> against every background.
    /// </summary>
    public AdminColor EnsureContrast(IReadOnlyCollection<AdminColor> backgrounds, double minimum, AdminColor target)
    {
        var result = this;

        for (var step = 1; result.MinContrast(backgrounds) < minimum && step <= 50; step++)
        {
            result = target.Mix(this, step * 0.02);
        }

        return result;
    }

    private static double ChannelLuminance(double value)
    {
        var v = value / 255;

        return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
    }

    private static int Channel(double value)
        => (int)Math.Clamp(Math.Round(value, MidpointRounding.AwayFromZero), 0, 255);
}
