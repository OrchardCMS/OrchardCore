using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Helpers;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public class WorkflowRouteEntries : IWorkflowRouteEntries
    {
        private readonly object _syncLock = new object();
        private IDictionary<int, IList<WorkflowRoutesEntry>> _entries = new Dictionary<int, IList<WorkflowRoutesEntry>>();

        public IEnumerable<WorkflowRoutesEntry> GetWorkflowRouteEntries(string httpMethod, RouteValueDictionary routeValues)
        {
            var controllerName = routeValues.GetValue<string>("controller");
            var actionName = routeValues.GetValue<string>("action");
            var areaName = routeValues.GetValue<string>("area");

            var entries = _entries.Values.SelectMany(x => x).Where(x =>
                x.HttpMethod == httpMethod
                && (x.ControllerName == controllerName || string.IsNullOrWhiteSpace(x.ControllerName))
                && (x.ActionName == actionName || string.IsNullOrWhiteSpace(x.ActionName))
                && (x.AreaName == areaName || string.IsNullOrWhiteSpace(x.AreaName)));

            return entries.ToList();
        }

        public void AddEntries(int workflowDefinitionId, IEnumerable<WorkflowRoutesEntry> entries)
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
