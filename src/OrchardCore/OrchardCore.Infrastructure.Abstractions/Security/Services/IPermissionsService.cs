using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Security.Permissions;
using OrchardCore.Environment.Extensions;

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
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public BasePermissionsService(
            IEnumerable<IPermissionProvider> permissionProviders,
            ITypeFeatureProvider typeFeatureProvider)
        {
            _permissionProviders = permissionProviders;
            _typeFeatureProvider = typeFeatureProvider;
        }

        public async Task<IEnumerable<Permission>> GetInstalledPermissionsAsync()
        {
            var installedPermissions = new List<Permission>();
            foreach (var permissionProvider in _permissionProviders)
            {
                var feature = _typeFeatureProvider.GetFeatureForDependency(permissionProvider.GetType());
                var featureName = feature.Id;

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