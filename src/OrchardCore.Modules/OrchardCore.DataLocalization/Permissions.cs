using System.Globalization;
using OrchardCore.Localization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataLocalization;

/// <summary>
/// Provides permissions for the Data Localization module.
/// </summary>
public sealed class Permissions : IPermissionProvider
{
    /// <summary>
    /// Read-only permission to view translations and statistics.
    /// </summary>
    public static readonly Permission ViewTranslations =
        new("ViewTranslations", "View dynamic translations and statistics");

    /// <summary>
    /// Permission to manage all dynamic translations.
    /// Implies <see cref="ViewTranslations"/>.
    /// </summary>
    public static readonly Permission ManageTranslations =
        new("ManageTranslations", "Manage all dynamic translations", [ViewTranslations]);

    /// <summary>
    /// Legacy permission for managing dynamic localizations.
    /// Kept for backward compatibility; use <see cref="ManageTranslations"/> instead.
    /// </summary>
    public static readonly Permission ManageLocalization =
        new("ManageLocalization", "Manage dynamic localizations", [ManageTranslations]);

    /// <summary>
    /// Template permission for culture-specific translation management.
    /// </summary>
    private static readonly Permission _manageTranslationsForCulture =
        new("ManageTranslations_{0}", "Manage {0} translations", [ManageTranslations, ViewTranslations]);

    private static readonly Dictionary<string, Permission> _culturePermissions = [];

    private readonly ILocalizationService _localizationService;

    public Permissions(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

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
            _manageTranslationsForCulture.ImpliedBy
        )
        {
            Category = "Data Localization",
        };

        _culturePermissions[cultureName] = permission;

        return permission;
    }

    /// <summary>
    /// Gets the permission name for a specific culture.
    /// </summary>
    /// <param name="cultureName">The culture name (e.g., "fr-FR").</param>
    /// <returns>The permission name (e.g., "ManageTranslations_fr-FR").</returns>
    public static string GetCulturePermissionName(string cultureName)
        => string.Format(_manageTranslationsForCulture.Name, cultureName);

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        var permissions = new List<Permission>
        {
            ViewTranslations,
            ManageTranslations,
            ManageLocalization,
        };

        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();

        foreach (var cultureName in supportedCultures)
        {
            var cultureInfo = CultureInfo.GetCultureInfo(cultureName);
            var displayName = !string.IsNullOrEmpty(cultureInfo.DisplayName)
                ? cultureInfo.DisplayName
                : cultureInfo.NativeName;

            permissions.Add(CreateCulturePermission(cultureName, displayName));
        }

        return permissions;
    }

    public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
    [
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Administrator,
            Permissions = [ManageTranslations],
        },
        new PermissionStereotype
        {
            Name = OrchardCoreConstants.Roles.Editor,
            Permissions = [ViewTranslations],
        },
    ];
}
