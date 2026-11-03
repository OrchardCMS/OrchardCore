using System.Text.Json.Nodes;

namespace OrchardCore.Recipes.Models;

/// <summary>
/// The state of the validation of a recipe, which goes through its steps in order like an execution.
/// </summary>
public sealed class RecipeValidationContext
{
    /// <summary>
    /// Gets the validated recipe.
    /// </summary>
    public RecipeDescriptor Recipe { get; init; }

    /// <summary>
    /// Gets or sets the zero-based index of the current step, or <see langword="null"/> before the steps.
    /// </summary>
    public int? StepIndex { get; set; }

    /// <summary>
    /// Gets or sets the name of the current step.
    /// </summary>
    public string StepName { get; set; }

    /// <summary>
    /// Gets or sets the current step, in which scripts aren't evaluated.
    /// </summary>
    public JsonObject Step { get; set; }

    /// <summary>
    /// Gets the disabled features that an earlier step of the recipe would enable. The handlers they register only
    /// exist once they are enabled, so a step that has no handler yet can't be proven wrong.
    /// </summary>
    public ISet<string> PendingFeatures { get; } = new HashSet<string>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets whether an earlier step may enable features that can't be told in advance, like a command or an
    /// inner recipe.
    /// </summary>
    public bool FeaturesMayChange { get; set; }

    /// <summary>
    /// Gets the findings so far.
    /// </summary>
    public IList<RecipeValidationFinding> Findings { get; } = [];

    /// <summary>
    /// Adds a finding that makes the recipe invalid: the execution can't apply it as written.
    /// </summary>
    public void AddError(string code, string message) => Add(RecipeValidationSeverity.Error, code, message);

    /// <summary>
    /// Adds a finding that doesn't make the recipe invalid, about something that is likely wrong or can't be verified.
    /// </summary>
    public void AddWarning(string code, string message) => Add(RecipeValidationSeverity.Warning, code, message);

    /// <summary>
    /// Adds a finding that describes a limit of the validation for the current step.
    /// </summary>
    public void AddInfo(string code, string message) => Add(RecipeValidationSeverity.Info, code, message);

    /// <summary>
    /// Gets whether the execution replaces this value by evaluating a script, as in <c>[js:...]</c> or <c>[file:...]</c>.
    /// </summary>
    public static bool IsScript(string value)
    {
        if (value is null || !value.StartsWith('[') || !value.EndsWith(']'))
        {
            return false;
        }

        var separator = value.IndexOf(':');

        return separator > 1 && value[1..separator].All(char.IsLetter);
    }

    private void Add(string severity, string code, string message)
        => Findings.Add(new RecipeValidationFinding
        {
            Severity = severity,
            Code = code,
            Message = message,
            StepIndex = StepIndex,
            StepName = StepName,
        });
}
