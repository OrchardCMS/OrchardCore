namespace OrchardCore.Security.Permissions;

public sealed class DefaultPermissionService : IPermissionService
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;

    // Cached per request.
    private Dictionary<string, Permission> _permissions;

    public DefaultPermissionService(IEnumerable<IPermissionProvider> permissionProviders)
    {
        _permissionProviders = permissionProviders;
    }

    public async ValueTask<Permission> FindByNameAsync(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (_permissions is null)
        {
            await LoadPermissionsAsync().ConfigureAwait(false);
        }

        if (_permissions.TryGetValue(name, out var permission))
        {
            return permission;
        }

        return null;
    }

    public async ValueTask<IEnumerable<Permission>> GetPermissionsAsync()
    {
        if (_permissions == null)
        {
            await LoadPermissionsAsync().ConfigureAwait(false);
        }

        return _permissions.Values;
    }

    private async Task LoadPermissionsAsync()
    {
        _permissions = new Dictionary<string, Permission>(StringComparer.OrdinalIgnoreCase);

        foreach (var permissionProvider in _permissionProviders)
        {
            var permissions = await permissionProvider.GetPermissionsAsync().ConfigureAwait(false);

            foreach (var permission in permissions)
            {
                _permissions[permission.Name] = permission;
            }
        }
    }
}
