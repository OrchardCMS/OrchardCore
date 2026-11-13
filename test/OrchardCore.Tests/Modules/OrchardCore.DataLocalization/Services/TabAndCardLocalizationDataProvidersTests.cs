using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Localization.Data;

[assembly: PlacementCatalog("OrchardCore.DataLocalization.Services.Tests.CatalogDriver", Locations = new[] { "Content:1#Settings;2%Details", "Content:2" }, Tabs = new[] { "Advanced" }, Cards = new[] { "Summary" })]
[assembly: PlacementCatalog("OrchardCore.DataLocalization.Services.Tests.CatalogBaseDriver`1", Locations = new[] { "Content:7#Capabilities;9", "Content:8#settings" })]
[assembly: PlacementCatalog("OrchardCore.DataLocalization.Services.Tests.CatalogContainer+NestedDriver", Locations = new[] { "Content:3#Nested" })]
[assembly: PlacementCatalog("OrchardCore.DataLocalization.Services.Tests.DisabledDriver", Locations = new[] { "Content:1#Disabled" })]

namespace OrchardCore.DataLocalization.Services.Tests;

/// <summary>
/// Tests the tab and card providers that the Data Localization module registers, through its startup, as they are
/// internal.
/// </summary>
public sealed class TabAndCardLocalizationDataProvidersTests
{
    [Fact]
    public async Task GetDescriptorsAsync_EnabledDriverTypes_ListsTheirCatalogNamesOnceWithTheDefaultGroup()
    {
        // Arrange
        var feature = CreateFeature();
        var typeFeatureProvider = Mock.Of<ITypeFeatureProvider>(provider =>
            provider.GetTypesForFeature(feature) == new[] { typeof(CatalogDriver), typeof(CatalogDerivedDriver), CatalogContainer.NestedDriverType });

        // Act
        var names = await GetNamesAsync(feature, typeFeatureProvider, new NullFileProvider());

        // Assert
        Assert.Equal(
            ["Advanced", "Capabilities", "Content", "Details", "Nested", "Settings", "Summary"],
            names.Order(StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetDescriptorsAsync_PlacementFile_ListsTheTabsAndCardsOfAllItsRules()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(Path.Combine(root, "Module"));
        await File.WriteAllTextAsync(Path.Combine(root, "Module", "placement.json"), """
            {
                "TextField": [ { "place": "Parts:1#Settings;1%Metadata" }, { "place": "Parts:2" } ],
                "TitlePart": [ { "place": "Parts:0#SEO", "contentType": [ "Article" ] } ]
            }
            """, TestContext.Current.CancellationToken);

        try
        {
            using var fileProvider = new PhysicalFileProvider(root);
            var feature = CreateFeature();
            var typeFeatureProvider = Mock.Of<ITypeFeatureProvider>(provider => provider.GetTypesForFeature(feature) == Array.Empty<Type>());

            // Act
            var names = await GetNamesAsync(feature, typeFeatureProvider, fileProvider);

            // Assert
            Assert.Equal(["Content", "Metadata", "SEO", "Settings"], names.Order(StringComparer.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static IFeatureInfo CreateFeature()
    {
        var extension = Mock.Of<IExtensionInfo>(extension => extension.Id == "Module" && extension.SubPath == "Module");

        return Mock.Of<IFeatureInfo>(feature => feature.Id == "Module" && feature.Extension == extension);
    }

    /// <summary>
    /// Gets the names of every data localization provider that the module registers, which all list tab and card names.
    /// </summary>
    private static async Task<string[]> GetNamesAsync(IFeatureInfo feature, ITypeFeatureProvider typeFeatureProvider, IFileProvider contentRoot)
    {
        var services = new ServiceCollection();
        new Startup().ConfigureServices(services);
        services.AddSingleton(Mock.Of<IShellFeaturesManager>(manager => manager.GetEnabledFeaturesAsync() == Task.FromResult<IEnumerable<IFeatureInfo>>(new[] { feature })));
        services.AddSingleton(typeFeatureProvider);
        services.AddSingleton(Mock.Of<IHostEnvironment>(environment => environment.ContentRootFileProvider == contentRoot));

        await using var serviceProvider = services.BuildServiceProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var descriptors = new List<DataLocalizedString>();
        foreach (var provider in scope.ServiceProvider.GetServices<ILocalizationDataProvider>())
        {
            descriptors.AddRange(await provider.GetDescriptorsAsync());
        }

        Assert.All(descriptors, descriptor => Assert.Equal(TabAndCardNames.DataLocalizationContext, descriptor.Context));

        return descriptors.Select(descriptor => descriptor.Name).ToArray();
    }
}

public class CatalogDriver;

public class CatalogBaseDriver<TModel>;

public sealed class CatalogDerivedDriver : CatalogBaseDriver<string>;

public sealed class DisabledDriver;

public sealed class CatalogContainer
{
    /// <summary>
    /// Gets the private driver, registered in DI as such.
    /// </summary>
    public static Type NestedDriverType => typeof(NestedDriver);

    private sealed class NestedDriver;
}
