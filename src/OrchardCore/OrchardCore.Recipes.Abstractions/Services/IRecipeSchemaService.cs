using System.Text.Json.Nodes;
using Json.Schema;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Service for building and retrieving combined recipe schemas.
/// </summary>
public interface IRecipeSchemaService
{
    /// <summary>
    /// Gets all registered recipe deployment steps.
    /// </summary>
    /// <returns>A collection of recipe deployment steps.</returns>
    IEnumerable<IRecipeDeploymentStep> GetSteps();

    /// <summary>
    /// Gets a specific recipe deployment step by name.
    /// </summary>
    /// <param name="stepName">The name of the recipe step.</param>
    /// <returns>The recipe deployment step, or <c>null</c> if not found.</returns>
    IRecipeDeploymentStep GetStep(string stepName);

    /// <summary>
    /// Gets the JSON schema for a specific recipe step.
    /// </summary>
    /// <param name="stepName">The name of the recipe step.</param>
    /// <returns>The <see cref="JsonSchema"/> for the step, or <c>null</c> if not available.</returns>
    JsonSchema GetStepSchema(string stepName);

    /// <summary>
    /// Builds and returns a combined JSON schema for all registered recipe steps.
    /// This schema can be used for validation and IDE autocompletion.
    /// </summary>
    /// <returns>The combined <see cref="JsonSchema"/> for recipes.</returns>
    JsonSchema GetCombinedSchema();

    /// <summary>
    /// Validates a recipe against the combined schema.
    /// </summary>
    /// <param name="recipe">The recipe JSON to validate.</param>
    /// <returns>A <see cref="RecipeSchemaValidationResult"/> containing validation results.</returns>
    RecipeSchemaValidationResult ValidateRecipe(JsonNode recipe);
}
