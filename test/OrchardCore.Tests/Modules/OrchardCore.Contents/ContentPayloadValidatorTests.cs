using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Flows.Models;
using OrchardCore.Title.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentPayloadValidatorTests
{
    [Theory]
    [InlineData("{\"TitlePart\":{\"Title\":{}}}", "TitlePart.Title")]
    [InlineData("{\"Details\":{\"Custom\":{\"Values\":[true]}}}", "Details.Custom.Values[0]")]
    [InlineData("{\"Details\":{\"Custom\":{\"Enabled\":\"wrong\"}}}", "Details.Custom.Enabled")]
    [InlineData("{\"Details\":{\"Custom\":[]}}", "Details.Custom")]
    [InlineData("{\"BagPart\":{\"ContentItems\":[{\"ContentType\":\"Article\",\"TitlePart\":{\"Title\":{}}}]}}", "BagPart.ContentItems[0].TitlePart.Title")]
    [InlineData("{\"BagPart\":{\"contentItems\":[{\"ContentType\":\"Article\",\"TitlePart\":{\"Title\":{}}}]}}", "BagPart.contentItems[0].TitlePart.Title")]
    [InlineData("{\"BagPart\":{\"ContentItems\":[{\"TitlePart\":{\"Title\":\"Missing type\"}}]}}", "BagPart.ContentItems[0].ContentType")]
    public async Task Validate_RegisteredAndEmbeddedTypes_ReportPropertyPaths(string json, string path)
    {
        var validator = CreateValidator();
        var errors = new ModelStateDictionary();
        await validator.ValidateAsync(JsonNode.Parse(json).AsObject(), "Article", errors);
        Assert.False(errors.IsValid);
        Assert.Contains(path, errors.Keys);
    }

    [Fact]
    public async Task Validate_PartialPayloadAndExtensionData_AreAccepted()
    {
        var validator = CreateValidator();
        var errors = new ModelStateDictionary();
        await validator.ValidateAsync(JsonNode.Parse("""
            {"TitlePart":{"Title":"Valid","Extension":{"Anything":true}},
             "Details":{"Custom":{"Values":["one","two"],"Enabled":true}},
             "BagPart":{"ContentItems":[{"ContentType":"Article","TitlePart":{"Title":"Nested"}}]},
             "UnknownExtension":{"Dynamic":42}}
            """).AsObject(), "Article", errors);
        Assert.True(errors.IsValid);
    }

    private static ContentPayloadValidator CreateValidator()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddContentPart<TitlePart>();
        services.AddContentPart<BagPart>();
        services.AddContentField<CustomField>();
        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ContentOptions>>().Value;
        var definitions = new Mock<IContentDefinitionManager>();
        var field = new ContentPartFieldDefinition(new ContentFieldDefinition(nameof(CustomField)), "Custom", []);
        var definition = new ContentTypeDefinition("Article", "Article", [
            new ContentTypePartDefinition(nameof(TitlePart), new ContentPartDefinition(nameof(TitlePart), [], []), []),
            new ContentTypePartDefinition(nameof(BagPart), new ContentPartDefinition(nameof(BagPart), [], []), []),
            new ContentTypePartDefinition("Details", new ContentPartDefinition("Details", [field], []), []),
        ], []);
        definitions.Setup(manager => manager.GetTypeDefinitionAsync("Article")).ReturnsAsync(definition);
        return new ContentPayloadValidator(definitions.Object, options);
    }

    public sealed class CustomField : ContentField
    {
        public string[] Values { get; set; }
        public bool Enabled { get; set; }
    }
}
