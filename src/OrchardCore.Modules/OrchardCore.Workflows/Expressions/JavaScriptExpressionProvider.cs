using Microsoft.Extensions.Localization;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions;

/// <summary>
/// A JavaScript expression, evaluated by <see cref="IWorkflowScriptEvaluator"/>.
/// </summary>
public sealed class JavaScriptExpressionProvider : IWorkflowExpressionProvider
{
    private readonly IWorkflowScriptEvaluator _scriptEvaluator;

    internal readonly IStringLocalizer S;

    public JavaScriptExpressionProvider(IWorkflowScriptEvaluator scriptEvaluator, IStringLocalizer<JavaScriptExpressionProvider> localizer)
    {
        _scriptEvaluator = scriptEvaluator;
        S = localizer;
    }

    public string Name => WorkflowExpressionSyntaxes.JavaScript;

    public LocalizedString DisplayName => S["JavaScript"];

    public string EditorLanguage => "javascript";

    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context)
        => _scriptEvaluator.EvaluateAsync(expression, workflowContext, context?.MethodProviders ?? []);

    // Scripts are checked when they run: a script error is logged and evaluates to the default value.
    public IReadOnlyList<string> Validate(string expression, Type valueType)
        => [];
}
