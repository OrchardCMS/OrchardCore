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
    }

    [Fact]
    public async Task BooleanFieldSettingsShouldDeserialize()
    {
        var settings = new BooleanFieldSettings
        {
            DefaultValue = true,
            Hint = "Test Hint",
            Label = "Test Label",
        };

        var contentDefinition = GetContentPartDefinition<BooleanField>(field => field.WithSettings(settings));

        var shapeResult = await GetShapeResult<BooleanFieldSettingsDriver>(contentDefinition);

        var shape = (BooleanFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Label, shape.Label);
        Assert.Equal(settings.DefaultValue, shape.DefaultValue);
    }

    [Fact]
    public async Task DateFieldSettingsShouldDeserialize()
    {
        var settings = new DateFieldSettings
        {
            Hint = "Test Hint",
            Required = true,
        };

        var contentDefinition = GetContentPartDefinition<DateField>(field => field.WithSettings(settings));

        var shapeResult = await GetShapeResult<DateFieldSettingsDriver>(contentDefinition);

        var shape = (DateFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Required, shape.Required);
    }

    [Fact]
    public async Task DateTimeFieldSettingsShouldDeserialize()
    {
        var settings = new DateTimeFieldSettings
        {
            Hint = "Test Hint",
            Required = true,
        };

        var contentDefinition = GetContentPartDefinition<DateTimeField>(field => field.WithSettings(settings));
                        
        var shapeResult = await GetShapeResult<DateTimeFieldSettingsDriver>(contentDefinition);

        var shape = (DateTimeFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Required, shape.Required);
    }

    [Fact]
    public async Task LinkFieldSettingsShouldDeserialize()
    {
        var settings = new LinkFieldSettings
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
        };

        var contentDefinition = GetContentPartDefinition<LinkField>(field => field.WithSettings(settings));

        var shapeResult = await GetShapeResult<LinkFieldSettingsDriver>(contentDefinition);

        var shape = (LinkFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.HintLinkText, shape.HintLinkText);
        Assert.Equal(settings.Required, shape.Required);
        Assert.Equal(settings.LinkTextMode, shape.LinkTextMode);
        Assert.Equal(settings.UrlPlaceholder, shape.UrlPlaceholder);
        Assert.Equal(settings.TextPlaceholder, shape.TextPlaceholder);
        Assert.Equal(settings.DefaultUrl, shape.DefaultUrl);
        Assert.Equal(settings.DefaultText, shape.DefaultText);
        Assert.Equal(settings.DefaultTarget, shape.DefaultTarget);
    }

    [Fact]
    public async Task LocalizationSetContentPickerFieldSettingsShouldDeserialize()
    {
        var settings = new LocalizationSetContentPickerFieldSettings
        { 
            Hint = "Test Hint",
            Required = true,
            Multiple = true,
            DisplayedContentTypes = ["one", "two", "three"],
        };

        var contentDefinition = GetContentPartDefinition<LocalizationSetContentPickerField>(field => field.WithSettings(settings));

        var shapeResult = await GetShapeResult<LocalizationSetContentPickerFieldSettingsDriver>(contentDefinition);

        var shape = (LocalizationSetContentPickerFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Required, shape.Required);
        Assert.Equal(settings.Multiple, shape.Multiple);
        Assert.Equal<string[]>(settings.DisplayedContentTypes, shape.DisplayedContentTypes);
    }

    [Fact]
    public async Task NumericFieldSettingsShouldDeserialize()
    {
        var settings = new NumericFieldSettings
        {
            Hint = "Test Hint",
            Required = true,
            Scale = 4,
            Minimum = 3,
            Maximum = 1000,
            Placeholder = "Test Placeholder",
            DefaultValue = "Test Default Value",
        };

        var contentDefinition = GetContentPartDefinition<NumericField>(field => field.WithSettings(settings));

        var shapeResult = await GetShapeResult<NumericFieldSettingsDriver>(contentDefinition);

        var shape = (NumericFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Required, shape.Required);
        Assert.Equal(settings.Scale, shape.Scale);
        Assert.Equal(settings.Minimum, shape.Minimum);
        Assert.Equal(settings.Maximum, shape.Maximum);
        Assert.Equal(settings.Placeholder, shape.Placeholder);
        Assert.Equal(settings.DefaultValue, shape.DefaultValue);
    }

    [Fact]
    public async Task TextFieldSettingsShouldDeserialize()
    {
        var settings = new TextFieldSettings
        {
            DefaultValue = "Test Default",
            Hint = "Test Hint",
            Required = true,
        };
        
        var contentDefinition = GetContentPartDefinition<TextField>(field => field.WithSettings());

        var shapeResult = await GetShapeResult<TextFieldSettingsDriver>(contentDefinition);

        var shape = (TextFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.DefaultValue, shape.DefaultValue);
        Assert.Equal(settings.Required, shape.Required);
    }

    [Fact]
    public async Task TimeFieldSettingsShouldDeserialize()
    {
        var settings = new TimeFieldSettings
        {
            Hint = "Test Hint",
            Required = true,
            Step = "Test Step",
        };

        var contentDefinition = GetContentPartDefinition<TimeField>(field => field.WithSettings());

        var shapeResult = await GetShapeResult<TimeFieldSettingsDriver>(contentDefinition);

        var shape = (TimeFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Required, shape.Required);
        Assert.Equal(settings.Step, shape.Step);
    }

    [Fact]
    public async Task YoutubeFieldSettingsShouldDeserialize()
    {
        var settings = new YoutubeFieldSettings
        {
            Hint = "Test Hint",
            Label = "Test Label",
            Width = 1024,
            Height = 768,
            Required = true,
        };

        var contentDefinition = GetContentPartDefinition<YoutubeField>(field => field.WithSettings(settings));

        var shapeResult = await GetShapeResult<YoutubeFieldSettingsDriver>(contentDefinition);

        var shape = (YoutubeFieldSettings)shapeResult.Shape;

        Assert.Equal(settings.Hint, shape.Hint);
        Assert.Equal(settings.Label, shape.Label);
        Assert.Equal(settings.Width, shape.Width);
        Assert.Equal(settings.Height, shape.Height);
        Assert.Equal(settings.Required, shape.Required);
    }

    #region Private methods
    private static ContentPartDefinition GetContentPartDefinition<TField>(Action<ContentPartFieldDefinitionBuilder> configuration)
    {        
        return new ContentPartDefinitionBuilder()
            .Named("SomeContentPart")
            .WithField<TField>("SomeField", configuration)
            .Build();
    }
    
    private async Task<ShapeResult> GetShapeResult<TDriver>(ContentPartDefinition contentDefinition)
        where TDriver : new(), IContentPartFieldDefinitionDisplayDriver
    {
        var factory = _serviceProvider.GetService<IShapeFactory>();
        var partFieldDefinition = contentDefinition.Fields.First();

        var partFieldDefinitionShape = await factory.CreateAsync("ContentPartFieldDefinition_Edit", () =>
            ValueTask.FromResult<IShape>(new ZoneHolding(() => factory.CreateAsync("ContentZone"))));
        partFieldDefinitionShape.Properties["ContentField"] = partFieldDefinition;

        var editorContext = new BuildEditorContext(partFieldDefinitionShape, "", false, "", factory, null, null);

        var driver = new TDriver()
        var result = await driver.BuildEditorAsync(partFieldDefinition, editorContext);
        await result.ApplyAsync(editorContext);

        return (ShapeResult)result;
    }
    #endregion
}
