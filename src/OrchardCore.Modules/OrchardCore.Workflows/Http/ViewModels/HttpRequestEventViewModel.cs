using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Workflows.Http.ViewModels
{
    public class HttpRequestEventViewModel
    {
        public string HttpMethod { get; set; }
        public string Url { get; set; }

        private IList<string> _availableHttpMethods = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
        public IList<SelectListItem> AvailableHttpMethods => _availableHttpMethods.Select(x => new SelectListItem { Text = x, Value = x }).ToList();
    }
}
