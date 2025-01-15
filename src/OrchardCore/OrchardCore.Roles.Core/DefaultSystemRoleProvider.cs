using OrchardCore.Environment.Shell;
using OrchardCore.Security;

namespace OrchardCore.Roles;

public class DefaultSystemRoleProvider : ISystemRoleProvider
{
    private readonly IEnumerable<IRole> _systemRoles;

    public DefaultSystemRoleProvider(ShellSettings shellSettings)
    {
        _systemRoles = [
            new Role
            {
                RoleName = string.IsNullOrEmpty(shellSettings["AdminRoleName"])
                    ? OrchardCoreConstants.Roles.Administrator
                    : shellSettings["AdminRoleName"],
                RoleDescription = "A system role that grants all permissions to the assigned users."
            },
            new Role
            {
                RoleName = "Authenticated",
                RoleDescription = "A system role representing all authenticated users."
            },
            new Role
            {
                RoleName = "Anonymous",
                RoleDescription = "A system role representing all non-authenticated users."
            }
        ];
    }

    public ValueTask<IEnumerable<IRole>> GetSystemRolesAsync()
        => ValueTask.FromResult(_systemRoles);
}
