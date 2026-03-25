using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.Localization;

/// <summary>
/// Provides extension methods for <see cref="IOrchardHelper"/> related to JavaScript localization.
/// </summary>
#pragma warning disable CA1050 // Declare types in namespaces
public static class LocalizationOrchardHelperExtensions
#pragma warning restore CA1050 // Declare types in namespaces
{
    /// <summary>
    /// Returns a merged dictionary of JavaScript localizations for the specified groups by aggregating all
    /// registered <see cref="IJSLocalizer"/> implementations.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
    /// <param name="groups">
    /// One or more group identifiers used to scope the set of translation keys to return.
    /// Each <see cref="IJSLocalizer"/> implementation decides which groups it handles.
    /// </param>
    /// <returns>
    /// A merged dictionary of translation keys and their localized values.
    /// If multiple <see cref="IJSLocalizer"/> implementations provide a value for the same key, the last
    /// registered implementation wins.
    /// </returns>
    /// <example>
    /// In a Razor view or layout:
    /// <code>
    /// var localizations = Orchard.GetJSLocalizations("media-app");
    /// </code>
    /// </example>
    public static IDictionary<string, string> GetJSLocalizations(this IOrchardHelper orchardHelper, params string[] groups)
    {
        ArgumentNullException.ThrowIfNull(groups);

        var jsLocalizerServices = orchardHelper.HttpContext.RequestServices.GetServices<IJSLocalizer>();

        var result = new Dictionary<string, string>();

        foreach (var group in groups)
        {
            foreach (var jsLocalizerService in jsLocalizerServices)
            {
                var jsLocDict = jsLocalizerService.GetLocalizations(group);

                if (jsLocDict is null)
                {
                    continue;
                }

                foreach (var kvp in jsLocDict)
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
        }

        return result;
    }
}
