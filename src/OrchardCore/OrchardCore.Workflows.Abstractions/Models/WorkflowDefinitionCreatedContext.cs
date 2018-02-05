namespace OrchardCore.Workflows.Models
{
    public class WorkflowDefinitionCreatedContext : WorkflowDefinitionContext
    {
        public WorkflowDefinitionCreatedContext(WorkflowDefinitionRecord workflowDefinition) : base(workflowDefinition)
        {
        }
    }
}
