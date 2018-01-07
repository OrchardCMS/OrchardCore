namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceCreatedContext : WorkflowInstanceContext
    {
        public WorkflowInstanceCreatedContext(WorkflowInstanceRecord workflowInstance) : base(workflowInstance)
        {
        }
    }
}
