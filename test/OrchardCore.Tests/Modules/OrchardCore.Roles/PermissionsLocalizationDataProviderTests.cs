using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Roles.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles;

public class PermissionsLocalizationDataProviderTests
{
    [Fact]
    public async Task GetDescriptorsAsync_DescriptionsWithoutContext_AreProvided()
    {
        // Arrange
        var provider = CreateProvider(
            new Permission("ViewThings", "View things"),
            new Permission("ViewArticles", "View Article by others"),
            new Permission("ViewThingsAgain", "View things"),
            new Permission("EditThing_{0}", "Edit {0}"),
            new Permission("NoDescription"));

        // Act
        var descriptors = await provider.GetDescriptorsAsync();

        // Assert
        Assert.Equal(["View things", "View Article by others"], descriptors.Select(d => d.Name).ToArray());
    }

    [Fact]
    public async Task GetDescriptorsAsync_DescriptionsWithContext_AreNotProvided()
    {
        // Arrange
        var provider = CreateProvider(
            new Permission("ManageThings", LocalizedString.Create("Manage things", typeof(PermissionsLocalizationDataProviderTests))),
            new Permission("ViewThings", "View things"));

        // Act
        var descriptors = await provider.GetDescriptorsAsync();

        // Assert
        Assert.Equal(["View things"], descriptors.Select(d => d.Name).ToArray());
    }

    [Fact]
    public async Task GetDescriptorsAsync_CategoryOfPermissionWithContext_IsProvided()
    {
        // Arrange
        var permission = new Permission("ManageThings", LocalizedString.Create("Manage things", typeof(PermissionsLocalizationDataProviderTests)))
        {
            Category = "Things",
        };

        var provider = CreateProvider(permission);

        // Act
        var descriptors = await provider.GetDescriptorsAsync();

        // Assert
        var descriptor = Assert.Single(descriptors);
        Assert.Equal("Things", descriptor.Name);
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
