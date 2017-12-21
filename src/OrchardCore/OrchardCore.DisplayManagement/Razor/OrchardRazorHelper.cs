using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.Razor
{
    public class OrchardRazorHelper
    {
        public OrchardRazorHelper(HttpContext context, dynamic displayHelper)
        {
            HttpContext = context;
            DisplayHelper = displayHelper;
        }

        public HttpContext HttpContext { get; set; }
        public dynamic DisplayHelper { get; set; }
    }
}
