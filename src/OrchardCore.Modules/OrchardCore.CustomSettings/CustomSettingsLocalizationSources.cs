using OrchardCore.Localization;

namespace OrchardCore.CustomSettings;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.CustomSettings.CustomSettingsLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class CustomSettingsLocalizationSources
{
    /// <summary>Gets the "Custom Settings" text.</summary>
    public static readonly LocalizationSource CustomSettings = LocalizationSource.Create("Custom Settings", typeof(CustomSettingsLocalizationSources));
}
