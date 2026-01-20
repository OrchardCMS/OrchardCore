using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;
using OrchardCore.Taxonomies.Drivers;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.Settings;
using OrchardCore.Tests.Modules.OrchardCore.ContentFields.Settings;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.Taxonomies;

public class SettingsDisplayDriverTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IShapeFactory _shapeFactory;
    private readonly ShapeTable _shapeTable;

    public SettingsDisplayDriverTests()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddScoped<ILoggerFactory, NullLoggerFactory>()
            .AddScoped<IThemeManager, ThemeManager>()
            .AddScoped<IShapeFactory, DefaultShapeFactory>()
            .AddScoped<IExtensionManager, StubExtensionManager>()
            .AddScoped<IShapeTableManager, TestShapeTableManager>();

        _shapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        serviceCollection.AddSingleton(_shapeTable);

        _serviceProvider = serviceCollection.BuildServiceProvider();

        _shapeFactory = _serviceProvider.GetRequiredService<IShapeFactory>();
    }

    [Fact]
    public async Task TaxonomyFieldSettingsShouldDeserialize()
    {
        var settings = new TaxonomyFieldSettings
        {
            Hint = "Test Hint",
            Required = true,
            TaxonomyContentItemId = "TestContentId",
            Unique = true,
            LeavesOnly = true,
            Open = true,
        };

        var contentDefinition = DisplayDriverTestHelper.GetContentPartDefinition<TaxonomyField>(field => field.WithSettings(settings));

        var shapeResult = await DisplayDriverTestHelper.GetShapeResultAsync<TaxonomyFieldSettingsDriver>(_shapeFactory, contentDefinition);

        var shape = (TaxonomyFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Required, shape.Required);
        Assert.Equal(settings.TaxonomyContentItemId, shape.TaxonomyContentItemId);
        Assert.Equal(settings.Unique, shape.Unique);
        Assert.Equal(settings.LeavesOnly, shape.LeavesOnly);
        Assert.Equal(settings.Open, shape.Open);
    }

    [Fact]
    public async Task TaxonomyFieldTagsEditorSettingsShouldDeserialize()
    {
        var settings = new TaxonomyFieldTagsEditorSettings
        {
            Open = true,
        };

        var contentDefinition = DisplayDriverTestHelper.GetContentPartDefinition<TaxonomyField>(field => field.WithSettings(settings));

        var shapeResult = await DisplayDriverTestHelper.GetShapeResultAsync<TaxonomyFieldTagsEditorSettingsDriver>(_shapeFactory, contentDefinition);

        var shape = (TaxonomyFieldTagsEditorSettings)shapeResult.Shape;

        Assert.Equal(settings.Open, shape.Open);
    }
}
