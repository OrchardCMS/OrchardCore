namespace OrchardCore.Security.Services;

public static class RoleHelper
{
    [Obsolete("This method has been marked as obsolete and will be removed in future releases. Instead of using this helper, please IRoleService and and use IsSystemRoleAsync method for role checks.")]
    public static readonly HashSet<string> SystemRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        OrchardCoreConstants.Roles.Anonymous,
        OrchardCoreConstants.Roles.Authenticated,
    };
}
