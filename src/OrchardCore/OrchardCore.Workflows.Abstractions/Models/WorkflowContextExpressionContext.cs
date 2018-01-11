using Fluid;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowContextExpressionContext : WorkflowContextHandlerContextBase
    {
        public WorkflowContextExpressionContext(TemplateContext templateContext, WorkflowContext workflowContext) : base(workflowContext)
        {
            TemplateContext = templateContext;
        }

        public TemplateContext TemplateContext { get; }
    }
}
