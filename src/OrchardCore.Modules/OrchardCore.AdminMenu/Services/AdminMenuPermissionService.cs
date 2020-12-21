using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminMenu.Services
{
    public class AdminMenuPermissionService : IAdminMenuPermissionService
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

            _permissions = new List<Permission>();

            foreach (var permissionProvider in _permissionProviders)
            {
                var permissions = await permissionProvider.GetPermissionsAsync();

                _permissions.AddRange(permissions);
            }

            return _permissions;
        }
    }
}

