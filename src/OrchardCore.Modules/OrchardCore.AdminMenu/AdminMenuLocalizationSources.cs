using OrchardCore.Localization;

namespace OrchardCore.AdminMenu;

/// <summary>
/// Untranslated strings of this module. Their PO context is <c>OrchardCore.AdminMenu.AdminMenuLocalizationSources</c>.
/// </summary>
/// <remarks>
/// Use the sources of <see cref="CommonLocalizationSources"/> for the strings that have the same meaning across modules.
/// </remarks>
internal static class AdminMenuLocalizationSources
{
    /// <summary>Gets the "Admin Menus" text.</summary>
    public static readonly LocalizationSource AdminMenus = LocalizationSource.Create("Admin Menus", typeof(AdminMenuLocalizationSources));
}
