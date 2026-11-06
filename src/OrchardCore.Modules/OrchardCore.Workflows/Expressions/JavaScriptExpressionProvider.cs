using OrchardCore.Localization;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions;

/// <summary>
/// A JavaScript expression, evaluated by <see cref="IWorkflowScriptEvaluator"/>.
/// </summary>
public sealed class JavaScriptExpressionProvider : IWorkflowExpressionProvider
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;

    public JavaScriptExpressionProvider(IWorkflowScriptEvaluator scriptEvaluator)
    {
        _scriptEvaluator = scriptEvaluator;
    }

    public static string Name => WorkflowExpressionSyntaxes.JavaScript;

    public static LocalizationSource DisplayName { get; } = LocalizationSource.Create<JavaScriptExpressionProvider>("JavaScript");

    public static string EditorLanguage => "javascript";

    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context)
        => _scriptEvaluator.EvaluateAsync(expression, workflowContext, context?.MethodProviders ?? []);

    // Scripts are checked when they run: a script error is logged and evaluates to the default value.
    public IReadOnlyList<string> Validate(string expression, Type valueType)
        => [];
}
