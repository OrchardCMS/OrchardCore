using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowDefinitionIdGenerator
    {
        string GenerateUniqueId(WorkflowDefinition workflowDefinitionecord);
    }
}
