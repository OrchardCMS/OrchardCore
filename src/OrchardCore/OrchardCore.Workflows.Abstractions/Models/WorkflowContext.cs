namespace OrchardCore.Workflows.Models
{
    public class WorkflowContext
    {
        public WorkflowContext(Workflow workflow)
        {
            Workflow = workflow;
        }

        public Workflow Workflow { get; }
    }
}
