using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Data.Descriptors;
using OrchardCore.Environment.Shell.Descriptor;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Descriptor.Settings;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Tests.Shell;

public class ImplicitFeatureShellBehaviorTests
{
    [Fact]
    public async Task ConfiguredFeaturesShellDescriptorManager_IncludesImplicitlyEnabledFeatures()
    {
        var implicitFeature = CreateFeatureInfo("Implicit.Feature", isImplicitlyEnabled: true);
        var extensionManager = CreateExtensionManager([implicitFeature], [implicitFeature]);
        var manager = new ConfiguredFeaturesShellDescriptorManager(
            CreateShellConfiguration(),
            [],
            extensionManager.Object);

        var descriptor = await manager.GetShellDescriptorAsync();

        Assert.Contains(descriptor.Features, feature => feature.Id == "Implicit.Feature" && feature.AlwaysEnabled);
    }

    [Fact]
    public async Task SetFeaturesShellDescriptorManager_IncludesImplicitlyEnabledFeatures()
    {
        var implicitFeature = CreateFeatureInfo("Implicit.Feature", isImplicitlyEnabled: true);
        var explicitFeature = CreateFeatureInfo("Explicit.Feature");
        var extensionManager = CreateExtensionManager([implicitFeature, explicitFeature], [implicitFeature, explicitFeature]);
        var manager = new SetFeaturesShellDescriptorManager(
            [new ShellFeature("Explicit.Feature")],
            extensionManager.Object);

        var descriptor = await manager.GetShellDescriptorAsync();

        Assert.Contains(descriptor.Features, feature => feature.Id == "Implicit.Feature" && feature.AlwaysEnabled);
        Assert.Contains(descriptor.Features, feature => feature.Id == "Explicit.Feature");
    }

    [Fact]
    public async Task AllFeaturesShellDescriptorManager_MarksImplicitlyEnabledFeaturesAsAlwaysEnabled()
    {
        var implicitFeature = CreateFeatureInfo("Implicit.Feature", isImplicitlyEnabled: true);
        var explicitFeature = CreateFeatureInfo("Explicit.Feature");
        var extensionManager = CreateExtensionManager([implicitFeature, explicitFeature], [implicitFeature, explicitFeature]);
        var manager = new AllFeaturesShellDescriptorManager(extensionManager.Object);

        var descriptor = await manager.GetShellDescriptorAsync();

        Assert.Contains(descriptor.Features, feature => feature.Id == "Implicit.Feature" && feature.AlwaysEnabled);
        Assert.Contains(descriptor.Features, feature => feature.Id == "Explicit.Feature" && !feature.AlwaysEnabled);
    }

    [Fact]
    public async Task ShellDescriptorManager_GetShellDescriptorAsync_IncludesImplicitlyEnabledFeatures()
    {
        var persistedDescriptor = new ShellDescriptor
        {
            SerialNumber = 1,
            Features = [new ShellFeature("Explicit.Feature")],
        };
        var implicitFeature = CreateFeatureInfo("Implicit.Feature", isImplicitlyEnabled: true);
        var explicitFeature = CreateFeatureInfo("Explicit.Feature");
        var extensionManager = CreateExtensionManager(
            [implicitFeature, explicitFeature],
            [implicitFeature, explicitFeature]);
        var documentStore = new Mock<IDocumentStore>();
        documentStore
            .Setup(store => store.GetOrCreateImmutableAsync<ShellDescriptor>(It.IsAny<Func<Task<ShellDescriptor>>>() ))
            .ReturnsAsync((true, persistedDescriptor));
        var manager = new ShellDescriptorManager(
            new ShellSettings { Name = "Test" },
            CreateShellConfiguration(),
            [],
            [],
            extensionManager.Object,
            documentStore.Object,
            NullLogger<ShellDescriptorManager>.Instance);

        var descriptor = await manager.GetShellDescriptorAsync();

        Assert.Contains(descriptor.Features, feature => feature.Id == "Implicit.Feature" && feature.AlwaysEnabled);
        Assert.Contains(descriptor.Features, feature => feature.Id == "Explicit.Feature");
    }

    [Fact]
    public async Task ShellDescriptorManager_UpdateShellDescriptorAsync_PersistsImplicitlyEnabledFeaturesAsAlwaysEnabled()
    {
        var mutableDescriptor = new ShellDescriptor
        {
            SerialNumber = 1,
            Features = [],
            Installed = [],
        };
        var implicitFeature = CreateFeatureInfo("Implicit.Feature", isImplicitlyEnabled: true);
        var explicitFeature = CreateFeatureInfo("Explicit.Feature");
        var extensionManager = CreateExtensionManager([implicitFeature, explicitFeature], [implicitFeature, explicitFeature]);
        var documentStore = new Mock<IDocumentStore>();
        documentStore
            .Setup(store => store.GetOrCreateMutableAsync<ShellDescriptor>(It.IsAny<Func<Task<ShellDescriptor>>>() ))
            .ReturnsAsync(mutableDescriptor);
        documentStore
            .Setup(store => store.UpdateAsync(It.IsAny<ShellDescriptor>(), It.IsAny<Func<ShellDescriptor, Task>>(), It.IsAny<bool>()))
            .Returns(Task.CompletedTask);
        documentStore
            .Setup(store => store.CommitAsync())
            .Returns(Task.CompletedTask);
        var manager = new ShellDescriptorManager(
            new ShellSettings { Name = "Test" },
            CreateShellConfiguration(),
            [],
            [],
            extensionManager.Object,
            documentStore.Object,
            NullLogger<ShellDescriptorManager>.Instance);

        await manager.UpdateShellDescriptorAsync(1, [new ShellFeature("Explicit.Feature")]);

        Assert.Contains(mutableDescriptor.Features, feature => feature.Id == "Implicit.Feature" && feature.AlwaysEnabled);
        Assert.Contains(mutableDescriptor.Features, feature => feature.Id == "Explicit.Feature");
        Assert.Contains(mutableDescriptor.Installed, feature => feature.Id == "Implicit.Feature" && feature.AlwaysEnabled);
    }

    [Fact]
    public async Task ShellDescriptorFeaturesManager_UpdateFeaturesAsync_DoesNotDisableImplicitlyEnabledFeature()
    {
        var implicitFeature = CreateFeatureInfo("Implicit.Feature", isImplicitlyEnabled: true);
        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(manager => manager.GetFeatures()).Returns([implicitFeature]);
        var descriptorManager = new Mock<IShellDescriptorManager>(MockBehavior.Strict);
        var manager = new ShellDescriptorFeaturesManager(
            extensionManager.Object,
            [],
            descriptorManager.Object,
            NullLogger<ShellFeaturesManager>.Instance);
        var shellDescriptor = new ShellDescriptor
        {
            SerialNumber = 1,
            Features = [new ShellFeature("Implicit.Feature", alwaysEnabled: true)],
            Installed = [new InstalledShellFeature(new ShellFeature("Implicit.Feature", alwaysEnabled: true))],
        };

        await RunInShellScopeAsync(async () =>
        {
            var (disabledFeatures, enabledFeatures) = await manager.UpdateFeaturesAsync(shellDescriptor, [implicitFeature], [], force: false);

            Assert.Empty(disabledFeatures);
            Assert.Empty(enabledFeatures);
        });

        descriptorManager.Verify(
            descriptor => descriptor.UpdateShellDescriptorAsync(It.IsAny<int>(), It.IsAny<IEnumerable<ShellFeature>>()),
            Times.Never);
    }

    [Fact]
    public async Task ShellDescriptorFeaturesManager_UpdateFeaturesAsync_DoesNotAutoDisableImplicitDependencyOnlyFeature()
    {
        var implicitFeature = CreateFeatureInfo(
            "Implicit.Feature",
            isImplicitlyEnabled: true,
            enabledByDependencyOnly: true);
        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(manager => manager.GetFeatures()).Returns([implicitFeature]);
        var descriptorManager = new Mock<IShellDescriptorManager>(MockBehavior.Strict);
        var manager = new ShellDescriptorFeaturesManager(
            extensionManager.Object,
            [],
            descriptorManager.Object,
            NullLogger<ShellFeaturesManager>.Instance);
        var shellDescriptor = new ShellDescriptor
        {
            SerialNumber = 1,
            Features = [new ShellFeature("Implicit.Feature", alwaysEnabled: true)],
            Installed = [new InstalledShellFeature(new ShellFeature("Implicit.Feature", alwaysEnabled: true))],
        };

        await RunInShellScopeAsync(async () =>
        {
            var (disabledFeatures, enabledFeatures) = await manager.UpdateFeaturesAsync(shellDescriptor, [], [], force: false);

            Assert.Empty(disabledFeatures);
            Assert.Empty(enabledFeatures);
        });

        descriptorManager.Verify(
            descriptor => descriptor.UpdateShellDescriptorAsync(It.IsAny<int>(), It.IsAny<IEnumerable<ShellFeature>>()),
            Times.Never);
    }

    private static Mock<IExtensionManager> CreateExtensionManager(
        IEnumerable<IFeatureInfo> discoveredFeatures,
        IEnumerable<IFeatureInfo> loadedFeatures)
    {
        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(manager => manager.GetFeatures()).Returns(discoveredFeatures);
        extensionManager.Setup(manager => manager.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync(loadedFeatures);

        return extensionManager;
    }

    private static FeatureInfo CreateFeatureInfo(
        string id,
        bool isImplicitlyEnabled = false,
        bool enabledByDependencyOnly = false)
    {
        return new FeatureInfo(
            id,
            id,
            0,
            "Test",
            string.Empty,
            new ExtensionInfo("TestExtension"),
            [],
            defaultTenantOnly: false,
            isAlwaysEnabled: false,
            isImplicitlyEnabled,
            enabledByDependencyOnly);
    }

    private static ShellConfiguration CreateShellConfiguration()
    {
        return new ShellConfiguration(new ConfigurationBuilder().Build());
    }

    private static async Task RunInShellScopeAsync(Func<Task> execute)
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var shellContext = new ShellContext
        {
            ServiceProvider = serviceProvider,
            Settings = new ShellSettings { Name = "Test" },
        };

        var shellScope = new ShellScope(shellContext);
        await shellScope.UsingServiceScopeAsync(_ => execute());
    }
}
