using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Builders.Models;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.Tests.Shell;

public class ShellContextFactoryTests
{
    private const string SetupFeatureId = "My.Setup";
    private const string DefaultTenantOnlySetupFeatureId = "My.Setup.Default";
    private const string GlobalFeatureId = "My.Global";

    [Fact]
    public async Task CreateSetupContextAsync_DefaultTenant_ComposesAllSetupFeatures()
    {
        // Arrange
        var (factory, descriptors) = CreateFactory();

        // Act
        await using var context = await factory.CreateSetupContextAsync(new ShellSettings().AsDefaultShell().AsUninitialized());

        // Assert
        var descriptor = Assert.Single(descriptors);
        Assert.Equal(-1, descriptor.SerialNumber);
        Assert.Equal([SetupFeatureId, DefaultTenantOnlySetupFeatureId, GlobalFeatureId], descriptor.Features.Select(feature => feature.Id));
    }

    [Fact]
    public async Task CreateSetupContextAsync_OtherTenant_SkipsDefaultTenantOnlySetupFeatures()
    {
        // Arrange
        var (factory, descriptors) = CreateFactory();

        // Act
        await using var context = await factory.CreateSetupContextAsync(new ShellSettings { Name = "Tenant1" }.AsUninitialized());

        // Assert
        var descriptor = Assert.Single(descriptors);
        Assert.Equal([SetupFeatureId, GlobalFeatureId], descriptor.Features.Select(feature => feature.Id));
        Assert.True(descriptor.Features.Single(feature => feature.Id == GlobalFeatureId).AlwaysEnabled);
    }

    private static (IShellContextFactory Factory, List<ShellDescriptor> Descriptors) CreateFactory()
    {
        var descriptors = new List<ShellDescriptor>();

        var compositionStrategy = new Mock<ICompositionStrategy>();
        compositionStrategy
            .Setup(strategy => strategy.ComposeAsync(It.IsAny<ShellSettings>(), It.IsAny<ShellDescriptor>()))
            .ReturnsAsync((ShellSettings settings, ShellDescriptor descriptor) =>
            {
                descriptors.Add(descriptor);

                return new ShellBlueprint
                {
                    Settings = settings,
                    Descriptor = descriptor,
                    Dependencies = new Dictionary<Type, IEnumerable<IFeatureInfo>>(),
                };
            });

        var shellContainerFactory = new Mock<IShellContainerFactory>();
        shellContainerFactory
            .Setup(containerFactory => containerFactory.CreateContainerAsync(It.IsAny<ShellSettings>(), It.IsAny<ShellBlueprint>()))
            .ReturnsAsync(() => new ServiceCollection().AddOptions().BuildServiceProvider());

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager
            .Setup(manager => manager.GetFeatures())
            .Returns(
            [
                CreateFeature(SetupFeatureId, defaultTenantOnly: false),
                CreateFeature(DefaultTenantOnlySetupFeatureId, defaultTenantOnly: true),
                CreateFeature(GlobalFeatureId, defaultTenantOnly: false),
            ]);

        IShellContextFactory factory = new ShellContextFactory(
            compositionStrategy.Object,
            shellContainerFactory.Object,
            [
                new ShellFeature(SetupFeatureId),
                new ShellFeature(DefaultTenantOnlySetupFeatureId),
                new ShellFeature(GlobalFeatureId, alwaysEnabled: true),
            ],
            extensionManager.Object,
            Mock.Of<ILogger<ShellContextFactory>>());

        return (factory, descriptors);
    }

    private static IFeatureInfo CreateFeature(string id, bool defaultTenantOnly)
    {
        var feature = new Mock<IFeatureInfo>();
        feature.Setup(f => f.Id).Returns(id);
        feature.Setup(f => f.DefaultTenantOnly).Returns(defaultTenantOnly);

        return feature.Object;
    }
}
