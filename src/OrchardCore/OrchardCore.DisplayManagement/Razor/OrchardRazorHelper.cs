using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.Razor
{
    public class OrchardRazorHelper
    {
        public OrchardRazorHelper(HttpContext context)
        {
            HttpContext = context;
        }

        public HttpContext HttpContext { get; set; }
    }
}
