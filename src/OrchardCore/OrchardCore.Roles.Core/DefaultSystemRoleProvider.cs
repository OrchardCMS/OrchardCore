using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;

namespace OrchardCore.Roles;

public sealed class DefaultSystemRoleProvider : ISystemRoleProvider
{
    private readonly IEnumerable<IRole> _systemRoles;
    private readonly IRole _adminRole;
    private readonly IStringLocalizer S;

    public DefaultSystemRoleProvider(
        ShellSettings shellSettings,
        IStringLocalizer<DefaultSystemRoleProvider> localizer,
        IOptions<SystemRoleOptions> options)
    {
        S = localizer;

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

        _systemRoles = [
            _adminRole,
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Authenticated,
                RoleDescription = S["A system role representing all authenticated users."]
            },
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Anonymous,
                RoleDescription = S["A system role representing all non-authenticated users."]
            }
        ];
    }

    public ValueTask<IEnumerable<IRole>> GetSystemRolesAsync()
        => ValueTask.FromResult(_systemRoles);

    public ValueTask<IRole> GetAdminRoleAsync()
        => ValueTask.FromResult(_adminRole);
}
