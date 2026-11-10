using OrchardCore.Localization;

namespace OrchardCore.Facebook;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Facebook.FacebookLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class FacebookLocalizationSources
{
    /// <summary>Gets the "Meta" text.</summary>
    public static readonly LocalizationSource Meta = LocalizationSource.Create("Meta", typeof(FacebookLocalizationSources));
}
