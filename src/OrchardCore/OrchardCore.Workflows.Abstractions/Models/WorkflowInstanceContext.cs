namespace OrchardCore.Workflows.Models
{
    public class WorkflowInstanceContext
    {
        public WorkflowInstanceContext(WorkflowInstanceRecord workflowInstance)
        {
            WorkflowInstance = workflowInstance;
        }

        public WorkflowInstanceRecord WorkflowInstance { get; }
    }
}
