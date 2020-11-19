using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Security.Services
{
    /// <summary>
    /// Service that returns the installed permissions
    /// </summary>
    public interface IPermissionsService
    {
        Task<IEnumerable<Permission>> GetInstalledPermissionsAsync();
    }

    public class BasePermissionsService : IPermissionsService
    {

        private readonly IEnumerable<IPermissionProvider> _permissionProviders;

        public BasePermissionsService(IEnumerable<IPermissionProvider> permissionProviders)
        {
            _permissionProviders = permissionProviders;
        }

        public async Task<IEnumerable<Permission>> GetInstalledPermissionsAsync()
        {
            var installedPermissions = new List<Permission>();

            foreach (var permissionProvider in _permissionProviders)
            {
                var permissions = await permissionProvider.GetPermissionsAsync();

                foreach (var permission in permissions)
                {
                    installedPermissions.Add(permission);
                }
            }

            return installedPermissions;
        }
    }
}
