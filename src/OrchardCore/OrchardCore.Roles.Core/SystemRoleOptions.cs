using OrchardCore.Security;

namespace OrchardCore.Roles;

public class SystemRoleOptions
{
    /// <summary>
    /// The name of the admin role the system should use. If not specified, it defaults to <see cref="OrchardCoreConstants.Roles.Administrator"/>. This allows for customization of the admin role name.
    /// </summary>
    public string SystemAdminRoleName { get; set; }

    /// <summary>
    /// Gets the additional system roles. This allows for defining custom system roles beyond the default ones (Administrator, Authenticated, Anonymous).
    /// The roles defined here will be included in the system roles provided by the application.
    /// </summary>
    public IDictionary<string, IRole> AdditionalSystemRoles { get; } = new Dictionary<string, IRole>(StringComparer.OrdinalIgnoreCase);
}
