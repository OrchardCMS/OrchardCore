namespace OrchardCore.Media.Core.Helpers;

public static class FileSizeHelpers
{
    private static string[] _fileSizeUnits = ["KB", "MB", "GB", "TB", "PB"];
    
    /// <summary>
    /// Formats <paramref name="bytes"/> as text, matching the closest byte unit. For example "1050" becomes "1.02 KB". 
    /// </summary>
    /// <param name="bytes">The number of bytes to format.</param>
    /// <param name="digits">The number of digits that may be displayed after the decimal point.</param>
    public static string FormatAsBytes(long bytes, int digits = 2)
    {
        var result = _fileSizeUnits.Aggregate(
            new { Value = (decimal)bytes, Unit = "B" },
            (item, unit) => item.Value > 1024 ? new { Value = item.Value / 1024, Unit = unit } : item);

        var multiplier = (decimal)Math.Pow(10, digits); 

        return $"{Math.Floor(result.Value * multiplier) / multiplier} {result.Unit}";
    }
}
