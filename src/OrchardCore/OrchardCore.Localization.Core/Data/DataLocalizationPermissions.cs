using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization.Data;

public class DataLocalizationPermissions
{
    private static readonly Dictionary<string, Permission> _culturePermissions = [];
    private static readonly Permission _manageTranslationsForCulture =
        new("ManageTranslations_{0}", "Manage {0} translations", [ManageTranslations, ViewDynamicTranslations]);

    /// <summary>
    /// Read-only permission to view translations and statistics.
    /// </summary>
    public static readonly Permission ViewDynamicTranslations =
        new("ViewDynamicTranslations", "View dynamic translations and statistics");

    /// <summary>
    /// Permission to manage all dynamic translations.
    /// Implies <see cref="ViewDynamicTranslations"/>.
    /// </summary>
    public static readonly Permission ManageTranslations =
        new("ManageTranslations", "Manage all dynamic translations", [ViewDynamicTranslations]);

    /// <summary>
    /// Legacy permission for managing dynamic localizations.
    /// Kept for backward compatibility; use <see cref="ManageTranslations"/> instead.
    /// </summary>
    public static readonly Permission ManageLocalization =
        new("ManageLocalization", "Manage dynamic localizations", [ManageTranslations]);

    /// <summary>
    /// Creates a dynamic permission for managing translations in a specific culture.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "fr-FR").</param>
    /// <param name="cultureDisplayName">The display name of the culture (e.g., "French (France)").</param>
    /// <returns>A permission for managing translations in the specified culture.</returns>
    public static Permission CreateCulturePermission(string cultureName, string cultureDisplayName)
    {
        ArgumentException.ThrowIfNullOrEmpty(cultureName);

        if (_culturePermissions.TryGetValue(cultureName, out var existingPermission))
        {
            return existingPermission;
        }

        var permission = new Permission(
            string.Format(_manageTranslationsForCulture.Name, cultureName),
            string.Format(_manageTranslationsForCulture.Description, cultureDisplayName),
            _manageTranslationsForCulture.ImpliedBy)
        {
            Category = "Data Localization",
        };

        _culturePermissions[cultureName] = permission;

        return permission;
    }
}
