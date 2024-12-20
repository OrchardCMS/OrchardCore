namespace OrchardCore.Security.Permissions;

public sealed class DefaultPermissionService : IPermissionService
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;

    // Cached per request.
    private List<Permission> _permissions;

    public DefaultPermissionService(IEnumerable<IPermissionProvider> permissionProviders)
    {
        _permissionProviders = permissionProviders;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
    {
        if (_permissions != null)
        {
            return _permissions;
        }

        _permissions = [];

        foreach (var permissionProvider in _permissionProviders)
        {
            var permissions = await permissionProvider.GetPermissionsAsync();

            _permissions.AddRange(permissions);
        }

        return _permissions;
    }
}
