namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceUpdatedContext : WorkflowInstanceContext
    {
        public WorkflowInstanceUpdatedContext(WorkflowInstanceRecord workflowInstance) : base(workflowInstance)
        {
        }
    }
}
