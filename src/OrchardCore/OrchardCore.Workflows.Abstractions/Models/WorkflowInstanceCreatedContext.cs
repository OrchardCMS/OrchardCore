namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceCreatedContext : WorkflowInstanceContext
    {
        public WorkflowInstanceCreatedContext(WorkflowInstance workflowInstance) : base(workflowInstance)
        {
        }
    }
}
