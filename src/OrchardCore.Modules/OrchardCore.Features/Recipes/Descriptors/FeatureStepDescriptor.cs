using System.Text.Json.Nodes;
using OrchardCore.Recipes.Services;

namespace OrchardCore.Features.Recipes.Descriptors;

/// <summary>
/// Describes the Feature recipe step that enables or disables features.
/// </summary>
public sealed class FeatureStepDescriptor : RecipeStepDescriptor
{
    private static readonly JsonObject _schema = new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["type"] = "object",
        ["title"] = "Feature",
        ["description"] = "Enables or disables features in the Orchard Core application.",
        ["required"] = new JsonArray("name"),
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["type"] = "string",
                ["const"] = "Feature",
                ["description"] = "The name of the recipe step.",
            },
            ["enable"] = new JsonObject
            {
                ["type"] = "array",
                ["description"] = "Array of feature IDs to enable.",
                ["items"] = new JsonObject { ["type"] = "string" },
            },
            ["disable"] = new JsonObject
            {
                ["type"] = "array",
                ["description"] = "Array of feature IDs to disable.",
                ["items"] = new JsonObject { ["type"] = "string" },
            },
        },
        ["additionalProperties"] = false,
    };

    /// <inheritdoc />
    public override string Name => "Feature";

    /// <inheritdoc />
    public override string DisplayName => "Feature";

    /// <inheritdoc />
    public override string Description => "Enables or disables features in the Orchard Core application.";

    /// <inheritdoc />
    public override string Category => "Configuration";

    /// <inheritdoc />
    public override ValueTask<JsonObject> GetSchemaAsync()
        => ValueTask.FromResult(_schema.DeepClone() as JsonObject);
}
