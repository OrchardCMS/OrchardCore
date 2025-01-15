using System.Collections.Frozen;

namespace OrchardCore.Roles;

[Obsolete("This interface has been deprecated, use ISystemRoleNameProvider instead.")]
public interface ISystemRoleNameProvider
{
    ValueTask<string> GetAdminRoleAsync();

    ValueTask<FrozenSet<string>> GetSystemRolesAsync();
}
