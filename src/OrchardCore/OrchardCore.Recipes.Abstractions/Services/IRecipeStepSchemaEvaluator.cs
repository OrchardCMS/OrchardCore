using System.Text.Json.Nodes;
using OrchardCore.Recipes.Schema;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Provides schema evaluation capabilities for validating recipe data against JSON schemas.
/// </summary>
public interface IRecipeStepSchemaEvaluator
{
    /// <summary>
    /// Evaluates JSON data against a schema.
    /// </summary>
    /// <param name="data">The JSON data to evaluate.</param>
    /// <param name="schema">The JSON schema to evaluate against.</param>
    /// <returns>The evaluation result containing validity and any errors.</returns>
    RecipeSchemaEvaluationResult Evaluate(JsonNode data, JsonSchema schema);

    /// <summary>
    /// Evaluates a recipe step against its schema.
    /// </summary>
    /// <param name="stepData">The step JSON data to evaluate.</param>
    /// <param name="stepName">The name of the recipe step.</param>
    /// <returns>The evaluation result containing validity and any errors.</returns>
    RecipeSchemaEvaluationResult EvaluateStep(JsonNode stepData, string stepName);

    /// <summary>
    /// Evaluates an entire recipe against the combined schema.
    /// </summary>
    /// <param name="recipeData">The recipe JSON data to evaluate.</param>
    /// <returns>The evaluation result containing validity and any errors.</returns>
    RecipeSchemaEvaluationResult EvaluateRecipe(JsonNode recipeData);
}
