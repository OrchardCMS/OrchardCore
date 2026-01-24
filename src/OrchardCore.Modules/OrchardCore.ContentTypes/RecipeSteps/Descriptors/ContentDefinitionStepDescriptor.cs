using System.Text.Json.Nodes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.ContentTypes.RecipeSteps.Descriptors;

/// <summary>
/// Describes the ContentDefinition recipe step that creates or updates content type definitions.
/// </summary>
#pragma warning disable CA1507 // Use nameof - these are JSON property names from recipe schema, not C# property references
public sealed class ContentDefinitionStepDescriptor : RecipeStepDescriptor
{
    private static readonly JsonObject _schema = new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = "object",
        ["title"] = "ContentDefinition",
        ["description"] = "Creates or updates content type and part definitions.",
        ["required"] = new JsonArray("name"),
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["type"] = "string",
                ["const"] = "ContentDefinition",
                ["description"] = "The name of the recipe step.",
            },
            ["ContentTypes"] = new JsonObject
            {
                ["type"] = "array",
                ["description"] = "Array of content type definitions to create or update.",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray("Name"),
                    ["properties"] = new JsonObject
                    {
                        ["Name"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The technical name of the content type.",
                        },
                        ["DisplayName"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The display name of the content type.",
                        },
                        ["Settings"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["description"] = "Content type settings.",
                            ["additionalProperties"] = true,
                        },
                        ["ContentTypePartDefinitionRecords"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Parts attached to this content type.",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "object",
                                ["properties"] = new JsonObject
                                {
                                    ["Name"] = new JsonObject
                                    {
                                        ["type"] = "string",
                                        ["description"] = "The name of the part instance on this type.",
                                    },
                                    ["PartName"] = new JsonObject
                                    {
                                        ["type"] = "string",
                                        ["description"] = "The technical name of the content part.",
                                    },
                                    ["Settings"] = new JsonObject
                                    {
                                        ["type"] = "object",
                                        ["description"] = "Part settings for this content type.",
                                        ["additionalProperties"] = true,
                                    },
                                },
                            },
                        },
                    },
                    ["additionalProperties"] = true,
                },
            },
            ["ContentParts"] = new JsonObject
            {
                ["type"] = "array",
                ["description"] = "Array of content part definitions to create or update.",
                ["items"] = new JsonObject
                {
                    ["type"] = "object",
                    ["required"] = new JsonArray("Name"),
                    ["properties"] = new JsonObject
                    {
                        ["Name"] = new JsonObject
                        {
                            ["type"] = "string",
                            ["description"] = "The technical name of the content part.",
                        },
                        ["Settings"] = new JsonObject
                        {
                            ["type"] = "object",
                            ["description"] = "Content part settings.",
                            ["additionalProperties"] = true,
                        },
                        ["ContentPartFieldDefinitionRecords"] = new JsonObject
                        {
                            ["type"] = "array",
                            ["description"] = "Fields attached to this content part.",
                            ["items"] = new JsonObject
                            {
                                ["type"] = "object",
                                ["properties"] = new JsonObject
                                {
                                    ["Name"] = new JsonObject
                                    {
                                        ["type"] = "string",
                                        ["description"] = "The name of the field.",
                                    },
                                    ["FieldName"] = new JsonObject
                                    {
                                        ["type"] = "string",
                                        ["description"] = "The type of the field (e.g., TextField, HtmlField).",
                                    },
                                    ["Settings"] = new JsonObject
                                    {
                                        ["type"] = "object",
                                        ["description"] = "Field settings.",
                                        ["additionalProperties"] = true,
                                    },
                                },
                            },
                        },
                    },
                    ["additionalProperties"] = true,
                },
            },
        },
        ["additionalProperties"] = false,
    };

    /// <inheritdoc />
    public override string Name => "ContentDefinition";

    /// <inheritdoc />
    public override string DisplayName => "Content Definition";

    /// <inheritdoc />
    public override string Description => "Creates or updates content type and part definitions.";

    /// <inheritdoc />
    public override string Category => "Content Types";

    /// <inheritdoc />
    public override ValueTask<JsonObject> GetSchemaAsync()
        => ValueTask.FromResult(_schema.DeepClone() as JsonObject);
}
#pragma warning restore CA1507
