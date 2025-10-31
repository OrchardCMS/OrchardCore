using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Shell;

public class CompositionStrategyTests
{
    [Fact]
    public async Task ComposeAsync_ReturnsBlueprintWithDependencies_WhenFeaturesPresent()
    {
        // Arrange
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(DummyType);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        // No required features for DummyType
        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Test" };
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.Equal(settings, blueprint.Settings);
        Assert.Equal(descriptor, blueprint.Descriptor);
        Assert.True(blueprint.Dependencies.ContainsKey(exportedType));
        Assert.Contains(feature, blueprint.Dependencies[exportedType]);
    }

    [Fact]
    public async Task ComposeAsync_SkipsType_WhenRequiredFeatureMissing()
    {
        // Arrange
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(TypeWithRequiredFeature);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Test" };
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.Empty(blueprint.Dependencies); // Should skip the type
    }

    [Fact]
    public async Task ComposeAsync_SkipsType_WhenNotAllRequiredFeaturesAreEnabled()
    {
        // Arrange
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(TypeWithMultipleRequiredFeatures);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Test" };
        // Only FeatureA is enabled, FeatureB is missing
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.Empty(blueprint.Dependencies); // Should skip the type
    }

    [Fact]
    public async Task ComposeAsync_IncludedType_WhenAllRequiredFeaturesAreEnabled()
    {
        // Arrange
        var featureAId = "FeatureA";
        var featureAMock = new Mock<IFeatureInfo>();
        featureAMock.Setup(f => f.Id).Returns(featureAId);
        var featureA = featureAMock.Object;

        var featureBId = "FeatureB";
        var featureBMock = new Mock<IFeatureInfo>();
        featureBMock.Setup(f => f.Id).Returns(featureBId);
        var featureB = featureBMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { featureA, featureB });

        var exportedType = typeof(TypeWithMultipleRequiredFeatures);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(featureA))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Test" };
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature>
            {
                new ShellFeature { Id = featureAId },
                new ShellFeature { Id = featureBId },
            },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.True(blueprint.Dependencies.ContainsKey(exportedType));
    }

    [Fact]
    public async Task ComposeAsync_LogsDebug_WhenLoggerEnabled()
    {
        // Arrange
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(DummyType);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        logger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Test" };
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        await strategy.ComposeAsync(settings, descriptor);

        // Assert
        logger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Composing blueprint")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        logger.Verify(l => l.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Done composing blueprint")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
    }

    [Fact]
    public async Task ComposeAsync_SkipsType_WhenFeatureIsDefaultTenantOnly_AndTenantIsNotDefault()
    {
        // Arrange - Bug #18244: DefaultTenantOnly feature types should be skipped on non-default tenants
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        featureMock.Setup(f => f.DefaultTenantOnly).Returns(true); // Feature is marked as DefaultTenantOnly
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(DummyType);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Tenant1" }; // Non-default tenant
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.Empty(blueprint.Dependencies); // Should skip the type from DefaultTenantOnly feature
    }

    [Fact]
    public async Task ComposeAsync_IncludesType_WhenFeatureIsDefaultTenantOnly_AndTenantIsDefault()
    {
        // Arrange
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        featureMock.Setup(f => f.DefaultTenantOnly).Returns(true); // Feature is marked as DefaultTenantOnly
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(DummyType);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = ShellSettings.DefaultShellName }; // Default tenant
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.True(blueprint.Dependencies.ContainsKey(exportedType)); // Should include the type for default tenant
        Assert.Contains(feature, blueprint.Dependencies[exportedType]);
    }

    [Fact]
    public async Task ComposeAsync_IncludesType_WhenFeatureIsNotDefaultTenantOnly_AndTenantIsNotDefault()
    {
        // Arrange
        var featureId = "FeatureA";
        var featureMock = new Mock<IFeatureInfo>();
        featureMock.Setup(f => f.Id).Returns(featureId);
        featureMock.Setup(f => f.DefaultTenantOnly).Returns(false); // Feature is NOT marked as DefaultTenantOnly
        var feature = featureMock.Object;

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new List<IFeatureInfo> { feature });

        var exportedType = typeof(DummyType);
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(feature))
            .Returns(new[] { exportedType });

        var logger = new Mock<ILogger<CompositionStrategy>>();
        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, logger.Object);

        var settings = new ShellSettings { Name = "Tenant1" }; // Non-default tenant
        var descriptor = new ShellDescriptor
        {
            Features = new List<ShellFeature> { new ShellFeature { Id = featureId } },
        };

        // Act
        var blueprint = await strategy.ComposeAsync(settings, descriptor);

        // Assert
        Assert.NotNull(blueprint);
        Assert.True(blueprint.Dependencies.ContainsKey(exportedType)); // Should include the type on non-default tenant
        Assert.Contains(feature, blueprint.Dependencies[exportedType]);
    }

    public class DummyType;

    [RequireFeatures("MissingFeature")] // This feature will not be present in descriptor
    public class TypeWithRequiredFeature;

    [RequireFeatures("FeatureA", "FeatureB")]
    public class TypeWithMultipleRequiredFeatures;
}
