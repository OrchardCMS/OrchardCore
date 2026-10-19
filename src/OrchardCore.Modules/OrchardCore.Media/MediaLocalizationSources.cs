using OrchardCore.Localization;

namespace OrchardCore.Media;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Media.MediaLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class MediaLocalizationSources
{
    /// <summary>Gets the "Media" text.</summary>
    public static readonly LocalizationSource Media = LocalizationSource.Create("Media", typeof(MediaLocalizationSources));
}
