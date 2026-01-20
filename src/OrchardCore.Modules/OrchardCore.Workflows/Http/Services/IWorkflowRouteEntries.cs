using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Http.Models;

namespace OrchardCore.Workflows.Http.Services
{
    internal interface IWorkflowRouteEntries
    {
        Task<IEnumerable<WorkflowRoutesEntry>> GetWorkflowRouteEntriesAsync(string httpMethod, RouteValueDictionary routeValues);
        Task AddEntriesAsync(IEnumerable<WorkflowRoutesEntry> entries);
        Task RemoveEntriesAsync(string workflowId);
    }
}
