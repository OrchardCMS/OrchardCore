using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Http.Models;

namespace OrchardCore.Workflows.Http.Services
{
    internal abstract class WorkflowRouteEntriesBase : IWorkflowRouteEntries
    {
        private readonly object _syncLock = new object();
        private IDictionary<string, IList<WorkflowRoutesEntry>> _entries = new Dictionary<string, IList<WorkflowRoutesEntry>>();

        public IEnumerable<WorkflowRoutesEntry> GetWorkflowRouteEntries(string httpMethod, RouteValueDictionary routeValues)
        {
            var controllerName = routeValues.GetValue<string>("controller");
            var actionName = routeValues.GetValue<string>("action");
            var areaName = routeValues.GetValue<string>("area");

            return _entries.Values.SelectMany(x => x).Where(x =>
                x.HttpMethod == httpMethod
                && (x.ControllerName == controllerName || string.IsNullOrWhiteSpace(x.ControllerName))
                && (x.ActionName == actionName || string.IsNullOrWhiteSpace(x.ActionName))
                && (x.AreaName == areaName || string.IsNullOrWhiteSpace(x.AreaName)))
                .ToArray();
        }

        public void AddEntries(IEnumerable<WorkflowRoutesEntry> entries)
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
