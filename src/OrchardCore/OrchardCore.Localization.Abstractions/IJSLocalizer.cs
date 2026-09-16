namespace OrchardCore.Localization;

/// <summary>
/// Provides JavaScript localizations for a specific group.
/// </summary>
/// <remarks>
/// Implement this interface and register it with <c>services.AddScoped&lt;IJSLocalizer, YourJSLocalizer&gt;()</c>
/// to supply JavaScript-side translations from PO files. Multiple implementations may be registered and their
/// results are merged by <see cref="LocalizationOrchardHelperExtensions.GetJSLocalizations"/>.
/// </remarks>
public interface IJSLocalizer
{
    /// <summary>
    /// Returns a dictionary of localized strings for the specified group, or <see langword="null"/> if this
    /// implementation does not handle the requested group.
    /// </summary>
    /// <param name="group">The group identifier that scopes the set of keys to return (e.g., <c>"media-gallery"</c>).</param>
    /// <returns>
    /// A dictionary mapping translation keys to their localized values, or <see langword="null"/> when this
    /// implementation has no localizations for the given group.
    /// </returns>
    IDictionary<string, string> GetLocalizations(string group);
}
