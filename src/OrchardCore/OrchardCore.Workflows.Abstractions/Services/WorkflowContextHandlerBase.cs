using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowContextHandlerBase : IWorkflowContextHandler
    {
        public virtual Task EvaluatingExpressionAsync(WorkflowContextExpressionContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task EvaluatingScriptAsync(WorkflowContextScriptContext context)
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
