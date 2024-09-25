using OrchardCore.Infrastructure.Security;

namespace OrchardCore.Security.Services;

public static class RoleHelper
{
    public static readonly HashSet<string> SystemRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        OrchardCoreConstants.Roles.Anonymous,
        OrchardCoreConstants.Roles.Authenticated,
    };

    public static RoleType GetRoleType(string roleName)
    {
        ArgumentException.ThrowIfNullOrEmpty(roleName);

        if (SystemRoleNames.Contains(roleName))
        {
            return RoleType.System;
        }

        return RoleType.Standard;
    }

    public static RoleType GetRoleType(string roleName, bool isOwnerType)
    {
        var roleType = GetRoleType(roleName);

        if (!roleType.HasFlag(RoleType.System) && isOwnerType)
        {
            roleType |= RoleType.Owner;
        }

        return roleType;
    }
}
