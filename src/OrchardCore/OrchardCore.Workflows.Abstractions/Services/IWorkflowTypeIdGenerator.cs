using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowTypeIdGenerator
    {
        string GenerateUniqueId(WorkflowType workflowType);
        string GenerateVersionUniqueId(WorkflowType workflowType);
    }
}
