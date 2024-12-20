namespace OrchardCore.Security.Permissions;

public interface IPermissionService
{
    Task<IEnumerable<Permission>> GetPermissionsAsync();
}
