using Fluid;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowExecutionExpressionContext : WorkflowExecutionHandlerContextBase
    {
        public WorkflowExecutionExpressionContext(TemplateContext templateContext, WorkflowExecutionContext workflowExecutionContext) : base(workflowExecutionContext)
        {
            TemplateContext = templateContext;
        }

        public TemplateContext TemplateContext { get; }
    }
}
