namespace OrchardCore.Recipes.Models;

/// <summary>
/// Represents the result of validating a recipe against its schema.
/// </summary>
public sealed class RecipeSchemaValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the recipe is valid according to the schema.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the collection of validation errors, if any.
    /// </summary>
    public IReadOnlyList<RecipeSchemaValidationError> Errors { get; init; } = [];

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static RecipeSchemaValidationResult Success()
        => new() { IsValid = true };

    /// <summary>
    /// Creates a failed validation result with the specified errors.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public static RecipeSchemaValidationResult Failure(IEnumerable<RecipeSchemaValidationError> errors)
        => new() { IsValid = false, Errors = errors.ToList() };

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    /// <param name="error">The validation error.</param>
    public static RecipeSchemaValidationResult Failure(RecipeSchemaValidationError error)
        => new() { IsValid = false, Errors = [error] };
}

/// <summary>
/// Represents a single validation error in a recipe.
/// </summary>
public sealed class RecipeSchemaValidationError
{
    /// <summary>
    /// Gets or sets the JSON path where the error occurred.
    /// </summary>
    public string Path { get; init; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets or sets the step name where the error occurred, if applicable.
    /// </summary>
    public string StepName { get; init; }

    /// <summary>
    /// Gets or sets the step index where the error occurred, if applicable.
    /// </summary>
    public int? StepIndex { get; init; }
}
