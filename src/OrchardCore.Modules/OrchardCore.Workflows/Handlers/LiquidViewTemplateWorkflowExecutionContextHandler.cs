using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;
using OrchardCore.Liquid;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.Handlers;

public class LiquidViewTemplateWorkflowExecutionContextHandler : WorkflowExecutionContextHandlerBase
{
    public override async Task EvaluatingExpressionAsync(WorkflowExecutionExpressionContext context)
    {
        if (context.TemplateContext is LiquidTemplateContext liquidTemplateContext &&
            liquidTemplateContext.Services.GetRequiredService<ViewContextAccessor>()?.ViewContext is { } viewContext)
        {
            await liquidTemplateContext.InitializeAsync(viewContext);
        }
    }
}
