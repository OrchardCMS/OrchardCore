using OrchardCore.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Roles;
using OrchardCore.Roles.Models;
using OrchardCore.Roles.Services;
using OrchardCore.Security;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles;

public class RoleUpdaterTests
{
    private const string FeatureId = "TestFeature";
    private const string RoleName = "Editor";

    private static readonly Permission s_permission = new("ManageThings");

    [Fact]
    public async Task RoleCreatedAsync_InstalledFeatureDisabledWithUnknownTypes_IsRecordedAsMissing()
    {
        // Arrange
        var feature = CreateFeature();
        var rolesDocument = CreateRolesDocument();

        // The types of a disabled feature that are not exported are unknown to the type feature provider.
        var updater = CreateUpdater(feature, rolesDocument, new TypeFeatureProvider(), featureEnabled: false, []);

        // Act
        await updater.RoleCreatedAsync(RoleName);

        // Assert
        Assert.Equal([FeatureId], rolesDocument.MissingFeaturesByRole[RoleName]);
    }

    [Fact]
    public async Task EnabledAsync_FeatureMissingWhenRoleWasCreated_GrantsStereotypePermissions()
    {
        // Arrange
        var feature = CreateFeature();
        var rolesDocument = CreateRolesDocument();
        var typeFeatureProvider = new TypeFeatureProvider();

        await CreateUpdater(feature, rolesDocument, typeFeatureProvider, featureEnabled: false, [])
            .RoleCreatedAsync(RoleName);

        // Enabling the feature registers its permission provider in DI and in the type feature provider.
        var permissionProvider = new InternalPermissionProvider();
        typeFeatureProvider.TryAdd(permissionProvider.GetType(), feature);

        var updater = CreateUpdater(feature, rolesDocument, typeFeatureProvider, featureEnabled: true, [permissionProvider]);

        // Act
        await updater.EnabledAsync(feature);

        // Assert
        var role = Assert.Single(rolesDocument.Roles);
        Assert.Contains(role.RoleClaims, claim => claim.ClaimType == Permission.ClaimType && claim.ClaimValue == s_permission.Name);
        Assert.Empty(rolesDocument.MissingFeaturesByRole[RoleName]);
    }

    private static IFeatureInfo CreateFeature()
        => Mock.Of<IFeatureInfo>(feature => feature.Id == FeatureId);

    private static RolesDocument CreateRolesDocument()
        => new() { Roles = [new Role { RoleName = RoleName }] };

    private static RoleUpdater CreateUpdater(
        IFeatureInfo feature,
        RolesDocument rolesDocument,
        ITypeFeatureProvider typeFeatureProvider,
        bool featureEnabled,
        IEnumerable<IPermissionProvider> permissionProviders)
    {
        var shellFeature = new ShellFeature(FeatureId);
        var shellDescriptor = new ShellDescriptor
        {
            Installed = [new InstalledShellFeature(shellFeature)],
            Features = featureEnabled ? [shellFeature] : [],
        };

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager
            .Setup(manager => manager.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync((IEnumerable<string> ids) => ids.Contains(FeatureId) ? [feature] : []);

        var documentManager = new Mock<IDocumentManager<RolesDocument>>();
        documentManager
            .Setup(manager => manager.GetOrCreateMutableAsync(It.IsAny<Func<Task<RolesDocument>>>()))
            .ReturnsAsync(rolesDocument);

        return new RoleUpdater(
            shellDescriptor,
            extensionManager.Object,
            documentManager.Object,
            Mock.Of<ISystemRoleProvider>(provider => provider.GetAdminRole() == new Role { RoleName = "Administrator" }),
            permissionProviders,
            typeFeatureProvider,
            NullLogger<RoleUpdater>.Instance);
    }

    private sealed class InternalPermissionProvider : IPermissionProvider
    {
        public Task<IEnumerable<Permission>> GetPermissionsAsync()
            => Task.FromResult<IEnumerable<Permission>>([s_permission]);

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() =>
        [
            new PermissionStereotype
            {
                Name = RoleName,
                Permissions = [s_permission],
            },
        ];
    }
}
