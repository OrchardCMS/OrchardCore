using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Liquid;
using OrchardCore.Tests.Modules.OrchardCore.Workflows.Variables;
using OrchardCore.Workflows.Expressions;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Workflows.Expressions;

/// <summary>
/// The built-in expression syntaxes over the given evaluators, for tests that run activities.
/// </summary>
internal static class TestExpressions
{
    public static IWorkflowExpressionManager CreateManager(
        IWorkflowScriptEvaluator scriptEvaluator = null,
        IWorkflowExpressionEvaluator expressionEvaluator = null,
        ILiquidTemplateManager templateManager = null,
        Action<IServiceCollection> configure = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(typeof(IStringLocalizer<>), typeof(PassThroughStringLocalizer<>));
        services.AddSingleton(scriptEvaluator ?? Mock.Of<IWorkflowScriptEvaluator>());
        services.AddSingleton(expressionEvaluator ?? Mock.Of<IWorkflowExpressionEvaluator>());
        services.AddSingleton(templateManager ?? Mock.Of<ILiquidTemplateManager>());
        services.AddWorkflowExpressionProvider<LiteralExpressionProvider>();
        services.AddWorkflowExpressionProvider<LiquidExpressionProvider>();
        services.AddWorkflowExpressionProvider<JavaScriptExpressionProvider>();
        configure?.Invoke(services);
        services.AddScoped<IWorkflowExpressionManager, WorkflowExpressionManager>();

        return services.BuildServiceProvider().GetRequiredService<IWorkflowExpressionManager>();
    }
}
