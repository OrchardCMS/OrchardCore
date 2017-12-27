using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowDefinitionRepository
    {
        Task<WorkflowDefinitionRecord> GetWorkflowDefinitionAsync(int id);
        Task<IList<WorkflowDefinitionRecord>> GetWorkflowDefinitionsByStartActivityAsync(string activityName);
    }
}
