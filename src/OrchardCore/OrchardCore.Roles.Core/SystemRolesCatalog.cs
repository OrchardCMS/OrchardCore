using System.Collections.Frozen;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles;

public sealed class SystemRolesCatalog
{
    public readonly string AdminRoleName;

    public readonly FrozenSet<string> SystemRoleNames;

    public SystemRolesCatalog(ShellSettings shellSettings)
    {
        AdminRoleName = shellSettings["AdminRoleName"];

        if (string.IsNullOrWhiteSpace(AdminRoleName))
        {
            AdminRoleName = OrchardCoreConstants.Roles.Administrator;
        }

        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            OrchardCoreConstants.Roles.Anonymous,
            OrchardCoreConstants.Roles.Authenticated,
            AdminRoleName,
        };

        SystemRoleNames = roles.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}
