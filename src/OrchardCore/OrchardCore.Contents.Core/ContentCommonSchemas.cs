using OrchardCore.Recipes.Schema;

namespace OrchardCore.Contents.Core;

public static class ContentCommonSchemas
{
    public static readonly RecipeStepSchema ContentItemSchema = new RecipeStepSchemaBuilder()
        .TypeObject()
        .Required("ContentItemId", "ContentType")
        .Properties(
            ("ContentItemId", new RecipeStepSchemaBuilder().TypeString().Description("The unique identifier for the content item.")),
            ("ContentItemVersionId", new RecipeStepSchemaBuilder().TypeString().Description("The version identifier for the content item.")),
            ("ContentType", new RecipeStepSchemaBuilder().TypeString().Description("The type of content item.")),
            ("DisplayText", new RecipeStepSchemaBuilder().TypeString().Description("The display text for the content item.")),
            ("Latest", new RecipeStepSchemaBuilder().TypeBoolean().Description("Whether this is the latest version.")),
            ("Published", new RecipeStepSchemaBuilder().TypeBoolean().Description("Whether this content item is published.")),
            ("ModifiedUtc", new RecipeStepSchemaBuilder().TypeString().Format("date-time").Description("The UTC date/time when the content item was last modified.")),
            ("PublishedUtc", new RecipeStepSchemaBuilder().TypeString().Format("date-time").Description("The UTC date/time when the content item was published.")),
            ("CreatedUtc", new RecipeStepSchemaBuilder().TypeString().Format("date-time").Description("The UTC date/time when the content item was created.")),
            ("Owner", new RecipeStepSchemaBuilder().TypeString().Description("The owner of the content item.")),
            ("Author", new RecipeStepSchemaBuilder().TypeString().Description("The author of the content item.")))
        .AdditionalProperties(true)
        .Build();
}
