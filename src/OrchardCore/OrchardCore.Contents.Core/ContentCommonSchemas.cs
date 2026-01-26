using Json.Schema;

namespace OrchardCore.Contents.Core;

public static class ContentCommonSchemas
{
    public readonly static JsonSchema ContentItemSchema = new JsonSchemaBuilder()
        .Type(SchemaValueType.Object)
        .Required("ContentItemId", "ContentType")
        .Properties(
            ("ContentItemId", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The unique identifier for the content item.")),
            ("ContentItemVersionId", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The version identifier for the content item.")),
            ("ContentType", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The type of content item.")),
            ("DisplayText", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The display text for the content item.")),
            ("Latest", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Whether this is the latest version.")),
            ("Published", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Whether this content item is published.")),
            ("ModifiedUtc", new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Description("The UTC date/time when the content item was last modified.")),
            ("PublishedUtc", new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Description("The UTC date/time when the content item was published.")),
            ("CreatedUtc", new JsonSchemaBuilder().Type(SchemaValueType.String).Format(Formats.DateTime).Description("The UTC date/time when the content item was created.")),
            ("Owner", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The owner of the content item.")),
            ("Author", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The author of the content item.")))
        .AdditionalProperties(JsonSchema.True)
        .Build();
}
