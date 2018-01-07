using System.Collections.Generic;
using OrchardCore.Workflows.Http.Models;

namespace OrchardCore.Workflows.Http.Services
{
    public interface IWorkflowPathEntries
    {
        IEnumerable<WorkflowPathEntry> GetEntries(string httpMethod, string path);
        WorkflowPathEntry GetEntry(string httpMethod, string workflowId, int activityId, string correlationId);
        void AddEntries(IEnumerable<WorkflowPathEntry> entries);
        void RemoveEntries(string workflowId);
    }
}
