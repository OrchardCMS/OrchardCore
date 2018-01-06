using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Workflows.Models
{
    public class WorkflowRoutesEntry
    {
        public int WorkflowDefinitionId { get; set; }
        public int ActivityId { get; set; }
        public string HttpMethod { get; set; }
        public RouteValueDictionary RouteValues { get; set; }

        public string ControllerName => RouteValues.GetValue<string>("controller");
        public string ActionName => RouteValues.GetValue<string>("action");
        public string AreaName => RouteValues.GetValue<string>("area");
    }
}
