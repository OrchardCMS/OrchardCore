using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.Services;

public interface IAdminMenuPermissionService
{
    Task<IEnumerable<Permission>> GetPermissionsAsync();
}
