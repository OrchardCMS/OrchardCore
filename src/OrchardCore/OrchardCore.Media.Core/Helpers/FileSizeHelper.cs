using Microsoft.Extensions.Localization;

namespace OrchardCore.Media.Core.Helpers;

/// <summary>
/// Provides helper methods for formatting file sizes.
/// </summary>
public sealed class FileSizeHelper
{
    private static readonly string[] _sizeUnits = ["B", "KB", "MB", "GB", "TB", "PB", "EB"];

    private readonly IStringLocalizer<FileSizeHelper> S;

    public FileSizeHelper(IStringLocalizer<FileSizeHelper> localizer)
    {
        S = localizer;
    }

    /// <summary>
    /// Formats the given file size in bytes into a human-readable string with appropriate units.
    /// </summary>
    /// <param name="bytes">The file size in bytes.</param>
    /// <param name="decimalPlaces">The number of decimal places to include in the formatted string.</param>
    /// <returns>A human-readable string representing the file size.</returns>
    public string FormatSize(long bytes, int decimalPlaces = 2)
    {
        if (bytes < 0)
        {
            return "-" + FormatSize(-bytes, decimalPlaces);
        }

        if (bytes == 0)
        {
            return $"0 {S["Bytes"]}";
        }

        var magnitude = (int)Math.Log(bytes, 1024);
        var adjustedSize = bytes / Math.Pow(1024, magnitude);

        var unitKey = _sizeUnits[magnitude];

        // Format with decimals, then trim if unnecessary
        var formatted = adjustedSize.ToString($"N{decimalPlaces}");
        if (formatted.Contains('.') || formatted.Contains(','))
        {
            formatted = formatted.TrimEnd('0').TrimEnd('.', ',');
        }

        return $"{formatted} {S[unitKey]}";
    }
}
