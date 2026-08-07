using OrchardCore.Security.Permissions;

namespace OrchardCore.Settings;

/// <summary>
/// Contains the permissions that grant access to each site settings group.
/// </summary>
public sealed class SiteSettingsPermissionOptions
{
    /// <summary>
    /// Gets the permissions indexed by site settings group identifier.
    /// </summary>
    public IDictionary<string, IList<Permission>> GroupPermissions { get; } =
        new Dictionary<string, IList<Permission>>(StringComparer.OrdinalIgnoreCase);
}
