using System.Text.Json.Nodes;
using OrchardCore.Deployment;

namespace OrchardCore.Contents.Deployment;

internal sealed class ContentDeploymentStepDefinition : IDeploymentStepDefinition
{
    public ContentDeploymentStepDefinition(string type) => Type = type;
    public string Type { get; }
    public JsonObject GetSchema()
    {
        var properties = new JsonObject { ["exportAsSetupRecipe"] = new JsonObject { ["type"] = "boolean" } };
        if (Type == nameof(ContentDeploymentStep))
        {
            properties["contentTypes"] = new JsonObject
            {
                ["type"] = new JsonArray("array", "null"), ["items"] = new JsonObject { ["type"] = "string" },
                ["description"] = "Content type names to select; null clears the selection. References may precede type creation in recipes.",
            };
        }
        return new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["type"] = "object",
            ["additionalProperties"] = false, ["properties"] = properties,
        };
    }
    public JsonObject Describe(DeploymentStep step) => step switch
    {
        AllContentDeploymentStep all when Type == nameof(AllContentDeploymentStep) => new() { ["exportAsSetupRecipe"] = all.ExportAsSetupRecipe },
        ContentDeploymentStep selected when Type == nameof(ContentDeploymentStep) => new()
        {
            ["exportAsSetupRecipe"] = selected.ExportAsSetupRecipe,
            ["contentTypes"] = selected.ContentTypes is null ? null : new JsonArray(selected.ContentTypes.Select(name => (JsonNode)JsonValue.Create(name)).ToArray()),
        },
        _ => throw new ArgumentException("The step does not match this content contract.", nameof(step)),
    };
    public ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (values is null) { errors["values"] = ["A configuration object is required."]; }
        else
        {
            foreach (var (key, value) in values)
            {
                if (key == "exportAsSetupRecipe" && value is JsonValue flag && flag.TryGetValue<bool>(out _)) { continue; }
                if (key == "contentTypes" && Type == nameof(ContentDeploymentStep)
                    && (value is null || value is JsonArray names && names.All(name => name is JsonValue text && text.TryGetValue<string>(out _)))) { continue; }
                errors[key] = ["This property is unknown or has an invalid type."];
            }
        }
        if (errors.Count == 0)
        {
            switch (step)
            {
                case AllContentDeploymentStep all when Type == nameof(AllContentDeploymentStep):
                    if (values.TryGetPropertyValue("exportAsSetupRecipe", out var flag)) { all.ExportAsSetupRecipe = flag.GetValue<bool>(); }
                    break;
                case ContentDeploymentStep selected when Type == nameof(ContentDeploymentStep):
                    var names = values.TryGetPropertyValue("contentTypes", out var types)
                        ? types?.AsArray().Select(name => name.GetValue<string>()).ToArray() ?? [] : selected.ContentTypes;
                    var export = values.TryGetPropertyValue("exportAsSetupRecipe", out var setup) ? setup.GetValue<bool>() : selected.ExportAsSetupRecipe;
                    ContentDeploymentStepEditor.Apply(selected, names, export);
                    break;
                default:
                    errors["type"] = ["The step does not match this content contract."];
                    break;
            }
        }
        return ValueTask.FromResult<IReadOnlyDictionary<string, string[]>>(errors);
    }
}
