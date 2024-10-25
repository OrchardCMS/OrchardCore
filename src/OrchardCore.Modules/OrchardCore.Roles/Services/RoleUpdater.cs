using Microsoft.Extensions.Logging;
using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Roles.Models;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Roles.Services;

public class RoleUpdater : FeatureEventHandler, IRoleCreatedEventHandler, IRoleRemovedEventHandler
{
    private readonly ShellDescriptor _shellDescriptor;
    private readonly IExtensionManager _extensionManager;
    private readonly IDocumentManager<RolesDocument> _documentManager;
    private readonly ISystemRoleNameProvider _systemRoleNameProvider;
    private readonly IEnumerable<IPermissionProvider> _permissionProviders;
    private readonly ITypeFeatureProvider _typeFeatureProvider;
    private readonly ILogger _logger;

    private readonly HashSet<string> _installedFeatures = [];

    public RoleUpdater(
        ShellDescriptor shellDescriptor,
        IExtensionManager extensionManager,
        IDocumentManager<RolesDocument> documentManager,
        ISystemRoleNameProvider systemRoleNameProvider,
        IEnumerable<IPermissionProvider> permissionProviders,
        ITypeFeatureProvider typeFeatureProvider,
        ILogger<RoleUpdater> logger)
    {
        _shellDescriptor = shellDescriptor;
        _extensionManager = extensionManager;
        _documentManager = documentManager;
        _systemRoleNameProvider = systemRoleNameProvider;
        _permissionProviders = permissionProviders;
        _typeFeatureProvider = typeFeatureProvider;
        _logger = logger;
    }

    public override Task InstalledAsync(IFeatureInfo feature)
        => UpdateRolesForInstalledFeatureAsync(feature);

    public override Task EnabledAsync(IFeatureInfo feature)
        => UpdateRolesForEnabledFeatureAsync(feature);

    public Task RoleCreatedAsync(string roleName)
        => UpdateRoleForInstalledFeaturesAsync(roleName);

    public Task RoleRemovedAsync(string roleName)
        => RemoveRoleForMissingFeaturesAsync(roleName);

    private async Task UpdateRolesForInstalledFeatureAsync(IFeatureInfo feature)
    {
        _installedFeatures.Add(feature.Id);

        var providers = _permissionProviders
            .Where(provider => _typeFeatureProvider.GetFeaturesForDependency(provider.GetType()).Any(p => p.Id == feature.Id));

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
                var role = rolesDocument.Roles.FirstOrDefault(role => string.Equals(role.RoleName, stereotype.Name, StringComparison.OrdinalIgnoreCase));
                if (role == null)
                {
                    continue;
                }

                var permissions = (stereotype.Permissions ?? [])
                    .Select(stereotype => stereotype.Name);

                if (await UpdatePermissionsAsync(role, permissions))
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
            .Where(provider => _typeFeatureProvider.GetFeaturesForDependency(provider.GetType()).Any(p => p.Id == feature.Id));

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
            await UpdateRolesForEnabledFeatureAsync(role, providers);
        }

        if (updated)
        {
            await _documentManager.UpdateAsync(rolesDocument);
        }
    }

    private async Task UpdateRoleForInstalledFeaturesAsync(string roleName)
    {
        var rolesDocument = await _documentManager.GetOrCreateMutableAsync();
        var role = rolesDocument.Roles.FirstOrDefault(role => string.Equals(role.RoleName, roleName, StringComparison.OrdinalIgnoreCase));
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
            .Where(entry => _typeFeatureProvider.GetTypesForFeature(entry).Any(type => type.IsAssignableTo(typeof(IPermissionProvider))))
            .Select(entry => entry.Id)
            .ToList();

        await _documentManager.UpdateAsync(rolesDocument);

        var stereotypes = _permissionProviders
            .SelectMany(provider => provider.GetDefaultStereotypes())
            .Where(stereotype => string.Equals(stereotype.Name, roleName, StringComparison.OrdinalIgnoreCase));

        if (!stereotypes.Any())
        {
            return;
        }

        var permissions = stereotypes
            .SelectMany(stereotype => stereotype.Permissions ?? [])
            .Select(stereotype => stereotype.Name);

        await UpdatePermissionsAsync(role, permissions);
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

    private Task<bool> UpdateRolesForEnabledFeatureAsync(Role role, IEnumerable<IPermissionProvider> providers)
    {
        var stereotypes = providers
            .SelectMany(provider => provider.GetDefaultStereotypes())
            .Where(stereotype => string.Equals(stereotype.Name, role.RoleName, StringComparison.OrdinalIgnoreCase));

        if (!stereotypes.Any())
        {
            return Task.FromResult(false);
        }

        var permissions = stereotypes
            .SelectMany(stereotype => stereotype.Permissions ?? [])
            .Select(stereotype => stereotype.Name);

        if (!permissions.Any())
        {
            return Task.FromResult(false);
        }

        return UpdatePermissionsAsync(role, permissions);
    }

    private async Task<bool> UpdatePermissionsAsync(Role role, IEnumerable<string> permissions)
    {
        if (await _systemRoleNameProvider.IsAdminRoleAsync(role.RoleName))
        {
            // Don't update claims for admin role.
            return true;
        }

        var currentPermissions = role.RoleClaims
            .Where(roleClaim => roleClaim.ClaimType == Permission.ClaimType)
            .Select(roleClaim => roleClaim.ClaimValue);

        var additionalPermissions = currentPermissions
            .Union(permissions)
            .Distinct()
            .Except(currentPermissions);

        if (!additionalPermissions.Any())
        {
            return false;
        }

        foreach (var permission in additionalPermissions)
        {
            _logger.LogDebug("Default role '{RoleName}' granted permission '{PermissionName}'.", role.RoleName, permission);

            role.RoleClaims.Add(RoleClaim.Create(permission));
        }

        return true;
    }
}
