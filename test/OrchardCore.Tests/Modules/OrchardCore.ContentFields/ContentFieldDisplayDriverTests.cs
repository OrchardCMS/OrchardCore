using OrchardCore.ContentFields.Drivers;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields;

public class ContentFieldDisplayDriverTests
{
    [Fact]
    public void GetSettings_FieldWithoutDisplaySettings_DisplaysNameAndHidesEmptyValue()
    {
        var partFieldDefinition = GetPartFieldDefinition(field => { });

        var settings = partFieldDefinition.GetSettings<ContentPartFieldSettings>();

        Assert.True(settings.DisplayFieldName);
        Assert.False(settings.AlwaysDisplay);
    }

    [Fact]
    public void WithDisplayFieldName_False_IsKeptWithOtherSettings()
    {
        var partFieldDefinition = GetPartFieldDefinition(field => field
            .WithDisplayName("Lead rating")
            .WithDisplayFieldName(false));

        var settings = partFieldDefinition.GetSettings<ContentPartFieldSettings>();

        Assert.False(settings.DisplayFieldName);
        Assert.Equal("Lead rating", settings.DisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task BuildDisplay_EmptyField_ReturnsNoResult(string text)
    {
        var result = await BuildDisplayAsync(text, alwaysDisplay: false);

        Assert.Null(result);
    }

    [Fact]
    public async Task BuildDisplay_EmptyFieldWithAlwaysDisplay_ReturnsResult()
    {
        var result = await BuildDisplayAsync(null, alwaysDisplay: true);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task BuildDisplay_FieldWithValue_ReturnsResult()
    {
        var result = await BuildDisplayAsync("Hot", alwaysDisplay: false);

        Assert.NotNull(result);
    }

    private static async Task<IDisplayResult> BuildDisplayAsync(string text, bool alwaysDisplay)
    {
        var partFieldDefinition = GetPartFieldDefinition(field => field.WithAlwaysDisplay(alwaysDisplay));
        var typePartDefinition = new ContentTypePartDefinition(partFieldDefinition.PartDefinition.Name, partFieldDefinition.PartDefinition, []);

        var contentPart = new ContentPart();
        contentPart.Apply(partFieldDefinition.Name, new TextField { Text = text });

        IContentFieldDisplayDriver driver = new TextFieldDisplayDriver(Mock.Of<IStringLocalizer<TextFieldDisplayDriver>>());
        var context = new BuildDisplayContext(null, OrchardCoreConstants.DisplayType.Detail, string.Empty, null, null, null);

        return await driver.BuildDisplayAsync(contentPart, partFieldDefinition, typePartDefinition, context);
    }

    private static ContentPartFieldDefinition GetPartFieldDefinition(Action<ContentPartFieldDefinitionBuilder> configuration)
    {
        return new ContentPartDefinitionBuilder()
            .Named("Lead")
            .WithField<TextField>("Rating", configuration)
            .Build()
            .Fields
            .First();
    }
}
