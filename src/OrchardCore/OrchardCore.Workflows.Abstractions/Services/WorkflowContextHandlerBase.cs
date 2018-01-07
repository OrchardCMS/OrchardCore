using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowContextHandlerBase : IWorkflowContextHandler
    {
        public virtual Task EvaluatingScriptAsync(WorkflowContextScriptEvalContext context)
        {
            return Task.CompletedTask;
        }
    }
}
