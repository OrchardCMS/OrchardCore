using System.Collections.Generic;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowPathEntries
    {
        IEnumerable<WorkflowPathEntry> GetEntries(string httpMethod, string path);
        WorkflowPathEntry GetEntry(string httpMethod, string workflowId, int activityId, string correlationId);
        void AddEntries(IEnumerable<WorkflowPathEntry> entries);
        void RemoveEntries(string workflowId);
    }
}
