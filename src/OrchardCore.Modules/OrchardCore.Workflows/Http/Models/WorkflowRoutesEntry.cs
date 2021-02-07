using Microsoft.AspNetCore.Routing;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Workflows.Http.Models
{
    public class WorkflowRoutesEntry
    {
        public string WorkflowId { get; set; }
        public string ActivityId { get; set; }
        public string HttpMethod { get; set; }
        public RouteValueDictionary RouteValues { get; set; } = new RouteValueDictionary();

        public string ControllerName => RouteValues.GetValue<string>("controller");
        public string ActionName => RouteValues.GetValue<string>("action");
        public string AreaName => RouteValues.GetValue<string>("area");
    }
}
