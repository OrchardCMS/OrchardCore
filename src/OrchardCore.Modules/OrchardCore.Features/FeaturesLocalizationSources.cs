using OrchardCore.Localization;

namespace OrchardCore.Features;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Features.FeaturesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class FeaturesLocalizationSources
{
    /// <summary>Gets the "All Features" text.</summary>
    public static readonly LocalizationSource AllFeatures = LocalizationSource.Create("All Features", typeof(FeaturesLocalizationSources));
}
