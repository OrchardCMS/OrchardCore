namespace OrchardCore.Roles;

[Obsolete("This class has been deprecated, please use SystemRoleProviderExtensions instead.")]
public static class SystemRoleNameProviderExtensions
{
    public static async ValueTask<bool> IsAdminRoleAsync(this ISystemRoleNameProvider provider, string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var adminRole = await provider.GetAdminRoleAsync().ConfigureAwait(false);

        return roleName.Equals(adminRole, StringComparison.OrdinalIgnoreCase);
    }

    public static async ValueTask<bool> IsSystemRoleAsync(this ISystemRoleNameProvider provider, string roleName)
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var roleNames = await provider.GetSystemRolesAsync().ConfigureAwait(false);

        return roleNames.Contains(roleName);
    }
}
