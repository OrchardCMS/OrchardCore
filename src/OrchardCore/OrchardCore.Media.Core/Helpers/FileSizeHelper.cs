using Microsoft.Extensions.Localization;

namespace OrchardCore.Media.Core.Helpers;

/// <summary>
/// Provides helper methods for formatting file sizes.
/// </summary>
public sealed class FileSizeHelper
{
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
            return S["{0} KB", 0];
        }

        var magnitude = (int)Math.Log(bytes, 1024);
        var adjustedSize = bytes / Math.Pow(1024, magnitude);

        return magnitude switch
        {
            0 or 1 => S["{0} B", adjustedSize],
            2 => S["{0} KB", adjustedSize],
            3 => S["{0} MB", adjustedSize],
            4 => S["{0} GB", adjustedSize],
            5 => S["{0} TB", adjustedSize],
            6 => S["{0} PB", adjustedSize],
            _ => S["{0} EB", adjustedSize]
        };
    }
}
