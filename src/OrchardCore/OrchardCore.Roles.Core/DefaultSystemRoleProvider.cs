using System.Collections.Frozen;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;

namespace OrchardCore.Roles;

public sealed class DefaultSystemRoleProvider : ISystemRoleProvider
{
    private readonly FrozenDictionary<string, Role> _systemRoles;
    private readonly Role _adminRole;

    public DefaultSystemRoleProvider(
        ShellSettings shellSettings,
        IOptions<SystemRoleOptions> options,
        IStringLocalizer<DefaultSystemRoleProvider> S)
    {
        var adminRoleName = shellSettings["AdminRoleName"];
        if (string.IsNullOrWhiteSpace(adminRoleName))
        {
            adminRoleName = options.Value.SystemAdminRoleName;
        }

        if (string.IsNullOrWhiteSpace(adminRoleName))
        {
            adminRoleName = OrchardCoreConstants.Roles.Administrator;
        }

        _adminRole = new Role
        {
            RoleName = adminRoleName,
            RoleDescription = S["A system role that grants all permissions to the assigned users."]
        };

        _systemRoles = new Dictionary<string, Role>()
        {
            { _adminRole.RoleName, _adminRole },
            {
                OrchardCoreConstants.Roles.Authenticated, new Role
                {
                    RoleName = OrchardCoreConstants.Roles.Authenticated,
                    RoleDescription = S["A system role representing all authenticated users."]
                }
            },
            {
                OrchardCoreConstants.Roles.Anonymous, new Role
                {
                    RoleName = OrchardCoreConstants.Roles.Anonymous,
                    RoleDescription = S["A system role representing all non-authenticated users."]
                }
            }
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<IRole> GetSystemRoles()
        => _systemRoles.Values;

    public IRole GetAdminRole() => _adminRole;

    public bool IsSystemRole(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return _systemRoles.ContainsKey(name);
    }
}
