using OrchardCore.Localization;

namespace OrchardCore.ContentTypes;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.ContentTypes.ContentTypesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class ContentTypesLocalizationSources
{
    /// <summary>Gets the "Replace Content Definitions" text.</summary>
    public static readonly LocalizationSource ReplaceContentDefinitions = LocalizationSource.Create("Replace Content Definitions", typeof(ContentTypesLocalizationSources));

    /// <summary>Gets the "Update Content Definitions" text.</summary>
    public static readonly LocalizationSource UpdateContentDefinitions = LocalizationSource.Create("Update Content Definitions", typeof(ContentTypesLocalizationSources));
}
