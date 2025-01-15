namespace OrchardCore.Roles;

public static class SystemRoleNameProviderExtensions
{
#pragma warning disable CS0618 // Type or member is obsolete
    public static async ValueTask<bool> IsAdminRoleAsync(this ISystemRoleNameProvider provider, string roleName)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var adminRole = await provider.GetAdminRoleAsync();

        return roleName.Equals(adminRole, StringComparison.OrdinalIgnoreCase);
    }

#pragma warning disable CS0618 // Type or member is obsolete
    public static async ValueTask<bool> IsSystemRoleAsync(this ISystemRoleNameProvider provider, string roleName)
#pragma warning restore CS0618 // Type or member is obsolete
    {
        ArgumentNullException.ThrowIfNull(roleName);

        var roleNames = await provider.GetSystemRolesAsync();

        return roleNames.Contains(roleName);
    }
}
