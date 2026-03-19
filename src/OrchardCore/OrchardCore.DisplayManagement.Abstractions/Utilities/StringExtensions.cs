using System.Buffers;

namespace OrchardCore.DisplayManagement.Utilities;

public static class StringExtensions
{
    private static readonly SearchValues<char> AlternateChars = SearchValues.Create("-.");

    /// <summary>
    /// Encodes dashed and dots so that they don't conflict in filenames.
    /// </summary>
    /// <param name="alternateElement"></param>
    /// <returns></returns>
    /// <remarks>
    /// Replaces '-' with "__" and '.' with '_'.
    /// </remarks>
    public static string EncodeAlternateElement(this string alternateElement)
    {
        if (string.IsNullOrEmpty(alternateElement))
        {
            return "";
        }

        var span = alternateElement.AsSpan();

        // Fast path: no '-' or '.'
        if (span.IndexOfAny(AlternateChars) < 0)
        {
            return alternateElement;
        }

        // Count dashes. If none, only dots are present.
        var dashCount = span.Count('-');
        if (dashCount == 0)
        {
            // Only '.' present; use optimized single-pass replace.
            return alternateElement.Replace('.', '_');
        }

        // Allocate once and write result in a single pass.
        var newLength = alternateElement.Length + dashCount;
        return string.Create(newLength, alternateElement, (dest, src) =>
        {
            var s = src.AsSpan();
            var di = 0;

            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c == '-')
                {
                    dest[di++] = '_';
                    dest[di++] = '_';
                }
                else if (c == '.')
                {
                    dest[di++] = '_';
                }
                else
                {
                    dest[di++] = c;
                }
            }
        });
    }
}
