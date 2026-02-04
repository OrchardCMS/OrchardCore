using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Schema;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Default implementation of <see cref="IRecipeSchemaService"/> that aggregates schema information
/// from registered <see cref="IRecipeDeploymentStep"/> instances.
/// </summary>
public sealed class RecipeSchemaService : IRecipeSchemaService
{
    private readonly IEnumerable<IRecipeDeploymentStep> _steps;
    private RecipeStepSchema _combinedSchema;

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
    public RecipeStepSchema GetStepSchema(string stepName)
    {
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        var step = GetStep(stepName);
        return step?.Schema ?? CreateMinimalStepSchema(stepName);
    }

    /// <inheritdoc />
    public RecipeStepSchema GetRecipeSchema()
    {
        if (_combinedSchema is not null)
        {
            return _combinedSchema;
        }

        var stepSchemas = new Dictionary<string, RecipeStepSchema>(StringComparer.OrdinalIgnoreCase);

        foreach (var step in _steps)
        {
            if (string.IsNullOrEmpty(step.Name))
            {
                continue;
            }

            RecipeStepSchema existing = null;

            if (stepSchemas.TryGetValue(step.Name, out var existingSchema))
            {
                existing = existingSchema;
            }

            var stepSchema = step.Schema;

            if (stepSchema is not null)
            {
                if (existing is not null)
                {
                    // Combine schemas for steps with the same name using allOf.
                    stepSchema = new RecipeStepSchemaBuilder()
                        .AllOf(existing, stepSchema)
                        .Build();

                    stepSchemas[step.Name] = stepSchema;
                }
                else
                {
                    stepSchemas.Add(step.Name, stepSchema);
                }
            }
            else
            {
                if (existing is not null)
                {
                    continue;
                }

                // Create a minimal schema for steps without a defined schema.
                var minimalSchema = CreateMinimalStepSchema(step.Name);
                stepSchemas.Add(step.Name, minimalSchema);
            }
        }

        var processedStepSchemas = stepSchemas
            .Select(kv =>
            {
                var stepName = kv.Key;
                var stepSchema = kv.Value;

                // Build discriminator schema.
                var discriminatorSchema = new RecipeStepSchemaBuilder()
                    .TypeObject()
                    .Properties(
                        ("name", new RecipeStepSchemaBuilder()
                            .TypeString()
                            .Enum(stepSchemas.Keys)
                            .Description("The step name.")))
                    .Required("name")
                    .Build();

                // Wrap the schema with an AllOf to enforce name enum.
                return new RecipeStepSchemaBuilder()
                    .AllOf(discriminatorSchema, stepSchema)
                    .UnevaluatedProperties(false) // only allow defined properties.
                    .Build();
            })
            .ToArray();

        // Build the combined schema following JSON Schema specification.
        var combinedSchemaBuilder = new RecipeStepSchemaBuilder()
            .SchemaDraft202012()
            .Id("https://orchardcore.net/schemas/recipe.json")
            .Title("Orchard Core Recipe")
            .Description("Schema for Orchard Core recipe files that define configuration steps.")
            .TypeObject()
            .Properties(
                ("name", new RecipeStepSchemaBuilder().TypeString().Description("The unique name of the recipe.")),
                ("displayName", new RecipeStepSchemaBuilder().TypeString().Description("The display name of the recipe.")),
                ("description", new RecipeStepSchemaBuilder().TypeString().Description("A description of what the recipe does.")),
                ("author", new RecipeStepSchemaBuilder().TypeString().Description("The author of the recipe.")),
                ("website", new RecipeStepSchemaBuilder().TypeString().Description("The website URL associated with the recipe.")),
                ("version", new RecipeStepSchemaBuilder().TypeString().Description("The version of the recipe.")),
                ("issetuprecipe", new RecipeStepSchemaBuilder().TypeBoolean().Description("Indicates whether this is a setup recipe.")),
                ("categories", new RecipeStepSchemaBuilder().TypeArray().Items(new RecipeStepSchemaBuilder().TypeString()).Description("Categories this recipe belongs to.")),
                ("tags", new RecipeStepSchemaBuilder().TypeArray().Items(new RecipeStepSchemaBuilder().TypeString()).Description("Tags associated with this recipe.")),
                ("variables", new RecipeStepSchemaBuilder().TypeObject().Description("Variables that can be used in the recipe.").AdditionalProperties(RecipeStepSchema.Any)),
                ("steps", new RecipeStepSchemaBuilder()
                    .TypeArray()
                    .Description("The list of recipe steps to execute.")
                    .Items(processedStepSchemas.Length > 0
                        ? new RecipeStepSchemaBuilder().AnyOf(processedStepSchemas)
                        : new RecipeStepSchemaBuilder()
                            .TypeObject()
                            .Required("name")
                            .Properties(("name", new RecipeStepSchemaBuilder().TypeString().Enum(stepSchemas.Keys)))
                    ).MinItems(1))
            )
            .Required("steps");

        _combinedSchema = combinedSchemaBuilder.Build();

        return _combinedSchema;
    }

    /// <inheritdoc />
    public RecipeSchemaValidationResult ValidateRecipe(JsonNode recipe)
    {
        var schema = GetRecipeSchema();
        var result = RecipeStepSchemaValidator.Validate(schema, recipe);

        if (result.IsValid)
        {
            return RecipeSchemaValidationResult.Success();
        }

        var errors = result.Errors.Select(e => new RecipeSchemaValidationError
        {
            Path = "/",
            Message = e,
        }).ToList();

        return RecipeSchemaValidationResult.Failure(errors);
    }

    private static RecipeStepSchema CreateMinimalStepSchema(string stepName)
    {
        return new RecipeStepSchemaBuilder()
            .TypeObject()
            .Title(stepName)
            .Required("name")
            .Properties(
                ("name", new RecipeStepSchemaBuilder()
                    .TypeString()
                    .Const(stepName)))
            .AdditionalProperties(RecipeStepSchema.Any)
            .Build();
    }
}
