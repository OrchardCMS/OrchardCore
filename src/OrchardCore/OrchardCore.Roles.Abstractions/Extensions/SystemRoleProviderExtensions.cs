namespace OrchardCore.Roles;

public static class SystemRoleProviderExtensions
{
    public static bool IsAdminRole(this ISystemRoleProvider systemRoleNameProvider, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        var adminRole = systemRoleNameProvider.GetAdminRole();

        return adminRole.RoleName.Equals(name, StringComparison.OrdinalIgnoreCase);
    }
}
