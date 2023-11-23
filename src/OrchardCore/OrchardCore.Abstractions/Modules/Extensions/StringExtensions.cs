using System;
using System.Linq;

namespace OrchardCore.Modules
{
    public static class StringExtensions
    {
        public static string ToHexString(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] ToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length).
                Where(x => 0 == x % 2).
                Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).
                ToArray();
        }

        public static bool EqualsOrdinalIgnoreCase(this string first, string second)
            => string.Equals(first, second, StringComparison.OrdinalIgnoreCase);

        public static bool StartsWithOrdinalIgnoreCase(this string first, string second)
            => first.StartsWith(second ?? "", StringComparison.OrdinalIgnoreCase);

        public static bool EndsWithOrdinalIgnoreCase(this string first, string second)
            => first.EndsWith(second, StringComparison.OrdinalIgnoreCase);
    }
}
