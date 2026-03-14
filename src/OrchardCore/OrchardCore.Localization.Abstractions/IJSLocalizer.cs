using System.Collections.Generic;

namespace OrchardCore.Localization;

/// <summary>
/// Provides JavaScript localizations for a specific set of groups.
/// </summary>
/// <remarks>
/// Implement this interface and register it with <c>services.AddScoped&lt;IJSLocalizer, YourJSLocalizer&gt;()</c>
/// to supply JavaScript-side translations from PO files. Multiple implementations may be registered and their
/// results are merged by <see cref="LocalizationOrchardHelperExtensions.GetJSLocalizations"/>.
/// </remarks>
public interface IJSLocalizer
{
    /// <summary>
    /// Returns a dictionary of localized strings for the specified groups, or <see langword="null"/> if this
    /// implementation does not handle any of the requested groups.
    /// </summary>
    /// <param name="groups">
    /// One or more group identifiers that scope the set of keys to return (e.g., <c>"media-app"</c>).
    /// </param>
    /// <returns>
    /// A dictionary mapping translation keys to their localized values, or <see langword="null"/> when this
    /// implementation has no localizations for the given groups.
    /// </returns>
    Dictionary<string, string> GetLocalizations(string[] groups);
}
