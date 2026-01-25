namespace OrchardCore.Recipes.Models;

/// <summary>
/// Represents the result of evaluating JSON data against a schema.
/// </summary>
public sealed class RecipeSchemaEvaluationResult
{
    /// <summary>
    /// Gets a value indicating whether the data is valid according to the schema.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the collection of evaluation details, including errors if any.
    /// </summary>
    public IReadOnlyList<RecipeSchemaEvaluationDetail> Details { get; init; } = [];

    /// <summary>
    /// Gets a summary message about the evaluation.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Creates a successful evaluation result.
    /// </summary>
    public static RecipeSchemaEvaluationResult Success(string message = null)
        => new() { IsValid = true, Message = message ?? "Schema validation passed." };

    /// <summary>
    /// Creates a failed evaluation result with the specified details.
    /// </summary>
    /// <param name="details">The evaluation details including errors.</param>
    /// <param name="message">Optional summary message.</param>
    public static RecipeSchemaEvaluationResult Failure(IEnumerable<RecipeSchemaEvaluationDetail> details, string message = null)
        => new()
        {
            IsValid = false,
            Details = details.ToList(),
            Message = message ?? "Schema validation failed.",
        };

    /// <summary>
    /// Creates a failed evaluation result with a single error.
    /// </summary>
    /// <param name="path">The JSON path where the error occurred.</param>
    /// <param name="message">The error message.</param>
    public static RecipeSchemaEvaluationResult Failure(string path, string message)
        => Failure([new RecipeSchemaEvaluationDetail { EvaluationPath = path, Message = message, IsValid = false }], message);
}

/// <summary>
/// Represents a single detail from schema evaluation.
/// </summary>
public sealed class RecipeSchemaEvaluationDetail
{
    /// <summary>
    /// Gets or sets the JSON path in the instance where this evaluation occurred.
    /// </summary>
    public string InstanceLocation { get; init; }

    /// <summary>
    /// Gets or sets the path in the schema that was evaluated.
    /// </summary>
    public string EvaluationPath { get; init; }

    /// <summary>
    /// Gets or sets whether this specific evaluation passed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets or sets the error message if validation failed.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets or sets the schema keyword that was evaluated (e.g., "required", "type", "pattern").
    /// </summary>
    public string SchemaKeyword { get; init; }

    /// <summary>
    /// Gets or sets nested evaluation details for hierarchical output.
    /// </summary>
    public IReadOnlyList<RecipeSchemaEvaluationDetail> NestedDetails { get; init; } = [];
}
