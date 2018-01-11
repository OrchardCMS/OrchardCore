using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowContextHandler
    {
        Task EvaluatingExpressionAsync(WorkflowContextExpressionContext context);
        Task EvaluatingScriptAsync(WorkflowContextScriptContext context);
    }
}
