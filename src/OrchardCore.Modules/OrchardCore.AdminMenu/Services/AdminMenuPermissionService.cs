using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.Services;

[Obsolete("This service is obsolete and will be removed in version 4. Instead, please use IPermissionService")]
public sealed class AdminMenuPermissionService : IAdminMenuPermissionService
{
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;

    // Cached per request.
    private List<Permission> _permissions;

    public AdminMenuPermissionService(IEnumerable<IPermissionProvider> permissionProviders)
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

