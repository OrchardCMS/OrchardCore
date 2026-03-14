using System.Collections.Generic;

namespace OrchardCore.Localization;

/// <summary>
/// A no-op implementation of <see cref="IJSLocalizer"/> that always returns an empty dictionary.
/// Register this as the default when a module has no JavaScript-specific localizations of its own.
/// </summary>
public sealed class NullJSLocalizer : IJSLocalizer
{
    /// <summary>
    /// Gets the singleton instance of <see cref="NullJSLocalizer"/>.
    /// </summary>
    public static readonly NullJSLocalizer Instance = new();

    /// <inheritdoc/>
    public Dictionary<string, string> GetLocalizations(string[] groups) => [];
}
