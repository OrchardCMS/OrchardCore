namespace OrchardCore.Workflows.Models
{
    public class WorkflowContextHandlerContextBase
    {
        protected WorkflowContextHandlerContextBase(WorkflowContext workflowContext)
        {
            WorkflowContext = workflowContext;
        }

        public WorkflowContext WorkflowContext { get; }
    }
}
