using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Roles.Models;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;
using OrchardCore.Security.Services;

namespace OrchardCore.Roles.Services
{
    public class RoleUpdater : ModularTenantEvents, IFeatureEventHandler
    {
        private readonly RoleManager<IRole> _roleManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ILogger _logger;
        private readonly IRoleService _roleService;
        private readonly IDocumentManager<RolesDocument> _rolesDocumentManager;

        private bool _updateInProgress;

        public RoleUpdater(
            RoleManager<IRole> roleManager,
            IEnumerable<IPermissionProvider> permissionProviders,
            ILogger<RoleUpdater> logger,
            IRoleService roleService,
            IDocumentManager<RolesDocument> rolesDocumentManager)
        {
            _roleManager = roleManager;
            _permissionProviders = permissionProviders;
            _logger = logger;
            _roleService = roleService;
            _rolesDocumentManager = rolesDocumentManager;
        }

        Task IFeatureEventHandler.InstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.InstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.EnablingAsync(IFeatureInfo feature) => AssignPermissionsToRolesAsync();

        Task IFeatureEventHandler.EnabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.DisablingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.DisabledAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.UninstallingAsync(IFeatureInfo feature) => Task.CompletedTask;

        Task IFeatureEventHandler.UninstalledAsync(IFeatureInfo feature) => Task.CompletedTask;

        /// <summary>
        /// This event is called of the very first request to the tenant after a tenant is built/rebuilt.
        /// Using this event will ensure that any new permission were added are auto assigned to a role.
        /// </summary>
        /// <returns></returns>
        public override Task ActivatedAsync() => AssignPermissionsToRolesAsync();

        /// <summary>
        /// Checks all available permissions to role mapping from any available <see cref="IPermissionProvider"/>.
        /// When a new permission is found, auto assign it to the mapped role. If the permission was previously assigned,
        /// do not assign the permission. This method could get called multiple time from the same request,
        /// so it should not be executed again if the parameter "_updateInProgress" is set to true.
        /// </summary>
        /// <returns></returns>
        private async Task AssignPermissionsToRolesAsync()
        {
            if (_updateInProgress)
            {
                return;
            }

            _updateInProgress = true;

            var roleNames = await _roleService.GetRoleNamesAsync();

            if (!roleNames.Any())
            {
                // Site roles are initially added using the "RolesStep" handler, while no role names are availble. This means
                // that RoleUpdater handler was called before the "RolesStep".
                // Its likley to be coming from a tenant setup request. Nothing to do.
                return;
            }

            var rolesDocument = await _rolesDocumentManager.GetOrCreateMutableAsync();
            var updateRolesDocument = false;

            // Get all the available permissions grouped by role name as defined by IPermissionProvider.
            var groups = _permissionProviders.SelectMany(x => x.GetDefaultStereotypes())
                .GroupBy(stereotype => stereotype.Name)
                .Select(x => new
                {
                    RoleName = x.Key,
                    PermissionNames = x.SelectMany(y => y.Permissions ?? Enumerable.Empty<Permission>()).Select(x => x.Name)
                });

            foreach (var group in groups)
            {
                if (!roleNames.Any(roleName => roleName.Equals(group.RoleName, StringComparison.OrdinalIgnoreCase))
                    || await _roleManager.FindByNameAsync(group.RoleName) is not Role role)
                {
                    // A role is mapped in IPermissionProvider, yet it isn't available for the tenant, ignore it.
                    continue;
                }

                var currentPermissionNames = role.RoleClaims.Where(x => x.ClaimType == Permission.ClaimType).Select(x => x.ClaimValue);

                var distinctPermissionNames = currentPermissionNames
                    .Union(group.PermissionNames)
                    .Distinct()
                    .ToList();

                if (!rolesDocument.PermissionGroups.ContainsKey(group.RoleName))
                {
                    rolesDocument.PermissionGroups.TryAdd(group.RoleName, new List<string>());
                }

                // Get all available permission names that isn't already assigned to the role.
                var additionalPermissionNames = distinctPermissionNames.Except(currentPermissionNames);

                foreach (var permissionName in additionalPermissionNames)
                {
                    if (_logger.IsEnabled(LogLevel.Debug))
                    {
                        _logger.LogDebug("Default role '{Role}' granted permission '{Permission}'", group.RoleName, permissionName);
                    }

                    if (rolesDocument.PermissionGroups[group.RoleName].Contains(permissionName, StringComparer.OrdinalIgnoreCase))
                    {
                        // The permission was previously assigned to the role, we can't assign it again.
                        continue;
                    }

                    await _roleManager.AddClaimAsync(role, new Claim(Permission.ClaimType, permissionName));
                }

                foreach (var distinctPermissionName in distinctPermissionNames)
                {
                    if (rolesDocument.PermissionGroups[group.RoleName].Contains(distinctPermissionName, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    rolesDocument.PermissionGroups[group.RoleName].Add(distinctPermissionName);
                    updateRolesDocument = true;
                }
            }

            if (updateRolesDocument)
            {
                await _rolesDocumentManager.UpdateAsync(rolesDocument);
            }

            _updateInProgress = false;
        }
    }
}
