using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Schema;

namespace OrchardCore.Recipes.Services;

/// <summary>
/// Defines a unified recipe step that handles both import (recipe execution) and export (deployment)
/// using JSON Schema for validation.
/// </summary>
public interface IRecipeDeploymentStep
{
    /// <summary>
    /// Gets the unique name of the step used in recipe JSON files.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the JSON Schema that defines this step's structure.
    /// Returns <c>null</c> if no schema is defined.
    /// </summary>
    RecipeStepSchema Schema { get; }

    /// <summary>
    /// Executes the recipe step during import.
    /// </summary>
    /// <param name="context">The execution context containing step data.</param>
    Task ExecuteAsync(RecipeExecutionContext context);

    /// <summary>
    /// Exports data during deployment.
    /// </summary>
    /// <param name="context">The export context to write to.</param>
    Task ExportAsync(RecipeExportContext context);
}
