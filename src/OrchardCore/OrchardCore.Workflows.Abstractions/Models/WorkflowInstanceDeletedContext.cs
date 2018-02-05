namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceDeletedContext : WorkflowInstanceContext
    {
        public WorkflowInstanceDeletedContext(WorkflowInstanceRecord workflowInstance) : base(workflowInstance)
        {
        }
    }
}
