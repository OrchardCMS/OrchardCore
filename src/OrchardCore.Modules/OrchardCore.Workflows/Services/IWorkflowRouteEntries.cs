using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowRouteEntries
    {
        IEnumerable<WorkflowRoutesEntry> GetWorkflowRouteEntries(string httpMethod, RouteValueDictionary routeValues, string correlationId);
        void AddEntries(IEnumerable<WorkflowRoutesEntry> entries);
        void RemoveEntries(string workflowId);
    }
}
