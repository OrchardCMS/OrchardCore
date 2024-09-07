#nullable enable

namespace OrchardCore.Modules;

public static class StringExtensions
{
    public static byte[] ToByteArray(this string hex)
    {
        return Enumerable.Range(0, hex.Length).
            Where(x => 0 == x % 2).
            Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
            ToArray();
    }

    public static bool EqualsOrdinalIgnoreCase(this string? a, string? b)
        => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    public static bool StartsWithOrdinalIgnoreCase(this string? a, string? b) =>
        a is not null && b is not null && a.StartsWith(b, StringComparison.OrdinalIgnoreCase);

    public static bool EndsWithOrdinalIgnoreCase(this string? a, string? b) =>
        a is not null && b is not null && a.EndsWith(b, StringComparison.OrdinalIgnoreCase);
}
