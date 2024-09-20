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
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Spatial;

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
    public async Task GeoPointFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(GeoPointField))
                        .WithSettings(new GeoPointFieldSettings
                        {
                            Hint = "Test Hint",
                            Required = true,
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new GeoPointFieldSettingsDriver());
        var settings = (GeoPointFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
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
