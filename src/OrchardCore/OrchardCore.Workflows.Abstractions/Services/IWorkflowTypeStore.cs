using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services;

public interface IWorkflowTypeStore
{
    Task<WorkflowType> GetAsync(long id);
    Task<WorkflowType> GetAsync(string uid);
    Task<IEnumerable<WorkflowType>> GetAsync(IEnumerable<long> ids);
    Task<IEnumerable<WorkflowType>> ListAsync();
    Task<IEnumerable<WorkflowType>> GetByStartActivityAsync(string activityName);
    Task SaveAsync(WorkflowType workflowType);
    Task DeleteAsync(WorkflowType workflowType);
}
