using System.Text.Encodings.Web;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowExpressionEvaluator
    {
        Task<T> EvaluateAsync<T>(WorkflowExpression<T> expression, WorkflowExecutionContext workflowContext, TextEncoder encoder);
    }
}
