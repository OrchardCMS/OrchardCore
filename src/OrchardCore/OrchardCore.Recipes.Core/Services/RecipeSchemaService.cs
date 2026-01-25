using System.Text.Json.Nodes;
using Json.Schema;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Default implementation of <see cref="IRecipeSchemaService"/> that aggregates schema information
/// from registered <see cref="IRecipeDeploymentStep"/> instances.
/// </summary>
public sealed class RecipeSchemaService : IRecipeSchemaService
{
    private readonly IEnumerable<IRecipeDeploymentStep> _steps;
    private JsonSchema _combinedSchema;

    public RecipeSchemaService(IEnumerable<IRecipeDeploymentStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);
        _steps = steps;
    }

    /// <inheritdoc />
    public IEnumerable<IRecipeDeploymentStep> GetSteps()
        => _steps;

    /// <inheritdoc />
    public IRecipeDeploymentStep GetStep(string stepName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        return _steps.FirstOrDefault(s => string.Equals(s.Name, stepName, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public JsonSchema GetStepSchema(string stepName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        var step = GetStep(stepName);
        return step?.Schema ?? CreateMinimalStepSchema(stepName);
    }

    /// <inheritdoc />
    public JsonSchema GetCombinedSchema()
    {
        if (_combinedSchema is not null)
        {
            return _combinedSchema;
        }

        var stepSchemas = new List<JsonSchema>();

        foreach (var step in _steps)
        {
            var stepSchema = step.Schema;
            if (stepSchema is not null)
            {
                stepSchemas.Add(stepSchema);
            }
            else
            {
                // Create a minimal schema for steps without a defined schema.
                var minimalSchema = CreateMinimalStepSchema(step.Name);
                stepSchemas.Add(minimalSchema);
            }
        }

        // Build the combined schema following JSON Schema specification.
        var combinedSchemaBuilder = new JsonSchemaBuilder()
            .Schema(MetaSchemas.Draft202012Id)
            .Id("https://orchardcore.net/schemas/recipe.json")
            .Title("Orchard Core Recipe")
            .Description("Schema for Orchard Core recipe files that define configuration steps.")
            .Type(SchemaValueType.Object)
            .Properties(
                ("name", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The unique name of the recipe.")),
                ("displayName", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The display name of the recipe.")),
                ("description", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("A description of what the recipe does.")),
                ("author", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The author of the recipe.")),
                ("website", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The website URL associated with the recipe.")),
                ("version", new JsonSchemaBuilder().Type(SchemaValueType.String).Description("The version of the recipe.")),
                ("issetuprecipe", new JsonSchemaBuilder().Type(SchemaValueType.Boolean).Description("Indicates whether this is a setup recipe.")),
                ("categories", new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Type(SchemaValueType.String)).Description("Categories this recipe belongs to.")),
                ("tags", new JsonSchemaBuilder().Type(SchemaValueType.Array).Items(new JsonSchemaBuilder().Type(SchemaValueType.String)).Description("Tags associated with this recipe.")),
                ("variables", new JsonSchemaBuilder().Type(SchemaValueType.Object).Description("Variables that can be used in the recipe.").AdditionalProperties(JsonSchema.Empty)),
                ("steps", new JsonSchemaBuilder()
                    .Type(SchemaValueType.Array)
                    .Description("The list of recipe steps to execute.")
                    .Items(stepSchemas.Count > 0
                        ? new JsonSchemaBuilder().OneOf(stepSchemas)
                        : new JsonSchemaBuilder()
                            .Type(SchemaValueType.Object)
                            .Required("name")
                            .Properties(("name", new JsonSchemaBuilder().Type(SchemaValueType.String)))))
            )
            .Required("steps");

        _combinedSchema = combinedSchemaBuilder.Build();
        return _combinedSchema;
    }

    /// <inheritdoc />
    public RecipeSchemaValidationResult ValidateRecipe(JsonNode recipe)
    {
        var schema = GetCombinedSchema();
        var result = schema.Evaluate(recipe, new EvaluationOptions { OutputFormat = OutputFormat.List });

        if (result.IsValid)
        {
            return RecipeSchemaValidationResult.Success();
        }

        var errors = new List<RecipeSchemaValidationError>();
        CollectErrors(result, errors);

        return RecipeSchemaValidationResult.Failure(errors);
    }

    private static JsonSchema CreateMinimalStepSchema(string stepName)
    {
        return new JsonSchemaBuilder()
            .Type(SchemaValueType.Object)
            .Title(stepName)
            .Required("name")
            .Properties(
                ("name", new JsonSchemaBuilder()
                    .Type(SchemaValueType.String)
                    .Const(stepName)))
            .AdditionalProperties(JsonSchema.Empty)
            .Build();
    }

    private static void CollectErrors(EvaluationResults result, List<RecipeSchemaValidationError> errors)
    {
        if (!result.IsValid && result.Errors is not null)
        {
            foreach (var error in result.Errors)
            {
                errors.Add(new RecipeSchemaValidationError
                {
                    Path = result.InstanceLocation?.ToString() ?? "/",
                    Message = error.Value,
                });
            }
        }

        if (result.Details is not null)
        {
            foreach (var detail in result.Details)
            {
                CollectErrors(detail, errors);
            }
        }
    }
}
