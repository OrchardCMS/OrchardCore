using OrchardCore.Localization;

namespace OrchardCore.Settings;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Settings.SettingsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class SettingsLocalizationSources
{
    /// <summary>Gets the "Site Settings" text.</summary>
    public static readonly LocalizationSource SiteSettings = LocalizationSource.Create("Site Settings", typeof(SettingsLocalizationSources));
}
