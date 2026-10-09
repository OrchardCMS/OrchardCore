using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Services;

/// <summary>
/// Reads the expression posted by a <c>WorkflowExpressionEditor</c> shape in an activity driver: its syntax must
/// be registered and allowed, and the provider of the syntax must accept its text.
/// </summary>
public sealed class WorkflowExpressionInputValidator
{
    private readonly IWorkflowExpressionManager _expressionManager;

    internal readonly IStringLocalizer S;

    public WorkflowExpressionInputValidator(IWorkflowExpressionManager expressionManager, IStringLocalizer<WorkflowExpressionInputValidator> localizer)
    {
        _expressionManager = expressionManager;
        S = localizer;
    }

    /// <summary>
    /// Returns the posted expression, with the name of its syntax as registered, and adds an error to
    /// <paramref name="modelState"/> for each problem, under <c>{prefix}.{name}.Expression</c> or
    /// <c>{prefix}.{name}.Syntax</c>.
    /// </summary>
    /// <param name="input">The posted input.</param>
    /// <param name="modelState">The model state of the editor.</param>
    /// <param name="prefix">The prefix of the editor (the driver's <c>Prefix</c>).</param>
    /// <param name="name">The name of the input's property in the view model.</param>
    /// <param name="options">What the input accepts.</param>
    public WorkflowExpression<T> Validate<T>(WorkflowExpressionInput input, ModelStateDictionary modelState, string prefix, string name, WorkflowExpressionInputOptions options)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(options);

        var syntax = string.IsNullOrWhiteSpace(input?.Syntax) ? options.DefaultSyntax : input.Syntax.Trim();
        var text = input?.Expression;
        var provider = _expressionManager.Get(syntax);
        var allowed = provider is not null && (options.Syntaxes.Count == 0 || options.Syntaxes.Contains(provider.Name, StringComparer.OrdinalIgnoreCase));

        if (provider is null)
        {
            modelState.AddModelError(prefix, $"{name}.{nameof(WorkflowExpressionInput.Syntax)}", S["The {0} syntax isn't available.", syntax]);
        }
        else if (!allowed)
        {
            modelState.AddModelError(prefix, $"{name}.{nameof(WorkflowExpressionInput.Syntax)}", S["{0} can't be written in {1}.", options.Label, provider.DisplayName]);
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            if (options.Required)
            {
                modelState.AddModelError(prefix, $"{name}.{nameof(WorkflowExpressionInput.Expression)}", S["{0} is required.", options.Label]);
            }
        }
        else if (allowed)
        {
            foreach (var error in provider.Validate(text, typeof(T)))
            {
                modelState.AddModelError(prefix, $"{name}.{nameof(WorkflowExpressionInput.Expression)}", error);
            }
        }

        return new WorkflowExpression<T>(text, provider?.Name ?? syntax);
    }
}

/// <summary>
/// What an expression input accepts.
/// </summary>
public sealed class WorkflowExpressionInputOptions
{
    /// <summary>
    /// The label of the input, used in the error messages.
    /// </summary>
    public string Label { get; init; }

    /// <summary>
    /// Whether an empty expression is an error.
    /// </summary>
    public bool Required { get; init; }

    /// <summary>
    /// The syntaxes the input accepts, or none for every registered syntax.
    /// </summary>
    public IReadOnlyCollection<string> Syntaxes { get; init; } = [];

    /// <summary>
    /// The syntax of an input posted without one.
    /// </summary>
    public string DefaultSyntax { get; init; } = WorkflowExpressionSyntaxes.JavaScript;
}
