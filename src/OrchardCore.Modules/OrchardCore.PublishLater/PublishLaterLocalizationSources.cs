using OrchardCore.Localization;

namespace OrchardCore.PublishLater;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.PublishLater.PublishLaterLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class PublishLaterLocalizationSources
{
    /// <summary>Gets the title of the job that publishes a scheduled content item.</summary>
    public static readonly LocalizationSource JobTitle = LocalizationSource.Create("Publish Later", typeof(PublishLaterLocalizationSources));

    /// <summary>Gets the description of the job that publishes a scheduled content item.</summary>
    public static readonly LocalizationSource JobDescription = LocalizationSource.Create("Publishes a content item whose scheduled publish date and time arrived.", typeof(PublishLaterLocalizationSources));
}
