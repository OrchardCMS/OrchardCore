using System.Text.Json.Nodes;
using Moq;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Templates.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Templates;

public class TemplateAlternatesTests
{
    [Fact]
    public async Task GetAlternatesAsync_Content_ListsEveryDisplayTypeOfEveryContentType()
    {
        var alternates = await CreateService().GetAlternatesAsync("Content", adminTemplates: false);

        Assert.Contains(alternates, alternate => alternate.Name == "Content__BlogPost" && alternate.ContentType == "BlogPost" && alternate.DisplayType == "Detail");
        Assert.Contains(alternates, alternate => alternate.Name == "Content_Summary__BlogPost" && alternate.DisplayType == "Summary");
        Assert.Contains(alternates, alternate => alternate.Name == "Content__Article");

        // A widget renders the Widget shape, not the Content one.
        Assert.DoesNotContain(alternates, alternate => alternate.Name.Contains("Ad", StringComparison.Ordinal));
    }

    [Fact]
    public async Task GetAlternatesAsync_TemplateOfDisplayType_ListsAlternatesOfThisDisplayType()
    {
        var names = (await CreateService().GetAlternatesAsync("Content_Summary", adminTemplates: false)).Select(alternate => alternate.Name).ToList();

        Assert.Contains("Content_Summary__BlogPost", names);
        Assert.DoesNotContain("Content__BlogPost", names);
    }

    [Fact]
    public async Task GetAlternatesAsync_Widget_ListsWidgetContentTypes()
    {
        var names = (await CreateService().GetAlternatesAsync("Widget", adminTemplates: false)).Select(alternate => alternate.Name).ToList();

        Assert.Contains("Widget__Ad", names);
        Assert.Contains("Widget_Summary__Ad", names);
    }

    [Fact]
    public async Task GetAlternatesAsync_Part_ListsPartAlternatesOnceForAllContentTypes()
    {
        var alternates = await CreateService().GetAlternatesAsync("HtmlBodyPart", adminTemplates: false);

        Assert.Contains(alternates, alternate => alternate.Name == "BlogPost__HtmlBodyPart" && alternate.ContentType == "BlogPost" && alternate.Element == "HtmlBodyPart");
        Assert.Contains(alternates, alternate => alternate.Name == "BlogPost_Summary__HtmlBodyPart");

        // Both content types have the part, so its display type alternate applies to all of them.
        var summary = Assert.Single(alternates, alternate => alternate.Name == "HtmlBodyPart_Summary");
        Assert.Null(summary.ContentType);
        Assert.DoesNotContain(alternates, alternate => alternate.Name == "HtmlBodyPart");
    }

    [Fact]
    public async Task GetAlternatesAsync_Field_ListsFieldAlternates()
    {
        var alternates = await CreateService().GetAlternatesAsync("TextField", adminTemplates: false);

        Assert.Contains(alternates, alternate => alternate.Name == "HtmlBodyPart__Description" && alternate.ContentType == null);
        Assert.Contains(alternates, alternate => alternate.Name == "BlogPost__HtmlBodyPart__Description" && alternate.Element == "HtmlBodyPart.Description");
    }

    [Fact]
    public async Task GetAlternatesAsync_AdminTemplates_ListsAdminDisplayTypes()
    {
        var names = (await CreateService().GetAlternatesAsync("Content", adminTemplates: true)).Select(alternate => alternate.Name).ToList();

        Assert.Contains("Content_SummaryAdmin__BlogPost", names);
        Assert.DoesNotContain("Content_Summary__BlogPost", names);
    }

    [Fact]
    public async Task GetTemplatesWithAlternatesAsync_ContentTypes_ListsTheirShapes()
    {
        var names = await CreateService().GetTemplatesWithAlternatesAsync(adminTemplates: false);

        Assert.Contains("Content", names);
        Assert.Contains("Content_Summary", names);
        Assert.Contains("Widget", names);
        Assert.Contains("HtmlBodyPart", names);
        Assert.Contains("TextField", names);
        Assert.DoesNotContain("Layout", names);
    }

    private static TemplateAlternatesService CreateService()
    {
        var description = new ContentPartFieldDefinition(new ContentFieldDefinition("TextField"), "Description", []);
        var htmlBodyPart = new ContentPartDefinition("HtmlBodyPart", [description], []);

        var definitions = new[]
        {
            new ContentTypeDefinition("BlogPost", "Blog Post",
            [
                new ContentTypePartDefinition("TitlePart", new ContentPartDefinition("TitlePart"), []),
                new ContentTypePartDefinition("HtmlBodyPart", htmlBodyPart, []),
            ], new JsonObject()),
            new ContentTypeDefinition("Article", "Article",
            [
                new ContentTypePartDefinition("HtmlBodyPart", htmlBodyPart, []),
            ], new JsonObject()),
            new ContentTypeDefinitionBuilder()
                .WithName("Ad")
                .Stereotype("Widget")
                .Build(),
        };

        var contentDefinitionManager = new Mock<IContentDefinitionManager>();
        contentDefinitionManager.Setup(manager => manager.ListTypeDefinitionsAsync()).ReturnsAsync(definitions);

        return new TemplateAlternatesService([new ContentTemplateAlternatesProvider(contentDefinitionManager.Object)]);
    }
}
