namespace OrchardCore.Security.Permissions;

public static class PermissionProviderExtensions
{
    public static async ValueTask<IEnumerable<Permission>> FindByNamesAsync(this IPermissionService permissionService, IEnumerable<string> names)
    {
        ArgumentNullException.ThrowIfNull(names);

        var selectedPermissions = new List<Permission>();

        foreach (var name in names)
        {
            var permission = await permissionService.FindByNameAsync(name);

            if (permission != null)
            {
                selectedPermissions.Add(permission);
            }
        }

        return selectedPermissions;
    }
}
