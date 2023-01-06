using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Services
{
    public class RoleUpdater : IFeatureEventHandler, IRoleCreatedEventHandler
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly ILogger _logger;

        public RoleUpdater(
            RoleManager<IRole> roleManager,
            IEnumerable<IPermissionProvider> permissionProviders,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<RoleUpdater> logger)
        {
            _roleManager = roleManager;
            _permissionProviders = permissionProviders;
            _typeFeatureProvider = typeFeatureProvider;
            _logger = logger;
        }

        public Task InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task InstalledAsync(IFeatureInfo feature) => UpdateDefaultRolesForInstalledFeatureAsync(feature);

        public Task EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task EnabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task DisabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

        public Task RoleCreatedAsync(string roleName) => UpdateCreatedRoleForEnabledFeaturesAsync(roleName);

        private async Task UpdateDefaultRolesForInstalledFeatureAsync(IFeatureInfo feature)
        {
            // When another feature is being installed, locate matching permission providers.
            var providersForInstalledFeature = _permissionProviders
                .Where(provider => _typeFeatureProvider.GetFeatureForDependency(provider.GetType()).Id == feature.Id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                if (providersForInstalledFeature.Any())
                {
                    _logger.LogDebug("Configuring default roles for feature '{FeatureName}'", feature.Id);
                }
                else
                {
                    _logger.LogDebug("No default roles for feature '{FeatureName}'", feature.Id);

                    return;
                }
            }

            foreach (var permissionProvider in providersForInstalledFeature)
            {
                // Get and iterate stereotypical groups of permissions.
                var stereotypes = permissionProvider.GetDefaultStereotypes();
                foreach (var stereotype in stereotypes)
                {
                    var role = await _roleManager.FindByNameAsync(stereotype.Name);
                    if (role == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogDebug("The default role '{RoleName}' for feature '{FeatureName}' doesn't exist.", stereotype.Name, feature.Id);
                        }

                        continue;
                    }

                    // Merge the stereotypical permissions into that role.
                    var permissionNames = (stereotype.Permissions ?? Enumerable.Empty<Permission>())
                        .Select(stereotype => stereotype.Name);

                    await AssignPermissionsToRole((Role)role, permissionNames);
                }
            }
        }

        private async Task UpdateCreatedRoleForEnabledFeaturesAsync(string roleName)
        {
            // When another role is being created, locate matching permission stereotypes.
            var stereotypesForCreatedRole = _permissionProviders
                .SelectMany(provider => provider.GetDefaultStereotypes())
                .Where(stereotype => stereotype.Name == roleName);

            if (!stereotypesForCreatedRole.Any())
            {
                return;
            }

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return;
            }

            // Merge the stereotypical permissions into that role.
            var permissionNames = stereotypesForCreatedRole
                .SelectMany(stereotype => stereotype.Permissions ?? Enumerable.Empty<Permission>())
                .Select(stereotype => stereotype.Name);

            await AssignPermissionsToRole((Role)role, permissionNames);
        }

        private async Task AssignPermissionsToRole(Role role, IEnumerable<string> permissionNames)
        {
            var currentPermissionNames = role.RoleClaims
                .Where(roleClaim => roleClaim.ClaimType == Permission.ClaimType)
                .Select(roleClaim => roleClaim.ClaimValue);

            var distinctPermissionNames = currentPermissionNames
                .Union(permissionNames)
                .Distinct();

            // Update role if set of permissions has increased.
            var additionalPermissionNames = distinctPermissionNames.Except(currentPermissionNames);
            if (additionalPermissionNames.Any())
            {
                foreach (var permissionName in additionalPermissionNames)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Default role '{RoleName}' granted permission '{PermissionName}'.", role.RoleName, permissionName);
                    }

                    await _roleManager.AddClaimAsync(role, new Claim(Permission.ClaimType, permissionName));
                }
            }
        }
    }
}
