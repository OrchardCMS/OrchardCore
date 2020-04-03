namespace OrchardCore.Workflows.Models
{
    public class WorkflowTypeContext
    {
        public WorkflowTypeContext(WorkflowType workflowType)
        {
            WorkflowType = workflowType;
        }

        public WorkflowType WorkflowType { get; }
    }
}
