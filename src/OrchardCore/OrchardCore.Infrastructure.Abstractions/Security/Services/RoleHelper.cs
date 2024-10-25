namespace OrchardCore.Security.Services;

public static class RoleHelper
{
    [Obsolete("This method has been marked as obsolete and will be removed in future releases. Instead of using this helper, please use the IRoleService.IsSystemRoleAsync() method.")]
    public static readonly HashSet<string> SystemRoleNames = new(StringComparer.OrdinalIgnoreCase)
    {
        OrchardCoreConstants.Roles.Anonymous,
        OrchardCoreConstants.Roles.Authenticated,
    };
}
