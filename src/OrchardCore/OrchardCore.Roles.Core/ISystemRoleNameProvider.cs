using System.Collections.Frozen;

namespace OrchardCore.Roles;

public interface ISystemRoleNameProvider
{
    ValueTask<string> GetAdminRoleAsync();

    ValueTask<FrozenSet<string>> GetSystemRolesAsync();
}
