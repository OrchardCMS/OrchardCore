using OrchardCore.Security;

namespace OrchardCore.Roles;

public interface ISystemRoleProvider
{
    ValueTask<IEnumerable<IRole>> GetSystemRolesAsync();

    ValueTask<IRole> GetAdminRoleAsync();
}
