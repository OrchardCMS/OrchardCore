using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;

namespace OrchardCore.Roles;

internal sealed class DefaultSystemRoleProvider : ISystemRoleProvider
{
    private readonly IEnumerable<IRole> _systemRoles;

    public DefaultSystemRoleProvider(ShellSettings shellSettings, IOptions<SystemRoleOptions> options)
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

        _systemRoles = [
            new Role
            {
                RoleName = adminRoleName,
                RoleDescription = "A system role that grants all permissions to the assigned users."
            },
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Authenticated,
                RoleDescription = "A system role representing all authenticated users."
            },
            new Role
            {
                RoleName = OrchardCoreConstants.Roles.Anonymous,
                RoleDescription = "A system role representing all non-authenticated users."
            }
        ];
    }

    public ValueTask<IEnumerable<IRole>> GetSystemRolesAsync()
        => ValueTask.FromResult(_systemRoles);
}
