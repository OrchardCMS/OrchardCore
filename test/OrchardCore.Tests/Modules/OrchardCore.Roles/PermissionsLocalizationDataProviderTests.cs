using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Roles.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles;

public class PermissionsLocalizationDataProviderTests
{
    [Fact]
    public async Task GetDescriptorsAsync_LocalizedDescriptions_UsesUntranslatedDescriptionAsName()
    {
        // Arrange
        var provider = CreateProvider(
            new Permission("ManageThings", LocalizedString.Create("Manage things", typeof(PermissionsLocalizationDataProviderTests))),
            new Permission("ViewThings", "View things"),
            new Permission("EditThing_{0}", "Edit {0}"),
            new Permission("NoDescription"),
            new Permission("ManageThingsAgain", LocalizedString.Create("Manage things", typeof(PermissionsLocalizationDataProviderTests))));

        // Act
        var descriptors = await provider.GetDescriptorsAsync();

        // Assert
        Assert.Equal(["Manage things", "View things"], descriptors.Select(d => d.Name).ToArray());
    }

    private static PermissionsLocalizationDataProvider CreateProvider(params Permission[] permissions)
    {
        var permissionProvider = new Mock<IPermissionProvider>();
        permissionProvider.Setup(p => p.GetPermissionsAsync()).ReturnsAsync(permissions);

        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetFeaturesForDependency(It.IsAny<Type>())).Returns([]);

        var shellFeaturesManager = new Mock<IShellFeaturesManager>();
        shellFeaturesManager.Setup(m => m.GetEnabledFeaturesAsync()).ReturnsAsync(Array.Empty<IFeatureInfo>());

        return new PermissionsLocalizationDataProvider(
            [permissionProvider.Object],
            typeFeatureProvider.Object,
            shellFeaturesManager.Object);
    }
}
