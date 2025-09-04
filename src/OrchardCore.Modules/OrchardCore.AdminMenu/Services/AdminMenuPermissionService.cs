using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.Services;

[Obsolete("This service is obsolete and will be removed in version 4. Instead, please use IPermissionService")]
public sealed class AdminMenuPermissionService : IAdminMenuPermissionService
{
    private readonly IPermissionService _permissionService;

    public AdminMenuPermissionService(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<IEnumerable<Permission>> GetPermissionsAsync()
        => await _permissionService.GetPermissionsAsync();
}

