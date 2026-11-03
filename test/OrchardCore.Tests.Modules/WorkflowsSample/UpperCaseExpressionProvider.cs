using System.Globalization;
using OrchardCore.Localization;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace WorkflowsSample;

/// <summary>
/// A custom expression syntax: the text of the expression, in upper case.
/// </summary>
public sealed class UpperCaseExpressionProvider : IWorkflowExpressionProvider
{
    public static string Name => "UpperCase";

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<UpperCaseExpressionProvider>("Upper case");

    public static string EditorLanguage => "plaintext";

    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context)
        => Task.FromResult((T)Convert.ChangeType(expression.Expression?.ToUpperInvariant(), typeof(T), CultureInfo.InvariantCulture));

    public IReadOnlyList<string> Validate(string expression, Type valueType)
        => [];
}
