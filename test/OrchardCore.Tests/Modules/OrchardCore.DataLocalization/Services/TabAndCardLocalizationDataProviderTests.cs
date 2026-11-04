using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;

[assembly: PlacementCatalog(typeof(OrchardCore.DataLocalization.Services.Tests.CatalogDriver), Locations = new[] { "Content:1#Settings;2%Details", "Content:2" }, Tabs = new[] { "Advanced" }, Cards = new[] { "Summary" })]
[assembly: PlacementCatalog(typeof(OrchardCore.DataLocalization.Services.Tests.CatalogBaseDriver<>), Locations = new[] { "Content:7#Capabilities;9" })]
[assembly: PlacementCatalog(typeof(OrchardCore.DataLocalization.Services.Tests.DisabledDriver), Locations = new[] { "Content:1#Disabled" })]

namespace OrchardCore.DataLocalization.Services.Tests;

public sealed class TabAndCardLocalizationDataProviderTests
{
    [Fact]
    public async Task GetDescriptorsAsync_Contributors_ListsEachNameOnceWithTheDefaultGroup()
    {
        // Arrange
        var provider = new TabAndCardLocalizationDataProvider(
        [
            CreateContributor("Settings", "Details", " "),
            CreateContributor("Settings", "Advanced"),
        ]);

        // Act
        var descriptors = (await provider.GetDescriptorsAsync()).ToArray();

        // Assert
        Assert.Equal(["Advanced", "Content", "Details", "Settings"], descriptors.Select(descriptor => descriptor.Name));
        Assert.All(descriptors, descriptor => Assert.Equal(TabAndCardNames.DataLocalizationContext, descriptor.Context));
    }

    [Fact]
    public async Task PlacementCatalogContributor_EnabledTypes_ListsTheirNamesAndThoseOfTheirBaseTypes()
    {
        // Arrange
        var feature = Mock.Of<IFeatureInfo>(feature => feature.Id == "Enabled");
        var shellFeaturesManager = Mock.Of<IShellFeaturesManager>(manager => manager.GetEnabledFeaturesAsync() == Task.FromResult<IEnumerable<IFeatureInfo>>(new[] { feature }));
        var typeFeatureProvider = Mock.Of<ITypeFeatureProvider>(provider => provider.GetTypesForFeature(feature) == new[] { typeof(CatalogDriver), typeof(CatalogDerivedDriver) });
        var contributor = new PlacementCatalogTabAndCardNameContributor(shellFeaturesManager, typeFeatureProvider);

        // Act
        var names = await contributor.GetNamesAsync();

        // Assert
        Assert.Equal(["Advanced", "Capabilities", "Details", "Settings", "Summary"], names.Order(StringComparer.Ordinal));
    }

    [Fact]
    public async Task PlacementFileContributor_PlacementFile_ListsTheTabsAndCardsOfAllItsRules()
    {
        // Arrange
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(Path.Combine(root, "Module"));
        await File.WriteAllTextAsync(Path.Combine(root, "Module", "placement.json"), """
            {
                "TextField": [ { "place": "Parts:1#Content;1%Metadata" }, { "place": "Parts:2" } ],
                "TitlePart": [ { "place": "Parts:0#SEO", "contentType": [ "Article" ] } ]
            }
            """);

        try
        {
            using var fileProvider = new PhysicalFileProvider(root);
            var extension = Mock.Of<IExtensionInfo>(extension => extension.Id == "Module" && extension.SubPath == "Module");
            var feature = Mock.Of<IFeatureInfo>(feature => feature.Id == "Module" && feature.Extension == extension);
            var shellFeaturesManager = Mock.Of<IShellFeaturesManager>(manager => manager.GetEnabledFeaturesAsync() == Task.FromResult<IEnumerable<IFeatureInfo>>(new[] { feature }));
            var hostEnvironment = Mock.Of<IHostEnvironment>(environment => environment.ContentRootFileProvider == fileProvider);
            var contributor = new PlacementFileTabAndCardNameContributor(hostEnvironment, shellFeaturesManager);

            // Act
            var names = await contributor.GetNamesAsync();

            // Assert
            Assert.Equal(["Content", "Metadata", "SEO"], names.Order(StringComparer.Ordinal));
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static ITabAndCardNameContributor CreateContributor(params string[] names)
        => Mock.Of<ITabAndCardNameContributor>(contributor => contributor.GetNamesAsync() == Task.FromResult<IEnumerable<string>>(names));
}

public class CatalogDriver;

public class CatalogBaseDriver<TModel>;

public sealed class CatalogDerivedDriver : CatalogBaseDriver<string>;

public sealed class DisabledDriver;
