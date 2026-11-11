using OrchardCore.Localization;

namespace OrchardCore.ArchiveLater;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.ArchiveLater.ArchiveLaterLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class ArchiveLaterLocalizationSources
{
    /// <summary>Gets the title of the job that archives a scheduled content item.</summary>
    public static readonly LocalizationSource JobTitle = LocalizationSource.Create("Archive Later", typeof(ArchiveLaterLocalizationSources));

    /// <summary>Gets the description of the job that archives a scheduled content item.</summary>
    public static readonly LocalizationSource JobDescription = LocalizationSource.Create("Archives a content item whose scheduled archive date and time arrived.", typeof(ArchiveLaterLocalizationSources));
}
