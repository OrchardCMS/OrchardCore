namespace OrchardCore.Workflows.Models
{
    public class WorkflowDefinitionContext
    {
        public WorkflowDefinitionContext(WorkflowDefinitionRecord workflowDefinition)
        {
            WorkflowDefinition = workflowDefinition;
        }

        public WorkflowDefinitionRecord WorkflowDefinition { get; }
    }
}
