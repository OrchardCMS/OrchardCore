using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Http.Models;

namespace OrchardCore.Workflows.Http.Services
{
    internal interface IWorkflowRouteEntries
    {
        IEnumerable<WorkflowRoutesEntry> GetWorkflowRouteEntries(string httpMethod, RouteValueDictionary routeValues);
        void AddEntries(IEnumerable<WorkflowRoutesEntry> entries);
        void RemoveEntries(string workflowId);
    }
}
