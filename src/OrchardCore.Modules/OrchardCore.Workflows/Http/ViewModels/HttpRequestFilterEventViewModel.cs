using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpRequestFilterEventViewModel
    {
        public string HttpMethod { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string AreaName { get; set; }
        // TODO: Add support for arbitrary route values.

        private readonly IList<string> _availableHttpMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public IList<SelectListItem> AvailableHttpMethods => _availableHttpMethods.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
    }
}
