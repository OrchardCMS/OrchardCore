using OrchardCore.Localization;

namespace OrchardCore.Users;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.Users.UsersLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class UsersLocalizationSources
{
    /// <summary>Gets the "Custom User Settings" text.</summary>
    public static readonly LocalizationSource CustomUserSettings = LocalizationSource.Create("Custom User Settings", typeof(UsersLocalizationSources));
}
