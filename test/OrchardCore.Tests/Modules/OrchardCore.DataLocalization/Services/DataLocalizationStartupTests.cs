using OrchardCore.AdminMenu.Services;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.Contents.Services;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Localization.Data;
using OrchardCore.Modules;
using StartupBase = OrchardCore.Modules.StartupBase;

namespace OrchardCore.DataLocalization.Services.Tests;

public sealed class DataLocalizationStartupTests
{
    [Theory]
    [InlineData(false, false, 0)]
    [InlineData(false, true, 0)]
    [InlineData(true, false, 2)]
    [InlineData(true, true, 3)]
    public async Task ConfigureServices_FeatureCombinations_ResolvesExpectedProviders(
        bool dataLocalizationEnabled,
        bool adminMenuEnabled,
        int expectedProviderCount)
    {
        var contentsFeature = Mock.Of<IFeatureInfo>(feature => feature.Id == "OrchardCore.Contents");
        var extensionManager = new Mock<IExtensionManager>();
        extensionManager.Setup(manager => manager.LoadFeaturesAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync([contentsFeature]);
        extensionManager.Setup(manager => manager.GetFeatures()).Returns([contentsFeature]);

        // Discover the Contents module's data-localization startups so the real feature
        // composition applies their RequireFeatures attributes before registration.
        var startupTypes = typeof(DataLocalizationStartup).Assembly.GetExportedTypes()
            .Where(type => typeof(StartupBase).IsAssignableFrom(type)
                && RequireFeaturesAttribute.GetRequiredFeatureNamesForType(type)
                    .Contains("OrchardCore.DataLocalization"))
            .ToArray();
        var typeFeatureProvider = new Mock<ITypeFeatureProvider>();
        typeFeatureProvider.Setup(provider => provider.GetTypesForFeature(contentsFeature)).Returns(startupTypes);

        var descriptor = new ShellDescriptor
        {
            Features = [new ShellFeature { Id = contentsFeature.Id }],
        };
        if (dataLocalizationEnabled)
        {
            descriptor.Features.Add(new ShellFeature { Id = "OrchardCore.DataLocalization" });
        }

        if (adminMenuEnabled)
        {
            descriptor.Features.Add(new ShellFeature { Id = "OrchardCore.AdminMenu" });
        }

        var strategy = new CompositionStrategy(extensionManager.Object, typeFeatureProvider.Object, Mock.Of<ILogger<CompositionStrategy>>());
        var blueprint = await strategy.ComposeAsync(new ShellSettings { Name = "Test" }, descriptor);
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IContentDefinitionManager>());
        if (adminMenuEnabled)
        {
            services.AddSingleton(Mock.Of<IAdminMenuAccessor>());
        }

        foreach (var startupType in blueprint.Dependencies.Keys)
        {
            ((StartupBase)Activator.CreateInstance(startupType)).ConfigureServices(services);
        }

        using var serviceProvider = services.BuildServiceProvider(validateScopes: true);
        using var scope = serviceProvider.CreateScope();
        // The Data Localization admin controller resolves this same provider collection.
        var providers = scope.ServiceProvider.GetServices<ILocalizationDataProvider>().ToArray();

        Assert.Equal(expectedProviderCount, providers.Length);
        if (dataLocalizationEnabled)
        {
            Assert.Single(providers.OfType<ContentTypeDataLocalizationProvider>());
            Assert.Single(providers.OfType<ContentFieldDataLocalizationProvider>());
        }

        Assert.Equal(dataLocalizationEnabled && adminMenuEnabled,
            providers.Any(provider => provider is ContentTypesAdminNodeDataLocalizationProvider));
    }
}
