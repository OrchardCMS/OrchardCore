namespace OrchardCore.Modules;

public static class LongExtensions
{
    private static string[] _fileSizeUnits = ["KB", "MB", "GB", "TB", "PB"];
    
    /// <summary>
    /// Formats <paramref name="bytes"/> as text, matching the closest byte unit. For example "1024" becomes "1 KB". 
    /// </summary>
    public static string FormatAsBytes(this long bytes)
    {
        var result = _fileSizeUnits.Aggregate(
            new { Value = (decimal)bytes, Unit = "B" },
            (item, unit) => item.Value > 1024 ? new { Value = item.Value / 1024, Unit = unit } : item);

        return $"{Math.Floor(result.Value * 100) / 100} {result.Unit}";
    }
}
