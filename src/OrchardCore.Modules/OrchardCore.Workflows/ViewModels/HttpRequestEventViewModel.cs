using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.ViewModels
{
    public class HttpRequestEventViewModel
    {
        public HttpProcessingMode Mode { get; set; }
        public string HttpMethod { get; set; }
        public string RequestPath { get; set; }

        private IList<string> _availableHttpMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public IList<SelectListItem> AvailableHttpMethods => _availableHttpMethods.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
    }
}
