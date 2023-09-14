using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Roles.Models;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Services
{
    public class RoleUpdater : FeatureEventHandler, IRoleCreatedEventHandler, IRoleRemovedEventHandler
    {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IExtensionManager _extensionManager;
        private readonly IDocumentManager<RolesDocument> _documentManager;
        private readonly IEnumerable<IPermissionProvider> _permissionProviders;
        private readonly ITypeFeatureProvider _typeFeatureProvider;
        private readonly ILogger _logger;

        private readonly HashSet<string> _installedFeatures = new();

        public RoleUpdater(
            ShellDescriptor shellDescriptor,
            IExtensionManager extensionManager,
            IDocumentManager<RolesDocument> documentManager,
            IEnumerable<IPermissionProvider> permissionProviders,
            ITypeFeatureProvider typeFeatureProvider,
            ILogger<RoleUpdater> logger)
        {
            _shellDescriptor = shellDescriptor;
            _extensionManager = extensionManager;
            _documentManager = documentManager;
            _permissionProviders = permissionProviders;
            _typeFeatureProvider = typeFeatureProvider;
            _logger = logger;
        }

        public override Task InstalledAsync(IFeatureInfo feature) => UpdateRolesForInstalledFeatureAsync(feature);

        public override Task EnabledAsync(IFeatureInfo feature) => UpdateRolesForEnabledFeatureAsync(feature);

        public Task RoleCreatedAsync(string roleName) => UpdateRoleForInstalledFeaturesAsync(roleName);

        public Task RoleRemovedAsync(string roleName) => RemoveRoleForMissingFeaturesAsync(roleName);

        private async Task UpdateRolesForInstalledFeatureAsync(IFeatureInfo feature)
        {
            _installedFeatures.Add(feature.Id);

            var providers = _permissionProviders
                .Where(provider => _typeFeatureProvider.GetFeatureForDependency(provider.GetType()).Id == feature.Id);

            if (!providers.Any())
            {
                return;
            }

            var updated = false;
            var rolesDocument = await _documentManager.GetOrCreateMutableAsync();
            foreach (var provider in providers)
            {
                var stereotypes = provider.GetDefaultStereotypes();
                foreach (var stereotype in stereotypes)
                {
                    var role = rolesDocument.Roles.FirstOrDefault(role => role.RoleName == stereotype.Name);
                    if (role == null)
                    {
                        continue;
                    }

                    var permissions = (stereotype.Permissions ?? Enumerable.Empty<Permission>())
                        .Select(stereotype => stereotype.Name);

                    if (UpdateRole(role, permissions, _logger))
                    {
                        updated = true;
                    }
                }
            }

            if (updated)
            {
                await _documentManager.UpdateAsync(rolesDocument);
            }
        }

        private async Task UpdateRolesForEnabledFeatureAsync(IFeatureInfo feature)
        {
            if (_installedFeatures.Contains(feature.Id))
            {
                return;
            }

            var providers = _permissionProviders
                .Where(provider => _typeFeatureProvider.GetFeatureForDependency(provider.GetType()).Id == feature.Id);

            if (!providers.Any())
            {
                return;
            }

            var updated = false;
            var rolesDocument = await _documentManager.GetOrCreateMutableAsync();
            foreach (var role in rolesDocument.Roles)
            {
                if (!rolesDocument.MissingFeaturesByRole.TryGetValue(role.RoleName, out var missingFeatures) ||
                    !missingFeatures.Contains(feature.Id))
                {
                    continue;
                }

                updated = true;

                missingFeatures.Remove(feature.Id);
                UpdateRoleAsync(role, providers, _logger);
            }

            if (updated)
            {
                await _documentManager.UpdateAsync(rolesDocument);
            }
        }

        private async Task UpdateRoleForInstalledFeaturesAsync(string roleName)
        {
            var rolesDocument = await _documentManager.GetOrCreateMutableAsync();
            var role = rolesDocument.Roles.FirstOrDefault(role => role.RoleName == roleName);
            if (role == null)
            {
                return;
            }

            // Get installed features that are no more enabled.
            var missingFeatures = _shellDescriptor.Installed
                .Except(_shellDescriptor.Features)
                .Select(feature => feature.Id)
                .ToArray();

            // And defining at least one 'IPermissionProvider'.
            rolesDocument.MissingFeaturesByRole[roleName] = (await _extensionManager.LoadFeaturesAsync(missingFeatures))
                .Where(entry => entry.ExportedTypes.Any(type => type.IsAssignableTo(typeof(IPermissionProvider))))
                .Select(entry => entry.FeatureInfo.Id)
                .ToList();

            await _documentManager.UpdateAsync(rolesDocument);

            var stereotypes = _permissionProviders
                .SelectMany(provider => provider.GetDefaultStereotypes())
                .Where(stereotype => stereotype.Name == roleName);

            if (!stereotypes.Any())
            {
                return;
            }

            var permissions = stereotypes
                .SelectMany(stereotype => stereotype.Permissions ?? Enumerable.Empty<Permission>())
                .Select(stereotype => stereotype.Name);

            UpdateRole(role, permissions, _logger);
        }

        private async Task RemoveRoleForMissingFeaturesAsync(string roleName)
        {
            var rolesDocument = await _documentManager.GetOrCreateMutableAsync();
            if (rolesDocument.MissingFeaturesByRole.TryGetValue(roleName, out _))
            {
                rolesDocument.MissingFeaturesByRole.Remove(roleName);
                await _documentManager.UpdateAsync(rolesDocument);
            }
        }

        private static bool UpdateRoleAsync(Role role, IEnumerable<IPermissionProvider> providers, ILogger logger)
        {
            var stereotypes = providers
                .SelectMany(provider => provider.GetDefaultStereotypes())
                .Where(stereotype => stereotype.Name == role.RoleName);

            if (!stereotypes.Any())
            {
                return false;
            }

            var permissions = stereotypes
                .SelectMany(stereotype => stereotype.Permissions ?? Enumerable.Empty<Permission>())
                .Select(stereotype => stereotype.Name);

            if (!permissions.Any())
            {
                return false;
            }

            return UpdateRole(role, permissions, logger);
        }

        private static bool UpdateRole(Role role, IEnumerable<string> permissions, ILogger logger)
        {
            var currentPermissions = role.RoleClaims
                .Where(roleClaim => roleClaim.ClaimType == Permission.ClaimType)
                .Select(roleClaim => roleClaim.ClaimValue);

            var distinctPermissions = currentPermissions
                .Union(permissions)
                .Distinct();

            var additionalPermissions = distinctPermissions.Except(currentPermissions);
            if (!additionalPermissions.Any())
            {
                return false;
            }

            foreach (var permission in additionalPermissions)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Default role '{RoleName}' granted permission '{PermissionName}'.", role.RoleName, permission);
                }

                role.RoleClaims.Add(new RoleClaim { ClaimType = Permission.ClaimType, ClaimValue = permission });
            }

            return true;
        }
    }
}
