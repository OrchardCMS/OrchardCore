using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Default implementation of <see cref="IRecipeSchemaService"/> that aggregates schema information
/// from registered <see cref="IRecipeStepDescriptor"/> and <see cref="IRecipeStepSchemaProvider"/> instances.
/// </summary>
public sealed class RecipeSchemaService : IRecipeSchemaService
{
    private readonly IEnumerable<IRecipeStepDescriptor> _stepDescriptors;
    private readonly IEnumerable<IRecipeStepSchemaProvider> _schemaProviders;

    public RecipeSchemaService(
        IEnumerable<IRecipeStepDescriptor> stepDescriptors,
        IEnumerable<IRecipeStepSchemaProvider> schemaProviders)
    {
        _stepDescriptors = stepDescriptors;
        _schemaProviders = schemaProviders;
    }

    /// <inheritdoc />
    public IEnumerable<IRecipeStepDescriptor> GetStepDescriptors()
        => _stepDescriptors;

    /// <inheritdoc />
    public IRecipeStepDescriptor GetStepDescriptor(string stepName)
        => _stepDescriptors.FirstOrDefault(d => string.Equals(d.Name, stepName, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public async ValueTask<JsonObject> GetStepSchemaAsync(string stepName)
    {
        // First, check if there's a schema provider for this step.
        var schemaProvider = _schemaProviders.FirstOrDefault(p =>
            string.Equals(p.StepName, stepName, StringComparison.OrdinalIgnoreCase));

        if (schemaProvider is not null)
        {
            var schema = await schemaProvider.GetSchemaAsync();
            if (schema is not null)
            {
                return schema;
            }
        }

        // Otherwise, get schema from the step descriptor.
        var descriptor = GetStepDescriptor(stepName);
        if (descriptor is not null)
        {
            return await descriptor.GetSchemaAsync();
        }

        return null;
    }

    /// <inheritdoc />
    public async ValueTask<JsonObject> GetCombinedSchemaAsync()
    {
        var stepSchemas = new JsonArray();

        foreach (var descriptor in _stepDescriptors)
        {
            var stepSchema = await GetStepSchemaAsync(descriptor.Name);
            if (stepSchema is not null)
            {
                stepSchemas.Add(stepSchema.DeepClone());
            }
            else
            {
                // Create a minimal schema for steps without a defined schema.
                var minimalSchema = CreateMinimalStepSchema(descriptor.Name);
                stepSchemas.Add(minimalSchema);
            }
        }

        // Build the combined schema following JSON Schema specification.
        var combinedSchema = new JsonObject
        {
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["$id"] = "https://orchardcore.net/schemas/recipe.json",
            ["title"] = "Orchard Core Recipe",
            ["description"] = "Schema for Orchard Core recipe files that define configuration steps.",
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The unique name of the recipe.",
                },
                ["displayName"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The display name of the recipe.",
                },
                ["description"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "A description of what the recipe does.",
                },
                ["author"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The author of the recipe.",
                },
                ["website"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The website URL associated with the recipe.",
                },
                ["version"] = new JsonObject
                {
                    ["type"] = "string",
                    ["description"] = "The version of the recipe.",
                },
                ["issetuprecipe"] = new JsonObject
                {
                    ["type"] = "boolean",
                    ["description"] = "Indicates whether this is a setup recipe.",
                },
                ["categories"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "string" },
                    ["description"] = "Categories this recipe belongs to.",
                },
                ["tags"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject { ["type"] = "string" },
                    ["description"] = "Tags associated with this recipe.",
                },
                ["variables"] = new JsonObject
                {
                    ["type"] = "object",
                    ["description"] = "Variables that can be used in the recipe.",
                    ["additionalProperties"] = true,
                },
                ["steps"] = new JsonObject
                {
                    ["type"] = "array",
                    ["description"] = "The list of recipe steps to execute.",
                    ["items"] = stepSchemas.Count > 0
                        ? new JsonObject { ["oneOf"] = stepSchemas }
                        : new JsonObject
                        {
                            ["type"] = "object",
                            ["required"] = new JsonArray("name"),
                            ["properties"] = new JsonObject
                            {
                                ["name"] = new JsonObject { ["type"] = "string" },
                            },
                        },
                },
            },
            ["required"] = new JsonArray("steps"),
        };

        return combinedSchema;
    }

    /// <inheritdoc />
    public async ValueTask<RecipeSchemaValidationResult> ValidateRecipeAsync(JsonObject recipe)
    {
        var errors = new List<RecipeSchemaValidationError>();

        // Validate that the recipe has a steps array.
        if (!recipe.TryGetPropertyValue("steps", out var stepsNode) || stepsNode is not JsonArray steps)
        {
            return RecipeSchemaValidationResult.Failure(new RecipeSchemaValidationError
            {
                Path = "/steps",
                Message = "Recipe must contain a 'steps' array.",
            });
        }

        // Validate each step.
        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            if (step is not JsonObject stepObject)
            {
                errors.Add(new RecipeSchemaValidationError
                {
                    Path = $"/steps/{i}",
                    Message = "Step must be a JSON object.",
                    StepIndex = i,
                });
                continue;
            }

            // Each step must have a name.
            if (!stepObject.TryGetPropertyValue("name", out var nameNode) ||
                nameNode is not JsonValue nameValue ||
                nameValue.GetValueKind() != System.Text.Json.JsonValueKind.String)
            {
                errors.Add(new RecipeSchemaValidationError
                {
                    Path = $"/steps/{i}/name",
                    Message = "Step must have a 'name' property of type string.",
                    StepIndex = i,
                });
                continue;
            }

            var stepName = nameValue.GetValue<string>();

            // Check if we have a descriptor for this step.
            var descriptor = GetStepDescriptor(stepName);
            if (descriptor is null)
            {
                // Step name not recognized - this is a warning, not an error,
                // as the step might be from an unregistered module.
                continue;
            }

            // Get the schema for this step and validate if available.
            var stepSchema = await GetStepSchemaAsync(stepName);
            if (stepSchema is not null)
            {
                var stepErrors = ValidateStepAgainstSchema(stepObject, stepSchema, i, stepName);
                errors.AddRange(stepErrors);
            }
        }

        return errors.Count == 0
            ? RecipeSchemaValidationResult.Success()
            : RecipeSchemaValidationResult.Failure(errors);
    }

    private static JsonObject CreateMinimalStepSchema(string stepName)
    {
        return new JsonObject
        {
            ["type"] = "object",
            ["title"] = stepName,
            ["required"] = new JsonArray("name"),
            ["properties"] = new JsonObject
            {
                ["name"] = new JsonObject
                {
                    ["type"] = "string",
                    ["const"] = stepName,
                },
            },
            ["additionalProperties"] = true,
        };
    }

    private static List<RecipeSchemaValidationError> ValidateStepAgainstSchema(
        JsonObject step,
        JsonObject schema,
        int stepIndex,
        string stepName)
    {
        var errors = new List<RecipeSchemaValidationError>();

        // Check required properties.
        if (schema.TryGetPropertyValue("required", out var requiredNode) && requiredNode is JsonArray requiredArray)
        {
            foreach (var required in requiredArray)
            {
                if (required is JsonValue requiredValue &&
                    requiredValue.GetValueKind() == System.Text.Json.JsonValueKind.String)
                {
                    var propertyName = requiredValue.GetValue<string>();
                    if (!step.ContainsKey(propertyName))
                    {
                        errors.Add(new RecipeSchemaValidationError
                        {
                            Path = $"/steps/{stepIndex}/{propertyName}",
                            Message = $"Required property '{propertyName}' is missing.",
                            StepIndex = stepIndex,
                            StepName = stepName,
                        });
                    }
                }
            }
        }

        // Note: Full JSON Schema validation would require a dedicated library.
        // This implementation provides basic validation for required properties.
        // For comprehensive validation, consider using a library like JsonSchema.Net.

        return errors;
    }
}
