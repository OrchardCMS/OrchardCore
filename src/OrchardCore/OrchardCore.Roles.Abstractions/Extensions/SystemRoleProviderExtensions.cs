using OrchardCore.Security;

namespace OrchardCore.Roles;

public static class SystemRoleProviderExtensions
{
    public static async ValueTask<bool> IsSystemRoleAsync(this ISystemRoleProvider systemRoleNameProvider, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        var systemRoles = await systemRoleNameProvider.GetSystemRolesAsync();

        return systemRoles.Any(role => role.RoleName.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public static async ValueTask<bool> IsAdminRoleAsync(this ISystemRoleProvider systemRoleNameProvider, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        var adminRole = await systemRoleNameProvider.GetAdminRoleAsync();

        return adminRole.RoleName.Equals(name, StringComparison.OrdinalIgnoreCase);
    }
}
