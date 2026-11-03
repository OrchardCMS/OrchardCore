using OrchardCore.Localization;

namespace OrchardCore.Contents;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Contents.ContentsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class ContentsLocalizationSources
{
    /// <summary>Gets the "All Content" text.</summary>
    public static readonly LocalizationSource AllContent = LocalizationSource.Create("All Content", typeof(ContentsLocalizationSources));

    /// <summary>Gets the "Content Types" text.</summary>
    public static readonly LocalizationSource ContentTypes = LocalizationSource.Create("Content Types", typeof(ContentsLocalizationSources));

    /// <summary>Gets the "Export Content To Deployment Target" text.</summary>
    public static readonly LocalizationSource ExportContentToDeploymentTarget = LocalizationSource.Create("Export Content To Deployment Target", typeof(ContentsLocalizationSources));
}
