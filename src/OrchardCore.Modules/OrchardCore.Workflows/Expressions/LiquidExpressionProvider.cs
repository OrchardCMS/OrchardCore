using Microsoft.Extensions.Localization;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Expressions;

/// <summary>
/// A Liquid template, evaluated by <see cref="IWorkflowExpressionEvaluator"/>.
/// </summary>
public sealed class LiquidExpressionProvider : IWorkflowExpressionProvider
{
    private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
    private readonly ILiquidTemplateManager _templateManager;

    internal readonly IStringLocalizer S;

    public LiquidExpressionProvider(
        IWorkflowExpressionEvaluator expressionEvaluator,
        ILiquidTemplateManager templateManager,
        IStringLocalizer<LiquidExpressionProvider> localizer)
    {
        _expressionEvaluator = expressionEvaluator;
        _templateManager = templateManager;
        S = localizer;
    }

    public string Name => WorkflowExpressionSyntaxes.Liquid;

    public LocalizedString DisplayName => S["Liquid"];

    public string EditorLanguage => "liquid";

    public Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, WorkflowExpressionEvaluationContext context)
        => _expressionEvaluator.EvaluateAsync(expression, workflowContext, context?.Encoder);

    public IReadOnlyList<string> Validate(string expression, Type valueType)
        => string.IsNullOrWhiteSpace(expression) || _templateManager.Validate(expression, out var errors)
            ? []
            : [S["The Liquid template isn't valid: {0}", string.Join(" ", errors ?? [])]];
}
