using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Data;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Services;

/// <summary>
/// Provides permission descriptions as translatable strings for data localization.
/// </summary>
public class PermissionsLocalizationDataProvider : ILocalizationDataProvider
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;
    private readonly ITypeFeatureProvider _typeFeatureProvider;
    private readonly IShellFeaturesManager _shellFeaturesManager;

    private const string PermissionsContext = "Permissions";

    public PermissionsLocalizationDataProvider(
        IEnumerable<IPermissionProvider> permissionProviders,
        ITypeFeatureProvider typeFeatureProvider,
        IShellFeaturesManager shellFeaturesManager)
    {
        _permissionProviders = permissionProviders;
        _typeFeatureProvider = typeFeatureProvider;
        _shellFeaturesManager = shellFeaturesManager;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var descriptors = new List<DataLocalizedString>();
        var seenDescriptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var enabledFeatures = await _shellFeaturesManager.GetEnabledFeaturesAsync();

        foreach (var provider in _permissionProviders)
        {
            // Get the feature associated with this permission provider.
            var feature = _typeFeatureProvider
                .GetFeaturesForDependency(provider.GetType())
                .LastOrDefault(f => enabledFeatures.Any(ef => f.Id == ef.Id));

            var permissions = await provider.GetPermissionsAsync();

            foreach (var permission in permissions)
            {
                // Skip permissions without descriptions or with template placeholders.
                if (string.IsNullOrWhiteSpace(permission.Description) || permission.Description.Contains("{0}"))
                {
                    continue;
                }

                // Determine the group name: use category if present, otherwise use feature name/id.
                var groupName = GetGroupName(feature, permission.Category);

                // Create a unique key for deduplication that includes the group.
                var descriptionKey = $"{groupName}|{permission.Description}";

                // Avoid duplicates (same description in same group).
                if (!seenDescriptions.Add(descriptionKey))
                {
                    continue;
                }

                var context = string.IsNullOrWhiteSpace(groupName)
                    ? PermissionsContext
                    : PermissionsContext + Constants.ContextSeparator + groupName;

                descriptors.Add(new DataLocalizedString(context, permission.Description, string.Empty));

                // Also add category if present and not a template.
                if (!string.IsNullOrWhiteSpace(permission.Category) && !permission.Category.Contains("{0}"))
                {
                    var categoryKey = $"{groupName}|{permission.Category}";

                    if (seenDescriptions.Add(categoryKey))
                    {
                        descriptors.Add(new DataLocalizedString(context, permission.Category, string.Empty));
                    }
                }
            }
        }

        return descriptors;
    }

    private static string GetGroupName(IFeatureInfo feature, string category)
    {
        if (!string.IsNullOrWhiteSpace(category))
        {
            return category;
        }

        if (feature is null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(feature.Name)
            ? feature.Id
            : feature.Name;
    }
}
