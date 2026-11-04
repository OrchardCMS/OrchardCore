namespace OrchardCore.Recipes.Models;

/// <summary>
/// The outcome of validating a recipe without executing it.
/// </summary>
public sealed class RecipeValidationResult
{
    /// <summary>
    /// Gets whether no error was found. This doesn't guarantee that the execution succeeds, see <see cref="Notice"/>.
    /// </summary>
    public bool Valid { get; init; }

    /// <summary>
    /// Gets the statement of what a valid result doesn't prove.
    /// </summary>
    public string Notice { get; init; }

    /// <summary>
    /// Gets the number of steps of the recipe.
    /// </summary>
    public int StepCount { get; init; }

    /// <summary>
    /// Gets the number of findings that make the recipe invalid.
    /// </summary>
    public int ErrorCount { get; init; }

    /// <summary>
    /// Gets the number of findings about something that is likely wrong or can't be verified.
    /// </summary>
    public int WarningCount { get; init; }

    /// <summary>
    /// Gets the findings, in the order of the steps.
    /// </summary>
    public IReadOnlyList<RecipeValidationFinding> Findings { get; init; } = [];
}

/// <summary>
/// A problem or a limit that the validation of a recipe found.
/// </summary>
public sealed class RecipeValidationFinding
{
    /// <summary>
    /// Gets the severity, one of the values of <see cref="RecipeValidationSeverity"/>.
    /// </summary>
    public string Severity { get; init; }

    /// <summary>
    /// Gets the stable code of the finding, for automation.
    /// </summary>
    public string Code { get; init; }

    /// <summary>
    /// Gets the description of the finding.
    /// </summary>
    public string Message { get; init; }

    /// <summary>
    /// Gets the zero-based index of the step, or <see langword="null"/> when the finding is about the whole recipe.
    /// </summary>
    public int? StepIndex { get; init; }

    /// <summary>
    /// Gets the name of the step, if it has one.
    /// </summary>
    public string StepName { get; init; }
}

/// <summary>
/// The severities of a <see cref="RecipeValidationFinding"/>.
/// </summary>
public static class RecipeValidationSeverity
{
    /// <summary>
    /// The execution can't apply the recipe as written, which makes it invalid.
    /// </summary>
    public const string Error = "error";

    /// <summary>
    /// Something is likely wrong or can't be verified, which doesn't make the recipe invalid.
    /// </summary>
    public const string Warning = "warning";

    /// <summary>
    /// A limit of the validation for this step.
    /// </summary>
    public const string Info = "info";
}
