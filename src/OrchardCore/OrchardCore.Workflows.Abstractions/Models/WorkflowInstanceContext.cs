namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceContext
    {
        public WorkflowInstanceContext(WorkflowInstanceRecord workflowInstance)
        {
            WorkflowInstanceRecord = workflowInstance;
        }

        public WorkflowInstanceRecord WorkflowInstanceRecord { get; }
    }
}
