namespace OrchardCore.Workflows.Models
{
    public class WorkflowDefinitionContext
    {
        public WorkflowDefinitionContext(WorkflowDefinition workflowDefinition)
        {
            WorkflowDefinition = workflowDefinition;
        }

        public WorkflowDefinition WorkflowDefinition { get; }
    }
}
