using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Display.TagHelpers;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.Tests.Modules.OrchardCore.Admin;

namespace OrchardCore.Tests.Modules.OrchardCore.ContentFields;

public class ContentFieldDisplayTagHelperTests
{
    private const string FieldValue = "<div class=\"field\">Hot</div>";

    [Fact]
    public void GetSettings_FieldWithoutDisplaySettings_DisplaysNameAndHidesEmptyValue()
    {
        var settings = GetPartFieldDefinition(field => { }).GetSettings<ContentPartFieldSettings>();

        Assert.True(settings.DisplayFieldName);
        Assert.False(settings.AlwaysDisplay);
    }

    [Fact]
    public void WithDisplayFieldName_False_IsKeptWithOtherSettings()
    {
        var settings = GetPartFieldDefinition(field => field
            .WithDisplayName("Lead rating")
            .WithDisplayFieldName(false))
            .GetSettings<ContentPartFieldSettings>();

        Assert.False(settings.DisplayFieldName);
        Assert.Equal("Lead rating", settings.DisplayName);
    }

    [Fact]
    public async Task Process_EmptyField_RendersNothing()
    {
        var (output, displayHelper) = await ProcessAsync(field => { }, hasValue: false);

        Assert.Empty(output.Content.GetContent());
        displayHelper.Verify(helper => helper.ShapeExecuteAsync(It.IsAny<IShape>()), Times.Never);
    }

    [Fact]
    public async Task Process_EmptyFieldWithAlwaysDisplay_RendersField()
    {
        var (output, _) = await ProcessAsync(field => field.WithAlwaysDisplay().WithDisplayFieldName(false), hasValue: false);

        Assert.Equal(FieldValue, output.Content.GetContent());
    }

    [Fact]
    public async Task Process_FieldNameNotDisplayed_RendersValueOnly()
    {
        var (output, displayHelper) = await ProcessAsync(field => field.WithDisplayFieldName(false), hasValue: true);

        Assert.Equal(FieldValue, output.Content.GetContent());
        Assert.Null(output.TagName);
        displayHelper.Verify(helper => helper.ShapeExecuteAsync(It.IsAny<IShape>()), Times.Never);
    }

    [Fact]
    public async Task Process_FieldNameDisplayed_RendersShapeWithDefinitionAndValue()
    {
        IShape renderedShape = null;
        var (output, _) = await ProcessAsync(field => { }, hasValue: true, shape => renderedShape = shape);

        Assert.Equal("named", output.Content.GetContent());
        Assert.Equal(ContentFieldDisplayTagHelper.ShapeType, renderedShape.Metadata.Type);
        Assert.Equal("Rating", ((ContentPartFieldDefinition)renderedShape.Properties["PartFieldDefinition"]).Name);
        Assert.Equal(FieldValue, ((TagHelperContent)renderedShape.Properties["Content"]).GetContent());
    }

    private static async Task<(TagHelperOutput Output, Mock<IDisplayHelper> DisplayHelper)> ProcessAsync(
        Action<ContentPartFieldDefinitionBuilder> configuration,
        bool hasValue,
        Action<IShape> rendered = null)
    {
        var displayHelper = new Mock<IDisplayHelper>();
        displayHelper
            .Setup(helper => helper.ShapeExecuteAsync(It.IsAny<IShape>()))
            .Callback<IShape>(shape => rendered?.Invoke(shape))
            .ReturnsAsync(new HtmlString("named"));

        var tagHelper = new ContentFieldDisplayTagHelper(new AdminListTestShapeFactory(), displayHelper.Object)
        {
            Definition = GetPartFieldDefinition(configuration),
            HasValue = hasValue,
        };

        var context = new TagHelperContext("content-field-display", [], new Dictionary<object, object>(), "test");
        var output = new TagHelperOutput("content-field-display", [], (_, _) =>
            Task.FromResult<TagHelperContent>(new DefaultTagHelperContent().SetHtmlContent(FieldValue)));

        await tagHelper.ProcessAsync(context, output);

        return (output, displayHelper);
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
