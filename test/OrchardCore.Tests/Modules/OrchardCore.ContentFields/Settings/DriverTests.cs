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
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields.Settings;

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
    public async Task BooleanFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(BooleanField))
                        .WithSettings(new BooleanFieldSettings
                        {
                            DefaultValue = true,
                            Hint = "Test Hint",
                            Label = "Test Label",
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new BooleanFieldSettingsDriver());
        var settings = (BooleanFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.Equal("Test Label", settings.Label);
        Assert.True(settings.DefaultValue);
    }

    [Fact]
    public async Task DateFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(DateField))
                        .WithSettings(new DateFieldSettings
                        {
                            Hint = "Test Hint",
                            Required = true,
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new DateFieldSettingsDriver());
        var settings = (DateFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
    }

    [Fact]
    public async Task DateTimeFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(DateTimeField))
                        .WithSettings(new DateTimeFieldSettings
                        {
                            Hint = "Test Hint",
                            Required = true,
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new DateTimeFieldSettingsDriver());
        var settings = (DateTimeFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
    }

    [Fact]
    public async Task LinkFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(LinkField))
                        .WithSettings(new LinkFieldSettings
                        {
                            Hint = "Test Hint",
                            HintLinkText = "Test Hint Link Text",
                            Required = true,
                            LinkTextMode = LinkTextMode.Static,
                            UrlPlaceholder = "Test Url Placeholder",
                            TextPlaceholder = "Test Text Placeholder",
                            DefaultUrl = "https://www.orchardcore.net",
                            DefaultText = "Test Text",
                            DefaultTarget = "Test Target",
                        }));
        var shapeResult = await GetShapeResult(contentDefinition, new LinkFieldSettingsDriver());
        var settings = (LinkFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.Equal("Test Hint Link Text", settings.HintLinkText);
        Assert.True(settings.Required);
        Assert.Equal(LinkTextMode.Static, settings.LinkTextMode);
        Assert.Equal("Test Url Placeholder", settings.UrlPlaceholder);
        Assert.Equal("Test Text Placeholder", settings.TextPlaceholder);
        Assert.Equal("https://www.orchardcore.net", settings.DefaultUrl);
        Assert.Equal("Test Text", settings.DefaultText);
        Assert.Equal("Test Target", settings.DefaultTarget);
    }

    [Fact]
    public async Task LocalizationSetContentPickerFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(LocalizationSetContentPickerField))
                        .WithSettings(new LocalizationSetContentPickerFieldSettings
                        { 
                            Hint = "Test Hint",
                            Required = true,
                            Multiple = true,
                            DisplayedContentTypes = ["one", "two", "three"],
                        }));

        var shapeResult = await GetShapeResult(contentDefinition, new LocalizationSetContentPickerFieldSettingsDriver());
        var settings = (LocalizationSetContentPickerFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
        Assert.True(settings.Multiple);
        Assert.Equal<string[]>(["one", "two", "three"], settings.DisplayedContentTypes);
    }

    [Fact]
    public async Task NumericFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(NumericField))
                        .WithSettings(new NumericFieldSettings
                        {
                            Hint = "Test Hint",
                            Required = true,
                            Scale = 4,
                            Minimum = 3,
                            Maximum = 1000,
                            Placeholder = "Test Placeholder",
                            DefaultValue = "Test Default Value",
                        }));

        var shapeResult = await GetShapeResult(contentDefinition, new NumericFieldSettingsDriver());
        var settings = (NumericFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
        Assert.Equal(4, settings.Scale);
        Assert.Equal(3, settings.Minimum);
        Assert.Equal(1000, settings.Maximum);
        Assert.Equal("Test Placeholder", settings.Placeholder);
        Assert.Equal("Test Default Value", settings.DefaultValue);
    }

    [Fact]
    public async Task TextFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(TextField))
                        .WithSettings(new TextFieldSettings
                        {
                            DefaultValue = "Test Default",
                            Hint = "Test Hint",
                            Required = true,
                        }));

        var shapeResult = await GetShapeResult(contentDefinition, new TextFieldSettingsDriver());
        var settings = (TextFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.Equal("Test Default", settings.DefaultValue);
        Assert.True(settings.Required);
    }

    [Fact]
    public async Task TimeFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(TimeField))
                        .WithSettings(new TimeFieldSettings
                        {
                            Hint = "Test Hint",
                            Required = true,
                            Step = "Test Step",
                        }));

        var shapeResult = await GetShapeResult(contentDefinition, new TimeFieldSettingsDriver());
        var settings = (TimeFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.True(settings.Required);
        Assert.Equal("Test Step", settings.Step);
    }

    [Fact]
    public async Task YoutubeFieldSettingsShouldDeserialize()
    {
        var contentDefinition =
            BuildContentPartWithField(field => field
                        .OfType(nameof(YoutubeField))
                        .WithSettings(new YoutubeFieldSettings
                        {
                            Hint = "Test Hint",
                            Label = "Test Label",
                            Width = 1024,
                            Height = 768,
                            Required = true,
                        }));

        var shapeResult = await GetShapeResult(contentDefinition, new YoutubeFieldSettingsDriver());
        var settings = (YoutubeFieldSettings)shapeResult.Shape;

        Assert.Equal("Test Hint", settings.Hint);
        Assert.Equal("Test Label", settings.Label);
        Assert.Equal(1024, settings.Width);
        Assert.Equal(768, settings.Height);
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
