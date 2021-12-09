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
    public class RoleUpdater : IFeatureEventHandler
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
            _typeFeatureProvider = typeFeatureProvider;
            _roleManager = roleManager;
            _permissionProviders = permissionProviders;
            _logger = logger;
        }

        Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => AddDefaultRolesForFeatureAsync(feature);

        Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

        public async Task AddDefaultRolesForFeatureAsync(IFeatureInfo feature)
        {
            // when another module is being enabled, locate matching permission providers
            var providersForEnabledModule = _permissionProviders
                .Where(x => _typeFeatureProvider.GetFeatureForDependency(x.GetType()).Id == feature.Id);

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                if (providersForEnabledModule.Any())
                {
                    _logger.LogDebug("Configuring default roles for feature '{FeatureName}'", feature.Id);
                }
                else
                {
                    _logger.LogDebug("No default roles for feature '{FeatureName}'", feature.Id);
                }
            }

            foreach (var permissionProvider in providersForEnabledModule)
            {
                // get and iterate stereotypical groups of permissions
                var stereotypes = permissionProvider.GetDefaultStereotypes();
                foreach (var stereotype in stereotypes)
                {
                    // turn those stereotypes into roles
                    var role = await _roleManager.FindByNameAsync(stereotype.Name);
                    if (role == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Information))
                        {
                            _logger.LogInformation("Defining new role '{RoleName}' for permission stereotype", stereotype.Name);
                        }

                        role = new Role { RoleName = stereotype.Name, RoleDescription = stereotype.Name + " role" };
                        await _roleManager.CreateAsync(role);
                    }

                    // and merge the stereotypical permissions into that role
                    var stereotypePermissionNames = (stereotype.Permissions ?? Enumerable.Empty<Permission>()).Select(x => x.Name);
                    var currentPermissionNames = ((Role)role).RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue);

                    var distinctPermissionNames = currentPermissionNames
                        .Union(stereotypePermissionNames)
                        .Distinct();

                    // update role if set of permissions has increased
                    var additionalPermissionNames = distinctPermissionNames.Except(currentPermissionNames);

                    if (additionalPermissionNames.Any())
                    {
                        foreach (var permissionName in additionalPermissionNames)
                        {
                            if (_logger.IsEnabled(LogLevel.Debug))
                            {
                                _logger.LogDebug("Default role '{Role}' granted permission '{Permission}'", stereotype.Name, permissionName);
                            }

                            await _roleManager.AddClaimAsync(role, new Claim(Permission.ClaimType, permissionName));
                        }
                    }
                }
            }
        }
    }
}
