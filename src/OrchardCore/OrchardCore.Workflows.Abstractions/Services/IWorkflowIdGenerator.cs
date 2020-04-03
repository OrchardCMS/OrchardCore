using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowIdGenerator
    {
        string GenerateUniqueId(Workflow workflow);
    }
}
