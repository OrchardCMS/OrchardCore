using OrchardCore.Liquid;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;

/// <summary>
/// The built-in expression syntaxes over the given evaluators, for tests that run activities.
/// </summary>
internal static class TestExpressions
{
    public static IWorkflowExpressionManager CreateManager(IWorkflowScriptEvaluator scriptEvaluator = null, IWorkflowExpressionEvaluator expressionEvaluator = null)
        => new WorkflowExpressionManager(
        [
            new LiteralExpressionProvider(new PassThroughStringLocalizer<LiteralExpressionProvider>()),
            new LiquidExpressionProvider(expressionEvaluator ?? Mock.Of<IWorkflowExpressionEvaluator>(), Mock.Of<ILiquidTemplateManager>(), new PassThroughStringLocalizer<LiquidExpressionProvider>()),
            new JavaScriptExpressionProvider(scriptEvaluator ?? Mock.Of<IWorkflowScriptEvaluator>(), new PassThroughStringLocalizer<JavaScriptExpressionProvider>()),
        ]);
}
