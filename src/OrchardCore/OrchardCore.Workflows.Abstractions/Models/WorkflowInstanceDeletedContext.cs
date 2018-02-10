namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceDeletedContext : WorkflowInstanceContext
    {
        public WorkflowInstanceDeletedContext(WorkflowInstance workflowInstance) : base(workflowInstance)
        {
        }
    }
}
