using System.Globalization;

namespace OrchardCore;

/// <summary>
/// Preserves text elements when shortening Unicode text.
/// </summary>
public static class TextHelper
{
    /// <summary>
    /// Returns the longest prefix of whole text elements within a UTF-16 length limit.
    /// </summary>
    public static string Truncate(string value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        return value is null || value.Length <= maxLength
            ? value
            : value[..GetTruncationLength(value.AsSpan(), maxLength)];
    }

    /// <summary>
    /// Returns the UTF-16 length of the longest prefix of whole text elements within the limit.
    /// </summary>
    public static int GetTruncationLength(ReadOnlySpan<char> value, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        if (value.Length <= maxLength)
        {
            return value.Length;
        }

        var length = 0;
        while (length < maxLength)
        {
            var nextLength = StringInfo.GetNextTextElementLength(value[length..]);
            if (nextLength > maxLength - length)
            {
                break;
            }

            length += nextLength;
        }

        return length;
    }
}
