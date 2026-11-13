namespace OrchardCore.Localization;

/// <summary>
/// Shared helpers for the context-free fallback of the <c>Plural</c> factory extensions.
/// </summary>
internal static class LocalizationSourcePluralHelper
{
    /// <summary>
    /// Selects the singular or plural text for a context-free <see cref="LocalizationSource"/>, and prepends
    /// <paramref name="count"/> to the format arguments, matching the convention used by the context-bearing
    /// <c>IStringLocalizer.Plural</c>/<c>IHtmlLocalizer.Plural</c> extensions.
    /// </summary>
    public static (string Text, object[] Arguments) SelectContextFreeForm(LocalizationSource source, int count, string plural, object[] arguments)
    {
        var text = count == 1 ? source.Value : plural;

        return (text, PrependCount(count, arguments));
    }

    private static object[] PrependCount(int count, object[] arguments)
    {
        if (arguments is not { Length: > 0 })
        {
            return [count];
        }

        var argumentsWithCount = new object[arguments.Length + 1];
        argumentsWithCount[0] = count;
        Array.Copy(arguments, 0, argumentsWithCount, 1, arguments.Length);

        return argumentsWithCount;
    }
}
