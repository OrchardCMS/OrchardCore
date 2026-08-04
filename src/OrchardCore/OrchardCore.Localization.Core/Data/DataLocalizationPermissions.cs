using OrchardCore.Security.Permissions;

namespace OrchardCore.Localization.Data;

public class DataLocalizationPermissions
{
    private static readonly Dictionary<string, Permission> _culturePermissions = [];

    /// <summary>
    /// Permission to manage all dynamic translations.
    /// </summary>
    public static readonly Permission ManageTranslations =
        new("ManageTranslations", "Manage all dynamic translations");

    /// <summary>
    /// Read-only permission to view translations and statistics.
    /// Implied by <see cref="ManageTranslations"/>.
    /// </summary>
    public static readonly Permission ViewDynamicTranslations =
        new("ViewDynamicTranslations", "View dynamic translations and statistics", [ManageTranslations]);

    /// <summary>
    /// Legacy permission for managing dynamic localizations.
    /// Kept for backward compatibility; use <see cref="ManageTranslations"/> instead.
    /// </summary>
    public static readonly Permission ManageLocalization =
        new("ManageLocalization", "Manage dynamic localizations", [ManageTranslations]);

    // Declared after ManageTranslations so its ImpliedBy list captures the real instance
    // rather than the default null a forward reference would read from a static field
    // initializer that hasn't run yet.
    private static readonly Permission _manageTranslationsForCulture =
        new("ManageTranslations_{0}", "Manage {0} translations", [ManageTranslations]);

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
