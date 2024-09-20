using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Environment.Extensions;
using OrchardCore.Spatial.Drivers;
using OrchardCore.Spatial.Fields;
using OrchardCore.Spatial.Settings;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Taxonomies;

public class DriverTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ShapeTable _shapeTable;

    public DriverTests()
    {
        IServiceCollection serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<ILoggerFactory, NullLoggerFactory>();
        serviceCollection.AddScoped<IThemeManager, ThemeManager>();
        serviceCollection.AddScoped<IShapeFactory, DefaultShapeFactory>();
        serviceCollection.AddScoped<IExtensionManager, StubExtensionManager>();
        serviceCollection.AddScoped<IShapeTableManager, TestShapeTableManager>();

        _shapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        serviceCollection.AddSingleton(_shapeTable);

        _serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public async Task TaxonomyFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(TaxonomyField))
                        .WithSettings(new TaxonomyFieldSettings
                        {
                            Hint = "Test Hint",
                            Required = true,
                            TaxonomyContentItemId = "TestContentId",
                            Unique = true,
                            LeavesOnly = true,
                            Open = true,
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new TaxonomyFieldSettingsDriver());
        var settings = (TaxonomyFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
        Assert.Equal("TestContentId", settings.TaxonomyContentItemId);
        Assert.True(settings.Unique);
        Assert.True(settings.LeavesOnly);
        Assert.True(settings.Open);
    }

    [Fact]
    public async Task TaxonomyFieldTagsEditorSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(TaxonomyField))
                        .WithSettings(new TaxonomyFieldTagsEditorSettings
                        {
                            Open = true,
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new TaxonomyFieldTagsEditorSettingsDriver());
        var settings = (TaxonomyFieldTagsEditorSettings)shapeResult.Shape;

        Assert.True(settings.Open);
    }

    #region Private methods
    private static ContentPartDefinition BuildContentPartWithField(Action<ContentPartFieldDefinitionBuilder> configuration)
    {        
        return new ContentPartDefinitionBuilder()
                .Named("SomeContentPart")
                .WithField("SomeField",
                    configuration)
                .Build();
    }
    private async Task<ShapeResult> GetShapeResult(ContentPartDefinition contentDefinition, IContentPartFieldDefinitionDisplayDriver driver)
    {
        var factory = _serviceProvider.GetService<IShapeFactory>();
        var partFieldDefinition = contentDefinition.Fields.First();

        var partFieldDefinitionShape = await factory.CreateAsync("ContentPartFieldDefinition_Edit", () =>
            ValueTask.FromResult<IShape>(new ZoneHolding(() => factory.CreateAsync("ContentZone"))));
        partFieldDefinitionShape.Properties["ContentField"] = partFieldDefinition;

        var editorContext = new BuildEditorContext(partFieldDefinitionShape, "", false, "", factory, null, null);

        var result = await driver.BuildEditorAsync(partFieldDefinition, editorContext);
        await result.ApplyAsync(editorContext);

        return (ShapeResult)result;
    }
    #endregion
}
