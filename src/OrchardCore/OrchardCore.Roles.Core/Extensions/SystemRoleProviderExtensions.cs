using OrchardCore.Security;

namespace OrchardCore.Roles;

public static class SystemRoleProviderExtensions
{
    public static async ValueTask<IRole> GetAdminRoleAsync(this ISystemRoleProvider systemRoleNameProvider)
        => (await systemRoleNameProvider.GetSystemRolesAsync()).First();

    public static async ValueTask<bool> IsSystemRoleAsync(this ISystemRoleProvider systemRoleNameProvider, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        var systemRoles = await systemRoleNameProvider.GetSystemRolesAsync();

        return systemRoles.Any(role => role.RoleName == name);
    }

    public static async ValueTask<bool> IsAdminRoleAsync(this ISystemRoleProvider systemRoleNameProvider, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

        var adminRole = await systemRoleNameProvider.GetAdminRoleAsync();

        return adminRole.RoleName == name;
    }
}
