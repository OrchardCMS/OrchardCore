using OrchardCore.Localization.Data;
using OrchardCore.Security.Permissions;

namespace OrchardCore.DataLocalization.Services;

/// <summary>
/// Provides permission descriptions as translatable strings for data localization.
/// </summary>
public class PermissionsLocalizationDataProvider : ILocalizationDataProvider
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;

    private const string PermissionsContext = "Permissions";

    public PermissionsLocalizationDataProvider(IEnumerable<IPermissionProvider> permissionProviders)
    {
        _permissionProviders = permissionProviders;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var descriptors = new List<DataLocalizedString>();
        var seenDescriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in _permissionProviders)
        {
            var permissions = await provider.GetPermissionsAsync();

            foreach (var permission in permissions)
            {
                // Skip permissions without descriptions or with template placeholders.
                if (string.IsNullOrWhiteSpace(permission.Description) ||
                    permission.Description.Contains("{0}"))
                {
                    continue;
                }

                // Avoid duplicates (same description from multiple providers).
                if (!seenDescriptions.Add(permission.Description))
                {
                    continue;
                }

                descriptors.Add(new DataLocalizedString(
                    PermissionsContext,
                    permission.Description,
                    string.Empty));

                // Also add category if present and not a template.
                if (!string.IsNullOrWhiteSpace(permission.Category) &&
                    !permission.Category.Contains("{0}") &&
                    seenDescriptions.Add(permission.Category))
                {
                    descriptors.Add(new DataLocalizedString(
                        PermissionsContext,
                        permission.Category,
                        string.Empty));
                }
            }
        }

        return descriptors;
    }
}
