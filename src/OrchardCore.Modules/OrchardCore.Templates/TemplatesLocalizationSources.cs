using OrchardCore.Localization;

namespace OrchardCore.Templates;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Templates.TemplatesLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class TemplatesLocalizationSources
{
    /// <summary>Gets the "All Admin Templates" text.</summary>
    public static readonly LocalizationSource AllAdminTemplates = LocalizationSource.Create("All Admin Templates", typeof(TemplatesLocalizationSources));

    /// <summary>Gets the "All Templates" text.</summary>
    public static readonly LocalizationSource AllTemplates = LocalizationSource.Create("All Templates", typeof(TemplatesLocalizationSources));
}
