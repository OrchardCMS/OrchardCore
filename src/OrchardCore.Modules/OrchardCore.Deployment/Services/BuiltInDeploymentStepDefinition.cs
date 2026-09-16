using System.Text.Json.Nodes;
using OrchardCore.Deployment.Deployment;
using OrchardCore.Deployment.Steps;

namespace OrchardCore.Deployment.Services;

internal sealed class BuiltInDeploymentStepDefinition : IDeploymentStepDefinition
{
    public BuiltInDeploymentStepDefinition(string type) => Type = type;
    public string Type { get; }

    public JsonObject GetSchema()
    {
        var properties = Type switch
        {
            nameof(JsonRecipeDeploymentStep) => new JsonObject { ["json"] = StringProperty(writeOnly: true) },
            nameof(CustomFileDeploymentStep) => new JsonObject
            {
                ["fileName"] = StringProperty(), ["fileContent"] = StringProperty(writeOnly: true),
            },
            nameof(DeploymentPlanDeploymentStep) => new JsonObject
            {
                ["includeAll"] = new JsonObject { ["type"] = "boolean" },
                ["deploymentPlanNames"] = new JsonObject
                {
                    ["type"] = new JsonArray("array", "null"), ["items"] = new JsonObject { ["type"] = "string" },
                },
            },
            nameof(RecipeFileDeploymentStep) => new JsonObject
            {
                ["recipeName"] = StringProperty(), ["displayName"] = StringProperty(),
                ["description"] = StringProperty(), ["author"] = StringProperty(),
                ["webSite"] = StringProperty(), ["version"] = StringProperty(),
                ["isSetupRecipe"] = new JsonObject { ["type"] = "boolean" },
                ["categories"] = StringProperty(), ["tags"] = StringProperty(),
            },
            _ => throw new InvalidOperationException("No explicit built-in deployment contract exists for this type."),
        };
        return new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema", ["type"] = "object",
            ["additionalProperties"] = false, ["properties"] = properties,
        };
    }

    public JsonObject Describe(DeploymentStep step) => step switch
    {
        JsonRecipeDeploymentStep when Type == nameof(JsonRecipeDeploymentStep) => new(),
        CustomFileDeploymentStep file when Type == nameof(CustomFileDeploymentStep) => new() { ["fileName"] = file.FileName },
        DeploymentPlanDeploymentStep plans when Type == nameof(DeploymentPlanDeploymentStep) => new()
        {
            ["includeAll"] = plans.IncludeAll, ["deploymentPlanNames"] = Names(plans.DeploymentPlanNames),
        },
        RecipeFileDeploymentStep recipe when Type == nameof(RecipeFileDeploymentStep) => new()
        {
            ["recipeName"] = recipe.RecipeName, ["displayName"] = recipe.DisplayName,
            ["description"] = recipe.Description, ["author"] = recipe.Author, ["webSite"] = recipe.WebSite,
            ["version"] = recipe.Version, ["isSetupRecipe"] = recipe.IsSetupRecipe,
            ["categories"] = recipe.Categories, ["tags"] = recipe.Tags,
        },
        _ => throw new ArgumentException("The step does not match this configuration contract.", nameof(step)),
    };

    public ValueTask<IReadOnlyDictionary<string, string[]>> UpdateAsync(DeploymentStep step, JsonObject values) => ValueTask.FromResult(Update(step, values));

    public IReadOnlyDictionary<string, string[]> Update(DeploymentStep step, JsonObject values)
    {
        var errors = ValidatePatch(values);
        if (errors.Count > 0) { return errors; }
        switch (step)
        {
            case JsonRecipeDeploymentStep recipe when Type == nameof(JsonRecipeDeploymentStep):
                recipe.Json = Read(values, "json", recipe.Json);
                break;
            case CustomFileDeploymentStep file when Type == nameof(CustomFileDeploymentStep):
                file.FileName = Read(values, "fileName", file.FileName);
                file.FileContent = Read(values, "fileContent", file.FileContent);
                break;
            case DeploymentPlanDeploymentStep plans when Type == nameof(DeploymentPlanDeploymentStep):
                plans.IncludeAll = ReadBool(values, "includeAll", plans.IncludeAll);
                if (values.TryGetPropertyValue("deploymentPlanNames", out var names))
                {
                    plans.DeploymentPlanNames = names?.AsArray().Select(value => value.GetValue<string>()).ToArray() ?? [];
                }
                break;
            case RecipeFileDeploymentStep recipe when Type == nameof(RecipeFileDeploymentStep):
                recipe.RecipeName = Read(values, "recipeName", recipe.RecipeName);
                recipe.DisplayName = Read(values, "displayName", recipe.DisplayName);
                recipe.Description = Read(values, "description", recipe.Description);
                recipe.Author = Read(values, "author", recipe.Author);
                recipe.WebSite = Read(values, "webSite", recipe.WebSite);
                recipe.Version = Read(values, "version", recipe.Version);
                recipe.IsSetupRecipe = ReadBool(values, "isSetupRecipe", recipe.IsSetupRecipe);
                recipe.Categories = Read(values, "categories", recipe.Categories);
                recipe.Tags = Read(values, "tags", recipe.Tags);
                break;
            default:
                return new Dictionary<string, string[]> { ["type"] = ["The step does not match this configuration contract."] };
        }
        errors = DeploymentStepValidation.Validate(step);
        if (errors.Count == 0) { DeploymentStepValidation.Normalize(step); }
        return errors;
    }

    private Dictionary<string, string[]> ValidatePatch(JsonObject values)
    {
        var errors = new Dictionary<string, string[]>();
        if (values is null)
        {
            errors["values"] = ["A configuration object is required."];
            return errors;
        }
        var properties = GetSchema()["properties"].AsObject();
        foreach (var (name, value) in values)
        {
            if (!properties.TryGetPropertyValue(name, out var property))
            {
                errors[name] = ["This property is not managed by the step contract."];
                continue;
            }
            var type = property["type"];
            var expected = type is JsonArray types ? types[0].GetValue<string>() : type.GetValue<string>();
            var nullable = type is JsonArray;
            var valid = value is null ? nullable : expected switch
            {
                "string" => value is JsonValue text && text.TryGetValue<string>(out _),
                "boolean" => value is JsonValue flag && flag.TryGetValue<bool>(out _),
                "array" => value is JsonArray array && array.All(item => item is JsonValue text && text.TryGetValue<string>(out _)),
                _ => false,
            };
            if (!valid) { errors[name] = ["The value does not match the step property's declared type."]; }
        }
        return errors;
    }

    private static JsonObject StringProperty(bool writeOnly = false) => new()
    {
        ["type"] = new JsonArray("string", "null"), ["writeOnly"] = writeOnly,
    };
    private static string Read(JsonObject values, string name, string current) => values.TryGetPropertyValue(name, out var value) ? value?.GetValue<string>() : current;
    private static bool ReadBool(JsonObject values, string name, bool current) => values.TryGetPropertyValue(name, out var value) ? value.GetValue<bool>() : current;
    private static JsonArray Names(string[] names) => names is null ? null : new JsonArray(names.Select(name => (JsonNode)JsonValue.Create(name)).ToArray());
}
