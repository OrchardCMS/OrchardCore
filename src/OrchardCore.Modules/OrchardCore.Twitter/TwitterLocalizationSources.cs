using OrchardCore.Localization;

namespace OrchardCore.Twitter;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Twitter.TwitterLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class TwitterLocalizationSources
{
    /// <summary>Gets the "Social" activity category.</summary>
    public static readonly LocalizationSource Social = LocalizationSource.Create("Social", typeof(TwitterLocalizationSources));
}
