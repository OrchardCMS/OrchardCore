using Microsoft.AspNetCore.Authorization;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security;

public class PermissionRequirement : IAuthorizationRequirement
{
    public PermissionRequirement(Permission permission)
    {
        ArgumentNullException.ThrowIfNull(permission);

        Permission = permission;
    }

    public Permission Permission { get; set; }
}
