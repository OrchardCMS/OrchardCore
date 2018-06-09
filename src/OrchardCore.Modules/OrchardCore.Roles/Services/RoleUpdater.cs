using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Shell;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Roles.Services
{
    public class RoleUpdater : IFeatureEventHandler
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITypeFeatureProvider _typeFeatureProvider;

        public RoleUpdater(
            RoleManager<IRole> roleManager,
            IEnumerable<IPermissionProvider> permissionProviders,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<RoleUpdater> logger)
        {
            _typeFeatureProvider = typeFeatureProvider;
            _roleManager = roleManager;
            _permissionProviders = permissionProviders;

            Logger = logger;
        }

        public ILogger Logger { get; set; }

        void IFeatureEventHandler.Installing(IFeatureInfo feature)
        {
        }

        void IFeatureEventHandler.Installed(IFeatureInfo feature)
        {
            AddDefaultRolesForFeatureAsync(feature).Wait();
        }

        void IFeatureEventHandler.Enabling(IFeatureInfo feature)
        {
        }

        void IFeatureEventHandler.Enabled(IFeatureInfo feature)
        {
        }

        void IFeatureEventHandler.Disabling(IFeatureInfo feature)
        {
        }

        void IFeatureEventHandler.Disabled(IFeatureInfo feature)
        {
        }

        void IFeatureEventHandler.Uninstalling(IFeatureInfo feature)
        {
        }

        void IFeatureEventHandler.Uninstalled(IFeatureInfo feature)
        {
        }

        public async Task AddDefaultRolesForFeatureAsync(IFeatureInfo feature)
        {
            // when another module is being enabled, locate matching permission providers
            var providersForEnabledModule = _permissionProviders
                .Where(x => _typeFeatureProvider.GetFeatureForDependency(x.GetType()).Id == feature.Id);

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                if (providersForEnabledModule.Any())
                {
                    Logger.LogDebug("Configuring default roles for feature '{FeatureName}'", feature.Id);
                }
                else
                {
                    Logger.LogDebug("No default roles for feature '{FeatureName}'", feature.Id);
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
                        if (Logger.IsEnabled(LogLevel.Information))
                        {
                            Logger.LogInformation("Defining new role '{RoleName}' for permission stereotype", stereotype.Name);
                        }

                        role = new Role { RoleName = stereotype.Name };
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
                            if (Logger.IsEnabled(LogLevel.Debug))
                            {
                                Logger.LogDebug("Default role '{Role}' granted permission '{Permission}'", stereotype.Name, permissionName);
                            }

                            await _roleManager.AddClaimAsync(role, new Claim(Permission.ClaimType, permissionName));
                        }
                    }
                }
            }
        }
    }
}
