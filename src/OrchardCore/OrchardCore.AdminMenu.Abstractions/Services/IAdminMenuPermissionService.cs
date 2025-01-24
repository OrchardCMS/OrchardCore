using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.Services;

[Obsolete("This service is obsolete and will be removed in version 4. Instead, please use IPermissionService")]
public interface IAdminMenuPermissionService
{
    Task<IEnumerable<Permission>> GetPermissionsAsync();
}
