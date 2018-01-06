using System.Collections.Generic;
using System.Linq;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public abstract class WorkflowPathEntriesBase : IWorkflowPathEntries
    {
        private readonly object _syncLock = new object();
        private IDictionary<string, IList<WorkflowPathEntry>> _entries = new Dictionary<string, IList<WorkflowPathEntry>>();

        public IEnumerable<WorkflowPathEntry> GetEntries(string httpMethod, string path)
        {
            var entries = _entries.Values.SelectMany(x => x).Where(x => string.Equals(x.Path, path, System.StringComparison.OrdinalIgnoreCase));
            return entries.ToList();
        }

        public WorkflowPathEntry GetEntry(string httpMethod, string workflowId, int activityId, string correlationId)
        {
            if (!_entries.ContainsKey(workflowId))
            {
                return null;
            }

            var entries = _entries[workflowId];
            var query =
                from entry in entries
                where entry.ActivityId == activityId
                && string.Equals(entry.HttpMethod, httpMethod, System.StringComparison.OrdinalIgnoreCase)
                && string.Equals(entry.CorrelationId, correlationId, System.StringComparison.OrdinalIgnoreCase)
                select entry;

            return query.FirstOrDefault();
        }

        public void AddEntries(IEnumerable<WorkflowPathEntry> entries)
        {
            lock (_syncLock)
            {
                foreach (var group in entries.GroupBy(x => x.WorkflowId))
                {
                    _entries[group.Key] = group.ToList();
                }
            }
        }

        public void RemoveEntries(string workflowId)
        {
            lock (_syncLock)
            {
                _entries.Remove(workflowId);
            }
        }
    }
}
