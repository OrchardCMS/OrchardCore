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
    public static string EncodeAlternateElement(this string alternateElement)
    {
        if (string.IsNullOrEmpty(alternateElement))
        {
            return "";
        }

        if (!alternateElement.AsSpan().ContainsAny(AlternateChars))
        {
            return alternateElement;
        }

        return alternateElement.Replace("-", "__").Replace('.', '_');
    }
}
