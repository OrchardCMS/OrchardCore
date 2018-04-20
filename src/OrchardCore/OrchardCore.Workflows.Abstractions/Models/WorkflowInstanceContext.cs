namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceContext
    {
        public WorkflowInstanceContext(WorkflowInstance workflowInstance)
        {
            WorkflowInstanceRecord = workflowInstance;
        }

        public WorkflowInstance WorkflowInstanceRecord { get; }
    }
}
