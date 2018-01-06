using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowPathEntries : IWorkflowPathEntries
    {
        private readonly object _syncLock = new object();
        private IDictionary<int, IList<WorkflowPathEntry>> _entries = new Dictionary<int, IList<WorkflowPathEntry>>();

        public IEnumerable<WorkflowPathEntry> GetWorkflowPathEntries(string httpMethod, string path)
        {
            var entries = _entries.Values.SelectMany(x => x).Where(x => string.Equals(x.Path, path, System.StringComparison.OrdinalIgnoreCase));
            return entries.ToList();
        }

        public void AddEntries(int workflowDefinitionId, IEnumerable<WorkflowPathEntry> entries)
        {
            lock (_syncLock)
            {
                _entries[workflowDefinitionId] = entries.ToList();
            }
        }

        public void RemoveEntries(int workflowDefinitionId)
        {
            lock (_syncLock)
            {
                _entries.Remove(workflowDefinitionId);
            }
        }
    }
}
