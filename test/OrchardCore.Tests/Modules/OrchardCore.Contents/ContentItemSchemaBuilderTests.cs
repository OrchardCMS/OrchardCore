using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.Options;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.Contents.Models;
using OrchardCore.Contents.Services;
using OrchardCore.Forms.Models;
using OrchardCore.Markdown.Fields;
using OrchardCore.Media.Fields;
using OrchardCore.Spatial.Fields;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Markdown.Models;
using OrchardCore.Seo.Models;
using OrchardCore.Title.Models;

namespace OrchardCore.Tests.Modules.OrchardCore.Contents;

public class ContentItemSchemaBuilderTests
{
    [Fact]
    public void ContentItemsResponse_UsesCamelCaseEnvelopeWithDocumentContent()
    {
        var response = new ContentItemsResponse
        {
            Skip = 1,
            Take = 2,
            TotalCount = 3,
            Items = [new ContentItem { ContentType = "Article" }],
        };

        var json = JsonSerializer.SerializeToNode(response, JsonSerializerOptions.Default)!.AsObject();

        Assert.Equal(1, json["skip"]!.GetValue<int>());
        Assert.Equal(2, json["take"]!.GetValue<int>());
        Assert.Equal(3, json["totalCount"]!.GetValue<int>());
        Assert.NotNull(json["items"]);
        Assert.Null(json["Skip"]);
    }

    [Fact]
    public void BuildSchema_IncludesContentItemEnvelopeAndNestedContentSchemas()
    {
        var schema = BuildSchema();
        var properties = Assert.IsType<JsonObject>(schema["properties"]);

        Assert.Equal("Article", properties["ContentType"]?["const"]?.GetValue<string>());
        Assert.Contains(nameof(ContentItem.ContentItemId), properties);
        Assert.Contains(nameof(ContentItem.ContentItemVersionId), properties);
        Assert.Contains(nameof(ContentItem.DisplayText), properties);
        Assert.Contains(nameof(ContentItem.Latest), properties);
        Assert.Contains(nameof(ContentItem.Published), properties);
        Assert.Contains(nameof(ContentItem.ModifiedUtc), properties);
        Assert.Contains(nameof(ContentItem.PublishedUtc), properties);
        Assert.Contains(nameof(ContentItem.CreatedUtc), properties);
        Assert.Contains(nameof(ContentItem.Owner), properties);
        Assert.Contains(nameof(ContentItem.Author), properties);
        Assert.Equal(
            "The content type that defines the item's available parts and fields.",
            properties[nameof(ContentItem.ContentType)]?["description"]?.GetValue<string>());
        Assert.Equal(
            "The human-readable text used to identify the content item.",
            properties[nameof(ContentItem.DisplayText)]?["description"]?.GetValue<string>());
        Assert.Equal(
            "The identifier of the user who owns the content item.",
            properties[nameof(ContentItem.Owner)]?["description"]?.GetValue<string>());
        Assert.Equal(
            "The user name of the person who last modified this version.",
            properties[nameof(ContentItem.Author)]?["description"]?.GetValue<string>());

        var part = Assert.IsType<JsonObject>(properties["Article/Part~1"]);
        var partProperties = Assert.IsType<JsonObject>(part["properties"]);
        var details = Assert.IsType<JsonObject>(partProperties[nameof(SchemaPart.Details)]);
        Assert.Equal("Part details.", details["description"]?.GetValue<string>());
        Assert.NotNull(details["properties"]?[nameof(SchemaDetails.Name)]);

        var field = Assert.IsType<JsonObject>(partProperties["Related/Field~1"]);
        var fieldDetails = field["properties"]?[nameof(SchemaField.Details)];
        Assert.Equal("Field details.", fieldDetails?["description"]?.GetValue<string>());
        Assert.NotNull(fieldDetails?["properties"]?[nameof(SchemaDetails.Name)]);
    }

    [Fact]
    public void BuildSchema_RebasesNestedLocalReferences()
    {
        var schema = BuildSchema();
        var part = schema["properties"]?["Article/Part~1"];
        var field = part?["properties"]?["Related/Field~1"];
        var partReferences = GetReferences(part)
            .Where(reference => !reference.Contains("Related~1Field", StringComparison.Ordinal))
            .ToArray();
        var fieldReferences = GetReferences(field).ToArray();

        Assert.NotEmpty(partReferences);
        Assert.All(partReferences, reference =>
            Assert.StartsWith("#/properties/Article~1Part~01/", reference, StringComparison.Ordinal));
        Assert.NotEmpty(fieldReferences);
        Assert.All(fieldReferences, reference =>
            Assert.StartsWith(
                "#/properties/Article~1Part~01/properties/Related~1Field~01/",
                reference,
                StringComparison.Ordinal));
    }

    [Fact]
    public void BuildSchema_IncludesProductionContentFieldDescriptions()
    {
        var schema = BuildProductionFieldSchema();

        AssertFieldDescription(schema, nameof(BooleanField), nameof(BooleanField.Value), "The Boolean value.");
        AssertFieldDescription(schema, nameof(TextField), nameof(TextField.Text), "The plain text value.");
        AssertFieldDescription(schema, nameof(HtmlField), nameof(HtmlField.Html), "The HTML content.");
        AssertFieldDescription(schema, nameof(LinkField), nameof(LinkField.Url), "The link URL.");
        AssertFieldDescription(schema, nameof(LinkField), nameof(LinkField.Text), "The text displayed for the link.");
        AssertFieldDescription(schema, nameof(LinkField), nameof(LinkField.Target), "The browsing context in which to open the link, such as _blank.");
        AssertFieldDescription(schema, nameof(MultiTextField), nameof(MultiTextField.Values), "The selected text values.");
        AssertFieldDescription(schema, nameof(NumericField), nameof(NumericField.Value), "The numeric value, or null when unset.");
        AssertFieldDescription(schema, nameof(DateTimeField), nameof(DateTimeField.Value), "The date and time in ISO 8601 format, or null when unset.");
        AssertFieldDescription(schema, nameof(DateField), nameof(DateField.Value), "The date in ISO 8601 format, or null when unset.");
        AssertFieldDescription(schema, nameof(TimeField), nameof(TimeField.Value), "The time of day in hh:mm:ss format, or null when unset.");
        AssertFieldDescription(schema, nameof(YoutubeField), nameof(YoutubeField.EmbeddedAddress), "The URL used to embed the YouTube video.");
        AssertFieldDescription(schema, nameof(YoutubeField), nameof(YoutubeField.RawAddress), "The original YouTube video URL.");
        AssertFieldDescription(schema, nameof(ContentPickerField), nameof(ContentPickerField.ContentItemIds), "The IDs of the selected content items.");
        AssertFieldDescription(schema, nameof(LocalizationSetContentPickerField), nameof(LocalizationSetContentPickerField.LocalizationSets), "The localization set IDs of the selected content items.");
        AssertFieldDescription(schema, nameof(UserPickerField), nameof(UserPickerField.UserIds), "The IDs of the selected users.");
        AssertFieldDescription(schema, nameof(MediaField), nameof(MediaField.Paths), "The paths of the selected media assets.");
        AssertFieldDescription(schema, nameof(MediaField), nameof(MediaField.MediaTexts), "The alternative text values for the selected media assets, in the same order as Paths.");
        AssertFieldDescription(schema, nameof(MarkdownField), nameof(MarkdownField.Markdown), "The Markdown content.");
        AssertFieldDescription(schema, nameof(TaxonomyField), nameof(TaxonomyField.TaxonomyContentItemId), "The ID of the taxonomy content item.");
        AssertFieldDescription(schema, nameof(TaxonomyField), nameof(TaxonomyField.TermContentItemIds), "The content item IDs of the selected taxonomy terms.");
        AssertFieldDescription(schema, nameof(GeoPointField), nameof(GeoPointField.Latitude), "The latitude in decimal degrees from -90 to 90, or null when unset.");
        AssertFieldDescription(schema, nameof(GeoPointField), nameof(GeoPointField.Longitude), "The longitude in decimal degrees from -180 to 180, or null when unset.");
    }

    [Theory]
    [InlineData(nameof(TitlePart), nameof(TitlePart.Title), "The title displayed for the content item.")]
    [InlineData(nameof(AutoroutePart), nameof(AutoroutePart.Path), "The URL path for the content item, relative to the site's base URL.")]
    [InlineData(nameof(MarkdownBodyPart), nameof(MarkdownBodyPart.Markdown), "The Markdown source for the content body.")]
    public void BuildSchema_ProductionContentParts_IncludePropertyDescriptions(
        string partName,
        string propertyName,
        string expectedDescription)
    {
        var schema = BuildBlogPostSchema();
        var description = schema["properties"]?[partName]?["properties"]?[propertyName]?["description"];
        Assert.Equal(expectedDescription, description?.GetValue<string>());
    }

    [Fact]
    public void BuildSchema_ProductionContentParts_DescribeNestedPropertiesAndExcludeTransientCommands()
    {
        var schema = BuildNestedPartSchema();
        var properties = schema["properties"];

        Assert.Null(properties?[nameof(AutoroutePart)]?["properties"]?[nameof(AutoroutePart.SetHomepage)]);
        Assert.Null(properties?[nameof(AuditTrailPart)]?["properties"]?[nameof(AuditTrailPart.ShowComment)]);
        Assert.Equal(
            "The text displayed for the option.",
            properties?[nameof(SelectPart)]?["properties"]?[nameof(SelectPart.Options)]?["items"]?
                ["properties"]?[nameof(SelectOption.Text)]?["description"]?.GetValue<string>());
        Assert.Equal(
            "The form field whose value is evaluated.",
            properties?[nameof(FormInputElementVisibilityPart)]?["properties"]?
                [nameof(FormInputElementVisibilityPart.Groups)]?["items"]?["properties"]?
                [nameof(FormVisibilityRuleGroup.Rules)]?["items"]?["properties"]?
                [nameof(FormVisibilityRule.Field)]?["description"]?.GetValue<string>());
        Assert.Equal(
            "The value of the HTML meta element's name attribute.",
            properties?[nameof(SeoMetaPart)]?["properties"]?[nameof(SeoMetaPart.CustomMetaTags)]?
                ["items"]?["properties"]?[nameof(global::OrchardCore.ResourceManagement.MetaEntry.Name)]?
                ["description"]?.GetValue<string>());
    }

    private static JsonObject BuildSchema()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddContentPart<SchemaPart>();
        services.AddContentField<SchemaField>();

        using var serviceProvider = services.BuildServiceProvider();
        var contentOptions = serviceProvider.GetRequiredService<IOptions<ContentOptions>>().Value;
        var fieldDefinition = new ContentPartFieldDefinition(
            new ContentFieldDefinition(nameof(SchemaField)),
            "Related/Field~1",
            []);
        var partDefinition = new ContentPartDefinition(nameof(SchemaPart), [fieldDefinition], []);
        var typePartDefinition = new ContentTypePartDefinition("Article/Part~1", partDefinition, []);
        var typeDefinition = new ContentTypeDefinition("Article", "Article", [typePartDefinition], []);

        return ContentItemSchemaBuilder.BuildSchema(
            typeDefinition,
            contentOptions,
            new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
    }

    private static JsonObject BuildProductionFieldSchema()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddContentPart<SchemaPart>();
        services.AddContentField<BooleanField>();
        services.AddContentField<TextField>();
        services.AddContentField<HtmlField>();
        services.AddContentField<LinkField>();
        services.AddContentField<MultiTextField>();
        services.AddContentField<NumericField>();
        services.AddContentField<DateTimeField>();
        services.AddContentField<DateField>();
        services.AddContentField<TimeField>();
        services.AddContentField<YoutubeField>();
        services.AddContentField<ContentPickerField>();
        services.AddContentField<LocalizationSetContentPickerField>();
        services.AddContentField<UserPickerField>();
        services.AddContentField<MediaField>();
        services.AddContentField<MarkdownField>();
        services.AddContentField<TaxonomyField>();
        services.AddContentField<GeoPointField>();

        using var serviceProvider = services.BuildServiceProvider();
        var contentOptions = serviceProvider.GetRequiredService<IOptions<ContentOptions>>().Value;
        var fieldTypes = new[]
        {
            typeof(BooleanField),
            typeof(TextField),
            typeof(HtmlField),
            typeof(LinkField),
            typeof(MultiTextField),
            typeof(NumericField),
            typeof(DateTimeField),
            typeof(DateField),
            typeof(TimeField),
            typeof(YoutubeField),
            typeof(ContentPickerField),
            typeof(LocalizationSetContentPickerField),
            typeof(UserPickerField),
            typeof(MediaField),
            typeof(MarkdownField),
            typeof(TaxonomyField),
            typeof(GeoPointField),
        };
        var fieldDefinitions = fieldTypes
            .Select(fieldType => new ContentPartFieldDefinition(
                new ContentFieldDefinition(fieldType.Name),
                fieldType.Name,
                []))
            .ToArray();
        var partDefinition = new ContentPartDefinition(nameof(SchemaPart), fieldDefinitions, []);
        var typePartDefinition = new ContentTypePartDefinition(nameof(SchemaPart), partDefinition, []);
        var typeDefinition = new ContentTypeDefinition("Article", "Article", [typePartDefinition], []);

        return ContentItemSchemaBuilder.BuildSchema(
            typeDefinition,
            contentOptions,
            new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
    }

    private static JsonObject BuildBlogPostSchema()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddContentPart<TitlePart>();
        services.AddContentPart<AutoroutePart>();
        services.AddContentPart<MarkdownBodyPart>();

        using var serviceProvider = services.BuildServiceProvider();
        var contentOptions = serviceProvider.GetRequiredService<IOptions<ContentOptions>>().Value;
        var typeDefinition = new ContentTypeDefinition(
            "BlogPost",
            "Blog Post",
            [
                CreateTypePartDefinition(nameof(TitlePart)),
                CreateTypePartDefinition(nameof(AutoroutePart)),
                CreateTypePartDefinition(nameof(MarkdownBodyPart)),
            ],
            []);

        return ContentItemSchemaBuilder.BuildSchema(
            typeDefinition,
            contentOptions,
            new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
    }

    private static JsonObject BuildNestedPartSchema()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddContentPart<AutoroutePart>();
        services.AddContentPart<AuditTrailPart>();
        services.AddContentPart<SelectPart>();
        services.AddContentPart<FormInputElementVisibilityPart>();
        services.AddContentPart<SeoMetaPart>();

        using var serviceProvider = services.BuildServiceProvider();
        var contentOptions = serviceProvider.GetRequiredService<IOptions<ContentOptions>>().Value;
        var typeDefinition = new ContentTypeDefinition(
            "Nested",
            "Nested",
            [
                CreateTypePartDefinition(nameof(AutoroutePart)),
                CreateTypePartDefinition(nameof(AuditTrailPart)),
                CreateTypePartDefinition(nameof(SelectPart)),
                CreateTypePartDefinition(nameof(FormInputElementVisibilityPart)),
                CreateTypePartDefinition(nameof(SeoMetaPart)),
            ],
            []);

        return ContentItemSchemaBuilder.BuildSchema(
            typeDefinition,
            contentOptions,
            new JsonSerializerOptions { TypeInfoResolver = new DefaultJsonTypeInfoResolver() });
    }

    private static void AssertFieldDescription(
        JsonObject schema,
        string fieldName,
        string propertyName,
        string expected)
    {
        var description = schema["properties"]?[nameof(SchemaPart)]?["properties"]?[fieldName]?
            ["properties"]?[propertyName]?["description"]?.GetValue<string>();
        Assert.Equal(expected, description);
    }

    private static ContentTypePartDefinition CreateTypePartDefinition(string name) =>
        new(name, new ContentPartDefinition(name, [], []), []);

    private static IEnumerable<string> GetReferences(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            if (jsonObject["$ref"]?.GetValue<string>() is { } reference)
            {
                yield return reference;
            }

            foreach (var property in jsonObject)
            {
                if (property.Value is not null)
                {
                    foreach (var nestedReference in GetReferences(property.Value))
                    {
                        yield return nestedReference;
                    }
                }
            }
        }
        else if (node is JsonArray jsonArray)
        {
            foreach (var item in jsonArray)
            {
                if (item is not null)
                {
                    foreach (var nestedReference in GetReferences(item))
                    {
                        yield return nestedReference;
                    }
                }
            }
        }
    }

    private sealed class SchemaPart : ContentPart
    {
        [Description("Part details.")]
        public SchemaDetails Details { get; set; }
    }

    private sealed class SchemaField : ContentField
    {
        [Description("Field details.")]
        public SchemaDetails Details { get; set; }
    }

    private sealed class SchemaDetails
    {
        public string Name { get; set; }

        public SchemaDetails Child { get; set; }
    }
}
