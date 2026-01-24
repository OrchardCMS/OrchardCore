using System.Text.Json.Nodes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Contents.Recipes.Descriptors;

/// <summary>
/// Describes the Content recipe step that imports content items.
/// </summary>
public sealed class ContentStepDescriptor : RecipeStepDescriptor
{
    private static readonly JsonObject _schema = new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = "object",
        ["title"] = "Content",
        ["description"] = "Imports content items into the Orchard Core application.",
        ["required"] = new JsonArray("name", "data"),
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["type"] = "string",
                ["const"] = "Content",
                ["description"] = "The name of the recipe step.",
            },
            ["data"] = new JsonObject
            {
                ["type"] = "array",
                ["description"] = "Array of content items to import.",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray("ContentItemId", "ContentType"),
                    ["properties"] = new JsonObject
                    {
                        ["ContentItemId"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The unique identifier for the content item.",
                        },
                        ["ContentItemVersionId"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The version identifier for the content item.",
                        },
                        ["ContentType"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The type of content item.",
                        },
                        ["DisplayText"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The display text for the content item.",
                        },
                        ["Latest"] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "Whether this is the latest version.",
                        },
                        ["Published"] = new JsonObject
                        {
                            ["type"] = "boolean",
                            ["description"] = "Whether this content item is published.",
                        },
                        ["ModifiedUtc"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["format"] = "date-time",
                            ["description"] = "The UTC date/time when the content item was last modified.",
                        },
                        ["PublishedUtc"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["format"] = "date-time",
                            ["description"] = "The UTC date/time when the content item was published.",
                        },
                        ["CreatedUtc"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["format"] = "date-time",
                            ["description"] = "The UTC date/time when the content item was created.",
                        },
                        ["Owner"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The owner of the content item.",
                        },
                        ["Author"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The author of the content item.",
                        },
                    },
                    ["additionalProperties"] = true,
                },
            },
        },
        ["additionalProperties"] = false,
    };

    /// <inheritdoc />
    public override string Name => "Content";

    /// <inheritdoc />
    public override string DisplayName => "Content";

    /// <inheritdoc />
    public override string Description => "Imports content items into the Orchard Core application.";

    /// <inheritdoc />
    public override string Category => "Content";

    /// <inheritdoc />
    public override ValueTask<JsonObject> GetSchemaAsync()
        => ValueTask.FromResult(_schema.DeepClone() as JsonObject);
}
