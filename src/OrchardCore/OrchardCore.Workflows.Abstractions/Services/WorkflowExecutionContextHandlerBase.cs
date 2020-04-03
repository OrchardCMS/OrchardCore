using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowExecutionContextHandlerBase : IWorkflowExecutionContextHandler
    {
        public virtual Task EvaluatingExpressionAsync(WorkflowExecutionExpressionContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task EvaluatingScriptAsync(WorkflowExecutionScriptContext context)
        {
            return Task.CompletedTask;
        }

        public Task DehydrateValueAsync(SerializeWorkflowValueContext context)
        {
            return Task.CompletedTask;
        }

        public Task RehydrateValueAsync(SerializeWorkflowValueContext context)
        {
            return Task.CompletedTask;
        }
    }
}
