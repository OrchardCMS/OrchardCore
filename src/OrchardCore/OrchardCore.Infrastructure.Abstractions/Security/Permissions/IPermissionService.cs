namespace OrchardCore.Security.Permissions;

public interface IPermissionService
{
    ValueTask<Permission> FindByNameAsync(string name);

    ValueTask<IEnumerable<Permission>> GetPermissionsAsync();
}
