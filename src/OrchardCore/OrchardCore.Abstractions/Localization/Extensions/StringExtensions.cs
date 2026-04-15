namespace OrchardCore.Localization;

/// <summary>
/// 
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converts the non-Latin characters to their ASCII equivalents.
    /// </summary>
    public static string Transliterate(this string text) => AnyAscii.Transliteration.Transliterate(text);
}
