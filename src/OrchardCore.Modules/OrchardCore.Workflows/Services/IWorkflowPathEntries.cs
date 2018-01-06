using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowPathEntries
    {
        IEnumerable<WorkflowPathEntry> GetWorkflowPathEntries(string httpMethod, string path);
        void AddEntries(int workflowDefinitionId, IEnumerable<WorkflowPathEntry> entries);
        void RemoveEntries(int workflowDefinitionId);
    }
}
