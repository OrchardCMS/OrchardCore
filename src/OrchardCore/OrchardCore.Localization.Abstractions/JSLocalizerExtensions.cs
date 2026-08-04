namespace OrchardCore.Localization;

/// <summary>
/// Provides extension methods for <see cref="IJSLocalizer"/> implementations.
/// </summary>
public static class JSLocalizerExtensions
{
    /// <summary>
    /// Returns a merged dictionary of JavaScript localizations for the specified groups by aggregating the
    /// given <see cref="IJSLocalizer"/> implementations. Implementations returning <see langword="null"/> for
    /// a group are skipped; when several provide a value for the same key, the last one wins.
    /// </summary>
    /// <param name="jsLocalizers">The registered <see cref="IJSLocalizer"/> implementations.</param>
    /// <param name="groups">One or more group identifiers scoping the set of translation keys to return.</param>
    /// <returns>A merged dictionary of translation keys and their localized values.</returns>
    public static Dictionary<string, string> GetMergedLocalizations(this IEnumerable<IJSLocalizer> jsLocalizers, params string[] groups)
    {
        ArgumentNullException.ThrowIfNull(jsLocalizers);
        ArgumentNullException.ThrowIfNull(groups);

        var result = new Dictionary<string, string>();

        foreach (var group in groups)
        {
            foreach (var jsLocalizer in jsLocalizers)
            {
                var localizations = jsLocalizer.GetLocalizations(group);

                if (localizations is null)
                {
                    continue;
                }

                foreach (var kvp in localizations)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        return result;
    }
}
