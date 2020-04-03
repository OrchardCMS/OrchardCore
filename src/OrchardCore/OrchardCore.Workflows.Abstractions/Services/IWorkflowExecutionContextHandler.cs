using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowExecutionContextHandler
    {
        Task EvaluatingExpressionAsync(WorkflowExecutionExpressionContext context);
        Task EvaluatingScriptAsync(WorkflowExecutionScriptContext context);
        Task DehydrateValueAsync(SerializeWorkflowValueContext context);
        Task RehydrateValueAsync(SerializeWorkflowValueContext context);
    }
}
