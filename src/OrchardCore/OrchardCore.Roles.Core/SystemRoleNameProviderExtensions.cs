namespace OrchardCore.Roles;

public static class SystemRoleNameProviderExtensions
{
    public static async ValueTask<bool> IsAdminRoleAsync(this ISystemRoleNameProvider provider, string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var adminRole = await provider.GetAdminRoleAsync();

        return roleName.Equals(adminRole, StringComparison.OrdinalIgnoreCase);
    }

    public static async ValueTask<bool> IsSystemRoleAsync(this ISystemRoleNameProvider provider, string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var roleNames = await provider.GetSystemRolesAsync();

        return roleNames.Contains(roleName);
    }
}
