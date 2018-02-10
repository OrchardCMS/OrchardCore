using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowInstanceIdGenerator
    {
        string GenerateUniqueId(WorkflowInstance workflowInstanceRecord);
    }
}
