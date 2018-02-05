namespace OrchardCore.Workflows.Models
{
    public class WorkflowDefinitionDeletedContext : WorkflowDefinitionContext
    {
        public WorkflowDefinitionDeletedContext(WorkflowDefinitionRecord workflowDefinition) : base(workflowDefinition)
        {
        }
    }
}
