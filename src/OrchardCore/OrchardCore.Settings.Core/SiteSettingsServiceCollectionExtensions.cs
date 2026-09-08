using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extensions for registering site settings permissions.
/// </summary>
public static class SiteSettingsServiceCollectionExtensions
{
    /// <summary>
    /// Registers a permission that grants access to a site settings group.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="groupId">The site settings group identifier.</param>
    /// <param name="permission">The permission that grants access to the group.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddSiteSettingsPermission(
        this IServiceCollection services,
        string groupId,
        Permission permission)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupId);
        ArgumentNullException.ThrowIfNull(permission);

        services.Configure<SiteSettingsPermissionOptions>(options =>
        {
            if (!options.GroupPermissions.TryGetValue(groupId, out var permissions))
            {
                permissions = [];
                options.GroupPermissions[groupId] = permissions;
            }

            if (!permissions.Any(existing => existing.Name.Equals(permission.Name, StringComparison.OrdinalIgnoreCase)))
            {
                permissions.Add(permission);
            }
        });

        return services;
    }
}
