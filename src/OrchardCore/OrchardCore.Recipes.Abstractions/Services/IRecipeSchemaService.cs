using System.Text.Json.Nodes;
using OrchardCore.Recipes.Models;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Service for building and retrieving combined recipe schemas.
/// </summary>
public interface IRecipeSchemaService
{
    /// <summary>
    /// Gets all registered recipe step descriptors.
    /// </summary>
    /// <returns>A collection of recipe step descriptors.</returns>
    IEnumerable<IRecipeStepDescriptor> GetStepDescriptors();

    /// <summary>
    /// Gets a specific recipe step descriptor by name.
    /// </summary>
    /// <param name="stepName">The name of the recipe step.</param>
    /// <returns>The recipe step descriptor, or <c>null</c> if not found.</returns>
    IRecipeStepDescriptor GetStepDescriptor(string stepName);

    /// <summary>
    /// Gets the JSON schema for a specific recipe step.
    /// </summary>
    /// <param name="stepName">The name of the recipe step.</param>
    /// <returns>A <see cref="JsonObject"/> representing the JSON schema, or <c>null</c> if not available.</returns>
    ValueTask<JsonObject> GetStepSchemaAsync(string stepName);

    /// <summary>
    /// Builds and returns a combined JSON schema for all registered recipe steps.
    /// This schema can be used for validation and IDE autocompletion.
    /// </summary>
    /// <returns>A <see cref="JsonObject"/> representing the combined recipe schema.</returns>
    ValueTask<JsonObject> GetCombinedSchemaAsync();

    /// <summary>
    /// Validates a recipe against the combined schema.
    /// </summary>
    /// <param name="recipe">The recipe JSON to validate.</param>
    /// <returns>A <see cref="RecipeSchemaValidationResult"/> containing validation results.</returns>
    ValueTask<RecipeSchemaValidationResult> ValidateRecipeAsync(JsonObject recipe);
}
