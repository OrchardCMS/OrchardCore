using OrchardCore.Security;

namespace OrchardCore.Roles;

public interface ISystemRoleProvider
{
    IEnumerable<IRole> GetSystemRoles();

    IRole GetAdminRole();

    bool IsSystemRole(string name);
}
