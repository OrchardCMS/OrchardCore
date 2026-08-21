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
    public async Task ComposeAsync_FeaturesPresent_ReturnsBlueprintWithDependencies()
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
    public async Task ComposeAsync_RequiredFeatureMissing_SkipsType()
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
    public async Task ComposeAsync_NotAllRequiredFeaturesAreEnabled_SkipsType()
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
    public void RequireFeatures_AbsentEmptyAndNamedAttributes_HaveExpectedCompositionSemantics()
    {
        Assert.Empty(RequireFeaturesAttribute.GetRequiredFeatureNamesForType(typeof(DummyType)));
        Assert.False(RequiredStartupAttribute.IsRequiredForType(typeof(DummyType)));

        Assert.Empty(RequireFeaturesAttribute.GetRequiredFeatureNamesForType(typeof(TypeWithEmptyRequiredFeatures)));
        Assert.False(RequiredStartupAttribute.IsRequiredForType(typeof(TypeWithEmptyRequiredFeatures)));

        Assert.Equal(["FeatureA", "FeatureB"], RequireFeaturesAttribute.GetRequiredFeatureNamesForType(typeof(TypeWithMultipleRequiredFeatures)));
        Assert.False(RequiredStartupAttribute.IsRequiredForType(typeof(TypeWithMultipleRequiredFeatures)));

        Assert.True(RequiredStartupAttribute.IsRequiredForType(typeof(RequiredStartupTypeWithRequiredFeatures)));
    }

    [Fact]
    public async Task ComposeAsync_EmptyRequiredFeaturesAndOwningFeatureDisabled_DoesNotCompose()
    {
        // Arrange
        var moduleFeature = CreateFeature("FeatureA");

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        extensionManager.Setup(m => m.GetFeatures())
            .Returns([moduleFeature]);

        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(moduleFeature))
            .Returns([typeof(TypeWithEmptyRequiredFeatures)]);

        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, Mock.Of<ILogger<CompositionStrategy>>());

        // Act
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, new ShellDescriptor());

        // Assert
        Assert.Empty(blueprint.Dependencies);
    }

    [Fact]
    public async Task ComposeAsync_EmptyRequiredFeaturesAndOwningFeatureEnabled_ComposesAsOwningFeature()
    {
        // Arrange
        var moduleFeature = CreateFeature("FeatureA");

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([moduleFeature]);
        extensionManager.Setup(m => m.GetFeatures())
            .Returns([moduleFeature]);

        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(moduleFeature))
            .Returns([typeof(TypeWithEmptyRequiredFeatures)]);

        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, Mock.Of<ILogger<CompositionStrategy>>());
        var descriptor = new ShellDescriptor
        {
            Features = [new ShellFeature { Id = moduleFeature.Id }],
        };

        // Act
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, descriptor);

        // Assert
        var dependency = Assert.Single(blueprint.Dependencies);
        Assert.Equal(typeof(TypeWithEmptyRequiredFeatures), dependency.Key);
        Assert.Equal(moduleFeature, Assert.Single(dependency.Value));
    }

    [Fact]
    public async Task ComposeAsync_RequiredStartupAndOwningFeatureDisabled_ComposesAsApplicationFeature()
    {
        // Arrange
        var moduleFeature = CreateFeature("FeatureA");
        var applicationFeature = CreateFeature(Application.DefaultFeatureId);

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        extensionManager.Setup(m => m.GetFeatures())
            .Returns([moduleFeature, applicationFeature]);

        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(moduleFeature))
            .Returns([typeof(RequiredStartupType)]);
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(applicationFeature))
            .Returns([]);

        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, Mock.Of<ILogger<CompositionStrategy>>());

        // Act
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, new ShellDescriptor());

        // Assert
        var dependency = Assert.Single(blueprint.Dependencies);
        Assert.Equal(typeof(RequiredStartupType), dependency.Key);
        Assert.Equal(applicationFeature, Assert.Single(dependency.Value));
    }

    [Fact]
    public async Task ComposeAsync_RequiredStartupAndNamedFeaturesDisabled_DoesNotCompose()
    {
        // Arrange
        var moduleFeature = CreateFeature("FeatureA");
        var applicationFeature = CreateFeature(Application.DefaultFeatureId);

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([]);
        extensionManager.Setup(m => m.GetFeatures())
            .Returns([moduleFeature, applicationFeature]);

        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(moduleFeature))
            .Returns([typeof(RequiredStartupTypeWithRequiredFeatures)]);
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(applicationFeature))
            .Returns([]);

        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, Mock.Of<ILogger<CompositionStrategy>>());

        // Act
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, new ShellDescriptor());

        // Assert
        Assert.Empty(blueprint.Dependencies);
    }

    [Fact]
    public async Task ComposeAsync_RequiredStartupAndNamedFeaturesEnabled_ComposesOnceAsApplicationFeature()
    {
        // Arrange
        var moduleFeature = CreateFeature("FeatureA");
        var requiredFeature = CreateFeature("FeatureB");
        var applicationFeature = CreateFeature(Application.DefaultFeatureId);

        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(m => m.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([moduleFeature, requiredFeature]);
        extensionManager.Setup(m => m.GetFeatures())
            .Returns([moduleFeature, requiredFeature, applicationFeature]);

        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(moduleFeature))
            .Returns([typeof(RequiredStartupTypeWithRequiredFeatures)]);
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(requiredFeature))
            .Returns([]);
        typeFeatureProvider.Setup(p => p.GetTypesForFeature(applicationFeature))
            .Returns([]);

        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, Mock.Of<ILogger<CompositionStrategy>>());
        var descriptor = new ShellDescriptor
        {
            Features =
            [
                new ShellFeature { Id = moduleFeature.Id },
                new ShellFeature { Id = requiredFeature.Id },
            ],
        };

        // Act
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, descriptor);

        // Assert
        var dependency = Assert.Single(blueprint.Dependencies);
        Assert.Equal(typeof(RequiredStartupTypeWithRequiredFeatures), dependency.Key);
        Assert.Equal(applicationFeature, Assert.Single(dependency.Value));
    }

    [Fact]
    public async Task ComposeAsyncIncludedType_AllRequiredFeaturesAreEnabled_Succeeds()
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
    public async Task ComposeAsyncLogsDebug_LoggerEnabled_Succeeds()
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
        var logMessages = logger.Invocations
            .Where(i => i.Method.Name == nameof(ILogger.Log))
            .Select(i => new { Level = (LogLevel)i.Arguments[0], Message = i.Arguments[2]?.ToString() })
            .ToList();

        Assert.Single(logMessages, m => m.Level == LogLevel.Debug && m.Message.Contains("Composing blueprint"));
        Assert.Single(logMessages, m => m.Level == LogLevel.Debug && m.Message.Contains("Done composing blueprint"));
    }

    [Fact]
    public async Task ComposeAsync_FeatureIsDefaultTenantOnlyAndTenantIsNotDefault_SkipsType()
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
    public async Task ComposeAsyncIncludesType_FeatureIsDefaultTenantOnlyAndTenantIsDefault_Succeeds()
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
    public async Task ComposeAsyncIncludesType_FeatureIsNotDefaultTenantOnlyAndTenantIsNotDefault_Succeeds()
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

    [RequireFeatures()]
    public class TypeWithEmptyRequiredFeatures;

    [RequireFeatures("MissingFeature")] // This feature will not be present in descriptor
    public class TypeWithRequiredFeature;

    [RequireFeatures("FeatureA", "FeatureB")]
    public class TypeWithMultipleRequiredFeatures;

    [RequiredStartup]
    public class RequiredStartupType;

    [RequiredStartup]
    [RequireFeatures("FeatureA", "FeatureB")]
    public class RequiredStartupTypeWithRequiredFeatures;

    private static IFeatureInfo CreateFeature(string id)
    {
        var feature = new Mock<IFeatureInfo>();
        feature.Setup(f => f.Id).Returns(id);
        return feature.Object;
    }
}
