using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Default implementation of <see cref="IRecipeStepSchemaEvaluator"/> using the in-house
/// RecipeStepSchemaValidator for JSON Schema evaluation capabilities for recipe validation.
/// </summary>
public sealed class RecipeStepSchemaEvaluator : IRecipeStepSchemaEvaluator
{
    private readonly IRecipeSchemaService _schemaService;

    public RecipeStepSchemaEvaluator(IRecipeSchemaService schemaService)
    {
        ArgumentNullException.ThrowIfNull(schemaService);
        _schemaService = schemaService;
    }

    /// <inheritdoc />
    public RecipeSchemaEvaluationResult Evaluate(JsonNode data, RecipeStepSchema schema)
    {
        try
        {
            var result = RecipeStepSchemaValidator.Validate(schema, data);

            if (result.IsValid)
            {
                return RecipeSchemaEvaluationResult.Success();
            }

            var details = result.Errors.Select(error => new RecipeSchemaEvaluationDetail
            {
                InstanceLocation = "/",
                EvaluationPath = "/",
                Message = error,
                IsValid = false,
            }).ToList();

            return RecipeSchemaEvaluationResult.Failure(details, $"Schema validation failed with {details.Count} error(s).");
        }
        catch (Exception ex)
        {
            return RecipeSchemaEvaluationResult.Failure("/", $"Schema evaluation error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public RecipeSchemaEvaluationResult EvaluateStep(JsonNode stepData, string stepName)
    {
        ArgumentNullException.ThrowIfNull(stepData);
        ArgumentException.ThrowIfNullOrEmpty(stepName);

        var schema = _schemaService.GetStepSchema(stepName);
        if (schema is null)
        {
            // No schema available - consider valid by default.
            return RecipeSchemaEvaluationResult.Success($"No schema available for step '{stepName}'. Skipping validation.");
        }

        return Evaluate(stepData, schema);
    }

    /// <inheritdoc />
    public RecipeSchemaEvaluationResult EvaluateRecipe(JsonNode recipeData)
    {
        ArgumentNullException.ThrowIfNull(recipeData);

        var combinedSchema = _schemaService.GetRecipeSchema();
        return Evaluate(recipeData, combinedSchema);
    }
}
