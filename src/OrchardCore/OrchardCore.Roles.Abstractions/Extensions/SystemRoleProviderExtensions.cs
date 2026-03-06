namespace OrchardCore.Roles;

public static class SystemRoleProviderExtensions
{
    public static bool IsAdminRole(this ISystemRoleProvider systemRoleNameProvider, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        var adminRole = systemRoleNameProvider.GetAdminRole();

        return adminRole.Name.Equals(name, StringComparison.OrdinalIgnoreCase);
    }
}
