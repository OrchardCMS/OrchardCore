using System.Threading.Tasks;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Scripting;
using OrchardCore.Workflows.Services;

namespace OrchardCore.Workflows.WorkflowContextProviders
{
    public class DefaultWorkflowExecutionContextHandler : WorkflowExecutionContextHandlerBase
    {
        public override Task EvaluatingScriptAsync(WorkflowExecutionScriptContext context)
        {
            context.ScopedMethodProviders.Add(new WorkflowMethodsProvider(context.WorkflowContext));
            return Task.CompletedTask;
        }
    }
}
